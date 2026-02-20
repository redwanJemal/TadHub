import { getAccessToken, getTenantId } from '../../features/auth/AuthProvider';
import { ApiResponse, ApiSuccessResponse, QueryParams, buildQueryString } from './types';

const API_BASE = import.meta.env.VITE_API_URL || '/api/v1';

// Retry configuration
const RETRY_CONFIG = {
  maxRetries: 3,
  baseDelay: 1000,
  maxDelay: 10000,
  retryableStatuses: [408, 429, 500, 502, 503, 504],
};

/**
 * Custom API Error class with status code and error details
 */
export class ApiError extends Error {
  constructor(
    public status: number,
    public code: string,
    message: string,
    public details?: unknown
  ) {
    super(message);
    this.name = 'ApiError';
  }

  isValidationError(): boolean {
    return this.code === 'VALIDATION_ERROR';
  }

  isUnauthorized(): boolean {
    return this.status === 401;
  }

  isForbidden(): boolean {
    return this.status === 403;
  }

  isNotFound(): boolean {
    return this.status === 404;
  }

  isRetryable(): boolean {
    return RETRY_CONFIG.retryableStatuses.includes(this.status);
  }
}

/**
 * Calculate exponential backoff delay with jitter
 */
function calculateBackoff(attempt: number): number {
  const exponentialDelay = RETRY_CONFIG.baseDelay * Math.pow(2, attempt);
  const jitter = Math.random() * 0.3 * exponentialDelay;
  return Math.min(exponentialDelay + jitter, RETRY_CONFIG.maxDelay);
}

/**
 * Sleep helper
 */
function sleep(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * Get authentication headers from OIDC context
 */
function getAuthHeaders(): HeadersInit {
  const headers: HeadersInit = {};
  
  const token = getAccessToken();
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  
  const tenantId = getTenantId();
  if (tenantId) {
    headers['X-Tenant-ID'] = tenantId;
  }
  
  return headers;
}

/**
 * Handle API response and throw on error
 */
async function handleResponse<T>(response: Response): Promise<T> {
  // Handle 401 - dispatch event for auth handling
  if (response.status === 401) {
    window.dispatchEvent(new CustomEvent('auth:unauthorized'));
    throw new ApiError(401, 'UNAUTHORIZED', 'Authentication required');
  }

  // Handle 403 - dispatch event for permission denied UI
  if (response.status === 403) {
    let errorMessage = 'You do not have permission to perform this action';
    let errorCode = 'FORBIDDEN';
    
    try {
      const json = await response.json();
      if (json.error?.message) {
        errorMessage = json.error.message;
      }
      if (json.error?.code) {
        errorCode = json.error.code;
      }
    } catch {
      // Ignore parse errors for 403
    }
    
    window.dispatchEvent(new CustomEvent('auth:forbidden', {
      detail: { message: errorMessage, code: errorCode }
    }));
    throw new ApiError(403, errorCode, errorMessage);
  }

  let json: ApiResponse<T>;

  try {
    json = await response.json();
  } catch {
    throw new ApiError(
      response.status,
      'PARSE_ERROR',
      'Failed to parse response'
    );
  }

  if (!response.ok || !json.success) {
    const error = 'error' in json ? json.error : undefined;
    throw new ApiError(
      response.status,
      error?.code ?? 'UNKNOWN_ERROR',
      error?.message ?? 'An error occurred',
      error?.details
    );
  }

  return json.data as T;
}

/**
 * Handle API response and return full response with meta
 */
async function handleResponseWithMeta<T>(response: Response): Promise<ApiSuccessResponse<T>> {
  // Handle 401 - dispatch event for auth handling
  if (response.status === 401) {
    window.dispatchEvent(new CustomEvent('auth:unauthorized'));
    throw new ApiError(401, 'UNAUTHORIZED', 'Authentication required');
  }

  // Handle 403 - dispatch event for permission denied UI
  if (response.status === 403) {
    let errorMessage = 'You do not have permission to perform this action';
    let errorCode = 'FORBIDDEN';
    
    try {
      const json = await response.json();
      if (json.error?.message) {
        errorMessage = json.error.message;
      }
      if (json.error?.code) {
        errorCode = json.error.code;
      }
    } catch {
      // Ignore parse errors for 403
    }
    
    window.dispatchEvent(new CustomEvent('auth:forbidden', {
      detail: { message: errorMessage, code: errorCode }
    }));
    throw new ApiError(403, errorCode, errorMessage);
  }

  let json: ApiResponse<T>;

  try {
    json = await response.json();
  } catch {
    throw new ApiError(
      response.status,
      'PARSE_ERROR',
      'Failed to parse response'
    );
  }

  if (!response.ok || !json.success) {
    const error = 'error' in json ? json.error : undefined;
    throw new ApiError(
      response.status,
      error?.code ?? 'UNKNOWN_ERROR',
      error?.message ?? 'An error occurred',
      error?.details
    );
  }

  return json as ApiSuccessResponse<T>;
}

/**
 * Build full URL with query parameters
 */
function buildUrl(endpoint: string, params?: QueryParams): string {
  const queryString = params ? buildQueryString(params) : '';
  return `${API_BASE}${endpoint}${queryString}`;
}

/**
 * Fetch with retry logic
 */
async function fetchWithRetry(
  url: string,
  options: RequestInit,
  maxRetries: number = RETRY_CONFIG.maxRetries
): Promise<Response> {
  let lastError: Error | null = null;

  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      const response = await fetch(url, options);

      // Check if we should retry based on status code
      if (
        RETRY_CONFIG.retryableStatuses.includes(response.status) &&
        attempt < maxRetries
      ) {
        const delay = calculateBackoff(attempt);
        console.warn(
          `Request to ${url} returned ${response.status}, retrying in ${Math.round(delay)}ms (attempt ${attempt + 1}/${maxRetries})`
        );
        await sleep(delay);
        continue;
      }

      return response;
    } catch (error) {
      lastError = error as Error;

      // Network errors are retryable
      if (attempt < maxRetries) {
        const delay = calculateBackoff(attempt);
        console.warn(
          `Network error for ${url}, retrying in ${Math.round(delay)}ms (attempt ${attempt + 1}/${maxRetries})`
        );
        await sleep(delay);
        continue;
      }
    }
  }

  // All retries failed
  throw new ApiError(
    0,
    'NETWORK_ERROR',
    lastError?.message || 'Network request failed after retries'
  );
}

/**
 * API client with typed methods and automatic retry
 */
export const apiClient = {
  async get<T>(endpoint: string, params?: QueryParams): Promise<T> {
    const response = await fetchWithRetry(buildUrl(endpoint, params), {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
    });
    return handleResponse<T>(response);
  },

  async getWithMeta<T>(endpoint: string, params?: QueryParams): Promise<ApiSuccessResponse<T>> {
    const response = await fetchWithRetry(buildUrl(endpoint, params), {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
    });
    return handleResponseWithMeta<T>(response);
  },

  async post<T>(endpoint: string, body?: unknown): Promise<T> {
    const response = await fetchWithRetry(buildUrl(endpoint), {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
      body: body ? JSON.stringify(body) : undefined,
    });
    return handleResponse<T>(response);
  },

  async patch<T>(endpoint: string, body?: unknown): Promise<T> {
    const response = await fetchWithRetry(buildUrl(endpoint), {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
      body: body ? JSON.stringify(body) : undefined,
    });
    return handleResponse<T>(response);
  },

  async put<T>(endpoint: string, body?: unknown): Promise<T> {
    const response = await fetchWithRetry(buildUrl(endpoint), {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
      body: body ? JSON.stringify(body) : undefined,
    });
    return handleResponse<T>(response);
  },

  async delete<T>(endpoint: string): Promise<T> {
    const response = await fetchWithRetry(buildUrl(endpoint), {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
    });
    return handleResponse<T>(response);
  },
};

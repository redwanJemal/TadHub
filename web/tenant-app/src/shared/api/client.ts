import { getAccessToken, getTenantId } from '../../features/auth/AuthProvider';
import { ApiResponse, ApiSuccessResponse, QueryParams, buildQueryString } from './types';

const API_BASE = import.meta.env.VITE_API_URL || '/api/v1';

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
 * API client with typed methods
 */
export const apiClient = {
  async get<T>(endpoint: string, params?: QueryParams): Promise<T> {
    const response = await fetch(buildUrl(endpoint, params), {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
    });
    return handleResponse<T>(response);
  },

  async getWithMeta<T>(endpoint: string, params?: QueryParams): Promise<ApiSuccessResponse<T>> {
    const response = await fetch(buildUrl(endpoint, params), {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
    });
    return handleResponseWithMeta<T>(response);
  },

  async post<T>(endpoint: string, body?: unknown): Promise<T> {
    const response = await fetch(buildUrl(endpoint), {
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
    const response = await fetch(buildUrl(endpoint), {
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
    const response = await fetch(buildUrl(endpoint), {
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
    const response = await fetch(buildUrl(endpoint), {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
    });
    return handleResponse<T>(response);
  },
};

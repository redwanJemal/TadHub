import { ApiResponse, ApiSuccessResponse, QueryParams, buildQueryString } from './types';

const API_BASE = '/api/v1';

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

// Track if we're currently refreshing to prevent multiple refresh calls
let isRefreshing = false;
let refreshPromise: Promise<boolean> | null = null;

/**
 * Get authentication headers from storage
 */
function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem('forgebase_admin_token');
  return token ? { Authorization: `Bearer ${token}` } : {};
}

/**
 * Attempt to refresh the access token
 */
async function refreshAccessToken(): Promise<boolean> {
  const refreshToken = localStorage.getItem('forgebase_refresh_token');
  if (!refreshToken) return false;

  try {
    const response = await fetch(`${API_BASE}/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    });

    if (!response.ok) return false;

    const json = await response.json();
    if (json.success && json.data?.tokens) {
      localStorage.setItem('forgebase_admin_token', json.data.tokens.accessToken);
      localStorage.setItem('forgebase_refresh_token', json.data.tokens.refreshToken);
      return true;
    }
    return false;
  } catch {
    return false;
  }
}

/**
 * Handle 401 - attempt refresh or redirect to login
 */
async function handleUnauthorized(): Promise<boolean> {
  // If already refreshing, wait for that to complete
  if (isRefreshing && refreshPromise) {
    return refreshPromise;
  }

  isRefreshing = true;
  refreshPromise = refreshAccessToken().finally(() => {
    isRefreshing = false;
    refreshPromise = null;
  });

  const success = await refreshPromise;
  
  if (!success) {
    // Clear tokens and redirect to login
    localStorage.removeItem('forgebase_admin_token');
    localStorage.removeItem('forgebase_refresh_token');
    localStorage.removeItem('forgebase_user');
    
    // Only redirect if not already on login page
    if (!window.location.pathname.includes('/login')) {
      window.location.href = '/login';
    }
  }
  
  return success;
}

/**
 * Handle API response and throw on error
 */
async function handleResponse<T>(response: Response, retryFn?: () => Promise<T>): Promise<T> {
  // Handle 401 - try refresh
  if (response.status === 401 && retryFn) {
    const refreshed = await handleUnauthorized();
    if (refreshed) {
      return retryFn();
    }
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
async function handleResponseWithMeta<T>(response: Response, retryFn?: () => Promise<ApiSuccessResponse<T>>): Promise<ApiSuccessResponse<T>> {
  // Handle 401 - try refresh
  if (response.status === 401 && retryFn) {
    const refreshed = await handleUnauthorized();
    if (refreshed) {
      return retryFn();
    }
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
    const doFetch = async (): Promise<T> => {
      const response = await fetch(buildUrl(endpoint, params), {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          ...getAuthHeaders(),
        },
      });
      return handleResponse<T>(response, doFetch);
    };
    return doFetch();
  },

  async getWithMeta<T>(endpoint: string, params?: QueryParams): Promise<ApiSuccessResponse<T>> {
    const doFetch = async (): Promise<ApiSuccessResponse<T>> => {
      const response = await fetch(buildUrl(endpoint, params), {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          ...getAuthHeaders(),
        },
      });
      return handleResponseWithMeta<T>(response, doFetch);
    };
    return doFetch();
  },

  async post<T>(endpoint: string, body?: unknown): Promise<T> {
    const doFetch = async (): Promise<T> => {
      const response = await fetch(buildUrl(endpoint), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...getAuthHeaders(),
        },
        body: body ? JSON.stringify(body) : undefined,
      });
      return handleResponse<T>(response, doFetch);
    };
    return doFetch();
  },

  async patch<T>(endpoint: string, body?: unknown): Promise<T> {
    const doFetch = async (): Promise<T> => {
      const response = await fetch(buildUrl(endpoint), {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          ...getAuthHeaders(),
        },
        body: body ? JSON.stringify(body) : undefined,
      });
      return handleResponse<T>(response, doFetch);
    };
    return doFetch();
  },

  async put<T>(endpoint: string, body?: unknown): Promise<T> {
    const doFetch = async (): Promise<T> => {
      const response = await fetch(buildUrl(endpoint), {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          ...getAuthHeaders(),
        },
        body: body ? JSON.stringify(body) : undefined,
      });
      return handleResponse<T>(response, doFetch);
    };
    return doFetch();
  },

  async delete<T>(endpoint: string): Promise<T> {
    const doFetch = async (): Promise<T> => {
      const response = await fetch(buildUrl(endpoint), {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
          ...getAuthHeaders(),
        },
      });
      return handleResponse<T>(response, doFetch);
    };
    return doFetch();
  },
};

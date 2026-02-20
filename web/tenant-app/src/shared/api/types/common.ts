/**
 * Standard API response meta with pagination info
 */
export interface ResponseMeta {
  requestId: string;
  timestamp: string;
  page?: number;
  perPage?: number;
  total?: number;
  totalPages?: number;
  hasNext?: boolean;
  hasPrev?: boolean;
}

/**
 * Error details for validation errors
 */
export interface ValidationError {
  field: string;
  message: string;
  constraint?: string;
}

/**
 * Standard API error response
 */
export interface ApiErrorResponse {
  success: false;
  error: {
    code: string;
    message: string;
    details?: ValidationError[];
  };
  meta: ResponseMeta;
}

/**
 * Standard API success response
 */
export interface ApiSuccessResponse<T> {
  success: true;
  data: T;
  meta: ResponseMeta;
}

/**
 * Union type for all API responses
 */
export type ApiResponse<T> = ApiSuccessResponse<T> | ApiErrorResponse;

/**
 * Paginated response data
 */
export interface PaginatedData<T> {
  items: T[];
  total: number;
  page: number;
  perPage: number;
  totalPages: number;
}

/**
 * Query parameters for list endpoints
 */
export interface QueryParams {
  page?: number;
  perPage?: number;
  sort?: string;
  order?: 'asc' | 'desc';
  q?: string;
  include?: string;
  fields?: string;
  filter?: Record<string, string | Record<string, string>>;
  [key: string]: unknown;
}

/**
 * Filter operators
 */
export type FilterOperator = 'eq' | 'ne' | 'gt' | 'gte' | 'lt' | 'lte' | 'in' | 'nin' | 'like' | 'null';

/**
 * Build filter query string
 * @example buildFilterParams({ status: 'active', age: { gte: '18' } })
 * // => { 'filter[status]': 'active', 'filter[age][gte]': '18' }
 */
export function buildFilterParams(
  filters: Record<string, string | Record<string, string>>
): Record<string, string> {
  const result: Record<string, string> = {};

  for (const [field, value] of Object.entries(filters)) {
    if (value === undefined || value === null || value === '') continue;

    if (typeof value === 'string') {
      result[`filter[${field}]`] = value;
    } else if (typeof value === 'object') {
      for (const [op, opValue] of Object.entries(value)) {
        if (opValue !== undefined && opValue !== null && opValue !== '') {
          result[`filter[${field}][${op}]`] = opValue;
        }
      }
    }
  }

  return result;
}

/**
 * Build query string from params
 */
export function buildQueryString(params: QueryParams): string {
  const searchParams = new URLSearchParams();

  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') continue;

    if (key === 'filter' && typeof value === 'object') {
      const filterParams = buildFilterParams(value as Record<string, string | Record<string, string>>);
      for (const [filterKey, filterValue] of Object.entries(filterParams)) {
        searchParams.set(filterKey, filterValue);
      }
    } else if (typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean') {
      searchParams.set(key, String(value));
    }
  }

  const queryString = searchParams.toString();
  return queryString ? `?${queryString}` : '';
}

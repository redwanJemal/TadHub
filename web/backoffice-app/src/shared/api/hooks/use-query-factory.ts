import {
  useQuery,
  useMutation,
  useQueryClient,
  UseMutationOptions,
  QueryKey,
  UseQueryResult,
} from '@tanstack/react-query';
import { apiClient, ApiError } from '../client';
import type { QueryParams, PaginatedData, ApiSuccessResponse, ResponseMeta } from '../types';

/**
 * Paginated query result with helper methods
 */
export interface PaginatedQueryResult<T> {
  items: T[];
  meta: ResponseMeta;
  isLoading: boolean;
  isError: boolean;
  error: ApiError | null;
  refetch: () => void;
  // Pagination helpers
  hasNextPage: boolean;
  hasPrevPage: boolean;
  totalPages: number;
  total: number;
  currentPage: number;
}

/**
 * Options for list queries
 */
export interface ListQueryOptions<T> {
  enabled?: boolean;
  staleTime?: number;
  cacheTime?: number;
  refetchOnMount?: boolean;
  select?: (data: PaginatedData<T>) => PaginatedData<T>;
}

/**
 * Options for detail queries
 */
export interface DetailQueryOptions<T> {
  enabled?: boolean;
  staleTime?: number;
  cacheTime?: number;
  select?: (data: T) => T;
}

/**
 * Create a paginated list query hook
 */
export function createListQuery<T>(
  key: string,
  endpoint: string
) {
  return function useListQuery(
    params?: QueryParams,
    options?: ListQueryOptions<T>
  ): PaginatedQueryResult<T> {
    const queryKey: QueryKey = [key, 'list', params];

    const query = useQuery<ApiSuccessResponse<PaginatedData<T>>, ApiError>({
      queryKey,
      queryFn: () => apiClient.getWithMeta<PaginatedData<T>>(endpoint, params),
      enabled: options?.enabled ?? true,
      staleTime: options?.staleTime,
      gcTime: options?.cacheTime,
      refetchOnMount: options?.refetchOnMount,
    });

    const data = query.data?.data;
    const meta = query.data?.meta ?? {} as ResponseMeta;

    return {
      items: data?.items ?? [],
      meta,
      isLoading: query.isLoading,
      isError: query.isError,
      error: query.error,
      refetch: query.refetch,
      hasNextPage: meta.hasNext ?? false,
      hasPrevPage: meta.hasPrev ?? false,
      totalPages: meta.totalPages ?? 0,
      total: meta.total ?? 0,
      currentPage: meta.page ?? 1,
    };
  };
}

/**
 * Create a detail query hook
 */
export function createDetailQuery<T>(
  key: string,
  endpoint: (id: string) => string
) {
  return function useDetailQuery(
    id: string | undefined,
    options?: DetailQueryOptions<T>
  ): UseQueryResult<T, ApiError> {
    const queryKey: QueryKey = [key, 'detail', id];

    return useQuery<T, ApiError>({
      queryKey,
      queryFn: () => apiClient.get<T>(endpoint(id!)),
      enabled: !!id && (options?.enabled ?? true),
      staleTime: options?.staleTime,
      gcTime: options?.cacheTime,
    });
  };
}

/**
 * Create mutation hooks for CRUD operations
 */
export function createMutations<TCreate, TUpdate, TItem>(
  key: string,
  endpoints: {
    create: string;
    update: (id: string) => string;
    delete: (id: string) => string;
  }
) {
  return {
    useCreate: (
      options?: UseMutationOptions<TItem, ApiError, TCreate>
    ) => {
      const queryClient = useQueryClient();

      return useMutation<TItem, ApiError, TCreate>({
        mutationFn: (data) => apiClient.post<TItem>(endpoints.create, data),
        onSuccess: () => {
          queryClient.invalidateQueries({ queryKey: [key, 'list'] });
        },
        ...options,
      });
    },

    useUpdate: (
      options?: UseMutationOptions<TItem, ApiError, { id: string; data: TUpdate }>
    ) => {
      const queryClient = useQueryClient();

      return useMutation<TItem, ApiError, { id: string; data: TUpdate }>({
        mutationFn: ({ id, data }) => apiClient.patch<TItem>(endpoints.update(id), data),
        onSuccess: (_, { id }) => {
          queryClient.invalidateQueries({ queryKey: [key, 'list'] });
          queryClient.invalidateQueries({ queryKey: [key, 'detail', id] });
        },
        ...options,
      });
    },

    useDelete: (
      options?: UseMutationOptions<void, ApiError, string>
    ) => {
      const queryClient = useQueryClient();

      return useMutation<void, ApiError, string>({
        mutationFn: (id) => apiClient.delete<void>(endpoints.delete(id)),
        onSuccess: () => {
          queryClient.invalidateQueries({ queryKey: [key, 'list'] });
        },
        ...options,
      });
    },
  };
}

/**
 * Prefetch list data
 */
export function createPrefetch<T>(
  key: string,
  endpoint: string
) {
  return async function prefetch(
    queryClient: ReturnType<typeof useQueryClient>,
    params?: QueryParams
  ) {
    await queryClient.prefetchQuery({
      queryKey: [key, 'list', params],
      queryFn: () => apiClient.getWithMeta<PaginatedData<T>>(endpoint, params),
    });
  };
}

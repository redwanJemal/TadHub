import { useState, useCallback, useMemo } from 'react';
import type { QueryParams } from '../types';

/**
 * Sort direction
 */
export type SortOrder = 'asc' | 'desc';

/**
 * Pagination state
 */
export interface PaginationState {
  page: number;
  pageSize: number;
  sort: string;
  order: SortOrder;
}

/**
 * Filter state
 */
export type FilterState = Record<string, string | Record<string, string>>;

/**
 * Search state
 */
export interface SearchState {
  search: string;
}

/**
 * Combined query state
 */
export interface QueryState extends PaginationState, SearchState {
  filter: FilterState;
}

/**
 * Pagination controls
 */
export interface PaginationControls {
  // State
  state: QueryState;
  params: QueryParams;

  // Pagination
  setPage: (page: number) => void;
  setPageSize: (pageSize: number) => void;
  nextPage: () => void;
  prevPage: () => void;
  goToPage: (page: number) => void;

  // Sorting
  setSort: (field: string, order?: SortOrder) => void;
  toggleSort: (field: string) => void;

  // Search
  setSearch: (query: string) => void;
  clearSearch: () => void;

  // Filters
  setFilter: (field: string, value: string | Record<string, string>) => void;
  removeFilter: (field: string) => void;
  clearFilters: () => void;

  // Reset
  reset: () => void;
}

/**
 * Default pagination options
 */
export interface UsePaginationOptions {
  defaultPage?: number;
  defaultPageSize?: number;
  defaultSort?: string;
  defaultOrder?: SortOrder;
  pageSizeOptions?: number[];
}

const DEFAULT_OPTIONS: Required<UsePaginationOptions> = {
  defaultPage: 1,
  defaultPageSize: 20,
  defaultSort: 'createdAt',
  defaultOrder: 'desc',
  pageSizeOptions: [10, 20, 50, 100],
};

/**
 * Build sort string with -prefix for descending.
 * Matches backend QueryParameters.Sort format: "-createdAt,name"
 */
function buildSortString(field: string, order: SortOrder): string {
  return order === 'desc' ? `-${field}` : field;
}

/**
 * Hook for managing pagination, sorting, search, and filter state.
 * Emits QueryParams matching the backend QueryParameters contract.
 */
export function usePagination(
  options?: UsePaginationOptions
): PaginationControls {
  const opts = { ...DEFAULT_OPTIONS, ...options };

  const [state, setState] = useState<QueryState>({
    page: opts.defaultPage,
    pageSize: opts.defaultPageSize,
    sort: opts.defaultSort,
    order: opts.defaultOrder,
    search: '',
    filter: {},
  });

  // Convert state to query params matching backend contract
  const params = useMemo<QueryParams>(() => {
    const p: QueryParams = {
      page: state.page,
      pageSize: state.pageSize,
      sort: buildSortString(state.sort, state.order),
    };

    if (state.search) {
      p.search = state.search;
    }

    if (Object.keys(state.filter).length > 0) {
      p.filter = state.filter;
    }

    return p;
  }, [state]);

  // Pagination
  const setPage = useCallback((page: number) => {
    setState(s => ({ ...s, page: Math.max(1, page) }));
  }, []);

  const setPageSize = useCallback((pageSize: number) => {
    setState(s => ({ ...s, pageSize, page: 1 }));
  }, []);

  const nextPage = useCallback(() => {
    setState(s => ({ ...s, page: s.page + 1 }));
  }, []);

  const prevPage = useCallback(() => {
    setState(s => ({ ...s, page: Math.max(1, s.page - 1) }));
  }, []);

  const goToPage = useCallback((page: number) => {
    setState(s => ({ ...s, page: Math.max(1, page) }));
  }, []);

  // Sorting
  const setSort = useCallback((field: string, order?: SortOrder) => {
    setState(s => ({
      ...s,
      sort: field,
      order: order ?? s.order,
      page: 1,
    }));
  }, []);

  const toggleSort = useCallback((field: string) => {
    setState(s => {
      if (s.sort === field) {
        return { ...s, order: s.order === 'asc' ? 'desc' : 'asc', page: 1 };
      }
      return { ...s, sort: field, order: 'asc', page: 1 };
    });
  }, []);

  // Search
  const setSearch = useCallback((query: string) => {
    setState(s => ({ ...s, search: query, page: 1 }));
  }, []);

  const clearSearch = useCallback(() => {
    setState(s => ({ ...s, search: '', page: 1 }));
  }, []);

  // Filters
  const setFilter = useCallback(
    (field: string, value: string | Record<string, string>) => {
      setState(s => ({
        ...s,
        filter: { ...s.filter, [field]: value },
        page: 1,
      }));
    },
    []
  );

  const removeFilter = useCallback((field: string) => {
    setState(s => {
      const { [field]: _removed, ...rest } = s.filter;
      void _removed;
      return { ...s, filter: rest, page: 1 };
    });
  }, []);

  const clearFilters = useCallback(() => {
    setState(s => ({ ...s, filter: {}, page: 1 }));
  }, []);

  // Reset
  const reset = useCallback(() => {
    setState({
      page: opts.defaultPage,
      pageSize: opts.defaultPageSize,
      sort: opts.defaultSort,
      order: opts.defaultOrder,
      search: '',
      filter: {},
    });
  }, [opts]);

  return {
    state,
    params,
    setPage,
    setPageSize,
    nextPage,
    prevPage,
    goToPage,
    setSort,
    toggleSort,
    setSearch,
    clearSearch,
    setFilter,
    removeFilter,
    clearFilters,
    reset,
  };
}

/**
 * Hook for debounced search
 */
export function useDebouncedSearch(delay = 300) {
  const [value, setValue] = useState('');
  const [debouncedValue, setDebouncedValue] = useState('');

  const setSearch = useCallback((newValue: string) => {
    setValue(newValue);
  }, []);

  // Debounce effect
  useState(() => {
    const timer = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => clearTimeout(timer);
  });

  return {
    value,
    debouncedValue,
    setSearch,
  };
}

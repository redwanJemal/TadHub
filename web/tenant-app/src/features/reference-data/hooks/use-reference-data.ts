import { useQuery } from '@tanstack/react-query';
import { jobCategoriesApi, countriesApi } from '../api/reference-data-api';
import type { JobCategoryFilterParams, CountryFilterParams } from '../types';

// =============================================================================
// Query Keys
// =============================================================================

export const jobCategoryKeys = {
  all: ['jobCategories'] as const,
  lists: () => [...jobCategoryKeys.all, 'list'] as const,
  list: (params: JobCategoryFilterParams) => [...jobCategoryKeys.lists(), params] as const,
  allActive: () => [...jobCategoryKeys.all, 'all'] as const,
  refs: () => [...jobCategoryKeys.all, 'refs'] as const,
  details: () => [...jobCategoryKeys.all, 'detail'] as const,
  detail: (id: string) => [...jobCategoryKeys.details(), id] as const,
  byCode: (code: string) => [...jobCategoryKeys.all, 'byCode', code] as const,
};

export const countryKeys = {
  all: ['countries'] as const,
  lists: () => [...countryKeys.all, 'list'] as const,
  list: (params: CountryFilterParams) => [...countryKeys.lists(), params] as const,
  refs: () => [...countryKeys.all, 'refs'] as const,
  commonNationalities: () => [...countryKeys.all, 'commonNationalities'] as const,
  details: () => [...countryKeys.all, 'detail'] as const,
  detail: (id: string) => [...countryKeys.details(), id] as const,
  byCode: (code: string) => [...countryKeys.all, 'byCode', code] as const,
};

// Cache for 24 hours - reference data rarely changes
const STALE_TIME = 24 * 60 * 60 * 1000;

// =============================================================================
// Job Category Hooks
// =============================================================================

/**
 * Fetch paginated job categories
 */
export function useJobCategories(params: JobCategoryFilterParams = {}) {
  return useQuery({
    queryKey: jobCategoryKeys.list(params),
    queryFn: () => jobCategoriesApi.list(params),
    staleTime: STALE_TIME,
  });
}

/**
 * Fetch all active job categories (no pagination)
 */
export function useAllJobCategories() {
  return useQuery({
    queryKey: jobCategoryKeys.allActive(),
    queryFn: () => jobCategoriesApi.getAll(),
    staleTime: STALE_TIME,
  });
}

/**
 * Fetch lightweight job category refs for dropdowns
 */
export function useJobCategoryRefs() {
  return useQuery({
    queryKey: jobCategoryKeys.refs(),
    queryFn: () => jobCategoriesApi.getRefs(),
    staleTime: STALE_TIME,
  });
}

/**
 * Fetch single job category by ID
 */
export function useJobCategory(id: string) {
  return useQuery({
    queryKey: jobCategoryKeys.detail(id),
    queryFn: () => jobCategoriesApi.getById(id),
    enabled: !!id,
    staleTime: STALE_TIME,
  });
}

/**
 * Fetch job category by MoHRE code
 */
export function useJobCategoryByCode(code: string) {
  return useQuery({
    queryKey: jobCategoryKeys.byCode(code),
    queryFn: () => jobCategoriesApi.getByCode(code),
    enabled: !!code,
    staleTime: STALE_TIME,
  });
}

// =============================================================================
// Country Hooks
// =============================================================================

/**
 * Fetch paginated countries
 */
export function useCountries(params: CountryFilterParams = {}) {
  return useQuery({
    queryKey: countryKeys.list(params),
    queryFn: () => countriesApi.list(params),
    staleTime: STALE_TIME,
  });
}

/**
 * Fetch lightweight country refs for dropdowns
 */
export function useCountryRefs() {
  return useQuery({
    queryKey: countryKeys.refs(),
    queryFn: () => countriesApi.getRefs(),
    staleTime: STALE_TIME,
  });
}

/**
 * Fetch common Tadbeer nationalities (top 10)
 */
export function useCommonNationalities() {
  return useQuery({
    queryKey: countryKeys.commonNationalities(),
    queryFn: () => countriesApi.getCommonNationalities(),
    staleTime: STALE_TIME,
  });
}

/**
 * Fetch single country by ID
 */
export function useCountry(id: string) {
  return useQuery({
    queryKey: countryKeys.detail(id),
    queryFn: () => countriesApi.getById(id),
    enabled: !!id,
    staleTime: STALE_TIME,
  });
}

/**
 * Fetch country by ISO code
 */
export function useCountryByCode(code: string) {
  return useQuery({
    queryKey: countryKeys.byCode(code),
    queryFn: () => countriesApi.getByCode(code),
    enabled: !!code,
    staleTime: STALE_TIME,
  });
}

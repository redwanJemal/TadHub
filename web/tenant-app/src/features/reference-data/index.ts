/**
 * Reference Data Feature
 * 
 * Provides reusable components and hooks for global lookup data:
 * - Job Categories (19 MoHRE domestic worker categories)
 * - Countries (ISO 3166-1 with Arabic translations)
 * - Nationalities (derived from countries)
 * 
 * @example
 * // Components
 * import { JobCategorySelect, CountrySelect, NationalitySelect } from '@/features/reference-data';
 * 
 * // Hooks
 * import { useJobCategoryRefs, useCountryRefs, useCommonNationalities } from '@/features/reference-data';
 * 
 * // Types
 * import type { JobCategoryDto, CountryDto, CountryRefDto } from '@/features/reference-data';
 */

// Components
export {
  JobCategorySelect,
  CountrySelect,
  NationalitySelect,
} from './components';
export type {
  JobCategorySelectProps,
  CountrySelectProps,
  NationalitySelectProps,
} from './components';

// Hooks
export {
  jobCategoryKeys,
  countryKeys,
  useJobCategories,
  useAllJobCategories,
  useJobCategoryRefs,
  useJobCategory,
  useJobCategoryByCode,
  useCountries,
  useCountryRefs,
  useCommonNationalities,
  useCountry,
  useCountryByCode,
} from './hooks';

// API
export { jobCategoriesApi, countriesApi } from './api';

// Types
export type {
  JobCategoryDto,
  JobCategoryRefDto,
  CountryDto,
  CountryRefDto,
  PagedList,
  JobCategoryFilterParams,
  CountryFilterParams,
} from './types';
export { getFlagEmoji } from './types';

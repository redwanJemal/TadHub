import { apiClient } from '@/shared/api/client';
import type {
  JobCategoryDto,
  JobCategoryRefDto,
  CountryDto,
  CountryRefDto,
  PagedList,
  JobCategoryFilterParams,
  CountryFilterParams,
} from '../types';

// =============================================================================
// Job Categories API
// =============================================================================

export const jobCategoriesApi = {
  /**
   * List job categories with filtering and pagination
   */
  async list(params: JobCategoryFilterParams = {}): Promise<PagedList<JobCategoryDto>> {
    const query: Record<string, unknown> = {};
    
    if (params.page) query['page'] = params.page;
    if (params.pageSize) query['pageSize'] = params.pageSize;
    if (params.sort) query['sort'] = params.sort;
    
    const filter: Record<string, string> = {};
    if (params.moHRECode) filter['moHRECode'] = params.moHRECode;
    if (params.isActive !== undefined) filter['isActive'] = String(params.isActive);
    
    if (Object.keys(filter).length > 0) {
      query['filter'] = filter;
    }

    return apiClient.get<PagedList<JobCategoryDto>>('/job-categories', query);
  },

  /**
   * Get all active job categories (no pagination)
   */
  async getAll(): Promise<JobCategoryDto[]> {
    return apiClient.get<JobCategoryDto[]>('/job-categories/all');
  },

  /**
   * Get lightweight refs for dropdowns
   */
  async getRefs(): Promise<JobCategoryRefDto[]> {
    return apiClient.get<JobCategoryRefDto[]>('/job-categories/refs');
  },

  /**
   * Get job category by ID
   */
  async getById(id: string): Promise<JobCategoryDto> {
    return apiClient.get<JobCategoryDto>(`/job-categories/${id}`);
  },

  /**
   * Get job category by MoHRE code
   */
  async getByCode(code: string): Promise<JobCategoryDto> {
    return apiClient.get<JobCategoryDto>(`/job-categories/by-code/${code}`);
  },
};

// =============================================================================
// Countries API
// =============================================================================

export const countriesApi = {
  /**
   * List countries with filtering and pagination
   */
  async list(params: CountryFilterParams = {}): Promise<PagedList<CountryDto>> {
    const query: Record<string, unknown> = {};
    
    if (params.page) query['page'] = params.page;
    if (params.pageSize) query['pageSize'] = params.pageSize;
    if (params.sort) query['sort'] = params.sort;
    
    const filter: Record<string, string> = {};
    if (params.code) filter['code'] = params.code;
    if (params.isActive !== undefined) filter['isActive'] = String(params.isActive);
    if (params.isCommonNationality !== undefined) {
      filter['isCommonNationality'] = String(params.isCommonNationality);
    }
    
    if (Object.keys(filter).length > 0) {
      query['filter'] = filter;
    }

    return apiClient.get<PagedList<CountryDto>>('/countries', query);
  },

  /**
   * Get lightweight refs for dropdowns
   */
  async getRefs(): Promise<CountryRefDto[]> {
    return apiClient.get<CountryRefDto[]>('/countries/refs');
  },

  /**
   * Get common Tadbeer nationalities (top 10)
   */
  async getCommonNationalities(): Promise<CountryRefDto[]> {
    return apiClient.get<CountryRefDto[]>('/countries/common-nationalities');
  },

  /**
   * Get country by ID
   */
  async getById(id: string): Promise<CountryDto> {
    return apiClient.get<CountryDto>(`/countries/${id}`);
  },

  /**
   * Get country by ISO alpha-2 code
   */
  async getByCode(code: string): Promise<CountryDto> {
    return apiClient.get<CountryDto>(`/countries/by-code/${code}`);
  },
};

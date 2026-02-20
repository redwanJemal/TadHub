import { apiClient } from '@/shared/api/client';
import type { QueryParams } from '@/shared/api/types';
import type {
  WorkerDto,
  CreateWorkerRequest,
  UpdateWorkerRequest,
  WorkerStateTransitionRequest,
  WorkerStateHistoryDto,
  WorkerFilterParams,
  PagedList,
  JobCategoryRefDto,
} from '../types';

const BASE_PATH = '/workers';

/**
 * Build query params for worker list
 */
function buildWorkerQueryParams(params: WorkerFilterParams): QueryParams {
  const query: QueryParams = {};

  if (params.page) query['page'] = params.page;
  if (params.pageSize) query['perPage'] = params.pageSize;
  if (params.sort) query['sort'] = params.sort;
  if (params.search) query['q'] = params.search;
  if (params.include?.length) query['include'] = params.include.join(',');

  // Build filter object
  const filter: Record<string, string | Record<string, string>> = {};
  
  if (params.status?.length) {
    filter['status'] = params.status.join(',');
  }
  if (params.nationality?.length) {
    filter['nationality'] = params.nationality.join(',');
  }
  if (params.jobCategoryId) {
    filter['jobCategoryId'] = params.jobCategoryId;
  }
  if (params.passportLocation) {
    filter['passportLocation'] = params.passportLocation;
  }
  if (params.isAvailableForFlexible !== undefined) {
    filter['isAvailableForFlexible'] = params.isAvailableForFlexible.toString();
  }
  if (params.createdAtGte) {
    filter['createdAt'] = { gte: params.createdAtGte };
  }
  if (params.createdAtLt) {
    if (filter['createdAt'] && typeof filter['createdAt'] === 'object') {
      filter['createdAt']['lt'] = params.createdAtLt;
    } else {
      filter['createdAt'] = { lt: params.createdAtLt };
    }
  }

  if (Object.keys(filter).length > 0) {
    query['filter'] = filter;
  }

  return query;
}

/**
 * Workers API
 */
export const workersApi = {
  /**
   * List workers with filtering, sorting, and pagination
   */
  async list(params: WorkerFilterParams = {}): Promise<PagedList<WorkerDto>> {
    const query = buildWorkerQueryParams(params);
    return apiClient.get<PagedList<WorkerDto>>(BASE_PATH, query);
  },

  /**
   * Get worker by ID
   */
  async getById(
    id: string,
    include?: ('skills' | 'languages' | 'media' | 'jobCategory')[]
  ): Promise<WorkerDto> {
    const query: Record<string, string> = {};
    if (include?.length) {
      query['include'] = include.join(',');
    }
    return apiClient.get<WorkerDto>(`${BASE_PATH}/${id}`, query);
  },

  /**
   * Create new worker
   */
  async create(data: CreateWorkerRequest): Promise<WorkerDto> {
    return apiClient.post<WorkerDto>(BASE_PATH, data);
  },

  /**
   * Update worker
   */
  async update(id: string, data: UpdateWorkerRequest): Promise<WorkerDto> {
    return apiClient.patch<WorkerDto>(`${BASE_PATH}/${id}`, data);
  },

  /**
   * Delete worker (soft delete)
   */
  async delete(id: string): Promise<void> {
    return apiClient.delete<void>(`${BASE_PATH}/${id}`);
  },

  /**
   * Transition worker state
   */
  async transition(id: string, data: WorkerStateTransitionRequest): Promise<WorkerDto> {
    return apiClient.post<WorkerDto>(`${BASE_PATH}/${id}/transition`, data);
  },

  /**
   * Get valid transitions for worker
   */
  async getValidTransitions(id: string): Promise<string[]> {
    return apiClient.get<string[]>(`${BASE_PATH}/${id}/valid-transitions`);
  },

  /**
   * Get worker state history
   */
  async getHistory(
    id: string,
    page = 1,
    pageSize = 20
  ): Promise<PagedList<WorkerStateHistoryDto>> {
    return apiClient.get<PagedList<WorkerStateHistoryDto>>(
      `${BASE_PATH}/${id}/history`,
      { page, perPage: pageSize }
    );
  },

  /**
   * Add skill to worker
   */
  async addSkill(
    workerId: string,
    skill: { skillName: string; rating: number }
  ): Promise<WorkerDto> {
    return apiClient.post<WorkerDto>(`${BASE_PATH}/${workerId}/skills`, skill);
  },

  /**
   * Remove skill from worker
   */
  async removeSkill(workerId: string, skillId: string): Promise<void> {
    return apiClient.delete<void>(`${BASE_PATH}/${workerId}/skills/${skillId}`);
  },

  /**
   * Add language to worker
   */
  async addLanguage(
    workerId: string,
    language: { language: string; proficiency: string }
  ): Promise<WorkerDto> {
    return apiClient.post<WorkerDto>(`${BASE_PATH}/${workerId}/languages`, language);
  },

  /**
   * Remove language from worker
   */
  async removeLanguage(workerId: string, languageId: string): Promise<void> {
    return apiClient.delete<void>(`${BASE_PATH}/${workerId}/languages/${languageId}`);
  },

  /**
   * Upload worker media
   * TODO: Implement multipart upload support in apiClient
   */
  async uploadMedia(
    _workerId: string,
    _file: File,
    _mediaType: string,
    _isPrimary = false
  ): Promise<WorkerDto> {
    // TODO: Implement multipart upload
    throw new Error('Not implemented - requires multipart form data support');
  },
};

/**
 * Job Categories API
 */
export const jobCategoriesApi = {
  /**
   * List all job categories
   */
  async list(): Promise<JobCategoryRefDto[]> {
    return apiClient.get<JobCategoryRefDto[]>('/job-categories');
  },

  /**
   * Get job category by ID
   */
  async getById(id: string): Promise<JobCategoryRefDto> {
    return apiClient.get<JobCategoryRefDto>(`/job-categories/${id}`);
  },
};

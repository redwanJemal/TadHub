// Components
export { WorkersList } from './components/WorkersList';
export { WorkerDetail } from './components/WorkerDetail';
export { WorkerForm } from './components/WorkerForm';

// Hooks
export {
  useWorkers,
  useWorker,
  useWorkerHistory,
  useValidTransitions,
  useJobCategories,
  useCreateWorker,
  useUpdateWorker,
  useDeleteWorker,
  useTransitionWorker,
  useAddWorkerSkill,
  useRemoveWorkerSkill,
  useAddWorkerLanguage,
  useRemoveWorkerLanguage,
  workerKeys,
  jobCategoryKeys,
} from './hooks/use-workers';

// API
export { workersApi, jobCategoriesApi } from './api/workers-api';

// Types
export type {
  WorkerDto,
  WorkerRefDto,
  WorkerStatus,
  WorkerFilterParams,
  CreateWorkerRequest,
  UpdateWorkerRequest,
  WorkerStateTransitionRequest,
  WorkerStateHistoryDto,
  WorkerSkillDto,
  WorkerLanguageDto,
  WorkerMediaDto,
  JobCategoryRefDto,
  Gender,
  LanguageProficiency,
  PassportLocation,
  PagedList,
} from './types';

export {
  STATUS_COLORS,
  RELIGIONS,
  MARITAL_STATUSES,
  EDUCATION_LEVELS,
} from './types';

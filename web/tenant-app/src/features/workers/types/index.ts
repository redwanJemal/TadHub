// Worker Status enum matching backend state machine
export type WorkerStatus =
  | 'Draft'
  | 'InTraining'
  | 'ReadyForMarket'
  | 'Reserved'
  | 'Hired'
  | 'OnLeave'
  | 'Terminated'
  | 'MedicallyUnfit'
  | 'Absconded'
  | 'Deported';

export type Gender = 'Male' | 'Female';
export type LanguageProficiency = 'Poor' | 'Fair' | 'Fluent';
export type PassportLocation = 'WithAgency' | 'WithWorker' | 'WithClient' | 'WithPRO' | 'WithGovernment';

// Job Category Reference
export interface JobCategoryRefDto {
  id: string;
  name: string;
  moHRECode: string;
}

// Worker Skill
export interface WorkerSkillDto {
  id: string;
  skillName: string;
  rating: number; // 0-100
}

export interface CreateWorkerSkillRequest {
  skillName: string;
  rating: number;
}

// Worker Language
export interface WorkerLanguageDto {
  id: string;
  language: string;
  proficiency: LanguageProficiency;
}

export interface CreateWorkerLanguageRequest {
  language: string;
  proficiency: LanguageProficiency;
}

// Worker Media
export interface WorkerMediaDto {
  id: string;
  mediaType: string;
  fileUrl: string;
  isPrimary: boolean;
  uploadedAt: string;
}

// Worker Reference (minimal)
export interface WorkerRefDto {
  id: string;
  fullNameEn: string;
  cvSerial: string;
  nationality: string;
  status: WorkerStatus;
  photoUrl?: string;
}

// Full Worker DTO
export interface WorkerDto {
  id: string;
  cvSerial: string;
  passportNumber: string;
  emiratesId?: string;
  fullNameEn: string;
  fullNameAr: string;
  nationality: string;
  dateOfBirth: string;
  age: number;
  gender: Gender;
  religion: string;
  maritalStatus: string;
  numberOfChildren?: number;
  education: string;
  currentStatus: WorkerStatus;
  passportLocation: PassportLocation;
  isAvailableForFlexible: boolean;
  jobCategory?: JobCategoryRefDto;
  monthlyBaseSalary: number;
  yearsOfExperience?: number;
  photoUrl?: string;
  videoUrl?: string;
  skills?: WorkerSkillDto[];
  languages?: WorkerLanguageDto[];
  media?: WorkerMediaDto[];
  notes?: string;
  sharedFromTenantId?: string;
  createdAt: string;
  updatedAt: string;
}

// Create Worker Request
export interface CreateWorkerRequest {
  passportNumber: string;
  emiratesId?: string;
  cvSerial?: string;
  fullNameEn: string;
  fullNameAr: string;
  nationality: string;
  dateOfBirth: string;
  gender: Gender;
  religion: string;
  maritalStatus: string;
  numberOfChildren?: number;
  education: string;
  yearsOfExperience?: number;
  jobCategoryId: string;
  monthlyBaseSalary: number;
  isAvailableForFlexible: boolean;
  photoUrl?: string;
  videoUrl?: string;
  skills?: CreateWorkerSkillRequest[];
  languages?: CreateWorkerLanguageRequest[];
  notes?: string;
}

// Update Worker Request
export interface UpdateWorkerRequest {
  fullNameEn?: string;
  fullNameAr?: string;
  emiratesId?: string;
  religion?: string;
  maritalStatus?: string;
  numberOfChildren?: number;
  education?: string;
  yearsOfExperience?: number;
  jobCategoryId?: string;
  monthlyBaseSalary?: number;
  isAvailableForFlexible?: boolean;
  photoUrl?: string;
  videoUrl?: string;
  notes?: string;
}

// State Transition Request
export interface WorkerStateTransitionRequest {
  targetState: WorkerStatus;
  reason?: string;
  relatedEntityId?: string;
}

// State History
export interface WorkerStateHistoryDto {
  id: string;
  fromStatus: WorkerStatus;
  toStatus: WorkerStatus;
  reason?: string;
  triggeredByUserId?: string;
  relatedEntityId?: string;
  occurredAt: string;
}

// Filter parameters
export interface WorkerFilterParams {
  status?: WorkerStatus[];
  nationality?: string[];
  jobCategoryId?: string;
  passportLocation?: PassportLocation;
  isAvailableForFlexible?: boolean;
  createdAtGte?: string;
  createdAtLt?: string;
  search?: string;
  page?: number;
  pageSize?: number;
  sort?: string;
  include?: ('skills' | 'languages' | 'media' | 'jobCategory')[];
}

// Paginated response
export interface PagedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// Common nationalities for dropdowns
export const COMMON_NATIONALITIES = [
  'Philippines',
  'Indonesia',
  'India',
  'Sri Lanka',
  'Ethiopia',
  'Nepal',
  'Bangladesh',
  'Uganda',
  'Kenya',
  'Myanmar',
] as const;

// Common religions
export const RELIGIONS = [
  'Islam',
  'Christianity',
  'Hinduism',
  'Buddhism',
  'Other',
] as const;

// Marital statuses
export const MARITAL_STATUSES = [
  'Single',
  'Married',
  'Divorced',
  'Widowed',
] as const;

// Education levels
export const EDUCATION_LEVELS = [
  'Primary',
  'Secondary',
  'HighSchool',
  'Diploma',
  'Bachelor',
  'Master',
  'PhD',
] as const;

// Status colors for UI
export const STATUS_COLORS: Record<WorkerStatus, string> = {
  Draft: 'bg-gray-100 text-gray-800',
  InTraining: 'bg-blue-100 text-blue-800',
  ReadyForMarket: 'bg-green-100 text-green-800',
  Reserved: 'bg-yellow-100 text-yellow-800',
  Hired: 'bg-purple-100 text-purple-800',
  OnLeave: 'bg-orange-100 text-orange-800',
  Terminated: 'bg-red-100 text-red-800',
  MedicallyUnfit: 'bg-pink-100 text-pink-800',
  Absconded: 'bg-red-200 text-red-900',
  Deported: 'bg-red-300 text-red-900',
};

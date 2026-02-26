export type CandidateStatus =
  | 'Received'
  | 'UnderReview'
  | 'Approved'
  | 'Rejected'
  | 'Cancelled';

export type CandidateSourceType = 'Supplier' | 'Local';

export type SkillProficiency = 'Basic' | 'Intermediate' | 'Advanced' | 'Expert';
export type LanguageProficiency = 'Basic' | 'Conversational' | 'Fluent' | 'Native';

export interface CandidateSkillDto {
  id: string;
  skillName: string;
  proficiencyLevel: SkillProficiency;
}

export interface CandidateSkillRequest {
  skillName: string;
  proficiencyLevel: SkillProficiency;
}

export interface CandidateLanguageDto {
  id: string;
  language: string;
  proficiencyLevel: LanguageProficiency;
}

export interface CandidateLanguageRequest {
  language: string;
  proficiencyLevel: LanguageProficiency;
}

export interface TenantFileDto {
  id: string;
  originalFileName: string;
  storageKey: string;
  url: string;
  fileType: string;
  fileSizeBytes: number;
}

export interface CandidateListDto {
  id: string;
  tenantId: string;
  fullNameEn: string;
  fullNameAr?: string;
  nationality?: string;
  sourceType: CandidateSourceType;
  status: CandidateStatus;
  tenantSupplierId?: string;
  supplier?: { id: string; name: string };
  jobCategoryId?: string;
  jobCategoryName?: string;
  photoUrl?: string;
  createdAt: string;
}

export interface CandidateDto {
  id: string;
  tenantId: string;
  fullNameEn: string;
  fullNameAr?: string;
  nationality?: string;
  dateOfBirth?: string;
  gender?: string;
  passportNumber?: string;
  passportExpiry?: string;
  phone?: string;
  email?: string;
  sourceType: CandidateSourceType;
  tenantSupplierId?: string;
  supplier?: { id: string; name: string };
  status: CandidateStatus;
  statusChangedAt?: string;
  statusReason?: string;
  // Professional Profile
  religion?: string;
  maritalStatus?: string;
  educationLevel?: string;
  jobCategoryId?: string;
  jobCategoryName?: string;
  experienceYears?: number;
  // Media
  photoUrl?: string;
  videoUrl?: string;
  passportDocumentUrl?: string;
  // Financial
  monthlySalary?: number;
  notes?: string;
  externalReference?: string;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
  // Collections
  skills: CandidateSkillDto[];
  languages: CandidateLanguageDto[];
  statusHistory?: CandidateStatusHistoryDto[];
}

export interface CandidateStatusHistoryDto {
  id: string;
  candidateId: string;
  fromStatus?: CandidateStatus;
  toStatus: CandidateStatus;
  changedAt: string;
  changedBy?: string;
  reason?: string;
  notes?: string;
}

export interface CreateCandidateRequest {
  fullNameEn: string;
  fullNameAr?: string;
  nationality?: string;
  dateOfBirth?: string;
  gender?: string;
  passportNumber?: string;
  passportExpiry?: string;
  phone?: string;
  email?: string;
  sourceType: string;
  tenantSupplierId?: string;
  // Professional Profile
  religion?: string;
  maritalStatus?: string;
  educationLevel?: string;
  jobCategoryId?: string;
  experienceYears?: number;
  monthlySalary?: number;
  // Skills & Languages
  skills?: CandidateSkillRequest[];
  languages?: CandidateLanguageRequest[];
  // Deferred file uploads (TenantFile IDs)
  photoFileId?: string;
  passportFileId?: string;
  notes?: string;
  externalReference?: string;
}

export interface UpdateCandidateRequest {
  fullNameEn?: string;
  fullNameAr?: string;
  nationality?: string;
  dateOfBirth?: string;
  gender?: string;
  passportNumber?: string;
  passportExpiry?: string;
  phone?: string;
  email?: string;
  // Professional Profile
  religion?: string;
  maritalStatus?: string;
  educationLevel?: string;
  jobCategoryId?: string;
  experienceYears?: number;
  monthlySalary?: number;
  photoUrl?: string;
  videoUrl?: string;
  passportDocumentUrl?: string;
  // Skills & Languages
  skills?: CandidateSkillRequest[];
  languages?: CandidateLanguageRequest[];
  notes?: string;
  externalReference?: string;
}

export interface TransitionStatusRequest {
  status: string;
  reason?: string;
  notes?: string;
}

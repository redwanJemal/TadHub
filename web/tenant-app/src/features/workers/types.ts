export type WorkerStatus =
  | 'Available'
  | 'InTraining'
  | 'UnderMedicalTest'
  | 'NewArrival'
  | 'Booked'
  | 'Hired'
  | 'OnProbation'
  | 'Active'
  | 'Renewed'
  | 'PendingReplacement'
  | 'Transferred'
  | 'MedicallyUnfit'
  | 'Absconded'
  | 'Terminated'
  | 'Pregnant'
  | 'Repatriated'
  | 'Deported'
  | 'Deceased';

export type WorkerLocation = 'Abroad' | 'InCountry';

export type WorkerStatusCategory = 'Pool' | 'Arrival' | 'Placement' | 'NegativeSpecial' | 'Terminal';

export type WorkerSourceType = 'Supplier' | 'Local';

export interface WorkerSkillDto {
  id: string;
  skillName: string;
  proficiencyLevel: string;
}

export interface WorkerLanguageDto {
  id: string;
  language: string;
  proficiencyLevel: string;
}

export interface WorkerListDto {
  id: string;
  workerCode: string;
  fullNameEn: string;
  fullNameAr?: string;
  nationality: string;
  gender?: string;
  sourceType: WorkerSourceType;
  tenantSupplierId?: string;
  supplier?: { id: string; name: string };
  jobCategoryId?: string;
  jobCategory?: { id: string; name: string };
  status: WorkerStatus;
  location: WorkerLocation;
  photoUrl?: string;
  activatedAt?: string;
  createdAt: string;
}

export interface WorkerDto {
  id: string;
  tenantId: string;
  candidateId: string;
  workerCode: string;
  fullNameEn: string;
  fullNameAr?: string;
  nationality: string;
  dateOfBirth?: string;
  gender?: string;
  passportNumber?: string;
  passportExpiry?: string;
  phone?: string;
  email?: string;
  // Professional
  religion?: string;
  maritalStatus?: string;
  educationLevel?: string;
  jobCategoryId?: string;
  jobCategory?: { id: string; name: string };
  experienceYears?: number;
  monthlySalary?: number;
  // Media
  photoUrl?: string;
  videoUrl?: string;
  passportDocumentUrl?: string;
  // Source
  sourceType: WorkerSourceType;
  tenantSupplierId?: string;
  supplier?: { id: string; name: string };
  // Status
  status: WorkerStatus;
  location: WorkerLocation;
  statusChangedAt?: string;
  statusReason?: string;
  activatedAt?: string;
  terminatedAt?: string;
  terminationReason?: string;
  procurementPaidAt?: string;
  flightDate?: string;
  arrivedAt?: string;
  notes?: string;
  // Audit
  createdBy?: string;
  updatedBy?: string;
  createdAt: string;
  updatedAt: string;
  // Collections
  skills: WorkerSkillDto[];
  languages: WorkerLanguageDto[];
  statusHistory?: WorkerStatusHistoryDto[];
}

export interface WorkerStatusHistoryDto {
  id: string;
  workerId: string;
  fromStatus?: WorkerStatus;
  toStatus: WorkerStatus;
  changedAt: string;
  changedBy?: string;
  reason?: string;
  notes?: string;
}

export interface UpdateWorkerRequest {
  fullNameEn?: string;
  fullNameAr?: string;
  phone?: string;
  email?: string;
  notes?: string;
  religion?: string;
  maritalStatus?: string;
  educationLevel?: string;
  jobCategoryId?: string;
  experienceYears?: number;
  monthlySalary?: number;
  location?: string;
  procurementPaidAt?: string;
  flightDate?: string;
  arrivedAt?: string;
  skills?: { skillName: string; proficiencyLevel: string }[];
  languages?: { language: string; proficiencyLevel: string }[];
}

export interface TransitionWorkerStatusRequest {
  status: string;
  reason?: string;
  notes?: string;
}

export interface WorkerCvDto {
  id: string;
  workerCode: string;
  location: WorkerLocation;
  fullNameEn: string;
  fullNameAr?: string;
  nationality: string;
  dateOfBirth?: string;
  gender?: string;
  passportNumber?: string;
  passportExpiry?: string;
  phone?: string;
  email?: string;
  religion?: string;
  maritalStatus?: string;
  educationLevel?: string;
  jobCategoryId?: string;
  jobCategory?: { id: string; name: string };
  experienceYears?: number;
  monthlySalary?: number;
  photoUrl?: string;
  videoUrl?: string;
  passportDocumentUrl?: string;
  skills: WorkerSkillDto[];
  languages: WorkerLanguageDto[];
}

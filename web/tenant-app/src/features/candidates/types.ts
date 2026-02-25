export type CandidateStatus =
  | 'Received'
  | 'UnderReview'
  | 'Approved'
  | 'Rejected'
  | 'ProcurementPaid'
  | 'InTransit'
  | 'Arrived'
  | 'Converted'
  | 'Cancelled'
  | 'FailedMedicalAbroad'
  | 'VisaDenied'
  | 'ReturnedAfterArrival';

export type CandidateSourceType = 'Supplier' | 'Local';

export interface CandidateListDto {
  id: string;
  tenantId: string;
  fullNameEn: string;
  fullNameAr?: string;
  nationality?: string;
  sourceType: CandidateSourceType;
  status: CandidateStatus;
  tenantSupplierId?: string;
  tenantSupplierName?: string;
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
  tenantSupplierName?: string;
  status: CandidateStatus;
  statusChangedAt?: string;
  statusReason?: string;
  medicalStatus?: string;
  visaStatus?: string;
  expectedArrivalDate?: string;
  actualArrivalDate?: string;
  notes?: string;
  externalReference?: string;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
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
  medicalStatus?: string;
  visaStatus?: string;
  expectedArrivalDate?: string;
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
  medicalStatus?: string;
  visaStatus?: string;
  expectedArrivalDate?: string;
  notes?: string;
  externalReference?: string;
}

export interface TransitionStatusRequest {
  targetStatus: string;
  reason?: string;
  notes?: string;
}

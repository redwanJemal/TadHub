export type VisaType = 'EmploymentVisa' | 'ResidenceVisa' | 'EmiratesId';

export type VisaApplicationStatus =
  | 'NotStarted'
  | 'DocumentsCollecting'
  | 'Applied'
  | 'UnderProcess'
  | 'Approved'
  | 'Rejected'
  | 'Issued'
  | 'Expired'
  | 'Cancelled';

export type VisaDocumentType =
  | 'AttestedMedicalCertificate'
  | 'PassportCopy'
  | 'Photo'
  | 'LocalMedical'
  | 'MedicalCertificate'
  | 'Other';

export interface VisaWorkerRefDto {
  id: string;
  fullNameEn: string;
  fullNameAr?: string;
  workerCode: string;
}

export interface VisaClientRefDto {
  id: string;
  nameEn: string;
  nameAr?: string;
}

export interface VisaApplicationDocumentDto {
  id: string;
  visaApplicationId: string;
  documentType: string;
  fileUrl: string;
  uploadedAt: string;
  isVerified: boolean;
}

export interface VisaApplicationStatusHistoryDto {
  id: string;
  visaApplicationId: string;
  fromStatus?: string;
  toStatus: string;
  changedAt: string;
  changedBy?: string;
  reason?: string;
  notes?: string;
}

export interface VisaApplicationDto {
  id: string;
  tenantId: string;
  applicationCode: string;
  visaType: VisaType;
  status: VisaApplicationStatus;
  statusChangedAt: string;
  statusReason?: string;
  workerId: string;
  clientId: string;
  contractId?: string;
  placementId?: string;
  worker?: VisaWorkerRefDto;
  client?: VisaClientRefDto;
  applicationDate?: string;
  approvalDate?: string;
  issuanceDate?: string;
  expiryDate?: string;
  referenceNumber?: string;
  visaNumber?: string;
  notes?: string;
  rejectionReason?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt: string;
  updatedAt: string;
  statusHistory?: VisaApplicationStatusHistoryDto[];
  documents?: VisaApplicationDocumentDto[];
}

export interface VisaApplicationListDto {
  id: string;
  applicationCode: string;
  visaType: VisaType;
  status: VisaApplicationStatus;
  statusChangedAt: string;
  workerId: string;
  clientId: string;
  placementId?: string;
  worker?: VisaWorkerRefDto;
  client?: VisaClientRefDto;
  applicationDate?: string;
  expiryDate?: string;
  referenceNumber?: string;
  documentCount: number;
  createdAt: string;
}

export interface CreateVisaApplicationRequest {
  workerId: string;
  clientId: string;
  visaType: string;
  contractId?: string;
  placementId?: string;
  applicationDate?: string;
  referenceNumber?: string;
  notes?: string;
}

export interface UpdateVisaApplicationRequest {
  applicationDate?: string;
  approvalDate?: string;
  issuanceDate?: string;
  expiryDate?: string;
  referenceNumber?: string;
  visaNumber?: string;
  notes?: string;
  contractId?: string;
  placementId?: string;
}

export interface TransitionVisaStatusRequest {
  status: string;
  reason?: string;
  notes?: string;
}

export interface UploadVisaDocumentRequest {
  documentType: string;
  fileUrl: string;
}

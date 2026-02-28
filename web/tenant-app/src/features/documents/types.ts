export type DocumentType =
  | 'Passport'
  | 'Visa'
  | 'WorkPermit'
  | 'MedicalCertificate'
  | 'InsurancePolicy'
  | 'EmiratesId'
  | 'LabourCard'
  | 'Other';

export type DocumentStatus = 'Pending' | 'Valid' | 'Expired' | 'Revoked';

export type EffectiveStatus = 'Pending' | 'Valid' | 'ExpiringSoon' | 'Expired' | 'Revoked';

export interface WorkerDocumentDto {
  id: string;
  tenantId: string;
  workerId: string;
  documentType: DocumentType;
  documentNumber?: string;
  issuedAt?: string;
  expiresAt?: string;
  status: DocumentStatus;
  effectiveStatus: EffectiveStatus;
  daysUntilExpiry?: number;
  issuingAuthority?: string;
  notes?: string;
  fileUrl?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt: string;
  updatedAt: string;
}

export interface WorkerDocumentListDto {
  id: string;
  workerId: string;
  documentType: DocumentType;
  documentNumber?: string;
  issuedAt?: string;
  expiresAt?: string;
  status: DocumentStatus;
  effectiveStatus: EffectiveStatus;
  daysUntilExpiry?: number;
  hasFile: boolean;
  createdAt: string;
  workerName?: string;
  workerCode?: string;
}

export interface CreateWorkerDocumentRequest {
  workerId: string;
  documentType: string;
  documentNumber?: string;
  issuedAt?: string;
  expiresAt?: string;
  status?: string;
  issuingAuthority?: string;
  notes?: string;
}

export interface UpdateWorkerDocumentRequest {
  documentNumber?: string;
  issuedAt?: string;
  expiresAt?: string;
  status?: string;
  issuingAuthority?: string;
  notes?: string;
}

export interface ComplianceSummaryDto {
  totalDocuments: number;
  valid: number;
  expiringSoon: number;
  expired: number;
  pending: number;
  byType: ComplianceByTypeDto[];
}

export interface ComplianceByTypeDto {
  documentType: string;
  valid: number;
  expiringSoon: number;
  expired: number;
  pending: number;
}

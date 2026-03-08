export type TrialStatus = 'Active' | 'Successful' | 'Failed' | 'Cancelled';

export type TrialOutcome = 'ProceedToContract' | 'ReturnToInventory';

export interface TrialWorkerRefDto {
  id: string;
  fullNameEn: string;
  fullNameAr?: string;
  workerCode: string;
}

export interface TrialClientRefDto {
  id: string;
  nameEn: string;
  nameAr?: string;
}

export interface TrialStatusHistoryDto {
  id: string;
  trialId: string;
  fromStatus?: string;
  toStatus: string;
  changedAt: string;
  changedBy?: string;
  reason?: string;
  notes?: string;
}

export interface TrialDto {
  id: string;
  tenantId: string;
  trialCode: string;
  status: TrialStatus;
  statusChangedAt: string;
  workerId: string;
  clientId: string;
  placementId?: string;
  contractId?: string;
  worker?: TrialWorkerRefDto;
  client?: TrialClientRefDto;
  startDate: string;
  endDate: string;
  daysRemaining: number;
  outcome?: string;
  outcomeNotes?: string;
  outcomeDate?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt: string;
  updatedAt: string;
  statusHistory?: TrialStatusHistoryDto[];
}

export interface TrialListDto {
  id: string;
  trialCode: string;
  status: TrialStatus;
  workerId: string;
  clientId: string;
  placementId?: string;
  contractId?: string;
  worker?: TrialWorkerRefDto;
  client?: TrialClientRefDto;
  startDate: string;
  endDate: string;
  daysRemaining: number;
  outcome?: string;
  createdAt: string;
}

export interface CreateTrialRequest {
  workerId: string;
  clientId: string;
  startDate: string;
  placementId?: string;
  notes?: string;
}

export interface CompleteTrialRequest {
  outcome: TrialOutcome;
  outcomeNotes?: string;
}

export interface CancelTrialRequest {
  reason?: string;
}

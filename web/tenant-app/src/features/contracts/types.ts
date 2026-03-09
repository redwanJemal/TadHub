export type ContractStatus =
  | 'Draft'
  | 'Confirmed'
  | 'OnProbation'
  | 'Active'
  | 'Completed'
  | 'Terminated'
  | 'Cancelled'
  | 'Closed';

export type ContractType = 'Traditional' | 'Temporary' | 'Flexible' | 'TrialContract' | 'TwoYearEmployment';

export type RatePeriod = 'Monthly' | 'Daily' | 'Hourly';

export type GuaranteePeriodType = 'SixMonths' | 'OneYear' | 'TwoYears';

export type TerminationReasonType =
  | 'ReturnToOffice'
  | 'ReturnToCountry'
  | 'Runaway'
  | 'MutualAgreement'
  | 'ContractExpiry'
  | 'ClientRequest'
  | 'WorkerRequest';

export interface ContractWorkerDto {
  id: string;
  fullNameEn: string;
  fullNameAr?: string;
  workerCode: string;
}

export interface ContractClientDto {
  id: string;
  nameEn: string;
  nameAr?: string;
}

export interface ContractListDto {
  id: string;
  contractCode: string;
  type: ContractType;
  status: ContractStatus;
  workerId: string;
  clientId: string;
  worker?: ContractWorkerDto;
  client?: ContractClientDto;
  startDate: string;
  endDate?: string;
  guaranteePeriod?: GuaranteePeriodType;
  rate: number;
  ratePeriod: RatePeriod;
  currency: string;
  createdAt: string;
}

export interface ContractDto {
  id: string;
  tenantId: string;
  contractCode: string;
  type: ContractType;
  status: ContractStatus;
  statusChangedAt?: string;
  statusReason?: string;
  // Parties
  workerId: string;
  clientId: string;
  worker?: ContractWorkerDto;
  client?: ContractClientDto;
  // Dates
  startDate: string;
  endDate?: string;
  probationEndDate?: string;
  guaranteeEndDate?: string;
  probationPassed: boolean;
  guaranteePeriod?: GuaranteePeriodType;
  // Financial
  rate: number;
  ratePeriod: RatePeriod;
  currency: string;
  totalValue?: number;
  // Termination
  terminatedAt?: string;
  terminationReason?: string;
  terminationReasonType?: TerminationReasonType;
  terminatedBy?: string;
  // Replacement
  replacementContractId?: string;
  originalContractId?: string;
  // Linked cases
  returneeCaseId?: string;
  runawayCaseId?: string;
  notes?: string;
  // Audit
  createdBy?: string;
  updatedBy?: string;
  createdAt: string;
  updatedAt: string;
  // Optional
  statusHistory?: ContractStatusHistoryDto[];
}

export interface ContractStatusHistoryDto {
  id: string;
  contractId: string;
  fromStatus?: ContractStatus;
  toStatus: ContractStatus;
  changedAt: string;
  changedBy?: string;
  reason?: string;
  notes?: string;
}

export interface CreateContractRequest {
  workerId: string;
  clientId: string;
  type: string;
  startDate: string;
  endDate?: string;
  probationEndDate?: string;
  guaranteeEndDate?: string;
  guaranteePeriod?: string;
  rate: number;
  ratePeriod: string;
  currency?: string;
  totalValue?: number;
  originalContractId?: string;
  notes?: string;
}

export interface UpdateContractRequest {
  startDate?: string;
  endDate?: string;
  probationEndDate?: string;
  guaranteeEndDate?: string;
  probationPassed?: boolean;
  rate?: number;
  ratePeriod?: string;
  currency?: string;
  totalValue?: number;
  notes?: string;
}

export interface TransitionContractStatusRequest {
  status: string;
  reason?: string;
  terminationReason?: string;
  notes?: string;
}

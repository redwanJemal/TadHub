export type RunawayCaseStatus = 'Reported' | 'UnderInvestigation' | 'Confirmed' | 'Settled' | 'Closed';

export type RunawayExpenseType = 'CommissionRefund' | 'VisaCost' | 'MedicalCost' | 'TransportationCost' | 'Other';

export type PaidByParty = 'Office' | 'Supplier' | 'Client';

export type GuaranteePeriodType = 'SixMonths' | 'OneYear' | 'TwoYears';

export interface RunawayWorkerRefDto {
  id: string;
  fullNameEn: string;
  fullNameAr?: string;
  workerCode: string;
}

export interface RunawayClientRefDto {
  id: string;
  nameEn: string;
  nameAr?: string;
}

export interface RunawayExpenseDto {
  id: string;
  runawayCaseId: string;
  expenseType: string;
  amount: number;
  currency: string;
  description?: string;
  paidBy: string;
}

export interface RunawayCaseStatusHistoryDto {
  id: string;
  runawayCaseId: string;
  fromStatus?: string;
  toStatus: string;
  changedAt: string;
  changedBy?: string;
  reason?: string;
  notes?: string;
}

export interface RunawayCaseDto {
  id: string;
  tenantId: string;
  caseCode: string;
  status: RunawayCaseStatus;
  statusChangedAt: string;
  workerId: string;
  contractId: string;
  clientId: string;
  supplierId?: string;
  worker?: RunawayWorkerRefDto;
  client?: RunawayClientRefDto;
  reportedDate: string;
  reportedBy: string;
  lastKnownLocation?: string;
  policeReportNumber?: string;
  policeReportDate?: string;
  isWithinGuarantee: boolean;
  guaranteePeriodType?: string;
  notes?: string;
  confirmedAt?: string;
  settledAt?: string;
  closedAt?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt: string;
  updatedAt: string;
  expenses?: RunawayExpenseDto[];
  statusHistory?: RunawayCaseStatusHistoryDto[];
}

export interface RunawayCaseListDto {
  id: string;
  caseCode: string;
  status: RunawayCaseStatus;
  workerId: string;
  contractId: string;
  clientId: string;
  supplierId?: string;
  worker?: RunawayWorkerRefDto;
  client?: RunawayClientRefDto;
  reportedDate: string;
  isWithinGuarantee: boolean;
  policeReportNumber?: string;
  createdAt: string;
}

export interface ReportRunawayCaseRequest {
  workerId: string;
  contractId: string;
  clientId: string;
  supplierId?: string;
  reportedDate: string;
  reportedBy: string;
  lastKnownLocation?: string;
  policeReportNumber?: string;
  policeReportDate?: string;
  notes?: string;
}

export interface UpdateRunawayCaseRequest {
  lastKnownLocation?: string;
  policeReportNumber?: string;
  policeReportDate?: string;
  notes?: string;
}

export interface ConfirmRunawayCaseRequest {
  notes?: string;
}

export interface SettleRunawayCaseRequest {
  notes?: string;
}

export interface CloseRunawayCaseRequest {
  notes?: string;
}

export interface CreateRunawayExpenseRequest {
  expenseType: string;
  amount: number;
  description?: string;
  paidBy: string;
}

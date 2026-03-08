export type ReturneeCaseStatus = 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected' | 'Settled';

export type ReturnType = 'ReturnToOffice' | 'ReturnToCountry';

export type ExpenseType = 'VisaCost' | 'TicketCost' | 'MedicalCost' | 'TransportationCost' | 'AccommodationCost' | 'Other';

export type PaidByParty = 'Office' | 'Supplier' | 'Client';

export type GuaranteePeriodType = 'SixMonths' | 'OneYear' | 'TwoYears';

export interface ReturneeWorkerRefDto {
  id: string;
  fullNameEn: string;
  fullNameAr?: string;
  workerCode: string;
}

export interface ReturneeClientRefDto {
  id: string;
  nameEn: string;
  nameAr?: string;
}

export interface ReturneeExpenseDto {
  id: string;
  returneeCaseId: string;
  expenseType: string;
  amount: number;
  currency: string;
  description?: string;
  paidBy: string;
}

export interface ReturneeCaseStatusHistoryDto {
  id: string;
  returneeCaseId: string;
  fromStatus?: string;
  toStatus: string;
  changedAt: string;
  changedBy?: string;
  reason?: string;
  notes?: string;
}

export interface RefundCalculationDto {
  contractId: string;
  totalAmountPaid: number;
  totalContractMonths: number;
  monthsWorked: number;
  valuePerMonth: number;
  refundAmount: number;
  currency: string;
}

export interface ReturneeCaseDto {
  id: string;
  tenantId: string;
  caseCode: string;
  returnType: string;
  status: ReturneeCaseStatus;
  statusChangedAt: string;
  workerId: string;
  contractId: string;
  clientId: string;
  supplierId?: string;
  worker?: ReturneeWorkerRefDto;
  client?: ReturneeClientRefDto;
  returnDate: string;
  returnReason: string;
  monthsWorked: number;
  isWithinGuarantee: boolean;
  guaranteePeriodType?: string;
  totalAmountPaid?: number;
  refundAmount?: number;
  currency: string;
  approvedBy?: string;
  approvedAt?: string;
  rejectedReason?: string;
  settledAt?: string;
  settlementNotes?: string;
  notes?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt: string;
  updatedAt: string;
  expenses?: ReturneeExpenseDto[];
  statusHistory?: ReturneeCaseStatusHistoryDto[];
}

export interface ReturneeCaseListDto {
  id: string;
  caseCode: string;
  returnType: string;
  status: ReturneeCaseStatus;
  workerId: string;
  contractId: string;
  clientId: string;
  worker?: ReturneeWorkerRefDto;
  client?: ReturneeClientRefDto;
  returnDate: string;
  monthsWorked: number;
  isWithinGuarantee: boolean;
  refundAmount?: number;
  createdAt: string;
}

export interface CreateReturneeCaseRequest {
  workerId: string;
  contractId: string;
  clientId: string;
  supplierId?: string;
  returnType: string;
  returnDate: string;
  returnReason: string;
  notes?: string;
}

export interface ApproveReturneeCaseRequest {
  notes?: string;
}

export interface RejectReturneeCaseRequest {
  reason: string;
  notes?: string;
}

export interface SettleReturneeCaseRequest {
  settlementNotes?: string;
}

export interface CreateReturneeExpenseRequest {
  expenseType: string;
  amount: number;
  description?: string;
  paidBy: string;
}

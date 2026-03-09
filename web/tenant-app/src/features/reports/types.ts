// ── Workforce Reports ──

export interface InventoryReportItem {
  id: string;
  workerCode: string;
  fullNameEn: string;
  fullNameAr: string;
  nationality: string | null;
  location: string;
  status: string;
  gender: string | null;
  dateOfBirth: string | null;
  experienceYears: number | null;
  monthlySalary: number | null;
  tenantSupplierId: string | null;
  supplierNameEn: string | null;
  supplierNameAr: string | null;
  createdAt: string;
}

export interface DeployedReportItem {
  workerId: string;
  workerCode: string;
  fullNameEn: string;
  fullNameAr: string;
  nationality: string | null;
  contractId: string | null;
  contractCode: string | null;
  clientId: string | null;
  clientNameEn: string | null;
  clientNameAr: string | null;
  startDate: string | null;
  endDate: string | null;
  contractType: string | null;
  rate: number | null;
  ratePeriod: string | null;
}

export interface ReturneeReportItem {
  id: string;
  caseCode: string;
  status: string;
  returnType: string | null;
  returnDate: string | null;
  returnReason: string | null;
  workerId: string | null;
  workerNameEn: string | null;
  workerNameAr: string | null;
  clientId: string | null;
  clientNameEn: string | null;
  clientNameAr: string | null;
  totalAmountPaid: number | null;
  refundAmount: number | null;
  isWithinGuarantee: boolean;
  guaranteePeriodType: string | null;
  settledAt: string | null;
  createdAt: string;
}

export interface RunawayReportItem {
  id: string;
  caseCode: string;
  status: string;
  reportedDate: string | null;
  workerId: string | null;
  workerNameEn: string | null;
  workerNameAr: string | null;
  clientId: string | null;
  clientNameEn: string | null;
  clientNameAr: string | null;
  isWithinGuarantee: boolean;
  guaranteePeriodType: string | null;
  policeReportNumber: string | null;
  totalExpenses: number;
  settledAt: string | null;
  createdAt: string;
}

// ── Operational Reports ──

export interface ArrivalReportItem {
  id: string;
  arrivalCode: string;
  status: string;
  workerId: string | null;
  workerNameEn: string | null;
  workerNameAr: string | null;
  flightNumber: string | null;
  airportName: string | null;
  scheduledArrivalDate: string | null;
  scheduledArrivalTime: string | null;
  actualArrivalTime: string | null;
  driverName: string | null;
  driverConfirmedPickupAt: string | null;
  createdAt: string;
}

export interface AccommodationDailyItem {
  id: string;
  stayCode: string;
  status: string;
  workerId: string | null;
  workerNameEn: string | null;
  workerNameAr: string | null;
  room: string | null;
  locationName: string | null;
  checkInDate: string;
  checkOutDate: string | null;
  departureReason: string | null;
}

export interface DeploymentPipelineItem {
  stage: string;
  count: number;
}

// ── Finance Reports (Extensions) ──

export interface SupplierCommissionItem {
  supplierId: string;
  supplierNameEn: string | null;
  supplierNameAr: string | null;
  paymentCount: number;
  totalPaid: number;
  totalPending: number;
}

export interface RefundReportItem {
  paymentId: string;
  paymentNumber: string;
  status: string;
  amount: number;
  refundAmount: number | null;
  method: string | null;
  paymentDate: string;
  clientId: string | null;
  clientNameEn: string | null;
  clientNameAr: string | null;
  invoiceId: string | null;
  invoiceNumber: string | null;
  createdAt: string;
}

export interface CostPerMaidItem {
  workerId: string;
  workerCode: string | null;
  workerNameEn: string | null;
  workerNameAr: string | null;
  procurementCost: number;
  flightCost: number;
  medicalCost: number;
  visaCost: number;
  insuranceCost: number;
  accommodationCost: number;
  trainingCost: number;
  otherCost: number;
  totalCost: number;
}

// ── Report Types Enum ──

export type ReportCategory = 'workforce' | 'operational' | 'finance';

export interface ReportDefinition {
  key: string;
  path: string;
  category: ReportCategory;
}

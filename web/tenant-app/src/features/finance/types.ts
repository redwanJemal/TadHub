// Invoice types
export type InvoiceStatus =
  | 'Draft'
  | 'Issued'
  | 'PartiallyPaid'
  | 'Paid'
  | 'Overdue'
  | 'Cancelled'
  | 'Refunded';

export type InvoiceType = 'Standard' | 'CreditNote' | 'ProformaDeposit';

export type MilestoneType = 'AdvanceDeposit' | 'ActivationBalance' | 'Installment' | 'FullPayment';

// Nested ref types for enriched responses
export interface InvoiceClientRef {
  id: string;
  nameEn: string;
  nameAr?: string;
}

export interface InvoiceWorkerRef {
  id: string;
  fullNameEn: string;
  fullNameAr?: string;
  workerCode: string;
}

export interface InvoiceContractRef {
  id: string;
  contractCode: string;
}

export interface InvoiceRef {
  id: string;
  invoiceNumber: string;
}

export interface InvoiceLineItemDto {
  id: string;
  lineNumber: number;
  description: string;
  descriptionAr?: string;
  quantity: number;
  unitPrice: number;
  discountAmount: number;
  lineTotal: number;
  itemCode?: string;
}

export interface InvoiceListDto {
  id: string;
  invoiceNumber: string;
  type: InvoiceType;
  status: InvoiceStatus;
  contractId: string;
  clientId: string;
  workerId?: string;
  client?: InvoiceClientRef;
  worker?: InvoiceWorkerRef;
  contract?: InvoiceContractRef;
  issueDate: string;
  dueDate: string;
  totalAmount: number;
  paidAmount: number;
  balanceDue: number;
  currency: string;
  milestoneType?: MilestoneType;
  createdAt: string;
}

export interface InvoiceDto extends InvoiceListDto {
  tenantId: string;
  statusChangedAt?: string;
  subtotal: number;
  discountAmount: number;
  taxableAmount: number;
  vatRate: number;
  vatAmount: number;
  tenantTrn?: string;
  clientTrn?: string;
  discountProgramId?: string;
  discountProgramName?: string;
  discountCardNumber?: string;
  discountPercentage?: number;
  originalInvoiceId?: string;
  creditNoteReason?: string;
  notes?: string;
  createdBy?: string;
  updatedBy?: string;
  updatedAt: string;
  lineItems?: InvoiceLineItemDto[];
  payments?: PaymentListDto[];
}

export interface InvoiceSummaryDto {
  totalInvoices: number;
  totalRevenue: number;
  totalPaid: number;
  totalOutstanding: number;
  overdueCount: number;
  overdueAmount: number;
  countsByStatus: Record<string, number>;
}

export interface CreateInvoiceLineItemRequest {
  description: string;
  descriptionAr?: string;
  quantity: number;
  unitPrice: number;
  discountAmount?: number;
  itemCode?: string;
}

export interface CreateInvoiceRequest {
  contractId: string;
  clientId: string;
  workerId?: string;
  type?: string;
  issueDate: string;
  dueDate: string;
  milestoneType?: string;
  currency?: string;
  tenantTrn?: string;
  clientTrn?: string;
  notes?: string;
  lineItems: CreateInvoiceLineItemRequest[];
}

export interface UpdateInvoiceRequest {
  issueDate?: string;
  dueDate?: string;
  tenantTrn?: string;
  clientTrn?: string;
  notes?: string;
  lineItems?: CreateInvoiceLineItemRequest[];
}

export interface TransitionInvoiceStatusRequest {
  status: string;
  reason?: string;
}

export interface GenerateInvoiceRequest {
  contractId: string;
  milestoneType?: string;
  overrideAmount?: number;
  notes?: string;
}

export interface CreateCreditNoteRequest {
  reason: string;
  amount?: number;
  notes?: string;
}

export interface ApplyDiscountRequest {
  discountProgramId: string;
  cardNumber?: string;
}

// Payment types
export type PaymentStatus = 'Pending' | 'Completed' | 'Failed' | 'Refunded' | 'Cancelled';

export type PaymentMethod = 'Cash' | 'Card' | 'BankTransfer' | 'Cheque' | 'EDirham' | 'Online';

export interface PaymentListDto {
  id: string;
  paymentNumber: string;
  status: PaymentStatus;
  invoiceId: string;
  clientId: string;
  invoice?: InvoiceRef;
  client?: InvoiceClientRef;
  amount: number;
  currency: string;
  method: PaymentMethod;
  referenceNumber?: string;
  paymentDate: string;
  cashierName?: string;
  createdAt: string;
}

export interface PaymentDto extends PaymentListDto {
  tenantId: string;
  gatewayProvider?: string;
  gatewayTransactionId?: string;
  gatewayStatus?: string;
  refundedPaymentId?: string;
  refundAmount?: number;
  cashierId?: string;
  notes?: string;
  createdBy?: string;
  updatedBy?: string;
  updatedAt: string;
}

export interface RecordPaymentRequest {
  invoiceId: string;
  clientId: string;
  amount: number;
  currency?: string;
  method: string;
  referenceNumber?: string;
  paymentDate: string;
  gatewayProvider?: string;
  cashierId?: string;
  cashierName?: string;
  notes?: string;
}

export interface TransitionPaymentStatusRequest {
  status: string;
  reason?: string;
}

export interface RefundPaymentRequest {
  amount: number;
  reason: string;
  notes?: string;
}

// Discount Program types
export type DiscountType = 'Saada' | 'Fazaa' | 'Custom';

export interface DiscountProgramListDto {
  id: string;
  name: string;
  nameAr?: string;
  type: DiscountType;
  discountPercentage: number;
  maxDiscountAmount?: number;
  isActive: boolean;
  validFrom?: string;
  validTo?: string;
  createdAt: string;
}

export interface DiscountProgramDto extends DiscountProgramListDto {
  tenantId: string;
  description?: string;
  createdBy?: string;
  updatedBy?: string;
  updatedAt: string;
}

export interface CreateDiscountProgramRequest {
  name: string;
  nameAr?: string;
  type: string;
  discountPercentage: number;
  maxDiscountAmount?: number;
  isActive?: boolean;
  validFrom?: string;
  validTo?: string;
  description?: string;
}

export interface UpdateDiscountProgramRequest {
  name?: string;
  nameAr?: string;
  discountPercentage?: number;
  maxDiscountAmount?: number;
  isActive?: boolean;
  validFrom?: string;
  validTo?: string;
  description?: string;
}

// Supplier Payment types
export type SupplierPaymentStatus = 'Pending' | 'Paid' | 'PartiallyPaid' | 'Cancelled';

export interface SupplierPaymentListDto {
  id: string;
  paymentNumber: string;
  status: SupplierPaymentStatus;
  supplierId: string;
  workerId?: string;
  contractId?: string;
  amount: number;
  currency: string;
  method: string;
  paymentDate: string;
  createdAt: string;
}

export interface SupplierPaymentDto extends SupplierPaymentListDto {
  tenantId: string;
  referenceNumber?: string;
  paidAt?: string;
  notes?: string;
  createdBy?: string;
  updatedBy?: string;
  updatedAt: string;
}

export interface CreateSupplierPaymentRequest {
  supplierId: string;
  workerId?: string;
  contractId?: string;
  amount: number;
  currency?: string;
  method: string;
  referenceNumber?: string;
  paymentDate: string;
  notes?: string;
}

export interface UpdateSupplierPaymentRequest {
  amount?: number;
  method?: string;
  referenceNumber?: string;
  paymentDate?: string;
  notes?: string;
}

export interface TransitionSupplierPaymentStatusRequest {
  status: string;
  reason?: string;
}

// Report types
export interface MarginReportDto {
  totalRevenue: number;
  totalCost: number;
  grossMargin: number;
  marginPercentage: number;
  lines: MarginLineDto[];
}

export interface MarginLineDto {
  contractId?: string;
  workerId?: string;
  clientId?: string;
  contract?: InvoiceContractRef;
  worker?: InvoiceWorkerRef;
  client?: InvoiceClientRef;
  revenue: number;
  cost: number;
  margin: number;
  marginPercentage: number;
}

export interface CashReconciliationDto {
  id: string;
  tenantId: string;
  reportDate: string;
  cashierId?: string;
  cashierName?: string;
  cashTotal: number;
  cardTotal: number;
  bankTransferTotal: number;
  chequeTotal: number;
  eDirhamTotal: number;
  onlineTotal: number;
  grandTotal: number;
  transactionCount: number;
  notes?: string;
  isClosed: boolean;
  closedAt?: string;
  createdAt: string;
}

export interface CashReconciliationListDto {
  id: string;
  reportDate: string;
  cashierName?: string;
  grandTotal: number;
  transactionCount: number;
  isClosed: boolean;
  createdAt: string;
}

export interface RevenueBreakdownDto {
  totalRevenue: number;
  byPeriod: Record<string, number>;
  byPaymentMethod: Record<string, number>;
}

// Invoice Template Settings
export interface InvoiceTemplateSettings {
  primaryColor: string;
  accentColor: string;
  showLogo: boolean;
  showArabicText: boolean;
  companyAddress?: string;
  companyAddressAr?: string;
}

// Financial Settings
export interface TenantFinancialSettings {
  vatRate: number;
  vatEnabled: boolean;
  taxRegistrationNumber?: string;
  defaultCurrency: string;
  invoicePrefix: string;
  paymentPrefix: string;
  invoiceDueDays: number;
  requireDepositOnBooking: boolean;
  depositPercentage: number;
  enableInstallments: boolean;
  maxInstallments: number;
  paymentMethods: string[];
  invoiceFooterText?: string;
  invoiceFooterTextAr?: string;
  invoiceTerms?: string;
  invoiceTermsAr?: string;
  autoGenerateInvoiceOnConfirm: boolean;
  invoiceTemplate?: InvoiceTemplateSettings;
}

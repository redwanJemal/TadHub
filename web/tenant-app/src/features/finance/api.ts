import { apiClient, API_BASE } from '@/shared/api/client';
import { getTenantId, getAccessToken } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  InvoiceDto,
  InvoiceListDto,
  InvoiceSummaryDto,
  CreateInvoiceRequest,
  UpdateInvoiceRequest,
  TransitionInvoiceStatusRequest,
  GenerateInvoiceRequest,
  CreateCreditNoteRequest,
  ApplyDiscountRequest,
  PaymentDto,
  PaymentListDto,
  RecordPaymentRequest,
  TransitionPaymentStatusRequest,
  RefundPaymentRequest,
  DiscountProgramDto,
  DiscountProgramListDto,
  CreateDiscountProgramRequest,
  UpdateDiscountProgramRequest,
  SupplierPaymentDto,
  SupplierPaymentListDto,
  CreateSupplierPaymentRequest,
  UpdateSupplierPaymentRequest,
  TransitionSupplierPaymentStatusRequest,
  MarginReportDto,
  RevenueBreakdownDto,
  CashReconciliationDto,
  CashReconciliationListDto,
  TenantFinancialSettings,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

// Invoices
export function listInvoices(params?: QueryParams) {
  return apiClient.getPaged<InvoiceListDto>(tenantPath('/invoices'), params);
}

export function getInvoice(id: string, params?: QueryParams) {
  return apiClient.get<InvoiceDto>(tenantPath(`/invoices/${id}`), params);
}

export function createInvoice(data: CreateInvoiceRequest) {
  return apiClient.post<InvoiceDto>(tenantPath('/invoices'), data);
}

export function generateInvoice(data: GenerateInvoiceRequest) {
  return apiClient.post<InvoiceDto>(tenantPath('/invoices/generate'), data);
}

export function updateInvoice(id: string, data: UpdateInvoiceRequest) {
  return apiClient.patch<InvoiceDto>(tenantPath(`/invoices/${id}`), data);
}

export function transitionInvoiceStatus(id: string, data: TransitionInvoiceStatusRequest) {
  return apiClient.post<InvoiceDto>(tenantPath(`/invoices/${id}/status`), data);
}

export function createCreditNote(id: string, data: CreateCreditNoteRequest) {
  return apiClient.post<InvoiceDto>(tenantPath(`/invoices/${id}/credit-note`), data);
}

export function applyDiscount(id: string, data: ApplyDiscountRequest) {
  return apiClient.post<InvoiceDto>(tenantPath(`/invoices/${id}/discount`), data);
}

export function deleteInvoice(id: string) {
  return apiClient.delete<void>(tenantPath(`/invoices/${id}`));
}

export function getInvoiceSummary() {
  return apiClient.get<InvoiceSummaryDto>(tenantPath('/invoices/summary'));
}

export async function downloadInvoicePdf(id: string): Promise<Blob> {
  const token = getAccessToken();
  const tenantId = getTenantId();
  const url = `${API_BASE}/tenants/${tenantId}/invoices/${id}/pdf`;

  const response = await fetch(url, {
    method: 'GET',
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(tenantId ? { 'X-Tenant-ID': tenantId } : {}),
    },
  });

  if (!response.ok) {
    throw new Error(`Failed to download PDF: ${response.status}`);
  }

  return response.blob();
}

// Payments
export function listPayments(params?: QueryParams) {
  return apiClient.getPaged<PaymentListDto>(tenantPath('/payments'), params);
}

export function getPayment(id: string) {
  return apiClient.get<PaymentDto>(tenantPath(`/payments/${id}`));
}

export function recordPayment(data: RecordPaymentRequest) {
  return apiClient.post<PaymentDto>(tenantPath('/payments'), data);
}

export function transitionPaymentStatus(id: string, data: TransitionPaymentStatusRequest) {
  return apiClient.post<PaymentDto>(tenantPath(`/payments/${id}/status`), data);
}

export function refundPayment(id: string, data: RefundPaymentRequest) {
  return apiClient.post<PaymentDto>(tenantPath(`/payments/${id}/refund`), data);
}

export function deletePayment(id: string) {
  return apiClient.delete<void>(tenantPath(`/payments/${id}`));
}

// Discount Programs
export function listDiscountPrograms(params?: QueryParams) {
  return apiClient.getPaged<DiscountProgramListDto>(tenantPath('/discount-programs'), params);
}

export function getDiscountProgram(id: string) {
  return apiClient.get<DiscountProgramDto>(tenantPath(`/discount-programs/${id}`));
}

export function createDiscountProgram(data: CreateDiscountProgramRequest) {
  return apiClient.post<DiscountProgramDto>(tenantPath('/discount-programs'), data);
}

export function updateDiscountProgram(id: string, data: UpdateDiscountProgramRequest) {
  return apiClient.patch<DiscountProgramDto>(tenantPath(`/discount-programs/${id}`), data);
}

export function deleteDiscountProgram(id: string) {
  return apiClient.delete<void>(tenantPath(`/discount-programs/${id}`));
}

// Supplier Payments
export function listSupplierPayments(params?: QueryParams) {
  return apiClient.getPaged<SupplierPaymentListDto>(tenantPath('/supplier-payments'), params);
}

export function getSupplierPayment(id: string) {
  return apiClient.get<SupplierPaymentDto>(tenantPath(`/supplier-payments/${id}`));
}

export function createSupplierPayment(data: CreateSupplierPaymentRequest) {
  return apiClient.post<SupplierPaymentDto>(tenantPath('/supplier-payments'), data);
}

export function updateSupplierPayment(id: string, data: UpdateSupplierPaymentRequest) {
  return apiClient.patch<SupplierPaymentDto>(tenantPath(`/supplier-payments/${id}`), data);
}

export function transitionSupplierPaymentStatus(id: string, data: TransitionSupplierPaymentStatusRequest) {
  return apiClient.post<SupplierPaymentDto>(tenantPath(`/supplier-payments/${id}/status`), data);
}

export function deleteSupplierPayment(id: string) {
  return apiClient.delete<void>(tenantPath(`/supplier-payments/${id}`));
}

// Financial Reports
export function getMarginReport() {
  return apiClient.get<MarginReportDto>(tenantPath('/financial-reports/margin'));
}

export function getRevenueBreakdown(from?: string, to?: string) {
  const params: Record<string, string> = {};
  if (from) params.from = from;
  if (to) params.to = to;
  return apiClient.get<RevenueBreakdownDto>(tenantPath('/financial-reports/revenue-breakdown'), params);
}

export function generateXReport(reportDate?: string) {
  const params = reportDate ? `?reportDate=${reportDate}` : '';
  return apiClient.post<CashReconciliationDto>(tenantPath(`/financial-reports/x-report${params}`), {});
}

export function closeXReport(id: string) {
  return apiClient.post<CashReconciliationDto>(tenantPath(`/financial-reports/x-report/${id}/close`), {});
}

export function listXReports(params?: QueryParams) {
  return apiClient.getPaged<CashReconciliationListDto>(tenantPath('/financial-reports/x-reports'), params);
}

// Financial Settings (tenant)
export function getFinancialSettings() {
  const tenantId = getTenantId();
  return apiClient.get<TenantFinancialSettings>(`/tenants/${tenantId}/settings/financial`);
}

export function updateFinancialSettings(data: TenantFinancialSettings) {
  const tenantId = getTenantId();
  return apiClient.put<void>(`/tenants/${tenantId}/settings/financial`, data);
}

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type {
  CreateInvoiceRequest,
  UpdateInvoiceRequest,
  TransitionInvoiceStatusRequest,
  GenerateInvoiceRequest,
  CreateCreditNoteRequest,
  ApplyDiscountRequest,
  RecordPaymentRequest,
  TransitionPaymentStatusRequest,
  RefundPaymentRequest,
  CreateDiscountProgramRequest,
  UpdateDiscountProgramRequest,
  CreateSupplierPaymentRequest,
  UpdateSupplierPaymentRequest,
  TransitionSupplierPaymentStatusRequest,
  TenantFinancialSettings,
} from './types';

// Cache keys
const INVOICES_KEY = 'invoices';
const PAYMENTS_KEY = 'payments';
const DISCOUNT_PROGRAMS_KEY = 'discount-programs';
const SUPPLIER_PAYMENTS_KEY = 'supplier-payments';
const FINANCIAL_REPORTS_KEY = 'financial-reports';
const FINANCIAL_SETTINGS_KEY = 'financial-settings';

// Invoice hooks
export function useInvoices(params?: QueryParams) {
  return useQuery({
    queryKey: [INVOICES_KEY, params],
    queryFn: () => api.listInvoices(params),
  });
}

export function useInvoice(id: string) {
  return useQuery({
    queryKey: [INVOICES_KEY, id],
    queryFn: () => api.getInvoice(id, { include: 'lineItems,payments' }),
    enabled: !!id,
  });
}

export function useInvoiceSummary() {
  return useQuery({
    queryKey: [INVOICES_KEY, 'summary'],
    queryFn: () => api.getInvoiceSummary(),
  });
}

export function useCreateInvoice() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateInvoiceRequest) => api.createInvoice(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [INVOICES_KEY] });
    },
  });
}

export function useGenerateInvoice() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: GenerateInvoiceRequest) => api.generateInvoice(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [INVOICES_KEY] });
    },
  });
}

export function useUpdateInvoice() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateInvoiceRequest }) =>
      api.updateInvoice(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [INVOICES_KEY] });
    },
  });
}

export function useTransitionInvoiceStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: TransitionInvoiceStatusRequest }) =>
      api.transitionInvoiceStatus(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [INVOICES_KEY] });
    },
  });
}

export function useCreateCreditNote() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateCreditNoteRequest }) =>
      api.createCreditNote(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [INVOICES_KEY] });
    },
  });
}

export function useApplyDiscount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ApplyDiscountRequest }) =>
      api.applyDiscount(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [INVOICES_KEY] });
    },
  });
}

export function useDeleteInvoice() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteInvoice(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [INVOICES_KEY] });
    },
  });
}

// Payment hooks
export function usePayments(params?: QueryParams) {
  return useQuery({
    queryKey: [PAYMENTS_KEY, params],
    queryFn: () => api.listPayments(params),
  });
}

export function usePayment(id: string) {
  return useQuery({
    queryKey: [PAYMENTS_KEY, id],
    queryFn: () => api.getPayment(id),
    enabled: !!id,
  });
}

export function useRecordPayment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: RecordPaymentRequest) => api.recordPayment(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PAYMENTS_KEY] });
      queryClient.invalidateQueries({ queryKey: [INVOICES_KEY] });
    },
  });
}

export function useTransitionPaymentStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: TransitionPaymentStatusRequest }) =>
      api.transitionPaymentStatus(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PAYMENTS_KEY] });
      queryClient.invalidateQueries({ queryKey: [INVOICES_KEY] });
    },
  });
}

export function useRefundPayment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: RefundPaymentRequest }) =>
      api.refundPayment(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PAYMENTS_KEY] });
      queryClient.invalidateQueries({ queryKey: [INVOICES_KEY] });
    },
  });
}

export function useDeletePayment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deletePayment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PAYMENTS_KEY] });
    },
  });
}

// Discount Program hooks
export function useDiscountPrograms(params?: QueryParams) {
  return useQuery({
    queryKey: [DISCOUNT_PROGRAMS_KEY, params],
    queryFn: () => api.listDiscountPrograms(params),
  });
}

export function useDiscountProgram(id: string) {
  return useQuery({
    queryKey: [DISCOUNT_PROGRAMS_KEY, id],
    queryFn: () => api.getDiscountProgram(id),
    enabled: !!id,
  });
}

export function useCreateDiscountProgram() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateDiscountProgramRequest) => api.createDiscountProgram(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [DISCOUNT_PROGRAMS_KEY] });
    },
  });
}

export function useUpdateDiscountProgram() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateDiscountProgramRequest }) =>
      api.updateDiscountProgram(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [DISCOUNT_PROGRAMS_KEY] });
    },
  });
}

export function useDeleteDiscountProgram() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteDiscountProgram(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [DISCOUNT_PROGRAMS_KEY] });
    },
  });
}

// Supplier Payment hooks
export function useSupplierPayments(params?: QueryParams) {
  return useQuery({
    queryKey: [SUPPLIER_PAYMENTS_KEY, params],
    queryFn: () => api.listSupplierPayments(params),
  });
}

export function useSupplierPayment(id: string) {
  return useQuery({
    queryKey: [SUPPLIER_PAYMENTS_KEY, id],
    queryFn: () => api.getSupplierPayment(id),
    enabled: !!id,
  });
}

export function useCreateSupplierPayment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateSupplierPaymentRequest) => api.createSupplierPayment(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [SUPPLIER_PAYMENTS_KEY] });
    },
  });
}

export function useUpdateSupplierPayment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSupplierPaymentRequest }) =>
      api.updateSupplierPayment(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [SUPPLIER_PAYMENTS_KEY] });
    },
  });
}

export function useTransitionSupplierPaymentStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: TransitionSupplierPaymentStatusRequest }) =>
      api.transitionSupplierPaymentStatus(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [SUPPLIER_PAYMENTS_KEY] });
    },
  });
}

export function useDeleteSupplierPayment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteSupplierPayment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [SUPPLIER_PAYMENTS_KEY] });
    },
  });
}

// Financial Report hooks
export function useMarginReport() {
  return useQuery({
    queryKey: [FINANCIAL_REPORTS_KEY, 'margin'],
    queryFn: () => api.getMarginReport(),
  });
}

export function useRevenueBreakdown(from?: string, to?: string) {
  return useQuery({
    queryKey: [FINANCIAL_REPORTS_KEY, 'revenue-breakdown', from, to],
    queryFn: () => api.getRevenueBreakdown(from, to),
  });
}

export function useXReports(params?: QueryParams) {
  return useQuery({
    queryKey: [FINANCIAL_REPORTS_KEY, 'x-reports', params],
    queryFn: () => api.listXReports(params),
  });
}

export function useGenerateXReport() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (reportDate?: string) => api.generateXReport(reportDate),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [FINANCIAL_REPORTS_KEY] });
    },
  });
}

export function useCloseXReport() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.closeXReport(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [FINANCIAL_REPORTS_KEY] });
    },
  });
}

// Financial Settings hooks
export function useFinancialSettings() {
  return useQuery({
    queryKey: [FINANCIAL_SETTINGS_KEY],
    queryFn: () => api.getFinancialSettings(),
  });
}

export function useUpdateFinancialSettings() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: TenantFinancialSettings) => api.updateFinancialSettings(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [FINANCIAL_SETTINGS_KEY] });
    },
  });
}

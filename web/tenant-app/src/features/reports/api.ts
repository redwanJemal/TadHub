import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type {
  InventoryReportItem,
  DeployedReportItem,
  ReturneeReportItem,
  RunawayReportItem,
  ArrivalReportItem,
  AccommodationDailyItem,
  DeploymentPipelineItem,
  SupplierCommissionItem,
  RefundReportItem,
  CostPerMaidItem,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

// ── Workforce Reports ──

export function listInventoryReport(params?: Record<string, unknown>) {
  return apiClient.getPaged<InventoryReportItem>(tenantPath('/reports/inventory'), params);
}

export function listDeployedReport(params?: Record<string, unknown>) {
  return apiClient.getPaged<DeployedReportItem>(tenantPath('/reports/deployed'), params);
}

export function listReturneeReport(params?: Record<string, unknown>) {
  return apiClient.getPaged<ReturneeReportItem>(tenantPath('/reports/returnees'), params);
}

export function listRunawayReport(params?: Record<string, unknown>) {
  return apiClient.getPaged<RunawayReportItem>(tenantPath('/reports/runaways'), params);
}

// ── Operational Reports ──

export function listArrivalsReport(params?: Record<string, unknown>) {
  return apiClient.getPaged<ArrivalReportItem>(tenantPath('/reports/arrivals'), params);
}

export function listAccommodationDaily(date: string, params?: Record<string, unknown>) {
  return apiClient.getPaged<AccommodationDailyItem>(tenantPath('/reports/accommodation-daily'), { date, ...params });
}

export function getDeploymentPipeline() {
  return apiClient.get<DeploymentPipelineItem[]>(tenantPath('/reports/deployment-pipeline'));
}

// ── Finance Reports (Extensions) ──

export function listSupplierCommissions(params?: Record<string, unknown>) {
  return apiClient.getPaged<SupplierCommissionItem>(tenantPath('/reports/supplier-commissions'), params);
}

export function listRefundReport(params?: Record<string, unknown>) {
  return apiClient.getPaged<RefundReportItem>(tenantPath('/reports/refunds'), params);
}

export function listCostPerMaid(params?: Record<string, unknown>) {
  return apiClient.getPaged<CostPerMaidItem>(tenantPath('/reports/cost-per-maid'), params);
}

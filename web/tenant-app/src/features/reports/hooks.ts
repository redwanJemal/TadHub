import { useQuery } from '@tanstack/react-query';
import {
  listInventoryReport,
  listDeployedReport,
  listReturneeReport,
  listRunawayReport,
  listArrivalsReport,
  listAccommodationDaily,
  getDeploymentPipeline,
  listSupplierCommissions,
  listRefundReport,
  listCostPerMaid,
} from './api';

const REPORTS_KEY = 'reports';

export function useInventoryReport(params?: Record<string, unknown>) {
  return useQuery({
    queryKey: [REPORTS_KEY, 'inventory', params],
    queryFn: () => listInventoryReport(params),
  });
}

export function useDeployedReport(params?: Record<string, unknown>) {
  return useQuery({
    queryKey: [REPORTS_KEY, 'deployed', params],
    queryFn: () => listDeployedReport(params),
  });
}

export function useReturneeReport(params?: Record<string, unknown>) {
  return useQuery({
    queryKey: [REPORTS_KEY, 'returnees', params],
    queryFn: () => listReturneeReport(params),
  });
}

export function useRunawayReport(params?: Record<string, unknown>) {
  return useQuery({
    queryKey: [REPORTS_KEY, 'runaways', params],
    queryFn: () => listRunawayReport(params),
  });
}

export function useArrivalsReport(params?: Record<string, unknown>) {
  return useQuery({
    queryKey: [REPORTS_KEY, 'arrivals', params],
    queryFn: () => listArrivalsReport(params),
  });
}

export function useAccommodationDaily(date: string, params?: Record<string, unknown>) {
  return useQuery({
    queryKey: [REPORTS_KEY, 'accommodation-daily', date, params],
    queryFn: () => listAccommodationDaily(date, params),
  });
}

export function useDeploymentPipeline() {
  return useQuery({
    queryKey: [REPORTS_KEY, 'deployment-pipeline'],
    queryFn: () => getDeploymentPipeline(),
  });
}

export function useSupplierCommissions(params?: Record<string, unknown>) {
  return useQuery({
    queryKey: [REPORTS_KEY, 'supplier-commissions', params],
    queryFn: () => listSupplierCommissions(params),
  });
}

export function useRefundReport(params?: Record<string, unknown>) {
  return useQuery({
    queryKey: [REPORTS_KEY, 'refunds', params],
    queryFn: () => listRefundReport(params),
  });
}

export function useCostPerMaid(params?: Record<string, unknown>) {
  return useQuery({
    queryKey: [REPORTS_KEY, 'cost-per-maid', params],
    queryFn: () => listCostPerMaid(params),
  });
}

import { useQuery } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';

const AUDIT_KEY = 'audit';

export function useAuditEvents(params?: QueryParams) {
  return useQuery({
    queryKey: [AUDIT_KEY, 'events', params],
    queryFn: () => api.listAuditEvents(params),
  });
}

export function useAuditLogs(params?: QueryParams) {
  return useQuery({
    queryKey: [AUDIT_KEY, 'logs', params],
    queryFn: () => api.listAuditLogs(params),
  });
}

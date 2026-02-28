import { useQuery } from '@tanstack/react-query';
import * as api from './api';

const DASHBOARD_KEY = 'dashboard';

export function useDashboardSummary() {
  return useQuery({
    queryKey: [DASHBOARD_KEY],
    queryFn: () => api.getDashboardSummary(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

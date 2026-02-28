import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTenantStore } from '@/features/auth/hooks/useTenant';
import * as api from './api';

const NOTIFICATIONS_KEY = 'notifications';
const UNREAD_COUNT_KEY = 'notifications-unread-count';

export function useNotifications(params?: Record<string, unknown>) {
  const { currentTenant } = useTenantStore();
  const tenantId = currentTenant?.id;

  return useQuery({
    queryKey: [NOTIFICATIONS_KEY, tenantId, params],
    queryFn: () => api.listNotifications(tenantId!, params),
    enabled: !!tenantId,
  });
}

export function useUnreadCount() {
  const { currentTenant } = useTenantStore();
  const tenantId = currentTenant?.id;

  return useQuery({
    queryKey: [UNREAD_COUNT_KEY, tenantId],
    queryFn: () => api.getUnreadCount(tenantId!),
    enabled: !!tenantId,
    refetchInterval: 60_000,
  });
}

export function useMarkAsRead() {
  const queryClient = useQueryClient();
  const { currentTenant } = useTenantStore();
  const tenantId = currentTenant?.id;

  return useMutation({
    mutationFn: (notificationId: string) => api.markAsRead(tenantId!, notificationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [NOTIFICATIONS_KEY] });
      queryClient.invalidateQueries({ queryKey: [UNREAD_COUNT_KEY] });
    },
  });
}

export function useMarkAllAsRead() {
  const queryClient = useQueryClient();
  const { currentTenant } = useTenantStore();
  const tenantId = currentTenant?.id;

  return useMutation({
    mutationFn: () => api.markAllAsRead(tenantId!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [NOTIFICATIONS_KEY] });
      queryClient.invalidateQueries({ queryKey: [UNREAD_COUNT_KEY] });
    },
  });
}

export function useDeleteNotification() {
  const queryClient = useQueryClient();
  const { currentTenant } = useTenantStore();
  const tenantId = currentTenant?.id;

  return useMutation({
    mutationFn: (notificationId: string) => api.deleteNotification(tenantId!, notificationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [NOTIFICATIONS_KEY] });
      queryClient.invalidateQueries({ queryKey: [UNREAD_COUNT_KEY] });
    },
  });
}

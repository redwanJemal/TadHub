import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { QueryParams } from '@/shared/api/types';
import {
  listNotifications,
  sendNotification,
  getTenantNotificationSettings,
  updateTenantNotificationSettings,
} from './api';
import {
  AdminSendNotificationRequest,
  UpdateNotificationSettingsRequest,
} from './types';

// ============================================================================
// Query Keys
// ============================================================================

export const notificationKeys = {
  all: ['admin-notifications'] as const,
  lists: () => [...notificationKeys.all, 'list'] as const,
  list: (params?: QueryParams) => [...notificationKeys.lists(), params] as const,
  tenantSettings: (tenantId: string) =>
    [...notificationKeys.all, 'tenant-settings', tenantId] as const,
};

// ============================================================================
// Queries
// ============================================================================

export function useAdminNotifications(params?: QueryParams) {
  return useQuery({
    queryKey: notificationKeys.list(params),
    queryFn: () => listNotifications(params),
  });
}

export function useTenantNotificationSettings(tenantId: string) {
  return useQuery({
    queryKey: notificationKeys.tenantSettings(tenantId),
    queryFn: () => getTenantNotificationSettings(tenantId),
    enabled: !!tenantId,
  });
}

// ============================================================================
// Mutations
// ============================================================================

export function useSendNotification() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: AdminSendNotificationRequest) => sendNotification(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.lists() });
    },
    meta: {
      successMessage: 'Notification sent successfully',
    },
  });
}

export function useUpdateTenantNotificationSettings() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      tenantId,
      data,
    }: {
      tenantId: string;
      data: UpdateNotificationSettingsRequest;
    }) => updateTenantNotificationSettings(tenantId, data),
    onSuccess: (_, { tenantId }) => {
      queryClient.invalidateQueries({
        queryKey: notificationKeys.tenantSettings(tenantId),
      });
    },
    meta: {
      successMessage: 'Notification settings updated successfully',
    },
  });
}

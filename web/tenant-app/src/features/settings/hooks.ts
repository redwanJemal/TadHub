import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as api from './api';
import type { TenantNotificationSettings } from './types';

const NOTIFICATION_SETTINGS_KEY = 'notification-settings';

export function useNotificationSettings() {
  return useQuery({
    queryKey: [NOTIFICATION_SETTINGS_KEY],
    queryFn: () => api.getNotificationSettings(),
  });
}

export function useUpdateNotificationSettings() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: TenantNotificationSettings) => api.updateNotificationSettings(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [NOTIFICATION_SETTINGS_KEY] });
    },
  });
}

export interface NotificationDto {
  id: string;
  tenantId: string;
  userId: string;
  title: string;
  body: string;
  type: 'info' | 'warning' | 'success' | 'error';
  link?: string;
  priority: 'normal' | 'urgent';
  eventType?: string;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
}

export interface UnreadCountDto {
  count: number;
}

export interface UserNotificationPreferenceDto {
  id: string;
  userId: string;
  eventType: string;
  muted: boolean;
  channels: string;
}

export interface UpdateUserNotificationPreferenceRequest {
  eventType: string;
  muted: boolean;
  channels: string;
}

export interface BulkUpdateUserPreferencesRequest {
  preferences: UpdateUserNotificationPreferenceRequest[];
}

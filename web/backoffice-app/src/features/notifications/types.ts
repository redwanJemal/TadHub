export interface NotificationDto {
  id: string;
  tenantId: string;
  userId: string;
  title: string;
  body: string;
  type: 'info' | 'warning' | 'success' | 'error';
  link?: string;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
}

export interface AdminSendNotificationRequest {
  tenantId: string;
  userIds?: string[];
  title: string;
  body: string;
  type: string;
  link?: string;
}

export interface AdminSendNotificationResponse {
  recipientCount: number;
  tenantCount: number;
}

export interface TenantNotificationSettings {
  settings: string | null;
}

export interface UpdateNotificationSettingsRequest {
  settingsJson: string;
}

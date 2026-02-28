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

export interface UnreadCountDto {
  count: number;
}

export interface AuditEventDto {
  id: string;
  eventName: string;
  payload: string | null;
  userId: string | null;
  createdAt: string;
}

export interface AuditLogDto {
  id: string;
  action: string;
  entityType: string;
  entityId: string;
  oldValues: string | null;
  newValues: string | null;
  userId: string | null;
  createdAt: string;
}

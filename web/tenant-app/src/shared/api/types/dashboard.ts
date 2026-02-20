/**
 * Types for the BFF Tenant Dashboard endpoint
 */

export interface DashboardTenantInfo {
  id: string;
  name: string;
  slug: string;
  logoUrl: string | null;
  status: string;
  createdAt: string;
}

export interface DashboardSubscriptionInfo {
  planName: string;
  planSlug: string;
  status: string;
  currentPeriodEnd: string | null;
  isTrialActive: boolean;
  trialDaysRemaining: number | null;
}

export interface DashboardMemberStats {
  total: number;
  active: number;
  pendingInvitations: number;
  joinedThisMonth: number;
}

export interface DashboardActivityItem {
  id: string;
  action: string;
  entityType: string;
  entityName: string | null;
  actorName: string | null;
  timestamp: string;
}

export interface DashboardMetrics {
  totalRoles: number;
  totalApiKeys: number;
  storageUsedBytes: number;
  storageLimitBytes: number;
}

export interface TenantDashboardResponse {
  tenant: DashboardTenantInfo;
  subscription: DashboardSubscriptionInfo;
  members: DashboardMemberStats;
  recentActivity: DashboardActivityItem[];
  unreadNotifications: number;
  metrics: DashboardMetrics;
}

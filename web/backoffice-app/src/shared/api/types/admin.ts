/**
 * User entity
 */
export interface User {
  id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  avatar: string | null;
  phone: string | null;
  isActive: boolean;
  isEmailVerified: boolean;
  lastLoginAt: string | null;
  createdAt: string;
  updatedAt: string;
}

/**
 * Role entity
 */
export interface Role {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
}

/**
 * Plan entity
 */
export interface Plan {
  id: string;
  name: string;
  slug: string;
}

/**
 * Tenant membership
 */
export interface TenantMembership {
  id: string;
  tenantId: string;
  tenantName: string;
  tenantSlug: string;
  role: Role;
  isDefault: boolean;
  isActive: boolean;
  joinedAt: string;
}

/**
 * User with memberships (admin view)
 */
export interface UserWithMemberships extends User {
  memberships: TenantMembership[];
}

/**
 * Tenant entity
 */
export interface Tenant {
  id: string;
  name: string;
  slug: string;
  domain: string | null;
  logo: string | null;
  timezone: string | null;
  locale: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

/**
 * Tenant with stats (list view)
 */
export interface TenantWithStats extends Tenant {
  plan: Plan | null;
  userCount: number;
  subscriptionCount: number;
}

/**
 * Tenant detail with owner
 */
export interface TenantDetail extends TenantWithStats {
  owner: {
    id: string;
    email: string;
    firstName: string | null;
    lastName: string | null;
  } | null;
  settings: Record<string, unknown>;
  country: {
    id: string;
    displayName: string;
    iso2: string;
  } | null;
  currency: {
    id: string;
    displayName: string;
    code: string;
    symbol: string;
  } | null;
}

/**
 * Audit log actor
 */
export interface AuditActor {
  id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
}

/**
 * Audit log tenant reference
 */
export interface AuditTenant {
  id: string;
  name: string;
  slug: string;
}

/**
 * Audit log entry
 */
export interface AuditLog {
  id: string;
  action: string;
  entityType: string;
  entityId: string;
  changes: Record<string, unknown> | null;
  metadata: Record<string, unknown> | null;
  ipAddress: string | null;
  userAgent: string | null;
  createdAt: string;
  actor: AuditActor | null;
  tenant: AuditTenant | null;
}

/**
 * Dashboard overview stats
 */
export interface DashboardStats {
  totalTenants: number;
  activeTenants: number;
  totalUsers: number;
  activeUsers: number;
  totalSubscriptions: number;
  activeSubscriptions: number;
  newTenantsThisMonth: number;
  newUsersThisMonth: number;
  revenueThisMonth?: number;
}

/**
 * Recent tenant for dashboard widget
 */
export interface RecentTenant {
  id: string;
  name: string;
  slug: string;
  logo: string | null;
  userCount: number;
  createdAt: string;
  plan: Plan | null;
}

/**
 * Recent user for dashboard widget
 */
export interface RecentUser {
  id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  avatar: string | null;
  createdAt: string;
  tenantCount: number;
}

/**
 * Recent activity for dashboard widget
 */
export interface RecentActivity {
  id: string;
  action: string;
  entityType: string;
  entityId: string;
  createdAt: string;
  actor: AuditActor | null;
  tenant: AuditTenant | null;
}

/**
 * Full dashboard response
 */
export interface AdminDashboard {
  stats: DashboardStats;
  recentTenants: RecentTenant[];
  recentUsers: RecentUser[];
  recentActivity: RecentActivity[];
  systemHealth: 'operational' | 'degraded' | 'outage';
}

/**
 * Audit summary
 */
export interface AuditSummary {
  totalActions: number;
  actionsByType: Record<string, number>;
  actionsByEntity: Record<string, number>;
  topActors: Array<{
    userId: string;
    email: string;
    actionCount: number;
  }>;
}

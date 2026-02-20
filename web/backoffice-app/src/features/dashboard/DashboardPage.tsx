import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { Avatar, AvatarFallback, AvatarImage } from "@/shared/components/ui/avatar";
import {
  Users,
  Building2,
  Shield,
  FileText,
  TrendingUp,
  TrendingDown,
  Activity,
  AlertCircle,
  CheckCircle2,
  Clock,
} from "lucide-react";
import { useAdminDashboard } from "@/shared/api";
import type { RecentActivity, RecentTenant, RecentUser } from "@/shared/api";

export function DashboardPage() {
  const { t } = useTranslation("dashboard");
  const { data: dashboard, isLoading, isError, error } = useAdminDashboard();

  if (isError) {
    return (
      <div className="flex flex-col items-center justify-center py-12">
        <AlertCircle className="h-12 w-12 text-destructive mb-4" />
        <h2 className="text-lg font-semibold">{t("error.title")}</h2>
        <p className="text-muted-foreground">{error?.message || t("error.generic")}</p>
      </div>
    );
  }

  const stats = dashboard
    ? [
        {
          key: "totalTenants",
          icon: Building2,
          value: dashboard.stats.totalTenants,
          change: dashboard.stats.newTenantsThisMonth,
          color: "text-blue-600",
        },
        {
          key: "totalUsers",
          icon: Users,
          value: dashboard.stats.totalUsers,
          change: dashboard.stats.newUsersThisMonth,
          color: "text-green-600",
        },
        {
          key: "activeSubscriptions",
          icon: Shield,
          value: dashboard.stats.activeSubscriptions,
          change: 0,
          color: "text-purple-600",
        },
        {
          key: "activeTenants",
          icon: CheckCircle2,
          value: dashboard.stats.activeTenants,
          change: 0,
          color: "text-orange-600",
        },
      ]
    : [];

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold md:text-3xl">{t("title")}</h1>
          <p className="text-muted-foreground">{t("subtitle")}</p>
        </div>
        {dashboard && (
          <SystemHealthBadge health={dashboard.systemHealth} />
        )}
      </div>

      {/* Stats grid */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {isLoading
          ? Array.from({ length: 4 }).map((_, i) => <StatCardSkeleton key={i} />)
          : stats.map((stat) => (
              <StatCard
                key={stat.key}
                title={t(`stats.${stat.key}`)}
                value={stat.value}
                change={stat.change}
                icon={stat.icon}
                color={stat.color}
              />
            ))}
      </div>

      {/* Content grid */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Recent tenants */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              {t("recentTenants")}
            </CardTitle>
            <Link
              to="/tenants"
              className="text-sm text-primary hover:underline"
            >
              {t("viewAll")}
            </Link>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <RecentItemsSkeleton count={3} />
            ) : (
              <RecentTenantsList tenants={dashboard?.recentTenants || []} />
            )}
          </CardContent>
        </Card>

        {/* Recent users */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Users className="h-5 w-5" />
              {t("recentUsers")}
            </CardTitle>
            <Link
              to="/users"
              className="text-sm text-primary hover:underline"
            >
              {t("viewAll")}
            </Link>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <RecentItemsSkeleton count={3} />
            ) : (
              <RecentUsersList users={dashboard?.recentUsers || []} />
            )}
          </CardContent>
        </Card>

        {/* Recent activity */}
        <Card className="lg:col-span-2">
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Activity className="h-5 w-5" />
              {t("recentActivity")}
            </CardTitle>
            <Link
              to="/audit-logs"
              className="text-sm text-primary hover:underline"
            >
              {t("viewAll")}
            </Link>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <RecentItemsSkeleton count={5} />
            ) : (
              <RecentActivityList activities={dashboard?.recentActivity || []} />
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════
// Sub-components
// ═══════════════════════════════════════════════════════════════

function SystemHealthBadge({ health }: { health: string }) {
  const colors = {
    operational: "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400",
    degraded: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
    outage: "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400",
  };

  const icons = {
    operational: CheckCircle2,
    degraded: AlertCircle,
    outage: AlertCircle,
  };

  const Icon = icons[health as keyof typeof icons] || CheckCircle2;
  const color = colors[health as keyof typeof colors] || colors.operational;

  return (
    <div className={`flex items-center gap-2 rounded-full px-3 py-1 text-sm font-medium ${color}`}>
      <Icon className="h-4 w-4" />
      <span className="capitalize">{health}</span>
    </div>
  );
}

interface StatCardProps {
  title: string;
  value: number;
  change: number;
  icon: typeof Users;
  color: string;
}

function StatCard({ title, value, change, icon: Icon, color }: StatCardProps) {
  const isPositive = change > 0;
  const TrendIcon = isPositive ? TrendingUp : TrendingDown;

  return (
    <Card className="relative overflow-hidden">
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-sm font-medium text-muted-foreground">
          {title}
        </CardTitle>
        <Icon className={`h-5 w-5 ${color}`} />
      </CardHeader>
      <CardContent>
        <div className="text-2xl font-bold">{value.toLocaleString()}</div>
        {change !== 0 && (
          <div className="mt-1 flex items-center text-xs">
            <TrendIcon
              className={`me-1 h-3 w-3 ${isPositive ? "text-green-500" : "text-red-500"}`}
            />
            <span className={isPositive ? "text-green-500" : "text-red-500"}>
              {isPositive ? "+" : ""}{change}
            </span>
            <span className="ms-1 text-muted-foreground">this month</span>
          </div>
        )}
      </CardContent>
      <div
        className={`absolute inset-x-0 bottom-0 h-1 bg-gradient-to-r from-transparent via-current to-transparent ${color} opacity-20`}
      />
    </Card>
  );
}

function StatCardSkeleton() {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-5 w-5 rounded" />
      </CardHeader>
      <CardContent>
        <Skeleton className="h-8 w-16" />
        <Skeleton className="mt-2 h-3 w-20" />
      </CardContent>
    </Card>
  );
}

function RecentItemsSkeleton({ count }: { count: number }) {
  return (
    <div className="space-y-4">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="flex items-center gap-3">
          <Skeleton className="h-10 w-10 rounded-full" />
          <div className="flex-1 space-y-2">
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-3 w-24" />
          </div>
          <Skeleton className="h-3 w-16" />
        </div>
      ))}
    </div>
  );
}

function RecentTenantsList({ tenants }: { tenants: RecentTenant[] }) {
  if (tenants.length === 0) {
    return <EmptyState message="No recent tenants" />;
  }

  return (
    <div className="space-y-4">
      {tenants.map((tenant) => (
        <Link
          key={tenant.id}
          to={`/tenants/${tenant.id}`}
          className="flex items-center gap-3 rounded-lg p-2 transition-colors hover:bg-muted"
        >
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10 text-primary font-semibold">
            {tenant.name.charAt(0).toUpperCase()}
          </div>
          <div className="flex-1 min-w-0">
            <p className="font-medium truncate">{tenant.name}</p>
            <p className="text-sm text-muted-foreground">
              {tenant.userCount} users · {tenant.plan?.name || "No plan"}
            </p>
          </div>
          <span className="text-xs text-muted-foreground">
            <Clock className="inline h-3 w-3 me-1" />
            {formatRelativeTime(tenant.createdAt)}
          </span>
        </Link>
      ))}
    </div>
  );
}

function RecentUsersList({ users }: { users: RecentUser[] }) {
  if (users.length === 0) {
    return <EmptyState message="No recent users" />;
  }

  return (
    <div className="space-y-4">
      {users.map((user) => (
        <Link
          key={user.id}
          to={`/users/${user.id}`}
          className="flex items-center gap-3 rounded-lg p-2 transition-colors hover:bg-muted"
        >
          <Avatar className="h-10 w-10">
            <AvatarImage src={user.avatar || undefined} />
            <AvatarFallback>
              {getInitials(user.firstName, user.lastName)}
            </AvatarFallback>
          </Avatar>
          <div className="flex-1 min-w-0">
            <p className="font-medium truncate">
              {user.firstName} {user.lastName}
            </p>
            <p className="text-sm text-muted-foreground truncate">{user.email}</p>
          </div>
          <span className="text-xs text-muted-foreground">
            {user.tenantCount} tenant{user.tenantCount !== 1 ? "s" : ""}
          </span>
        </Link>
      ))}
    </div>
  );
}

function RecentActivityList({ activities }: { activities: RecentActivity[] }) {
  if (activities.length === 0) {
    return <EmptyState message="No recent activity" />;
  }

  return (
    <div className="space-y-3">
      {activities.map((activity) => (
        <div
          key={activity.id}
          className="flex items-center justify-between border-b pb-3 last:border-0 last:pb-0"
        >
          <div className="flex items-center gap-3">
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted">
              <FileText className="h-4 w-4 text-muted-foreground" />
            </div>
            <div>
              <p className="font-medium text-sm">
                {formatActionLabel(activity.action)}
              </p>
              <p className="text-xs text-muted-foreground">
                {activity.actor?.email || "System"} · {activity.tenant?.name || "Global"}
              </p>
            </div>
          </div>
          <span className="text-xs text-muted-foreground whitespace-nowrap">
            {formatRelativeTime(activity.createdAt)}
          </span>
        </div>
      ))}
    </div>
  );
}

function EmptyState({ message }: { message: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-8 text-muted-foreground">
      <FileText className="h-8 w-8 mb-2 opacity-50" />
      <p className="text-sm">{message}</p>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════
// Helpers
// ═══════════════════════════════════════════════════════════════

function getInitials(firstName: string | null, lastName: string | null): string {
  const first = firstName?.charAt(0) || "";
  const last = lastName?.charAt(0) || "";
  return (first + last).toUpperCase() || "?";
}

function formatActionLabel(action: string): string {
  // Convert 'user.created' to 'User Created'
  return action
    .split(".")
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");
}

function formatRelativeTime(dateString: string): string {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMins / 60);
  const diffDays = Math.floor(diffHours / 24);

  if (diffMins < 1) return "Just now";
  if (diffMins < 60) return `${diffMins}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;

  return date.toLocaleDateString();
}

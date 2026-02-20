import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useAuth } from 'react-oidc-context';
import { Users, Activity, Key, HardDrive, Clock, AlertCircle } from 'lucide-react';
import { apiClient } from '../../shared/api';
import type { TenantDashboardResponse } from '../../shared/api/types';

function formatBytes(bytes: number): string {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
}

function formatRelativeTime(timestamp: string): string {
  const date = new Date(timestamp);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return 'Just now';
  if (diffMins < 60) return `${diffMins}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;
  return date.toLocaleDateString();
}

export function DashboardPage() {
  const { t } = useTranslation('dashboard');
  const auth = useAuth();
  const firstName = (auth.user?.profile?.given_name as string) || 'User';

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dashboard, setDashboard] = useState<TenantDashboardResponse | null>(null);

  useEffect(() => {
    async function fetchDashboard() {
      try {
        setLoading(true);
        const data = await apiClient.get<TenantDashboardResponse>('/bff/tenant/dashboard');
        setDashboard(data);
        setError(null);
      } catch (err) {
        console.error('Failed to fetch dashboard:', err);
        setError('Failed to load dashboard data');
      } finally {
        setLoading(false);
      }
    }

    fetchDashboard();
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (error || !dashboard) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[400px] text-muted-foreground">
        <AlertCircle className="h-12 w-12 mb-4" />
        <p>{error || 'No data available'}</p>
        <button
          onClick={() => window.location.reload()}
          className="mt-4 px-4 py-2 bg-primary text-primary-foreground rounded-lg hover:bg-primary/90"
        >
          Retry
        </button>
      </div>
    );
  }

  const stats = [
    { label: t('stats.teamMembers'), value: dashboard.members.total.toString(), icon: Users },
    { label: t('stats.activeUsers'), value: dashboard.members.active.toString(), icon: Activity },
    { label: t('stats.apiKeys'), value: dashboard.metrics.totalApiKeys.toString(), icon: Key },
    { 
      label: t('stats.storage'), 
      value: formatBytes(dashboard.metrics.storageUsedBytes), 
      icon: HardDrive,
      subtitle: `/ ${formatBytes(dashboard.metrics.storageLimitBytes)}`
    },
  ];

  return (
    <div className="space-y-8">
      {/* Welcome header */}
      <div>
        <h1 className="text-2xl font-bold text-foreground">
          {t('welcome', { name: firstName })}
        </h1>
        <p className="text-muted-foreground mt-1">{t('overview')}</p>
      </div>

      {/* Subscription banner */}
      {dashboard.subscription.isTrialActive && dashboard.subscription.trialDaysRemaining !== null && (
        <div className="rounded-xl bg-amber-500/10 border border-amber-500/20 p-4 flex items-center gap-3">
          <Clock className="h-5 w-5 text-amber-500" />
          <div>
            <p className="font-medium text-foreground">
              Trial ends in {dashboard.subscription.trialDaysRemaining} days
            </p>
            <p className="text-sm text-muted-foreground">
              Upgrade to {dashboard.subscription.planName} to keep all features
            </p>
          </div>
        </div>
      )}

      {/* Stats grid */}
      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => (
          <div
            key={stat.label}
            className="rounded-xl bg-card border border-border p-6"
          >
            <div className="flex items-center justify-between">
              <stat.icon className="h-5 w-5 text-primary" />
            </div>
            <div className="mt-4">
              <p className="text-2xl font-bold text-foreground">{stat.value}</p>
              <p className="text-sm text-muted-foreground">
                {stat.label}
                {'subtitle' in stat && (
                  <span className="opacity-60"> {stat.subtitle}</span>
                )}
              </p>
            </div>
          </div>
        ))}
      </div>

      {/* Quick actions */}
      <div className="rounded-xl bg-card border border-border p-6">
        <h2 className="text-lg font-semibold text-foreground mb-4">
          {t('quickActions')}
        </h2>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Object.entries({
            inviteTeam: t('actions.inviteTeam'),
            createApiKey: t('actions.createApiKey'),
            viewSettings: t('actions.viewSettings'),
            viewBilling: t('actions.viewBilling'),
          }).map(([key, label]) => (
            <button
              key={key}
              className="rounded-lg border border-border p-4 text-left hover:bg-accent transition-colors"
            >
              <p className="font-medium text-foreground">{label}</p>
            </button>
          ))}
        </div>
      </div>

      {/* Recent activity */}
      <div className="rounded-xl bg-card border border-border p-6">
        <h2 className="text-lg font-semibold text-foreground mb-4">
          {t('recentActivity')}
        </h2>
        {dashboard.recentActivity.length === 0 ? (
          <p className="text-muted-foreground">{t('activity.noActivity')}</p>
        ) : (
          <div className="space-y-4">
            {dashboard.recentActivity.slice(0, 5).map((activity) => (
              <div
                key={activity.id}
                className="flex items-start justify-between gap-4 py-3 border-b border-border last:border-0"
              >
                <div>
                  <p className="text-foreground">
                    <span className="font-medium">{activity.actorName || 'System'}</span>
                    {' '}{activity.action.toLowerCase()}{' '}
                    {activity.entityName && (
                      <span className="text-primary">{activity.entityName}</span>
                    )}
                  </p>
                  <p className="text-sm text-muted-foreground">{activity.entityType}</p>
                </div>
                <p className="text-sm text-muted-foreground whitespace-nowrap">
                  {formatRelativeTime(activity.timestamp)}
                </p>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

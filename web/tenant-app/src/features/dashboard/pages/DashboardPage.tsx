import { useTranslation } from 'react-i18next';
import {
  Users, FileText, UserCheck, Building2, ShieldCheck, AlertTriangle,
} from 'lucide-react';
import { useDashboardSummary } from '../hooks';
import { usePermissions } from '@/features/auth/hooks/usePermissions';
import { KpiCard } from '../components/KpiCard';
import { ActivityFeed } from '../components/ActivityFeed';
import { ComplianceMiniCard } from '../components/ComplianceMiniCard';
import { QuickActions } from '../components/QuickActions';

export function DashboardPage() {
  const { t } = useTranslation('dashboard');
  const { data, isLoading } = useDashboardSummary();
  const { hasPermission } = usePermissions();

  const kpis = data?.kpis;
  const compliance = data?.compliance;

  const kpiCards = [
    { permission: 'workers.view', title: t('kpi.activeWorkers'), value: kpis?.activeWorkers ?? 0, subtitle: `/ ${kpis?.totalWorkers ?? 0}`, icon: Users, href: '/workers' },
    { permission: 'contracts.view', title: t('kpi.activeContracts'), value: kpis?.activeContracts ?? 0, subtitle: `/ ${kpis?.totalContracts ?? 0}`, icon: FileText, href: '/contracts' },
    { permission: 'candidates.view', title: t('kpi.pendingCandidates'), value: kpis?.pendingCandidates ?? 0, subtitle: `/ ${kpis?.totalCandidates ?? 0}`, icon: UserCheck, href: '/candidates', className: 'text-amber-600' },
    { permission: 'clients.view', title: t('kpi.totalClients'), value: kpis?.activeClients ?? 0, subtitle: `/ ${kpis?.totalClients ?? 0}`, icon: Building2, href: '/clients' },
    { permission: 'documents.view', title: t('kpi.complianceRate'), value: compliance?.complianceRate ?? 0, subtitle: '%', icon: ShieldCheck, href: '/compliance', className: 'text-emerald-600' },
    { permission: 'documents.view', title: t('kpi.expiringSoon'), value: compliance?.expiringSoon ?? 0, icon: AlertTriangle, href: '/compliance', className: 'text-amber-600' },
  ].filter(card => hasPermission(card.permission));

  // Determine grid columns based on visible cards
  const gridCols = kpiCards.length <= 3
    ? 'sm:grid-cols-2 lg:grid-cols-3'
    : kpiCards.length <= 4
      ? 'sm:grid-cols-2 lg:grid-cols-4'
      : 'sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6';

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{t('title')}</h1>
        <p className="text-muted-foreground">{t('description')}</p>
      </div>

      {/* KPI Cards Row */}
      {kpiCards.length > 0 && (
        <div className={`grid gap-4 ${gridCols}`}>
          {kpiCards.map((card) => (
            <KpiCard
              key={card.href + card.title}
              title={card.title}
              value={card.value}
              subtitle={card.subtitle}
              icon={card.icon}
              href={card.href}
              className={card.className}
              isLoading={isLoading}
            />
          ))}
        </div>
      )}

      {/* Lower Section: Activity Feed + Compliance & Quick Actions */}
      <div className="grid gap-4 lg:grid-cols-3">
        <ActivityFeed
          items={data?.recentActivity ?? []}
          isLoading={isLoading}
        />
        <div className="space-y-4">
          {hasPermission('documents.view') && (
            <ComplianceMiniCard
              compliance={compliance}
              isLoading={isLoading}
            />
          )}
          <QuickActions />
        </div>
      </div>
    </div>
  );
}

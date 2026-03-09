import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Users, UserCheck, HardHat, DollarSign, Clock, CheckCircle, XCircle, Briefcase } from 'lucide-react';
import { useSupplierDashboard, useSupplierProfile } from '../hooks';

function DashboardSkeleton() {
  return (
    <div className="space-y-6 p-6">
      <Skeleton className="h-8 w-64" />
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <Card key={i}>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-4 w-4" />
            </CardHeader>
            <CardContent>
              <Skeleton className="h-8 w-16" />
            </CardContent>
          </Card>
        ))}
      </div>
      <Card>
        <CardHeader><Skeleton className="h-5 w-40" /></CardHeader>
        <CardContent><Skeleton className="h-24 w-full" /></CardContent>
      </Card>
    </div>
  );
}

export function SupplierDashboardPage() {
  const { t } = useTranslation('supplierPortal');
  const { data: profile, isLoading: profileLoading } = useSupplierProfile();
  const { data: dashboard, isLoading } = useSupplierDashboard();

  if (isLoading || profileLoading) {
    return <DashboardSkeleton />;
  }

  const stats = dashboard;

  return (
    <div className="space-y-6 p-6">
      <div>
        <h1 className="text-2xl font-semibold">{t('dashboard.title')}</h1>
        {profile && (
          <p className="text-sm text-muted-foreground mt-1">
            {profile.supplierNameEn || profile.displayName || t('dashboard.welcome')}
          </p>
        )}
      </div>

      {/* Candidate Stats */}
      <div>
        <h2 className="text-lg font-medium mb-3">{t('dashboard.candidates')}</h2>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          <StatCard
            title={t('dashboard.totalCandidates')}
            value={stats?.totalCandidates ?? 0}
            icon={Users}
          />
          <StatCard
            title={t('dashboard.pendingCandidates')}
            value={stats?.pendingCandidates ?? 0}
            icon={Clock}
            className="text-yellow-600"
          />
          <StatCard
            title={t('dashboard.approvedCandidates')}
            value={stats?.approvedCandidates ?? 0}
            icon={CheckCircle}
            className="text-green-600"
          />
          <StatCard
            title={t('dashboard.rejectedCandidates')}
            value={stats?.rejectedCandidates ?? 0}
            icon={XCircle}
            className="text-red-600"
          />
        </div>
      </div>

      {/* Worker Stats */}
      <div>
        <h2 className="text-lg font-medium mb-3">{t('dashboard.workers')}</h2>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          <StatCard
            title={t('dashboard.totalWorkers')}
            value={stats?.totalWorkers ?? 0}
            icon={HardHat}
          />
          <StatCard
            title={t('dashboard.activeWorkers')}
            value={stats?.activeWorkers ?? 0}
            icon={UserCheck}
            className="text-green-600"
          />
          <StatCard
            title={t('dashboard.deployedWorkers')}
            value={stats?.deployedWorkers ?? 0}
            icon={Briefcase}
            className="text-blue-600"
          />
        </div>
      </div>

      {/* Commission Stats */}
      <div>
        <h2 className="text-lg font-medium mb-3">{t('dashboard.commissions')}</h2>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          <StatCard
            title={t('dashboard.totalCommissions')}
            value={stats?.totalCommissions ?? 0}
            icon={DollarSign}
            isCurrency
          />
          <StatCard
            title={t('dashboard.pendingCommissions')}
            value={stats?.pendingCommissions ?? 0}
            icon={Clock}
            className="text-yellow-600"
            isCurrency
          />
          <StatCard
            title={t('dashboard.paidCommissions')}
            value={stats?.paidCommissions ?? 0}
            icon={CheckCircle}
            className="text-green-600"
            isCurrency
          />
        </div>
      </div>
    </div>
  );
}

function StatCard({
  title,
  value,
  icon: Icon,
  className,
  isCurrency,
}: {
  title: string;
  value: number;
  icon: React.ElementType;
  className?: string;
  isCurrency?: boolean;
}) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium">{title}</CardTitle>
        <Icon className={`h-4 w-4 ${className ?? 'text-muted-foreground'}`} />
      </CardHeader>
      <CardContent>
        <div className={`text-2xl font-bold ${className ?? ''}`}>
          {isCurrency ? `AED ${value.toLocaleString()}` : value}
        </div>
      </CardContent>
    </Card>
  );
}

import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { ShieldCheck } from 'lucide-react';
import type { DashboardCompliance } from '@/shared/api/types/dashboard';

interface ComplianceMiniCardProps {
  compliance: DashboardCompliance | undefined;
  isLoading?: boolean;
}

export function ComplianceMiniCard({ compliance, isLoading }: ComplianceMiniCardProps) {
  const { t } = useTranslation('dashboard');
  const navigate = useNavigate();

  return (
    <Card
      className="cursor-pointer transition-colors hover:bg-accent/50"
      onClick={() => navigate('/compliance')}
    >
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <ShieldCheck className="h-5 w-5" />
          {t('compliance.title')}
        </CardTitle>
      </CardHeader>
      <CardContent>
        {isLoading || !compliance ? (
          <div className="space-y-3">
            <Skeleton className="h-8 w-20" />
            <Skeleton className="h-4 w-full" />
          </div>
        ) : (
          <div className="space-y-3">
            <div className="text-2xl font-bold">
              {compliance.complianceRate}%
              <span className="text-sm font-normal text-muted-foreground ms-1">
                {t('compliance.valid').toLowerCase()}
              </span>
            </div>
            <div className="grid grid-cols-2 gap-2 text-sm">
              <div>
                <span className="text-emerald-600 font-medium">{compliance.valid}</span>
                <span className="text-muted-foreground ms-1">{t('compliance.valid')}</span>
              </div>
              <div>
                <span className="text-amber-600 font-medium">{compliance.expiringSoon}</span>
                <span className="text-muted-foreground ms-1">{t('compliance.expiringSoon')}</span>
              </div>
              <div>
                <span className="text-destructive font-medium">{compliance.expired}</span>
                <span className="text-muted-foreground ms-1">{t('compliance.expired')}</span>
              </div>
              <div>
                <span className="text-muted-foreground font-medium">{compliance.pending}</span>
                <span className="text-muted-foreground ms-1">{t('compliance.pending')}</span>
              </div>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

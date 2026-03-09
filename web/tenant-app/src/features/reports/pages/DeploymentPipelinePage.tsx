import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ChevronLeft } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { useDeploymentPipeline } from '../hooks';

export function DeploymentPipelinePage() {
  const { t } = useTranslation('reports');
  const { data, isLoading } = useDeploymentPipeline();

  const total = data?.reduce((sum, item) => sum + item.count, 0) ?? 0;
  const maxCount = data ? Math.max(...data.map((d) => d.count), 1) : 1;

  if (isLoading) return <PipelineSkeleton />;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Link to="/reports" className="text-muted-foreground hover:text-foreground">
          <ChevronLeft className="h-5 w-5" />
        </Link>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t('reports.deploymentPipeline.title')}</h1>
          <p className="text-muted-foreground text-sm">{t('reports.deploymentPipeline.description')}</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            <span>{t('reports.deploymentPipeline.title')}</span>
            <span className="text-muted-foreground text-sm font-normal">
              {t('reports.deploymentPipeline.total')}: {total}
            </span>
          </CardTitle>
        </CardHeader>
        <CardContent>
          {data && data.length > 0 ? (
            <div className="space-y-4">
              {data.map((item) => (
                <div key={item.stage} className="space-y-1">
                  <div className="flex items-center justify-between text-sm">
                    <span className="font-medium">{item.stage}</span>
                    <span className="text-muted-foreground">{item.count}</span>
                  </div>
                  <div className="h-3 w-full rounded-full bg-muted">
                    <div
                      className="h-3 rounded-full bg-primary transition-all"
                      style={{ width: `${(item.count / maxCount) * 100}%` }}
                    />
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-muted-foreground text-sm text-center py-8">No active placements in pipeline</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function PipelineSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Skeleton className="h-5 w-5" />
        <div className="space-y-2">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-4 w-72" />
        </div>
      </div>
      <Card>
        <CardHeader><Skeleton className="h-5 w-40" /></CardHeader>
        <CardContent className="space-y-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="space-y-1">
              <div className="flex justify-between">
                <Skeleton className="h-4 w-32" />
                <Skeleton className="h-4 w-8" />
              </div>
              <Skeleton className="h-3 w-full" />
            </div>
          ))}
        </CardContent>
      </Card>
    </div>
  );
}

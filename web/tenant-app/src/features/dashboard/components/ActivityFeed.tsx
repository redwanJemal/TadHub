import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Activity } from 'lucide-react';
import type { DashboardActivityItem } from '@/shared/api/types/dashboard';

interface ActivityFeedProps {
  items: DashboardActivityItem[];
  isLoading?: boolean;
}

function formatRelativeTime(dateStr: string, t: (key: string, opts?: Record<string, unknown>) => string): string {
  const now = Date.now();
  const date = new Date(dateStr).getTime();
  const diffMs = now - date;
  const diffMin = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMin < 1) return t('time.justNow');
  if (diffMin < 60) return t('time.minutesAgo', { count: diffMin });
  if (diffHours < 24) return t('time.hoursAgo', { count: diffHours });
  return t('time.daysAgo', { count: diffDays });
}

function formatEventName(eventName: string): string {
  // "CandidateCreated" → "Candidate Created"
  return eventName.replace(/([a-z])([A-Z])/g, '$1 $2');
}

export function ActivityFeed({ items, isLoading }: ActivityFeedProps) {
  const { t } = useTranslation('dashboard');

  return (
    <Card className="lg:col-span-2">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Activity className="h-5 w-5" />
          {t('activity.title')}
        </CardTitle>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="space-y-4">
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="flex items-center gap-3">
                <Skeleton className="h-2 w-2 rounded-full" />
                <Skeleton className="h-4 flex-1" />
                <Skeleton className="h-4 w-16" />
              </div>
            ))}
          </div>
        ) : items.length === 0 ? (
          <p className="text-sm text-muted-foreground">{t('activity.empty')}</p>
        ) : (
          <div className="space-y-3">
            {items.map((item) => (
              <div key={item.id} className="flex items-start justify-between gap-4 text-sm">
                <div className="flex items-start gap-3 min-w-0">
                  <div className="h-2 w-2 rounded-full bg-primary mt-1.5 shrink-0" />
                  <div className="min-w-0">
                    <span className="font-medium">{formatEventName(item.eventName)}</span>
                    {item.entityName && (
                      <span className="text-muted-foreground"> — {item.entityName}</span>
                    )}
                  </div>
                </div>
                <span className="text-muted-foreground text-xs whitespace-nowrap shrink-0">
                  {formatRelativeTime(item.createdAt, t)}
                </span>
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

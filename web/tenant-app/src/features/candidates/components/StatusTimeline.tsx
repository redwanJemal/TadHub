import { useTranslation } from 'react-i18next';
import { StatusBadge } from './StatusBadge';
import { STATUS_CONFIG } from '../constants';
import type { CandidateStatusHistoryDto } from '../types';

interface StatusTimelineProps {
  history: CandidateStatusHistoryDto[];
}

export function StatusTimeline({ history }: StatusTimelineProps) {
  const { t } = useTranslation('candidates');

  if (!history || history.length === 0) {
    return null;
  }

  const sorted = [...history].sort(
    (a, b) => new Date(b.changedAt).getTime() - new Date(a.changedAt).getTime()
  );

  return (
    <div className="space-y-4">
      {sorted.map((entry, index) => {
        const config = STATUS_CONFIG[entry.toStatus];
        const Icon = config?.icon;
        const isFirst = index === sorted.length - 1;

        return (
          <div key={entry.id} className="flex gap-4">
            {/* Timeline line + dot */}
            <div className="flex flex-col items-center">
              <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full border-2 bg-background">
                {Icon && <Icon className="h-4 w-4 text-muted-foreground" />}
              </div>
              {index < sorted.length - 1 && (
                <div className="w-px flex-1 bg-border" />
              )}
            </div>

            {/* Content */}
            <div className="pb-6 pt-1">
              <div className="flex items-center gap-2">
                {isFirst && !entry.fromStatus ? (
                  <p className="text-sm font-medium">{t('timeline.initialStatus')}</p>
                ) : (
                  <p className="text-sm font-medium">
                    {t('timeline.changedTo')}{' '}
                    <StatusBadge status={entry.toStatus} />
                  </p>
                )}
              </div>
              <p className="mt-1 text-xs text-muted-foreground">
                {new Date(entry.changedAt).toLocaleString()}
              </p>
              {entry.reason && (
                <p className="mt-1 text-sm text-muted-foreground">
                  <span className="font-medium">{t('timeline.reason')}:</span> {entry.reason}
                </p>
              )}
              {entry.notes && (
                <p className="mt-1 text-sm text-muted-foreground">{entry.notes}</p>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}

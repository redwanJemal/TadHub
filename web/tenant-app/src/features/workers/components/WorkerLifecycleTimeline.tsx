import { useTranslation } from 'react-i18next';
import { Check } from 'lucide-react';
import { cn } from '@/shared/lib/cn';
import { LIFECYCLE_STAGES, STATUS_CONFIG } from '../constants';
import type { WorkerStatus, WorkerStatusHistoryDto } from '../types';

interface WorkerLifecycleTimelineProps {
  currentStatus: WorkerStatus;
  statusHistory?: WorkerStatusHistoryDto[];
}

export function WorkerLifecycleTimeline({ currentStatus, statusHistory }: WorkerLifecycleTimelineProps) {
  const { t } = useTranslation('workers');

  // Determine which lifecycle stages have been reached based on history
  const reachedStatuses = new Set<WorkerStatus>();
  if (statusHistory) {
    for (const entry of statusHistory) {
      reachedStatuses.add(entry.toStatus);
      if (entry.fromStatus) reachedStatuses.add(entry.fromStatus);
    }
  }
  reachedStatuses.add(currentStatus);

  // Find the current stage index in the lifecycle
  const currentStageIndex = LIFECYCLE_STAGES.indexOf(currentStatus);

  return (
    <div className="flex items-center gap-0 w-full overflow-x-auto py-2">
      {LIFECYCLE_STAGES.map((stage, index) => {
        const config = STATUS_CONFIG[stage];
        const Icon = config.icon;
        const isReached = reachedStatuses.has(stage);
        const isCurrent = stage === currentStatus;
        const isPast = currentStageIndex >= 0 && index < currentStageIndex;
        const isActive = isReached || isPast;

        return (
          <div key={stage} className="flex items-center flex-1 min-w-0">
            <div className="flex flex-col items-center gap-1 min-w-0">
              <div
                className={cn(
                  'flex h-9 w-9 shrink-0 items-center justify-center rounded-full border-2 transition-colors',
                  isCurrent && 'border-primary bg-primary text-primary-foreground',
                  isActive && !isCurrent && 'border-primary/60 bg-primary/10 text-primary',
                  !isActive && !isCurrent && 'border-muted-foreground/30 bg-muted text-muted-foreground/50',
                )}
              >
                {isActive && !isCurrent ? (
                  <Check className="h-4 w-4" />
                ) : (
                  <Icon className="h-4 w-4" />
                )}
              </div>
              <span
                className={cn(
                  'text-[10px] font-medium text-center leading-tight truncate max-w-[72px]',
                  isCurrent && 'text-primary font-semibold',
                  isActive && !isCurrent && 'text-primary/70',
                  !isActive && !isCurrent && 'text-muted-foreground/50',
                )}
              >
                {t(`lifecycle.stages.${stage}`)}
              </span>
            </div>
            {index < LIFECYCLE_STAGES.length - 1 && (
              <div
                className={cn(
                  'flex-1 h-0.5 mx-1 mt-[-18px]',
                  (isPast || (isActive && currentStageIndex > index))
                    ? 'bg-primary/40'
                    : 'bg-muted-foreground/20',
                )}
              />
            )}
          </div>
        );
      })}
    </div>
  );
}

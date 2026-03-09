import { ChevronRight } from 'lucide-react';
import { cn } from '@/shared/lib/cn';
import { Button } from '@/shared/components/ui/button';
import type { PlacementStatus, PlacementListDto } from '../types';

interface PlacementCardProps {
  placement: PlacementListDto;
  status: PlacementStatus;
  onAdvance: () => void;
  onClick: () => void;
}

function getDaysInStage(statusChangedAt: string): number {
  const changed = new Date(statusChangedAt);
  const now = new Date();
  return Math.floor((now.getTime() - changed.getTime()) / (1000 * 60 * 60 * 24));
}

function getDaysColor(days: number): string {
  if (days < 3) return 'text-green-600 bg-green-50';
  if (days <= 7) return 'text-amber-600 bg-amber-50';
  return 'text-red-600 bg-red-50';
}

export function PlacementCard({ placement, status, onAdvance, onClick }: PlacementCardProps) {
  const days = getDaysInStage(placement.statusChangedAt);
  const daysColor = getDaysColor(days);
  const isTerminal = status === 'Completed' || status === 'Cancelled';

  return (
    <div
      className="cursor-pointer rounded-lg border bg-card p-3 shadow-sm transition-shadow hover:shadow-md"
      onClick={onClick}
    >
      {/* Candidate info */}
      <div className="flex items-start gap-2">
        {placement.candidate?.photoUrl ? (
          <img
            src={placement.candidate.photoUrl}
            alt=""
            className="h-8 w-8 rounded-full object-cover"
          />
        ) : (
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary/10 text-xs font-medium text-primary">
            {placement.candidate?.fullNameEn?.[0] || '?'}
          </div>
        )}
        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-medium">
            {placement.candidate?.fullNameEn || 'Unknown'}
          </p>
          <p className="truncate text-xs text-muted-foreground">
            {placement.client?.nameEn || 'Unknown client'}
          </p>
        </div>
      </div>

      {/* Step progress */}
      {placement.currentStep > 0 && (
        <div className="mt-2">
          <div className="flex items-center gap-1.5">
            <div className="h-1.5 flex-1 overflow-hidden rounded-full bg-muted">
              <div
                className="h-full rounded-full bg-primary"
                style={{ width: `${((placement.currentStep - 1) / (placement.totalSteps || 9)) * 100}%` }}
              />
            </div>
            <span className="text-[10px] text-muted-foreground">{placement.currentStep}/{placement.totalSteps}</span>
          </div>
        </div>
      )}

      {/* Meta row */}
      <div className="mt-2 flex items-center gap-2 text-xs">
        <span className={cn('rounded px-1.5 py-0.5 font-medium', daysColor)}>
          {days}d
        </span>
        {placement.totalCost > 0 && (
          <span className="text-muted-foreground">
            AED {placement.totalCost.toLocaleString()}
          </span>
        )}
        {placement.expectedArrivalDate && (
          <span className="ml-auto text-muted-foreground">
            ETA: {new Date(placement.expectedArrivalDate).toLocaleDateString('en-GB', { month: 'short', day: 'numeric' })}
          </span>
        )}
      </div>

      {/* Advance button */}
      {!isTerminal && (
        <Button
          size="sm"
          variant="ghost"
          className="mt-2 h-7 w-full text-xs"
          onClick={(e) => {
            e.stopPropagation();
            onAdvance();
          }}
        >
          Advance <ChevronRight className="ml-1 h-3 w-3" />
        </Button>
      )}
    </div>
  );
}

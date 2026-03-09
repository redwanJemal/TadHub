import { cn } from '@/shared/lib/cn';
import { STATUS_CONFIG } from '../constants';
import { PlacementCard } from './PlacementCard';
import type { PlacementStatus, PlacementListDto } from '../types';

interface PlacementKanbanColumnProps {
  status: PlacementStatus;
  placements: PlacementListDto[];
  onAdvance: (placement: PlacementListDto) => void;
  onCardClick: (placement: PlacementListDto) => void;
}

export function PlacementKanbanColumn({ status, placements, onAdvance, onCardClick }: PlacementKanbanColumnProps) {
  const config = STATUS_CONFIG[status];
  const Icon = config?.icon;

  return (
    <div className="flex w-72 flex-shrink-0 flex-col rounded-lg border bg-muted/30">
      {/* Column header */}
      <div className="flex items-center gap-2 border-b px-3 py-2.5">
        {Icon && <Icon className="h-4 w-4 text-muted-foreground" />}
        <span className="text-sm font-medium">{config?.shortLabel || status}</span>
        <span className={cn(
          "ml-auto flex h-5 min-w-5 items-center justify-center rounded-full px-1.5 text-xs font-medium",
          placements.length > 0 ? "bg-primary text-primary-foreground" : "bg-muted text-muted-foreground"
        )}>
          {placements.length}
        </span>
      </div>

      {/* Cards */}
      <div className="flex-1 space-y-2 overflow-y-auto p-2">
        {placements.length === 0 && (
          <div className="py-8 text-center text-xs text-muted-foreground">
            No placements
          </div>
        )}
        {placements.map((p) => (
          <PlacementCard
            key={p.id}
            placement={p}
            status={status}
            onAdvance={() => onAdvance(p)}
            onClick={() => onCardClick(p)}
          />
        ))}
      </div>
    </div>
  );
}

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, RefreshCw } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { PlacementKanbanColumn } from '../components/PlacementKanbanColumn';
import { PlacementTransitionDialog } from '../components/PlacementTransitionDialog';
import { usePlacementBoard, useTransitionPlacementStatus } from '../hooks';
import { OUTSIDE_COUNTRY_PIPELINE, INSIDE_COUNTRY_PIPELINE, ALL_PIPELINE_STATUSES, STATUS_CONFIG } from '../constants';
import type { PlacementListDto } from '../types';

type BoardView = 'all' | 'outside' | 'inside';

function BoardSkeleton() {
  return (
    <div className="flex-1 overflow-x-auto p-4">
      <div className="flex gap-3" style={{ minWidth: 'max-content' }}>
        {OUTSIDE_COUNTRY_PIPELINE.map((status) => {
          const config = STATUS_CONFIG[status];
          const Icon = config.icon;
          return (
            <div key={status} className="w-72 shrink-0">
              <div className="rounded-lg border bg-muted/30 p-3">
                <div className="mb-3 flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Icon className="h-4 w-4 text-muted-foreground" />
                    <Skeleton className="h-4 w-20" />
                  </div>
                  <Skeleton className="h-5 w-5 rounded-full" />
                </div>
                <div className="space-y-2">
                  {Array.from({ length: Math.floor(Math.random() * 2) + 1 }).map((_, i) => (
                    <div key={i} className="rounded-lg border bg-card p-3 shadow-sm">
                      <div className="flex items-start gap-2">
                        <Skeleton className="h-8 w-8 rounded-full" />
                        <div className="flex-1 space-y-1">
                          <Skeleton className="h-4 w-28" />
                          <Skeleton className="h-3 w-20" />
                        </div>
                      </div>
                      <div className="mt-2 flex items-center gap-2">
                        <Skeleton className="h-5 w-8 rounded" />
                        <Skeleton className="h-3 w-16" />
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export function PlacementBoardPage() {
  const navigate = useNavigate();
  const { data: board, isLoading, isError, error, refetch } = usePlacementBoard();
  const transitionMutation = useTransitionPlacementStatus();

  const [transitionTarget, setTransitionTarget] = useState<PlacementListDto | null>(null);
  const [boardView, setBoardView] = useState<BoardView>('all');

  const handleAdvance = (placement: PlacementListDto) => {
    setTransitionTarget(placement);
  };

  const handleTransition = (status: string, reason?: string, notes?: string) => {
    if (!transitionTarget) return;
    transitionMutation.mutate(
      { id: transitionTarget.id, data: { status, reason, notes } },
      {
        onSuccess: () => {
          setTransitionTarget(null);
        },
      }
    );
  };

  const getVisibleStatuses = () => {
    switch (boardView) {
      case 'outside': return OUTSIDE_COUNTRY_PIPELINE;
      case 'inside': return INSIDE_COUNTRY_PIPELINE;
      default: return ALL_PIPELINE_STATUSES;
    }
  };

  const getPlacementsForColumn = (status: string) => {
    const items = board?.columns?.[status] || [];
    if (boardView === 'all') return items;
    return items.filter((p) =>
      boardView === 'inside'
        ? p.flowType === 'InsideCountry'
        : p.flowType === 'OutsideCountry'
    );
  };

  const visibleStatuses = getVisibleStatuses();

  return (
    <div className="flex h-full flex-col">
      {/* Header */}
      <div className="flex items-center justify-between border-b px-6 py-4">
        <div>
          <h1 className="text-2xl font-semibold">Placement Pipeline</h1>
          <p className="text-sm text-muted-foreground">
            Track candidates through the booking-to-placement pipeline
          </p>
        </div>
        <div className="flex items-center gap-2">
          <div className="flex items-center rounded-lg border p-0.5">
            {(['all', 'outside', 'inside'] as const).map((view) => (
              <button
                key={view}
                onClick={() => setBoardView(view)}
                className={`rounded-md px-3 py-1.5 text-xs font-medium transition-colors ${
                  boardView === view
                    ? 'bg-primary text-primary-foreground'
                    : 'text-muted-foreground hover:text-foreground'
                }`}
              >
                {view === 'all' ? 'All' : view === 'outside' ? 'Outside Country' : 'Inside Country'}
              </button>
            ))}
          </div>
          <Button variant="outline" size="sm" onClick={() => refetch()}>
            <RefreshCw className="mr-2 h-4 w-4" />
            Refresh
          </Button>
          <PermissionGate permission="placements.create">
            <Button size="sm" onClick={() => navigate('/placements/new')}>
              <Plus className="mr-2 h-4 w-4" />
              Book Candidate
            </Button>
          </PermissionGate>
        </div>
      </div>

      {/* Board */}
      {isError ? (
        <div className="p-6 text-center text-destructive">
          <p className="font-medium">Failed to load board</p>
          <p className="mt-1 text-sm text-muted-foreground">{(error as Error)?.message || 'Unknown error'}</p>
          <Button variant="outline" size="sm" className="mt-3" onClick={() => refetch()}>Retry</Button>
        </div>
      ) : isLoading ? (
        <BoardSkeleton />
      ) : (
        <div className="flex-1 overflow-x-auto p-4">
          <div className="flex gap-3" style={{ minWidth: 'max-content' }}>
            {visibleStatuses.map((status) => (
              <PlacementKanbanColumn
                key={status}
                status={status}
                placements={getPlacementsForColumn(status)}
                onAdvance={handleAdvance}
                onCardClick={(p) => navigate(`/placements/${p.id}`)}
              />
            ))}
          </div>
        </div>
      )}

      {/* Transition dialog */}
      {transitionTarget && (
        <PlacementTransitionDialog
          open={!!transitionTarget}
          onOpenChange={(open) => !open && setTransitionTarget(null)}
          currentStatus={transitionTarget.status}
          onTransition={handleTransition}
          isPending={transitionMutation.isPending}
        />
      )}
    </div>
  );
}

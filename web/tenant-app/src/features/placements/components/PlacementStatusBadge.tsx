import { Badge } from '@/shared/components/ui/badge';
import { STATUS_CONFIG } from '../constants';
import type { PlacementStatus } from '../types';

interface PlacementStatusBadgeProps {
  status: PlacementStatus;
  showIcon?: boolean;
}

export function PlacementStatusBadge({ status, showIcon = true }: PlacementStatusBadgeProps) {
  const config = STATUS_CONFIG[status];
  if (!config) return <Badge variant="outline">{status}</Badge>;

  const Icon = config.icon;

  return (
    <Badge variant={config.variant} className="gap-1">
      {showIcon && <Icon className="h-3 w-3" />}
      {config.label}
    </Badge>
  );
}

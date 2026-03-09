import { Badge } from '@/shared/components/ui/badge';
import { STATUS_CONFIG } from '../constants';
import type { ArrivalStatus } from '../types';

interface ArrivalStatusBadgeProps {
  status: ArrivalStatus;
  showIcon?: boolean;
}

export function ArrivalStatusBadge({ status, showIcon = true }: ArrivalStatusBadgeProps) {
  const config = STATUS_CONFIG[status];
  if (!config) {
    return <Badge variant="outline">{status}</Badge>;
  }

  const Icon = config.icon;
  return (
    <Badge variant={config.variant}>
      {showIcon && <Icon className="mr-1 h-3 w-3" />}
      {config.label}
    </Badge>
  );
}

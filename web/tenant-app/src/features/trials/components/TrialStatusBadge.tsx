import { Badge } from '@/shared/components/ui/badge';
import { STATUS_CONFIG } from '../constants';
import type { TrialStatus } from '../types';

interface TrialStatusBadgeProps {
  status: TrialStatus;
  showIcon?: boolean;
}

export function TrialStatusBadge({ status, showIcon = true }: TrialStatusBadgeProps) {
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

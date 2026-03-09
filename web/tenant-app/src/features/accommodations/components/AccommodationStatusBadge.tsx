import { Badge } from '@/shared/components/ui/badge';
import { STATUS_CONFIG } from '../constants';
import type { AccommodationStayStatus } from '../types';

interface Props {
  status: AccommodationStayStatus;
  size?: 'sm' | 'md';
}

export function AccommodationStatusBadge({ status, size = 'md' }: Props) {
  const config = STATUS_CONFIG[status];
  if (!config) return <Badge variant="outline">{status}</Badge>;
  const Icon = config.icon;

  return (
    <Badge variant={config.variant} className="gap-1">
      <Icon className={size === 'sm' ? 'h-3 w-3' : 'h-4 w-4'} />
      {size === 'sm' ? config.shortLabel : config.label}
    </Badge>
  );
}

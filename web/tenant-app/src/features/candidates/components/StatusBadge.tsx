import { useTranslation } from 'react-i18next';
import { Badge } from '@/shared/components/ui/badge';
import { STATUS_CONFIG } from '../constants';
import type { CandidateStatus } from '../types';

interface StatusBadgeProps {
  status: CandidateStatus;
  showIcon?: boolean;
  className?: string;
}

export function StatusBadge({ status, showIcon = true, className }: StatusBadgeProps) {
  const { t } = useTranslation('candidates');
  const config = STATUS_CONFIG[status];

  if (!config) {
    return <Badge variant="outline">{status}</Badge>;
  }

  const Icon = config.icon;

  return (
    <Badge variant={config.variant} className={className}>
      {showIcon && <Icon className="me-1 h-3 w-3" />}
      {t(`status.${status}`)}
    </Badge>
  );
}

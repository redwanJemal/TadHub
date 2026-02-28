import { useTranslation } from 'react-i18next';
import { Badge } from '@/shared/components/ui/badge';
import { EFFECTIVE_STATUS_CONFIG } from '../constants';
import type { EffectiveStatus } from '../types';

interface DocumentStatusBadgeProps {
  status: EffectiveStatus;
}

export function DocumentStatusBadge({ status }: DocumentStatusBadgeProps) {
  const { t } = useTranslation('documents');
  const config = EFFECTIVE_STATUS_CONFIG[status];

  if (!config) {
    return <Badge variant="outline">{status}</Badge>;
  }

  const Icon = config.icon;

  return (
    <Badge variant={config.variant as any}>
      <Icon className="me-1 h-3 w-3" />
      {t(`effectiveStatus.${status}`)}
    </Badge>
  );
}

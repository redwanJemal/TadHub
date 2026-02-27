import { useTranslation } from 'react-i18next';
import { Badge } from '@/shared/components/ui/badge';
import { STATUS_CONFIG } from '../constants';
import type { ContractStatus } from '../types';

interface ContractStatusBadgeProps {
  status: ContractStatus;
}

export function ContractStatusBadge({ status }: ContractStatusBadgeProps) {
  const { t } = useTranslation('contracts');
  const config = STATUS_CONFIG[status];

  if (!config) {
    return <Badge variant="outline">{status}</Badge>;
  }

  const Icon = config.icon;

  return (
    <Badge variant={config.variant as any}>
      <Icon className="me-1 h-3 w-3" />
      {t(`status.${status}`)}
    </Badge>
  );
}

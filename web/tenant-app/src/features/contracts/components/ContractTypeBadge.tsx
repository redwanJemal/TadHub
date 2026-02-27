import { useTranslation } from 'react-i18next';
import { Badge } from '@/shared/components/ui/badge';
import { TYPE_CONFIG } from '../constants';
import type { ContractType } from '../types';

interface ContractTypeBadgeProps {
  type: ContractType;
}

export function ContractTypeBadge({ type }: ContractTypeBadgeProps) {
  const { t } = useTranslation('contracts');
  const config = TYPE_CONFIG[type];

  if (!config) {
    return <Badge variant="outline">{type}</Badge>;
  }

  const Icon = config.icon;

  return (
    <Badge variant={config.variant as any}>
      <Icon className="me-1 h-3 w-3" />
      {t(`type.${type}`)}
    </Badge>
  );
}

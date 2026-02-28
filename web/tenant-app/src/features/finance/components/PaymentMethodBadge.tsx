import { cn } from '@/shared/lib/cn';
import { Badge } from '@/shared/components/ui/badge';
import type { PaymentMethod } from '../types';

interface PaymentMethodBadgeProps {
  method: PaymentMethod | string;
}

const METHOD_CLASSES: Record<string, string> = {
  Cash:         'bg-emerald-100 text-emerald-800 dark:bg-emerald-900 dark:text-emerald-300',
  Card:         'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
  BankTransfer: 'bg-indigo-100 text-indigo-800 dark:bg-indigo-900 dark:text-indigo-300',
  Cheque:       'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-300',
  EDirham:      'bg-teal-100 text-teal-800 dark:bg-teal-900 dark:text-teal-300',
  Online:       'bg-violet-100 text-violet-800 dark:bg-violet-900 dark:text-violet-300',
};

const METHOD_LABELS: Record<string, string> = {
  Cash:         'Cash',
  Card:         'Card',
  BankTransfer: 'Bank Transfer',
  Cheque:       'Cheque',
  EDirham:      'E-Dirham',
  Online:       'Online',
};

export function PaymentMethodBadge({ method }: PaymentMethodBadgeProps) {
  const classes = METHOD_CLASSES[method];

  if (!classes) {
    return <Badge variant="outline">{method}</Badge>;
  }

  return (
    <Badge
      variant="outline"
      className={cn('border-transparent', classes)}
    >
      {METHOD_LABELS[method] ?? method}
    </Badge>
  );
}

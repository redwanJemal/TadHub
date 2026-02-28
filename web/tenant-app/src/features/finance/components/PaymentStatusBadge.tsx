import { cn } from '@/shared/lib/cn';
import { Badge } from '@/shared/components/ui/badge';
import type { PaymentStatus } from '../types';

interface PaymentStatusBadgeProps {
  status: PaymentStatus;
}

const STATUS_CLASSES: Record<PaymentStatus, string> = {
  Pending:   'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300',
  Completed: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
  Failed:    'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300',
  Refunded:  'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300',
  Cancelled: 'bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-400',
};

const STATUS_LABELS: Record<PaymentStatus, string> = {
  Pending:   'Pending',
  Completed: 'Completed',
  Failed:    'Failed',
  Refunded:  'Refunded',
  Cancelled: 'Cancelled',
};

export function PaymentStatusBadge({ status }: PaymentStatusBadgeProps) {
  const classes = STATUS_CLASSES[status];

  if (!classes) {
    return <Badge variant="outline">{status}</Badge>;
  }

  return (
    <Badge
      variant="outline"
      className={cn('border-transparent', classes)}
    >
      {STATUS_LABELS[status] ?? status}
    </Badge>
  );
}

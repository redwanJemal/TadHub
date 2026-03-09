import { cn } from '@/shared/lib/cn';
import { Badge } from '@/shared/components/ui/badge';
import type { SupplierDebitStatus } from '../types';

interface SupplierDebitStatusBadgeProps {
  status: SupplierDebitStatus;
}

const STATUS_CLASSES: Record<SupplierDebitStatus, string> = {
  Outstanding:   'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300',
  PartiallyPaid: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300',
  Settled:       'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
  Waived:        'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
  Cancelled:     'bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-400',
};

const STATUS_LABELS: Record<SupplierDebitStatus, string> = {
  Outstanding:   'Outstanding',
  PartiallyPaid: 'Partially Paid',
  Settled:       'Settled',
  Waived:        'Waived',
  Cancelled:     'Cancelled',
};

export function SupplierDebitStatusBadge({ status }: SupplierDebitStatusBadgeProps) {
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

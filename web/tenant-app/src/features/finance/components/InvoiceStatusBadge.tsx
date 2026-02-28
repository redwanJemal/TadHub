import { cn } from '@/shared/lib/cn';
import { Badge } from '@/shared/components/ui/badge';
import type { InvoiceStatus } from '../types';

interface InvoiceStatusBadgeProps {
  status: InvoiceStatus;
}

const STATUS_CLASSES: Record<InvoiceStatus, string> = {
  Draft:         'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300',
  Issued:        'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
  PartiallyPaid: 'bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-300',
  Paid:          'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
  Overdue:       'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300',
  Cancelled:     'bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-400',
  Refunded:      'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300',
};

const STATUS_LABELS: Record<InvoiceStatus, string> = {
  Draft:         'Draft',
  Issued:        'Issued',
  PartiallyPaid: 'Partially Paid',
  Paid:          'Paid',
  Overdue:       'Overdue',
  Cancelled:     'Cancelled',
  Refunded:      'Refunded',
};

export function InvoiceStatusBadge({ status }: InvoiceStatusBadgeProps) {
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

import {
  AlertTriangle,
  Search,
  CheckCircle2,
  Banknote,
  Lock,
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import type { RunawayCaseStatus, RunawayExpenseType, PaidByParty } from './types';

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'outline' | 'success' | 'warning';

interface StatusConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
  label: string;
}

export const STATUS_CONFIG: Record<RunawayCaseStatus, StatusConfig> = {
  Reported:           { variant: 'destructive', icon: AlertTriangle, label: 'Reported' },
  UnderInvestigation: { variant: 'warning',     icon: Search,       label: 'Under Investigation' },
  Confirmed:          { variant: 'secondary',   icon: CheckCircle2, label: 'Confirmed' },
  Settled:            { variant: 'success',     icon: Banknote,     label: 'Settled' },
  Closed:             { variant: 'default',     icon: Lock,         label: 'Closed' },
};

export const ALL_STATUSES: RunawayCaseStatus[] = ['Reported', 'UnderInvestigation', 'Confirmed', 'Settled', 'Closed'];

export const EXPENSE_TYPES: { value: RunawayExpenseType; label: string }[] = [
  { value: 'CommissionRefund', label: 'Commission Refund' },
  { value: 'VisaCost', label: 'Visa Cost' },
  { value: 'MedicalCost', label: 'Medical Cost' },
  { value: 'TransportationCost', label: 'Transportation Cost' },
  { value: 'Other', label: 'Other' },
];

export const PAID_BY_OPTIONS: { value: PaidByParty; label: string }[] = [
  { value: 'Office', label: 'Office' },
  { value: 'Supplier', label: 'Supplier' },
  { value: 'Client', label: 'Client' },
];

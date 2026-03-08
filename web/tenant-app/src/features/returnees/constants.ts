import {
  FileText,
  Search,
  CheckCircle2,
  XCircle,
  Banknote,
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import type { ReturneeCaseStatus, ReturnType, ExpenseType, PaidByParty } from './types';

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'outline' | 'success' | 'warning';

interface StatusConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
  label: string;
}

export const STATUS_CONFIG: Record<ReturneeCaseStatus, StatusConfig> = {
  Submitted:   { variant: 'secondary', icon: FileText,     label: 'Submitted' },
  UnderReview: { variant: 'warning',   icon: Search,       label: 'Under Review' },
  Approved:    { variant: 'success',   icon: CheckCircle2, label: 'Approved' },
  Rejected:    { variant: 'destructive', icon: XCircle,    label: 'Rejected' },
  Settled:     { variant: 'default',   icon: Banknote,     label: 'Settled' },
};

export const ALL_STATUSES: ReturneeCaseStatus[] = ['Submitted', 'UnderReview', 'Approved', 'Rejected', 'Settled'];

export const RETURN_TYPES: { value: ReturnType; label: string }[] = [
  { value: 'ReturnToOffice', label: 'Return to Office' },
  { value: 'ReturnToCountry', label: 'Return to Country' },
];

export const EXPENSE_TYPES: { value: ExpenseType; label: string }[] = [
  { value: 'VisaCost', label: 'Visa Cost' },
  { value: 'TicketCost', label: 'Ticket Cost' },
  { value: 'MedicalCost', label: 'Medical Cost' },
  { value: 'TransportationCost', label: 'Transportation Cost' },
  { value: 'AccommodationCost', label: 'Accommodation Cost' },
  { value: 'Other', label: 'Other' },
];

export const PAID_BY_OPTIONS: { value: PaidByParty; label: string }[] = [
  { value: 'Office', label: 'Office' },
  { value: 'Supplier', label: 'Supplier' },
  { value: 'Client', label: 'Client' },
];

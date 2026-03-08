import {
  Play,
  CheckCircle2,
  XCircle,
  Ban,
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import type { TrialStatus } from './types';

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'outline' | 'success' | 'warning';

interface StatusConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
  label: string;
}

export const STATUS_CONFIG: Record<TrialStatus, StatusConfig> = {
  Active:     { variant: 'warning',     icon: Play,         label: 'Active' },
  Successful: { variant: 'success',     icon: CheckCircle2, label: 'Successful' },
  Failed:     { variant: 'destructive', icon: XCircle,      label: 'Failed' },
  Cancelled:  { variant: 'secondary',   icon: Ban,          label: 'Cancelled' },
};

export const ALL_STATUSES: TrialStatus[] = ['Active', 'Successful', 'Failed', 'Cancelled'];

export const OUTCOME_OPTIONS = [
  { value: 'ProceedToContract' as const, label: 'Proceed to Contract' },
  { value: 'ReturnToInventory' as const, label: 'Return to Inventory' },
];

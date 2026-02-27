import type { LucideIcon } from 'lucide-react';
import {
  FileEdit, CheckCircle, Shield, Briefcase, CheckCheck,
  XCircle, Ban, Lock, Clock, Zap,
} from 'lucide-react';
import type { ContractStatus, ContractType } from './types';

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'success' | 'warning' | 'outline';

interface StatusConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
}

export const STATUS_CONFIG: Record<ContractStatus, StatusConfig> = {
  Draft:       { variant: 'secondary',   icon: FileEdit },
  Confirmed:   { variant: 'default',     icon: CheckCircle },
  OnProbation: { variant: 'warning',     icon: Shield },
  Active:      { variant: 'success',     icon: Briefcase },
  Completed:   { variant: 'default',     icon: CheckCheck },
  Terminated:  { variant: 'destructive', icon: XCircle },
  Cancelled:   { variant: 'destructive', icon: Ban },
  Closed:      { variant: 'secondary',   icon: Lock },
};

export const ALL_STATUSES: ContractStatus[] = [
  'Draft', 'Confirmed', 'OnProbation', 'Active', 'Completed', 'Terminated', 'Cancelled', 'Closed',
];

export const ALL_TYPES: ContractType[] = ['Traditional', 'Temporary', 'Flexible'];

interface TypeConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
}

export const TYPE_CONFIG: Record<ContractType, TypeConfig> = {
  Traditional: { variant: 'default',   icon: FileEdit },
  Temporary:   { variant: 'warning',   icon: Clock },
  Flexible:    { variant: 'outline',   icon: Zap },
};

export const ALLOWED_TRANSITIONS: Partial<Record<ContractStatus, ContractStatus[]>> = {
  Draft:       ['Confirmed', 'Cancelled'],
  Confirmed:   ['OnProbation', 'Active', 'Cancelled'],
  OnProbation: ['Active', 'Terminated', 'Cancelled'],
  Active:      ['Completed', 'Terminated'],
  Completed:   ['Closed'],
  Terminated:  ['Closed'],
};

export const REASON_REQUIRED_STATUSES: ContractStatus[] = [
  'Terminated', 'Cancelled',
];

export const ALL_RATE_PERIODS = ['Monthly', 'Daily', 'Hourly'] as const;

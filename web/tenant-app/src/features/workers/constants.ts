import type { LucideIcon } from 'lucide-react';
import { CheckCircle, Briefcase, PalmtreeIcon, XCircle } from 'lucide-react';
import type { WorkerStatus, WorkerSourceType } from './types';

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'success' | 'warning' | 'outline';

interface StatusConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
}

export const STATUS_CONFIG: Record<WorkerStatus, StatusConfig> = {
  Active: { variant: 'success', icon: CheckCircle },
  Deployed: { variant: 'default', icon: Briefcase },
  OnLeave: { variant: 'warning', icon: PalmtreeIcon },
  Terminated: { variant: 'destructive', icon: XCircle },
};

export const ALL_STATUSES: WorkerStatus[] = [
  'Active',
  'Deployed',
  'OnLeave',
  'Terminated',
];

export const ALL_SOURCE_TYPES: WorkerSourceType[] = ['Supplier', 'Local'];

/** Allowed manual status transitions */
export const ALLOWED_TRANSITIONS: Partial<Record<WorkerStatus, WorkerStatus[]>> = {
  Active: ['Deployed', 'OnLeave', 'Terminated'],
  Deployed: ['Active', 'OnLeave', 'Terminated'],
  OnLeave: ['Active', 'Deployed', 'Terminated'],
  // Terminated is terminal
};

/** Statuses that require a reason */
export const REASON_REQUIRED_STATUSES: WorkerStatus[] = ['Terminated', 'OnLeave'];

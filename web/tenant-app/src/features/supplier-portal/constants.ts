import {
  Clock,
  CheckCircle,
  XCircle,
  UserCheck,
  Briefcase,
  AlertCircle,
  type LucideIcon,
} from 'lucide-react';

export interface StatusConfig {
  variant: 'default' | 'secondary' | 'destructive' | 'outline' | 'success' | 'warning';
  icon: LucideIcon;
  label: string;
}

export const CANDIDATE_STATUS_CONFIG: Record<string, StatusConfig> = {
  Registered: { variant: 'secondary', icon: Clock, label: 'Registered' },
  UnderReview: { variant: 'warning', icon: AlertCircle, label: 'Under Review' },
  Approved: { variant: 'success', icon: CheckCircle, label: 'Approved' },
  Rejected: { variant: 'destructive', icon: XCircle, label: 'Rejected' },
  OnHold: { variant: 'outline', icon: Clock, label: 'On Hold' },
};

export const WORKER_STATUS_CONFIG: Record<string, StatusConfig> = {
  Active: { variant: 'success', icon: UserCheck, label: 'Active' },
  OnContract: { variant: 'default', icon: Briefcase, label: 'On Contract' },
  Deployed: { variant: 'default', icon: Briefcase, label: 'Deployed' },
  Available: { variant: 'secondary', icon: UserCheck, label: 'Available' },
  Inactive: { variant: 'outline', icon: XCircle, label: 'Inactive' },
};

export const COMMISSION_STATUS_CONFIG: Record<string, StatusConfig> = {
  Pending: { variant: 'warning', icon: Clock, label: 'Pending' },
  Approved: { variant: 'secondary', icon: CheckCircle, label: 'Approved' },
  Paid: { variant: 'success', icon: CheckCircle, label: 'Paid' },
  Cancelled: { variant: 'destructive', icon: XCircle, label: 'Cancelled' },
};

export const ARRIVAL_STATUS_CONFIG: Record<string, StatusConfig> = {
  Scheduled: { variant: 'secondary', icon: Clock, label: 'Scheduled' },
  InTransit: { variant: 'warning', icon: AlertCircle, label: 'In Transit' },
  Arrived: { variant: 'success', icon: CheckCircle, label: 'Arrived' },
  PickedUp: { variant: 'default', icon: UserCheck, label: 'Picked Up' },
  Delivered: { variant: 'success', icon: CheckCircle, label: 'Delivered' },
  NoShow: { variant: 'destructive', icon: XCircle, label: 'No Show' },
  Cancelled: { variant: 'destructive', icon: XCircle, label: 'Cancelled' },
};

export const ALL_CANDIDATE_STATUSES = ['Registered', 'UnderReview', 'Approved', 'Rejected', 'OnHold'];
export const ALL_COMMISSION_STATUSES = ['Pending', 'Approved', 'Paid', 'Cancelled'];

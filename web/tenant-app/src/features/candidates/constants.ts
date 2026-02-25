import type { LucideIcon } from 'lucide-react';
import {
  Clock,
  Search,
  CheckCircle,
  XCircle,
  CreditCard,
  Plane,
  MapPin,
  UserCheck,
  Ban,
  HeartPulse,
  FileX,
  Undo2,
} from 'lucide-react';
import type { CandidateStatus, CandidateSourceType } from './types';

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'success' | 'warning' | 'outline';

interface StatusConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
}

export const STATUS_CONFIG: Record<CandidateStatus, StatusConfig> = {
  Received: { variant: 'secondary', icon: Clock },
  UnderReview: { variant: 'warning', icon: Search },
  Approved: { variant: 'success', icon: CheckCircle },
  Rejected: { variant: 'destructive', icon: XCircle },
  ProcurementPaid: { variant: 'default', icon: CreditCard },
  InTransit: { variant: 'default', icon: Plane },
  Arrived: { variant: 'success', icon: MapPin },
  Converted: { variant: 'success', icon: UserCheck },
  Cancelled: { variant: 'secondary', icon: Ban },
  FailedMedicalAbroad: { variant: 'destructive', icon: HeartPulse },
  VisaDenied: { variant: 'destructive', icon: FileX },
  ReturnedAfterArrival: { variant: 'warning', icon: Undo2 },
};

export const ALL_STATUSES: CandidateStatus[] = [
  'Received',
  'UnderReview',
  'Approved',
  'Rejected',
  'ProcurementPaid',
  'InTransit',
  'Arrived',
  'Converted',
  'Cancelled',
  'FailedMedicalAbroad',
  'VisaDenied',
  'ReturnedAfterArrival',
];

export const ALL_SOURCE_TYPES: CandidateSourceType[] = ['Supplier', 'Local'];

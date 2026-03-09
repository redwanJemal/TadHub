import {
  LogIn,
  LogOut,
  Truck,
  AlertTriangle,
  Plane,
  ArrowLeftRight,
  Stethoscope,
  MoreHorizontal,
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import type { AccommodationStayStatus, DepartureReason } from './types';

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'outline' | 'success' | 'warning';

interface StatusConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
  label: string;
  shortLabel: string;
}

export const STATUS_CONFIG: Record<AccommodationStayStatus, StatusConfig> = {
  CheckedIn:  { variant: 'success',   icon: LogIn,  label: 'Checked In',  shortLabel: 'In' },
  CheckedOut: { variant: 'secondary', icon: LogOut,  label: 'Checked Out', shortLabel: 'Out' },
};

export const ALL_STATUSES: AccommodationStayStatus[] = ['CheckedIn', 'CheckedOut'];

interface DepartureReasonConfig {
  label: string;
  icon: LucideIcon;
}

export const DEPARTURE_REASON_CONFIG: Record<DepartureReason, DepartureReasonConfig> = {
  DeployedToCustomer: { label: 'Deployed to Customer', icon: Truck },
  Runaway:            { label: 'Runaway',               icon: AlertTriangle },
  ReturnedToCountry:  { label: 'Returned to Country',   icon: Plane },
  Transferred:        { label: 'Transferred',           icon: ArrowLeftRight },
  MedicalReason:      { label: 'Medical Reason',        icon: Stethoscope },
  Other:              { label: 'Other',                  icon: MoreHorizontal },
};

export const ALL_DEPARTURE_REASONS: DepartureReason[] = [
  'DeployedToCustomer',
  'Runaway',
  'ReturnedToCountry',
  'Transferred',
  'MedicalReason',
  'Other',
];

import {
  Calendar,
  Navigation,
  MapPin,
  Truck,
  Building2,
  AlertTriangle,
  XCircle,
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import type { ArrivalStatus } from './types';

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'outline' | 'success' | 'warning';

interface StatusConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
  label: string;
  shortLabel: string;
}

export const STATUS_CONFIG: Record<ArrivalStatus, StatusConfig> = {
  Scheduled:       { variant: 'secondary', icon: Calendar,      label: 'Scheduled',          shortLabel: 'Scheduled' },
  InTransit:       { variant: 'warning',   icon: Navigation,    label: 'In Transit',         shortLabel: 'Transit' },
  Arrived:         { variant: 'default',   icon: MapPin,        label: 'Arrived',            shortLabel: 'Arrived' },
  PickedUp:        { variant: 'default',   icon: Truck,         label: 'Picked Up',          shortLabel: 'Picked Up' },
  AtAccommodation: { variant: 'success',   icon: Building2,     label: 'At Accommodation',   shortLabel: 'At Accom.' },
  NoShow:          { variant: 'destructive',icon: AlertTriangle, label: 'No Show',            shortLabel: 'No Show' },
  Cancelled:       { variant: 'destructive',icon: XCircle,       label: 'Cancelled',          shortLabel: 'Cancelled' },
};

export const ALL_STATUSES: ArrivalStatus[] = [
  'Scheduled',
  'InTransit',
  'Arrived',
  'PickedUp',
  'AtAccommodation',
  'NoShow',
  'Cancelled',
];

export const ACTIVE_STATUSES: ArrivalStatus[] = [
  'Scheduled',
  'InTransit',
  'Arrived',
  'PickedUp',
];

export const TERMINAL_STATUSES: ArrivalStatus[] = [
  'AtAccommodation',
  'NoShow',
  'Cancelled',
];

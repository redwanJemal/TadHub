import type { LucideIcon } from 'lucide-react';
import {
  CheckCircle, Briefcase, XCircle, GraduationCap, Stethoscope,
  Plane, BookOpen, UserCheck, Shield, RefreshCw, ArrowRightLeft,
  HeartPulse, AlertTriangle, Baby, Home, Ban, Skull, Clock,
  Globe, MapPin,
} from 'lucide-react';
import type { WorkerStatus, WorkerLocation, WorkerSourceType, WorkerStatusCategory } from './types';

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'success' | 'warning' | 'outline';

interface StatusConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
  category: WorkerStatusCategory;
}

export const STATUS_CONFIG: Record<WorkerStatus, StatusConfig> = {
  // Pool
  Available:        { variant: 'success',     icon: CheckCircle,    category: 'Pool' },
  InTraining:       { variant: 'default',     icon: GraduationCap,  category: 'Pool' },
  UnderMedicalTest: { variant: 'default',     icon: Stethoscope,    category: 'Pool' },
  // Arrival
  NewArrival:       { variant: 'default',     icon: Plane,          category: 'Arrival' },
  // Placement
  Booked:           { variant: 'default',     icon: BookOpen,       category: 'Placement' },
  Hired:            { variant: 'default',     icon: UserCheck,      category: 'Placement' },
  OnProbation:      { variant: 'warning',     icon: Shield,         category: 'Placement' },
  Active:           { variant: 'success',     icon: Briefcase,      category: 'Placement' },
  Renewed:          { variant: 'success',     icon: RefreshCw,      category: 'Placement' },
  // Negative / Special
  PendingReplacement: { variant: 'warning',   icon: Clock,          category: 'NegativeSpecial' },
  Transferred:      { variant: 'secondary',   icon: ArrowRightLeft, category: 'NegativeSpecial' },
  MedicallyUnfit:   { variant: 'destructive', icon: HeartPulse,     category: 'NegativeSpecial' },
  Absconded:        { variant: 'destructive', icon: AlertTriangle,  category: 'NegativeSpecial' },
  Terminated:       { variant: 'destructive', icon: XCircle,        category: 'NegativeSpecial' },
  Pregnant:         { variant: 'warning',     icon: Baby,           category: 'NegativeSpecial' },
  // Terminal
  Repatriated:      { variant: 'secondary',   icon: Home,           category: 'Terminal' },
  Deported:         { variant: 'destructive', icon: Ban,            category: 'Terminal' },
  Deceased:         { variant: 'destructive', icon: Skull,          category: 'Terminal' },
};

export const ALL_STATUSES: WorkerStatus[] = [
  'Available', 'InTraining', 'UnderMedicalTest',
  'NewArrival',
  'Booked', 'Hired', 'OnProbation', 'Active', 'Renewed',
  'PendingReplacement', 'Transferred', 'MedicallyUnfit', 'Absconded', 'Terminated', 'Pregnant',
  'Repatriated', 'Deported', 'Deceased',
];

export const ALL_LOCATIONS: WorkerLocation[] = ['Abroad', 'InCountry'];

export const ALL_SOURCE_TYPES: WorkerSourceType[] = ['Supplier', 'Local'];

/** Allowed manual status transitions */
export const ALLOWED_TRANSITIONS: Partial<Record<WorkerStatus, WorkerStatus[]>> = {
  Available:          ['Booked', 'UnderMedicalTest', 'InTraining', 'Absconded', 'Repatriated', 'Deceased'],
  InTraining:         ['Available', 'UnderMedicalTest', 'Absconded', 'Repatriated', 'Deceased'],
  UnderMedicalTest:   ['Available', 'MedicallyUnfit', 'Deceased'],
  NewArrival:         ['Available', 'InTraining', 'UnderMedicalTest', 'MedicallyUnfit', 'Absconded', 'Repatriated', 'Deceased'],
  Booked:             ['Hired', 'NewArrival', 'Available', 'Deceased'],
  Hired:              ['OnProbation', 'Available', 'Deceased'],
  OnProbation:        ['Active', 'PendingReplacement', 'Terminated', 'Absconded', 'Pregnant', 'Deceased'],
  Active:             ['Renewed', 'PendingReplacement', 'Terminated', 'Absconded', 'Pregnant', 'Transferred', 'Deceased'],
  Renewed:            ['Active', 'PendingReplacement', 'Terminated', 'Absconded', 'Pregnant', 'Transferred', 'Deceased'],
  PendingReplacement: ['Available', 'Terminated', 'Repatriated', 'Deceased'],
  Transferred:        ['Repatriated'],
  MedicallyUnfit:     ['Repatriated', 'Available', 'Deceased'],
  Absconded:          ['Terminated', 'Repatriated', 'Deported', 'Available', 'Deceased'],
  Terminated:         ['Available', 'Repatriated', 'Transferred'],
  Pregnant:           ['Active', 'Terminated', 'Repatriated', 'Deceased'],
  // Terminal states have no transitions
};

/** Statuses that require a reason */
export const REASON_REQUIRED_STATUSES: WorkerStatus[] = [
  'Terminated', 'Absconded', 'MedicallyUnfit', 'PendingReplacement',
  'Transferred', 'Repatriated', 'Deported', 'Pregnant', 'Deceased',
];

/** Status category groupings */
export const STATUS_CATEGORIES: Record<WorkerStatusCategory, WorkerStatus[]> = {
  Pool:            ['Available', 'InTraining', 'UnderMedicalTest'],
  Arrival:         ['NewArrival'],
  Placement:       ['Booked', 'Hired', 'OnProbation', 'Active', 'Renewed'],
  NegativeSpecial: ['PendingReplacement', 'Transferred', 'MedicallyUnfit', 'Absconded', 'Terminated', 'Pregnant'],
  Terminal:        ['Repatriated', 'Deported', 'Deceased'],
};

interface LocationConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
}

export const LOCATION_CONFIG: Record<WorkerLocation, LocationConfig> = {
  Abroad:    { variant: 'outline', icon: Globe },
  InCountry: { variant: 'default', icon: MapPin },
};

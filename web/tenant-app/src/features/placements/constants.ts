import {
  BookMarked,
  Plane,
  Navigation,
  MapPin,
  Stethoscope,
  HeartPulse,
  FileText,
  CheckCircle2,
  GraduationCap,
  UserCheck,
  Building2,
  Trophy,
  XCircle,
  FileSignature,
  Stamp,
  CreditCard,
  IdCard,
  Home,
  Clock,
  RefreshCw,
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import type { PlacementStatus, PlacementFlowType, PlacementCostType } from './types';

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'outline' | 'success' | 'warning';

interface StatusConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
  label: string;
  shortLabel: string;
}

export const STATUS_CONFIG: Record<PlacementStatus, StatusConfig> = {
  // Outside-country 9-step pipeline
  Booked:                   { variant: 'secondary', icon: BookMarked,     label: 'Booked',                   shortLabel: 'Booked' },
  InTrial:                  { variant: 'warning',   icon: Clock,          label: 'In Trial',                 shortLabel: 'Trial' },
  TrialSuccessful:          { variant: 'success',   icon: CheckCircle2,   label: 'Trial Successful',         shortLabel: 'Trial OK' },
  ContractCreated:          { variant: 'outline',   icon: FileSignature,  label: 'Contract Created',         shortLabel: 'Contract' },
  StatusChanged:            { variant: 'default',   icon: RefreshCw,      label: 'Status Changed',           shortLabel: 'Status' },
  EmploymentVisaProcessing: { variant: 'warning',   icon: Stamp,          label: 'Employment Visa',          shortLabel: 'Emp Visa' },
  TicketArranged:           { variant: 'outline',   icon: Plane,          label: 'Ticket Arranged',          shortLabel: 'Ticket' },
  InTransit:                { variant: 'warning',   icon: Navigation,     label: 'In Transit',               shortLabel: 'Transit' },
  Arrived:                  { variant: 'default',   icon: MapPin,         label: 'Arrived',                  shortLabel: 'Arrived' },
  MedicalInProgress:        { variant: 'warning',   icon: Stethoscope,    label: 'Medical In Progress',      shortLabel: 'Medical' },
  MedicalCleared:           { variant: 'success',   icon: HeartPulse,     label: 'Medical Cleared',          shortLabel: 'Med OK' },
  GovtProcessing:           { variant: 'warning',   icon: FileText,       label: 'Govt Processing',          shortLabel: 'Govt' },
  GovtCleared:              { variant: 'success',   icon: CheckCircle2,   label: 'Govt Cleared',             shortLabel: 'Govt OK' },
  Training:                 { variant: 'outline',   icon: GraduationCap,  label: 'Training',                 shortLabel: 'Training' },
  ReadyForPlacement:        { variant: 'default',   icon: UserCheck,      label: 'Ready for Placement',      shortLabel: 'Ready' },
  Deployed:                 { variant: 'success',   icon: Home,           label: 'Deployed',                 shortLabel: 'Deployed' },
  Placed:                   { variant: 'success',   icon: Building2,      label: 'Placed',                   shortLabel: 'Placed' },
  FullPaymentReceived:      { variant: 'success',   icon: CreditCard,     label: 'Full Payment Received',    shortLabel: 'Paid' },
  ResidenceVisaProcessing:  { variant: 'warning',   icon: Stamp,          label: 'Residence Visa',           shortLabel: 'Res Visa' },
  EmiratesIdProcessing:     { variant: 'warning',   icon: IdCard,         label: 'Emirates ID',              shortLabel: 'EID' },
  Completed:                { variant: 'success',   icon: Trophy,         label: 'Completed',                shortLabel: 'Done' },
  Cancelled:                { variant: 'destructive',icon: XCircle,       label: 'Cancelled',                shortLabel: 'Cancelled' },
};

// Outside-country 9-step pipeline transitions
export const ALLOWED_TRANSITIONS: Partial<Record<PlacementStatus, PlacementStatus[]>> = {
  Booked:                   ['ContractCreated', 'InTrial', 'Cancelled'],
  InTrial:                  ['TrialSuccessful', 'Cancelled'],
  TrialSuccessful:          ['ContractCreated', 'Cancelled'],
  ContractCreated:          ['EmploymentVisaProcessing', 'StatusChanged', 'Cancelled'],
  StatusChanged:            ['EmploymentVisaProcessing', 'Cancelled'],
  EmploymentVisaProcessing: ['TicketArranged', 'ResidenceVisaProcessing', 'Cancelled'],
  TicketArranged:           ['InTransit', 'Arrived', 'Cancelled'],
  InTransit:                ['Arrived', 'Cancelled'],
  Arrived:                  ['Deployed', 'MedicalInProgress', 'Cancelled'],
  MedicalInProgress:        ['MedicalCleared', 'Cancelled'],
  MedicalCleared:           ['GovtProcessing', 'Cancelled'],
  GovtProcessing:           ['GovtCleared', 'Cancelled'],
  GovtCleared:              ['Training', 'ReadyForPlacement', 'Cancelled'],
  Training:                 ['ReadyForPlacement', 'Cancelled'],
  ReadyForPlacement:        ['Placed', 'Deployed', 'Cancelled'],
  Placed:                   ['Completed', 'Cancelled'],
  Deployed:                 ['FullPaymentReceived', 'Cancelled'],
  FullPaymentReceived:      ['ResidenceVisaProcessing', 'Cancelled'],
  ResidenceVisaProcessing:  ['EmiratesIdProcessing', 'Cancelled'],
  EmiratesIdProcessing:     ['Completed', 'Cancelled'],
};

export const REASON_REQUIRED_STATUSES: PlacementStatus[] = ['Cancelled'];

// Outside-country 9-step pipeline statuses for the board
export const OUTSIDE_COUNTRY_PIPELINE: PlacementStatus[] = [
  'Booked',
  'ContractCreated',
  'EmploymentVisaProcessing',
  'TicketArranged',
  'Arrived',
  'Deployed',
  'FullPaymentReceived',
  'ResidenceVisaProcessing',
  'EmiratesIdProcessing',
];

// Inside-country 8-step pipeline statuses for the board
export const INSIDE_COUNTRY_PIPELINE: PlacementStatus[] = [
  'Booked',
  'InTrial',
  'TrialSuccessful',
  'ContractCreated',
  'StatusChanged',
  'EmploymentVisaProcessing',
  'ResidenceVisaProcessing',
  'EmiratesIdProcessing',
];

// Default pipeline (outside-country) — kept for backward compat
export const PIPELINE_STATUSES = OUTSIDE_COUNTRY_PIPELINE;

export function getPipelineForFlow(flowType: PlacementFlowType): PlacementStatus[] {
  return flowType === 'InsideCountry' ? INSIDE_COUNTRY_PIPELINE : OUTSIDE_COUNTRY_PIPELINE;
}

// All unique pipeline statuses (both flows combined)
export const ALL_PIPELINE_STATUSES: PlacementStatus[] = [
  ...new Set([...OUTSIDE_COUNTRY_PIPELINE, ...INSIDE_COUNTRY_PIPELINE]),
];

export const ALL_STATUSES: PlacementStatus[] = [
  ...ALL_PIPELINE_STATUSES,
  'Completed',
  'Cancelled',
];

export const COST_TYPES: { value: PlacementCostType; label: string }[] = [
  { value: 'Procurement', label: 'Procurement' },
  { value: 'Flight', label: 'Flight' },
  { value: 'Medical', label: 'Medical' },
  { value: 'Visa', label: 'Visa' },
  { value: 'EmiratesId', label: 'Emirates ID' },
  { value: 'Insurance', label: 'Insurance' },
  { value: 'Accommodation', label: 'Accommodation' },
  { value: 'Training', label: 'Training' },
  { value: 'Other', label: 'Other' },
];

// Step descriptions for the outside-country checklist
export const STEP_DESCRIPTIONS: Record<number, { label: string; description: string }> = {
  1: { label: 'Booking', description: 'Candidate booked with partial/advance payment' },
  2: { label: 'Contract Creation', description: '2-year employment contract created' },
  3: { label: 'Employment Visa', description: 'Employment visa application submitted and processed' },
  4: { label: 'Ticket Processing', description: 'Flight ticket issued and travel date set' },
  5: { label: 'Arrival', description: 'Maid arrived and processed through arrival management' },
  6: { label: 'Deployment', description: 'Maid deployed to customer household' },
  7: { label: 'Full Payment', description: 'Remaining balance paid by customer' },
  8: { label: 'Residence Visa', description: 'Residence visa application submitted' },
  9: { label: 'Emirates ID', description: 'Emirates ID application submitted' },
};

// Step descriptions for the inside-country checklist
export const INSIDE_COUNTRY_STEP_DESCRIPTIONS: Record<number, { label: string; description: string }> = {
  1: { label: 'Booking', description: 'Candidate booked for inside-country placement' },
  2: { label: 'Trial Period', description: '5-day trial period with client' },
  3: { label: 'Trial Outcome', description: 'Trial completed successfully' },
  4: { label: 'Contract Creation', description: '2-year employment contract created' },
  5: { label: 'Status Change', description: 'Worker status updated to reflect new contract' },
  6: { label: 'Employment Visa', description: 'Passport + Photo required; Medical optional' },
  7: { label: 'Residence Visa', description: 'Local Medical + Passport + Photo required' },
  8: { label: 'Emirates ID', description: 'Emirates ID application submitted' },
};

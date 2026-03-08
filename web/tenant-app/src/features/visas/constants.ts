import {
  FileText,
  Clock,
  Send,
  Loader2,
  CheckCircle2,
  XCircle,
  Award,
  AlertTriangle,
  Ban,
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import type { VisaApplicationStatus, VisaType, VisaDocumentType } from './types';

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'outline' | 'success' | 'warning';

interface StatusConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
  label: string;
  shortLabel: string;
}

export const STATUS_CONFIG: Record<VisaApplicationStatus, StatusConfig> = {
  NotStarted:          { variant: 'secondary',    icon: Clock,         label: 'Not Started',          shortLabel: 'Not Started' },
  DocumentsCollecting: { variant: 'outline',      icon: FileText,      label: 'Documents Collecting', shortLabel: 'Docs' },
  Applied:             { variant: 'default',      icon: Send,          label: 'Applied',              shortLabel: 'Applied' },
  UnderProcess:        { variant: 'warning',      icon: Loader2,       label: 'Under Process',        shortLabel: 'Processing' },
  Approved:            { variant: 'success',      icon: CheckCircle2,  label: 'Approved',             shortLabel: 'Approved' },
  Rejected:            { variant: 'destructive',  icon: XCircle,       label: 'Rejected',             shortLabel: 'Rejected' },
  Issued:              { variant: 'success',      icon: Award,         label: 'Issued',               shortLabel: 'Issued' },
  Expired:             { variant: 'warning',      icon: AlertTriangle, label: 'Expired',              shortLabel: 'Expired' },
  Cancelled:           { variant: 'destructive',  icon: Ban,           label: 'Cancelled',            shortLabel: 'Cancelled' },
};

export const ALLOWED_TRANSITIONS: Partial<Record<VisaApplicationStatus, VisaApplicationStatus[]>> = {
  NotStarted:          ['DocumentsCollecting', 'Applied', 'Cancelled'],
  DocumentsCollecting: ['Applied', 'Cancelled'],
  Applied:             ['UnderProcess', 'Rejected', 'Cancelled'],
  UnderProcess:        ['Approved', 'Rejected', 'Cancelled'],
  Approved:            ['Issued', 'Cancelled'],
  Rejected:            ['Applied', 'Cancelled'],
  Issued:              ['Expired'],
};

export const REASON_REQUIRED_STATUSES: VisaApplicationStatus[] = ['Rejected', 'Cancelled'];

export const ALL_STATUSES: VisaApplicationStatus[] = [
  'NotStarted',
  'DocumentsCollecting',
  'Applied',
  'UnderProcess',
  'Approved',
  'Rejected',
  'Issued',
  'Expired',
  'Cancelled',
];

export const ALL_VISA_TYPES: { value: VisaType; label: string }[] = [
  { value: 'EmploymentVisa', label: 'Employment Visa' },
  { value: 'ResidenceVisa', label: 'Residence Visa' },
  { value: 'EmiratesId', label: 'Emirates ID' },
];

export const VISA_DOCUMENT_TYPES: { value: VisaDocumentType; label: string }[] = [
  { value: 'AttestedMedicalCertificate', label: 'Attested Medical Certificate' },
  { value: 'PassportCopy', label: 'Passport Copy' },
  { value: 'Photo', label: 'Photo' },
  { value: 'LocalMedical', label: 'Local Medical' },
  { value: 'MedicalCertificate', label: 'Medical Certificate' },
  { value: 'Other', label: 'Other' },
];

interface DocumentRequirement {
  type: VisaDocumentType;
  mandatory: boolean;
}

export const DOCUMENT_REQUIREMENTS: Record<string, DocumentRequirement[]> = {
  'EmploymentVisa_Outside': [
    { type: 'AttestedMedicalCertificate', mandatory: true },
    { type: 'PassportCopy', mandatory: true },
    { type: 'Photo', mandatory: true },
  ],
  'EmploymentVisa_Inside': [
    { type: 'PassportCopy', mandatory: true },
    { type: 'Photo', mandatory: true },
    { type: 'MedicalCertificate', mandatory: false },
  ],
  'ResidenceVisa_Outside': [
    { type: 'LocalMedical', mandatory: true },
    { type: 'PassportCopy', mandatory: true },
    { type: 'Photo', mandatory: true },
  ],
  'ResidenceVisa_Inside': [
    { type: 'LocalMedical', mandatory: true },
    { type: 'PassportCopy', mandatory: true },
    { type: 'Photo', mandatory: true },
  ],
  'EmiratesId_Outside': [],
  'EmiratesId_Inside': [],
};

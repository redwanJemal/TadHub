import type { LucideIcon } from 'lucide-react';
import {
  BookOpen, Plane, Briefcase, Stethoscope, Shield,
  CreditCard, FileText, File,
  Clock, CheckCircle, AlertTriangle, XCircle, Ban,
} from 'lucide-react';
import type { DocumentType, DocumentStatus, EffectiveStatus } from './types';

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'success' | 'warning' | 'outline';

interface StatusConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
}

export const EFFECTIVE_STATUS_CONFIG: Record<EffectiveStatus, StatusConfig> = {
  Valid:        { variant: 'success',     icon: CheckCircle },
  ExpiringSoon: { variant: 'warning',    icon: AlertTriangle },
  Expired:     { variant: 'destructive', icon: XCircle },
  Pending:     { variant: 'secondary',   icon: Clock },
  Revoked:     { variant: 'destructive', icon: Ban },
};

export const STATUS_CONFIG: Record<DocumentStatus, StatusConfig> = {
  Pending: { variant: 'secondary',   icon: Clock },
  Valid:   { variant: 'success',     icon: CheckCircle },
  Expired: { variant: 'destructive', icon: XCircle },
  Revoked: { variant: 'destructive', icon: Ban },
};

interface TypeConfig {
  variant: BadgeVariant;
  icon: LucideIcon;
}

export const TYPE_CONFIG: Record<DocumentType, TypeConfig> = {
  Passport:           { variant: 'default',   icon: BookOpen },
  Visa:               { variant: 'default',   icon: Plane },
  WorkPermit:         { variant: 'default',   icon: Briefcase },
  MedicalCertificate: { variant: 'default',   icon: Stethoscope },
  InsurancePolicy:    { variant: 'default',   icon: Shield },
  EmiratesId:         { variant: 'default',   icon: CreditCard },
  LabourCard:         { variant: 'default',   icon: FileText },
  Other:              { variant: 'outline',   icon: File },
};

export const ALL_DOCUMENT_TYPES: DocumentType[] = [
  'Passport', 'Visa', 'WorkPermit', 'MedicalCertificate',
  'InsurancePolicy', 'EmiratesId', 'LabourCard', 'Other',
];

export const ALL_STATUSES: DocumentStatus[] = [
  'Pending', 'Valid', 'Expired', 'Revoked',
];

export const ALL_EFFECTIVE_STATUSES: EffectiveStatus[] = [
  'Pending', 'Valid', 'ExpiringSoon', 'Expired', 'Revoked',
];

export const DOCUMENT_TYPE_LABELS: Record<DocumentType, string> = {
  Passport: 'Passport',
  Visa: 'Visa',
  WorkPermit: 'Work Permit',
  MedicalCertificate: 'Medical Certificate',
  InsurancePolicy: 'Insurance Policy',
  EmiratesId: 'Emirates ID',
  LabourCard: 'Labour Card',
  Other: 'Other',
};

export const STATUS_LABELS: Record<DocumentStatus, string> = {
  Pending: 'Pending',
  Valid: 'Valid',
  Expired: 'Expired',
  Revoked: 'Revoked',
};

export const EFFECTIVE_STATUS_LABELS: Record<EffectiveStatus, string> = {
  Pending: 'Pending',
  Valid: 'Valid',
  ExpiringSoon: 'Expiring Soon',
  Expired: 'Expired',
  Revoked: 'Revoked',
};

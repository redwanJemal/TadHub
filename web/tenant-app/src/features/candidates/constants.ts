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
import type { CandidateStatus, CandidateSourceType, SkillProficiency, LanguageProficiency } from './types';

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

/** Allowed manual status transitions (review phase only) */
export const ALLOWED_TRANSITIONS: Partial<Record<CandidateStatus, CandidateStatus[]>> = {
  Received: ['UnderReview', 'Cancelled'],
  UnderReview: ['Approved', 'Rejected', 'Cancelled'],
};

export const PRESET_SKILLS = [
  'Cooking',
  'Cleaning',
  'Childcare',
  'Eldercare',
  'Laundry',
  'Ironing',
  'Driving',
];

export const PRESET_LANGUAGES = [
  'Arabic',
  'English',
  'Hindi',
  'Urdu',
  'Tagalog',
  'Sinhala',
  'Bengali',
  'Nepali',
  'Indonesian',
  'Amharic',
];

export const SKILL_PROFICIENCY_LEVELS: SkillProficiency[] = [
  'Basic',
  'Intermediate',
  'Advanced',
  'Expert',
];

export const LANGUAGE_PROFICIENCY_LEVELS: LanguageProficiency[] = [
  'Basic',
  'Conversational',
  'Fluent',
  'Native',
];

export const RELIGION_OPTIONS = [
  'Islam',
  'Christianity',
  'Hinduism',
  'Buddhism',
  'Other',
];

export const MARITAL_STATUS_OPTIONS = [
  'Single',
  'Married',
  'Divorced',
  'Widowed',
];

export const EDUCATION_LEVEL_OPTIONS = [
  'None',
  'Primary',
  'Secondary',
  'HighSchool',
  'Diploma',
  'Bachelor',
  'Master',
  'PhD',
];

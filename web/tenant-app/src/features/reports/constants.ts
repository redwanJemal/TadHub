import type { ReportDefinition } from './types';

export const REPORT_DEFINITIONS: ReportDefinition[] = [
  // Workforce
  { key: 'inventory', path: '/reports/inventory', category: 'workforce' },
  { key: 'deployed', path: '/reports/deployed', category: 'workforce' },
  { key: 'returnees', path: '/reports/returnees', category: 'workforce' },
  { key: 'runaways', path: '/reports/runaways', category: 'workforce' },
  // Operational
  { key: 'arrivals', path: '/reports/arrivals', category: 'operational' },
  { key: 'accommodationDaily', path: '/reports/accommodation-daily', category: 'operational' },
  { key: 'deploymentPipeline', path: '/reports/deployment-pipeline', category: 'operational' },
  // Finance
  { key: 'supplierCommissions', path: '/reports/supplier-commissions', category: 'finance' },
  { key: 'refunds', path: '/reports/refunds', category: 'finance' },
  { key: 'costPerMaid', path: '/reports/cost-per-maid', category: 'finance' },
];

export const INVENTORY_STATUSES = ['Available', 'NewArrival', 'InTraining', 'UnderMedicalTest'];
export const RETURNEE_STATUSES = ['Submitted', 'UnderReview', 'Approved', 'Rejected', 'Settled'];
export const RUNAWAY_STATUSES = ['Reported', 'UnderInvestigation', 'Confirmed', 'Settled', 'Closed'];
export const ARRIVAL_STATUSES = ['Scheduled', 'InTransit', 'Arrived', 'PickedUp', 'AtAccommodation', 'NoShow', 'Cancelled'];
export const PLACEMENT_STAGES = [
  'Booked', 'TicketArranged', 'InTransit', 'Arrived', 'MedicalInProgress',
  'MedicalCleared', 'GovtProcessing', 'GovtCleared', 'Training',
  'ReadyForPlacement', 'Placed',
];

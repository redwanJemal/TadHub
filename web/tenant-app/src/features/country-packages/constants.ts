import type { DefaultGuaranteePeriod, SupplierCommissionType } from './types';

export const ALL_GUARANTEE_PERIODS: DefaultGuaranteePeriod[] = ['SixMonths', 'OneYear', 'TwoYears'];

export const ALL_COMMISSION_TYPES: SupplierCommissionType[] = ['FixedAmount', 'Percentage'];

export const COST_FIELDS = [
  'maidCost',
  'monthlyAccommodationCost',
  'visaCost',
  'employmentVisaCost',
  'residenceVisaCost',
  'medicalCost',
  'transportationCost',
  'ticketCost',
  'insuranceCost',
  'emiratesIdCost',
  'otherCosts',
] as const;

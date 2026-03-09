export type SupplierCommissionType = 'FixedAmount' | 'Percentage';

export type DefaultGuaranteePeriod = 'SixMonths' | 'OneYear' | 'TwoYears';

export interface CountryPackageDto {
  id: string;
  countryId: string;
  name: string;
  isDefault: boolean;
  countryNameEn: string | null;
  countryNameAr: string | null;
  countryCode: string | null;
  maidCost: number;
  monthlyAccommodationCost: number;
  visaCost: number;
  employmentVisaCost: number;
  residenceVisaCost: number;
  medicalCost: number;
  transportationCost: number;
  ticketCost: number;
  insuranceCost: number;
  emiratesIdCost: number;
  otherCosts: number;
  totalPackagePrice: number;
  supplierCommission: number;
  supplierCommissionType: SupplierCommissionType;
  defaultGuaranteePeriod: DefaultGuaranteePeriod;
  currency: string;
  effectiveFrom: string;
  effectiveTo: string | null;
  isActive: boolean;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CountryPackageListDto {
  id: string;
  countryId: string;
  name: string;
  isDefault: boolean;
  countryNameEn: string | null;
  countryNameAr: string | null;
  countryCode: string | null;
  totalPackagePrice: number;
  currency: string;
  effectiveFrom: string;
  effectiveTo: string | null;
  isActive: boolean;
  defaultGuaranteePeriod: DefaultGuaranteePeriod;
  createdAt: string;
}

export interface CreateCountryPackageRequest {
  countryId: string;
  name: string;
  isDefault: boolean;
  maidCost: number;
  monthlyAccommodationCost: number;
  visaCost: number;
  employmentVisaCost: number;
  residenceVisaCost: number;
  medicalCost: number;
  transportationCost: number;
  ticketCost: number;
  insuranceCost: number;
  emiratesIdCost: number;
  otherCosts: number;
  totalPackagePrice: number;
  supplierCommission: number;
  supplierCommissionType: string;
  defaultGuaranteePeriod: string;
  currency: string;
  effectiveFrom: string;
  effectiveTo?: string;
  isActive: boolean;
  notes?: string;
}

export interface UpdateCountryPackageRequest {
  countryId?: string;
  name?: string;
  isDefault?: boolean;
  maidCost?: number;
  monthlyAccommodationCost?: number;
  visaCost?: number;
  employmentVisaCost?: number;
  residenceVisaCost?: number;
  medicalCost?: number;
  transportationCost?: number;
  ticketCost?: number;
  insuranceCost?: number;
  emiratesIdCost?: number;
  otherCosts?: number;
  totalPackagePrice?: number;
  supplierCommission?: number;
  supplierCommissionType?: string;
  defaultGuaranteePeriod?: string;
  currency?: string;
  effectiveFrom?: string;
  effectiveTo?: string;
  isActive?: boolean;
  notes?: string;
}

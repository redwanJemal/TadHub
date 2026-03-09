import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  CountryPackageDto,
  CountryPackageListDto,
  CreateCountryPackageRequest,
  UpdateCountryPackageRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listCountryPackages(params?: QueryParams) {
  return apiClient.getPaged<CountryPackageListDto>(tenantPath('/country-packages'), params);
}

export function getCountryPackage(id: string) {
  return apiClient.get<CountryPackageDto>(tenantPath(`/country-packages/${id}`));
}

export function getCountryPackagesByCountry(countryId: string) {
  return apiClient.get<CountryPackageListDto[]>(tenantPath(`/country-packages/by-country/${countryId}`));
}

export function getDefaultCountryPackage(countryId: string) {
  return apiClient.get<CountryPackageDto>(tenantPath(`/country-packages/default/${countryId}`));
}

export function createCountryPackage(data: CreateCountryPackageRequest) {
  return apiClient.post<CountryPackageDto>(tenantPath('/country-packages'), data);
}

export function updateCountryPackage(id: string, data: UpdateCountryPackageRequest) {
  return apiClient.patch<CountryPackageDto>(tenantPath(`/country-packages/${id}`), data);
}

export function deleteCountryPackage(id: string) {
  return apiClient.delete(tenantPath(`/country-packages/${id}`));
}

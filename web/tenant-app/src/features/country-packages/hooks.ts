import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type { CreateCountryPackageRequest, UpdateCountryPackageRequest } from './types';

const PACKAGES_KEY = 'countryPackages';

export function useCountryPackages(params?: QueryParams) {
  return useQuery({
    queryKey: [PACKAGES_KEY, params],
    queryFn: () => api.listCountryPackages(params),
  });
}

export function useCountryPackage(id: string) {
  return useQuery({
    queryKey: [PACKAGES_KEY, id],
    queryFn: () => api.getCountryPackage(id),
    enabled: !!id,
  });
}

export function useCountryPackagesByCountry(countryId: string) {
  return useQuery({
    queryKey: [PACKAGES_KEY, 'byCountry', countryId],
    queryFn: () => api.getCountryPackagesByCountry(countryId),
    enabled: !!countryId,
  });
}

export function useDefaultCountryPackage(countryId: string) {
  return useQuery({
    queryKey: [PACKAGES_KEY, 'default', countryId],
    queryFn: () => api.getDefaultCountryPackage(countryId),
    enabled: !!countryId,
  });
}

export function useCreateCountryPackage() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCountryPackageRequest) => api.createCountryPackage(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PACKAGES_KEY] });
    },
  });
}

export function useUpdateCountryPackage() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCountryPackageRequest }) =>
      api.updateCountryPackage(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PACKAGES_KEY] });
    },
  });
}

export function useDeleteCountryPackage() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteCountryPackage(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PACKAGES_KEY] });
    },
  });
}

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type { CreateSupplierUserRequest, UpdateSupplierUserRequest } from './types';

const SUPPLIER_PORTAL_KEY = 'supplier-portal';

export function useSupplierProfile() {
  return useQuery({
    queryKey: [SUPPLIER_PORTAL_KEY, 'profile'],
    queryFn: () => api.getSupplierProfile(),
  });
}

export function useSupplierDashboard() {
  return useQuery({
    queryKey: [SUPPLIER_PORTAL_KEY, 'dashboard'],
    queryFn: () => api.getSupplierDashboard(),
  });
}

export function useSupplierCandidates(params?: QueryParams) {
  return useQuery({
    queryKey: [SUPPLIER_PORTAL_KEY, 'candidates', params],
    queryFn: () => api.listSupplierCandidates(params),
  });
}

export function useSupplierWorkers(params?: QueryParams) {
  return useQuery({
    queryKey: [SUPPLIER_PORTAL_KEY, 'workers', params],
    queryFn: () => api.listSupplierWorkers(params),
  });
}

export function useSupplierCommissions(params?: QueryParams) {
  return useQuery({
    queryKey: [SUPPLIER_PORTAL_KEY, 'commissions', params],
    queryFn: () => api.listSupplierCommissions(params),
  });
}

export function useSupplierArrivals(params?: QueryParams) {
  return useQuery({
    queryKey: [SUPPLIER_PORTAL_KEY, 'arrivals', params],
    queryFn: () => api.listSupplierArrivals(params),
  });
}

export function useSupplierUsers(params?: QueryParams) {
  return useQuery({
    queryKey: [SUPPLIER_PORTAL_KEY, 'users', params],
    queryFn: () => api.listSupplierUsers(params),
  });
}

export function useCreateSupplierUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateSupplierUserRequest) => api.createSupplierUser(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [SUPPLIER_PORTAL_KEY] });
    },
  });
}

export function useUpdateSupplierUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSupplierUserRequest }) =>
      api.updateSupplierUser(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [SUPPLIER_PORTAL_KEY] });
    },
  });
}

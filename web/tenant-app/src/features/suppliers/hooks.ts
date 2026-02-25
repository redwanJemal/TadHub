import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type {
  CreateSupplierRequest,
  LinkSupplierRequest,
  UpdateTenantSupplierRequest,
} from './types';

const SUPPLIERS_KEY = 'tenant-suppliers';

export function useSuppliers(params?: QueryParams) {
  return useQuery({
    queryKey: [SUPPLIERS_KEY, params],
    queryFn: () => api.listTenantSuppliers(params),
  });
}

export function useSupplier(id: string) {
  return useQuery({
    queryKey: [SUPPLIERS_KEY, id],
    queryFn: () => api.getTenantSupplier(id),
    enabled: !!id,
  });
}

export function useCreateAndLinkSupplier() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateSupplierRequest) => api.createAndLinkSupplier(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [SUPPLIERS_KEY] });
    },
  });
}

export function useLinkSupplier() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: LinkSupplierRequest) => api.linkSupplier(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [SUPPLIERS_KEY] });
    },
  });
}

export function useUpdateTenantSupplier() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTenantSupplierRequest }) =>
      api.updateTenantSupplier(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [SUPPLIERS_KEY] });
    },
  });
}

export function useUnlinkSupplier() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.unlinkSupplier(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [SUPPLIERS_KEY] });
    },
  });
}

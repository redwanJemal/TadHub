import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  ClientDto,
  ClientListDto,
  CreateClientRequest,
  UpdateClientRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listClients(params?: QueryParams) {
  return apiClient.getPaged<ClientListDto>(tenantPath('/clients'), params);
}

export function getClient(id: string) {
  return apiClient.get<ClientDto>(tenantPath(`/clients/${id}`));
}

export function createClient(data: CreateClientRequest) {
  return apiClient.post<ClientDto>(tenantPath('/clients'), data);
}

export function updateClient(id: string, data: UpdateClientRequest) {
  return apiClient.patch<ClientDto>(tenantPath(`/clients/${id}`), data);
}

export function deleteClient(id: string) {
  return apiClient.delete<void>(tenantPath(`/clients/${id}`));
}

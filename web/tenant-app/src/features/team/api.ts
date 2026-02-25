import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  TenantMember,
  TenantInvitation,
  Role,
  InviteMemberRequest,
  AssignRoleRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

// Members
export function listMembers(params?: QueryParams) {
  return apiClient.getPaged<TenantMember>(tenantPath('/members'), params);
}

export function getMember(userId: string) {
  return apiClient.get<TenantMember>(tenantPath(`/members/${userId}`));
}

export function removeMember(userId: string) {
  return apiClient.delete<void>(tenantPath(`/members/${userId}`));
}

// Invitations
export function listInvitations(params?: QueryParams) {
  return apiClient.getPaged<TenantInvitation>(tenantPath('/invitations'), params);
}

export function inviteMember(data: InviteMemberRequest) {
  return apiClient.post<TenantInvitation>(tenantPath('/invitations'), data);
}

export function revokeInvitation(invitationId: string) {
  return apiClient.delete<void>(tenantPath(`/invitations/${invitationId}`));
}

// Roles
export function listRoles(params?: QueryParams) {
  return apiClient.getPaged<Role>(tenantPath('/roles'), params);
}

export function assignRole(data: AssignRoleRequest) {
  return apiClient.post<unknown>(tenantPath('/roles/assign'), data);
}

export function removeRole(userId: string, roleId: string) {
  return apiClient.delete<void>(tenantPath(`/roles/users/${userId}/roles/${roleId}`));
}

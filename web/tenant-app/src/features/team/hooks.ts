import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type { InviteMemberRequest, AssignRoleRequest } from './types';

const MEMBERS_KEY = 'team-members';
const INVITATIONS_KEY = 'team-invitations';
const ROLES_KEY = 'team-roles';

export function useTeamMembers(params?: QueryParams) {
  return useQuery({
    queryKey: [MEMBERS_KEY, params],
    queryFn: () => api.listMembers(params),
  });
}

export function useTeamMember(userId: string) {
  return useQuery({
    queryKey: [MEMBERS_KEY, userId],
    queryFn: () => api.getMember(userId),
    enabled: !!userId,
  });
}

export function useRemoveMember() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (userId: string) => api.removeMember(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [MEMBERS_KEY] });
    },
  });
}

export function useInviteMember() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: InviteMemberRequest) => api.inviteMember(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [INVITATIONS_KEY] });
    },
  });
}

export function useInvitations(params?: QueryParams) {
  return useQuery({
    queryKey: [INVITATIONS_KEY, params],
    queryFn: () => api.listInvitations(params),
  });
}

export function useRevokeInvitation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (invitationId: string) => api.revokeInvitation(invitationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [INVITATIONS_KEY] });
    },
  });
}

export function useRoles() {
  return useQuery({
    queryKey: [ROLES_KEY],
    queryFn: () => api.listRoles({ pageSize: 100 }),
    staleTime: 5 * 60 * 1000,
  });
}

export function useAssignRole() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: AssignRoleRequest) => api.assignRole(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [MEMBERS_KEY] });
    },
  });
}

export function useRemoveRole() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, roleId }: { userId: string; roleId: string }) =>
      api.removeRole(userId, roleId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [MEMBERS_KEY] });
    },
  });
}

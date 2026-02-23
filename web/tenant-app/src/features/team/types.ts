export interface TenantMember {
  id: string;
  tenantId: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  avatarUrl?: string;
  isOwner: boolean;
  status: 'Active' | 'Suspended' | 'Invited';
  roles: { id: string; name: string }[];
  joinedAt: string;
}

export interface TenantInvitation {
  id: string;
  tenantId: string;
  tenantName: string;
  email: string;
  defaultRoleId?: string;
  token: string;
  expiresAt: string;
  acceptedAt?: string;
  invitedByUserId: string;
  invitedByName: string;
  isExpired: boolean;
  isAccepted: boolean;
  createdAt: string;
}

export interface Role {
  id: string;
  name: string;
  description?: string;
  isSystem: boolean;
}

export interface InviteMemberRequest {
  email: string;
  roleId?: string;
}

export interface AssignRoleRequest {
  userId: string;
  roleId: string;
}

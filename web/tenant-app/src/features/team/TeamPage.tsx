import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { DataTableAdvanced, type Column, type Filter } from '@/shared/components/data-table';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { Avatar, AvatarFallback } from '@/shared/components/ui/avatar';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { Plus, Edit, Trash2, Loader2, RotateCw } from 'lucide-react';
import { InviteMemberDialog } from './components/InviteMemberDialog';
import { EditMemberDialog } from './components/EditMemberDialog';
import { apiClient } from '@/shared/api';
import { toast } from 'sonner';

interface TeamMember {
  id: string;
  authId: string;
  email: string;
  displayName: string;
  jobTitle?: string;
  department?: string;
  phoneNumber?: string;
  avatarUrl?: string;
  status: 'active' | 'inactive' | 'pending';
  joinedAtUtc: string;
  roleIds: string[];
  roles?: Array<{
    id: string;
    name: string;
    slug: string;
  }>;
}

interface Role {
  id: string;
  name: string;
  slug: string;
  description?: string;
}

export function TeamPage() {
  const { t } = useTranslation('team');

  // State
  const [data, setData] = useState<TeamMember[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState<string | undefined>();
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [page, setPage] = useState(1);
  const [perPage, setPerPage] = useState(20);
  const [total, setTotal] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  // Dialog states
  const [inviteDialogOpen, setInviteDialogOpen] = useState(false);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editingMember, setEditingMember] = useState<TeamMember | null>(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deletingMember, setDeletingMember] = useState<TeamMember | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [resendingId, setResendingId] = useState<string | null>(null);

  // Fetch roles for filter
  useEffect(() => {
    async function fetchRoles() {
      try {
        const response = await apiClient.getWithMeta<Role[]>('/roles');
        setRoles(response.data);
      } catch (error) {
        console.error('Failed to fetch roles:', error);
      }
    }
    fetchRoles();
  }, []);

  // Fetch data
  const fetchTeam = useCallback(async () => {
    setIsLoading(true);
    try {
      const params: Record<string, string> = {
        page: String(page),
        pageSize: String(perPage),
      };
      if (search) params.search = search;
      if (roleFilter) params.roleId = roleFilter;
      if (statusFilter) params.status = statusFilter;

      const response = await apiClient.getWithMeta<TeamMember[]>('/members', params);
      setData(response.data);
      if (response.meta) {
        setTotal(response.meta.total ?? 0);
        setTotalPages(response.meta.totalPages ?? 1);
      }
    } catch (error) {
      console.error('Failed to fetch team:', error);
    } finally {
      setIsLoading(false);
    }
  }, [page, perPage, search, roleFilter, statusFilter]);

  useEffect(() => {
    fetchTeam();
  }, [fetchTeam]);

  // Debounced search
  useEffect(() => {
    const timer = setTimeout(() => {
      setPage(1);
    }, 300);
    return () => clearTimeout(timer);
  }, [search]);

  // Handlers
  const handleEdit = (member: TeamMember) => {
    setEditingMember(member);
    setEditDialogOpen(true);
  };

  const handleDeleteClick = (member: TeamMember) => {
    setDeletingMember(member);
    setDeleteDialogOpen(true);
  };

  const handleDeactivate = async () => {
    if (!deletingMember) return;
    setIsDeleting(true);
    try {
      await apiClient.post(`/members/${deletingMember.id}/deactivate`);
      toast.success(t('notifications.memberDeactivated'));
      fetchTeam();
      setDeleteDialogOpen(false);
      setDeletingMember(null);
    } catch (error) {
      console.error('Failed to deactivate member:', error);
      toast.error(t('notifications.deactivateFailed'));
    } finally {
      setIsDeleting(false);
    }
  };

  const handleResendInvitation = async (memberId: string) => {
    setResendingId(memberId);
    try {
      // Find the invitation for this member and resend
      await apiClient.post(`/invitations/${memberId}/resend`);
      toast.success(t('notifications.invitationResent'));
    } catch (error) {
      console.error('Failed to resend invitation:', error);
      toast.error(t('notifications.resendFailed'));
    } finally {
      setResendingId(null);
    }
  };

  const getInitials = (displayName: string, email: string) => {
    if (displayName) {
      const parts = displayName.split(' ');
      if (parts.length >= 2) {
        return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
      }
      return displayName[0].toUpperCase();
    }
    return email[0].toUpperCase();
  };

  // Columns
  const columns: Column<TeamMember>[] = [
    {
      key: 'name',
      header: t('columns.member'),
      cell: (row) => (
        <div className="flex items-center gap-3">
          <Avatar className="h-9 w-9">
            <AvatarFallback className="bg-primary/10 text-primary text-xs">
              {getInitials(row.displayName, row.email)}
            </AvatarFallback>
          </Avatar>
          <div>
            <p className="font-medium">{row.displayName}</p>
            <p className="text-xs text-muted-foreground">{row.email}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'role',
      header: t('columns.role'),
      cell: (row) => (
        <div className="flex flex-wrap gap-1">
          {row.roles?.map((role) => (
            <Badge key={role.id} variant="secondary">{role.name}</Badge>
          )) || <Badge variant="outline">No role</Badge>}
        </div>
      ),
    },
    {
      key: 'status',
      header: t('columns.status'),
      cell: (row) => {
        switch (row.status) {
          case 'pending':
            return <Badge variant="outline">{t('status.pending')}</Badge>;
          case 'active':
            return <Badge variant="success">{t('status.active')}</Badge>;
          case 'inactive':
            return <Badge variant="secondary">{t('status.inactive')}</Badge>;
          default:
            return <Badge>{row.status}</Badge>;
        }
      },
    },
    {
      key: 'joinedAt',
      header: t('columns.joinedAt'),
      cell: (row) =>
        row.joinedAtUtc
          ? new Date(row.joinedAtUtc).toLocaleDateString()
          : '-',
    },
    {
      key: 'actions',
      header: t('columns.actions'),
      cell: (row) => (
        <div className="flex items-center gap-1">
          {row.status === 'pending' && (
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8"
              onClick={() => handleResendInvitation(row.id)}
              disabled={resendingId === row.id}
            >
              {resendingId === row.id ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <RotateCw className="h-4 w-4" />
              )}
            </Button>
          )}
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={() => handleEdit(row)}
          >
            <Edit className="h-4 w-4" />
          </Button>
          {row.status !== 'inactive' && (
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-destructive"
              onClick={() => handleDeleteClick(row)}
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          )}
        </div>
      ),
      className: 'w-[120px]',
    },
  ];

  // Filters
  const filters: Filter[] = [
    {
      key: 'role',
      label: t('filters.role'),
      value: roleFilter,
      options: roles.map((r) => ({ label: r.name, value: r.id })),
    },
    {
      key: 'status',
      label: t('filters.status'),
      value: statusFilter,
      options: [
        { label: t('status.active'), value: 'active' },
        { label: t('status.inactive'), value: 'inactive' },
        { label: t('status.pending'), value: 'pending' },
      ],
    },
  ];

  const handleFilterChange = (key: string, value: string | undefined) => {
    if (key === 'role') {
      setRoleFilter(value);
    } else if (key === 'status') {
      setStatusFilter(value);
    }
    setPage(1);
  };

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold md:text-3xl">{t('title')}</h1>
          <p className="text-muted-foreground">{t('subtitle')}</p>
        </div>
      </div>

      {/* Data table */}
      <DataTableAdvanced
        columns={columns}
        data={data}
        isLoading={isLoading}
        // Search
        searchValue={search}
        onSearchChange={setSearch}
        searchPlaceholder={t('searchPlaceholder')}
        // Filters
        filters={filters}
        onFilterChange={handleFilterChange}
        // Pagination
        page={page}
        totalPages={totalPages}
        total={total}
        perPage={perPage}
        onPageChange={setPage}
        onPerPageChange={(newPerPage) => {
          setPerPage(newPerPage);
          setPage(1);
        }}
        // Selection
        selectable
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        // Actions
        onRefresh={fetchTeam}
        actions={
          <Button onClick={() => setInviteDialogOpen(true)}>
            <Plus className="h-4 w-4" />
            <span className="hidden sm:inline ms-2">{t('inviteMember')}</span>
          </Button>
        }
        // Empty state
        emptyVariant="team"
        emptyAction={
          <Button onClick={() => setInviteDialogOpen(true)}>
            <Plus className="h-4 w-4 me-2" />
            {t('inviteMember')}
          </Button>
        }
      />

      {/* Invite Dialog */}
      <InviteMemberDialog
        open={inviteDialogOpen}
        onOpenChange={setInviteDialogOpen}
        onSuccess={fetchTeam}
        roles={roles}
      />

      {/* Edit Dialog */}
      <EditMemberDialog
        open={editDialogOpen}
        onOpenChange={setEditDialogOpen}
        onSuccess={fetchTeam}
        member={editingMember}
        roles={roles}
      />

      {/* Deactivate Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t('deactivateDialog.title')}</DialogTitle>
            <DialogDescription>
              {t('deactivateDialog.description', {
                name: deletingMember?.displayName || '',
              })}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDeleteDialogOpen(false)}
              disabled={isDeleting}
            >
              {t('actions.cancel')}
            </Button>
            <Button
              variant="destructive"
              onClick={handleDeactivate}
              disabled={isDeleting}
            >
              {isDeleting ? (
                <>
                  <Loader2 className="me-2 h-4 w-4 animate-spin" />
                  {t('actions.deactivating')}
                </>
              ) : (
                t('actions.deactivate')
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/components/ui/tabs';
import { DataTableAdvanced, type Column } from '@/shared/components/data-table/DataTableAdvanced';
import { Avatar, AvatarFallback, AvatarImage } from '@/shared/components/ui/avatar';
import { Badge } from '@/shared/components/ui/badge';
import { Button } from '@/shared/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';
import { MoreHorizontal, UserPlus, Shield, Trash2, Users } from 'lucide-react';
import { useTeamMembers, useRemoveMember } from '../hooks';
import { InviteMemberDialog } from '../components/InviteMemberDialog';
import { ChangeMemberRoleDialog } from '../components/ChangeMemberRoleDialog';
import { PendingInvitations } from '../components/PendingInvitations';
import type { TenantMember } from '../types';

export function TeamPage() {
  const { t } = useTranslation('team');

  // Table state
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  // Dialog state
  const [inviteOpen, setInviteOpen] = useState(false);
  const [roleDialogMember, setRoleDialogMember] = useState<TenantMember | null>(null);
  const [removeTarget, setRemoveTarget] = useState<TenantMember | null>(null);
  const [bulkRemoveOpen, setBulkRemoveOpen] = useState(false);

  // Data
  const queryParams = useMemo(() => ({
    page,
    pageSize,
    search: search || undefined,
  }), [page, pageSize, search]);

  const { data, isLoading, refetch } = useTeamMembers(queryParams);
  const removeMember = useRemoveMember();

  const handleRemoveMember = async () => {
    if (!removeTarget) return;
    await removeMember.mutateAsync(removeTarget.userId);
    setRemoveTarget(null);
  };

  const handleBulkRemove = async () => {
    const members = data?.items ?? [];
    const selected = members.filter((m) => selectedIds.includes(m.id));
    await Promise.all(selected.map((m) => removeMember.mutateAsync(m.userId)));
    setSelectedIds([]);
    setBulkRemoveOpen(false);
  };

  const columns: Column<TenantMember>[] = [
    {
      key: 'name',
      header: t('members.name'),
      cell: (row) => (
        <div className="flex items-center gap-3">
          <Avatar className="h-8 w-8">
            {row.avatarUrl && <AvatarImage src={row.avatarUrl} alt={row.fullName} />}
            <AvatarFallback className="text-xs">
              {row.firstName?.[0] ?? ''}{row.lastName?.[0] ?? ''}
            </AvatarFallback>
          </Avatar>
          <div className="min-w-0">
            <p className="font-medium truncate">{row.fullName}</p>
            <p className="text-xs text-muted-foreground truncate">{row.email}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'roles',
      header: t('members.roles'),
      cell: (row) => {
        // Filter out the "Owner" role when isOwner is true to avoid duplicate badge
        const displayRoles = row.isOwner
          ? row.roles.filter((role) => role.name !== 'Owner')
          : row.roles;

        return (
          <div className="flex flex-wrap gap-1">
            {row.isOwner && (
              <Badge variant="default">{t('members.owner')}</Badge>
            )}
            {displayRoles.map((role) => (
              <Badge key={role.id} variant="secondary">
                {role.name}
              </Badge>
            ))}
            {!row.isOwner && displayRoles.length === 0 && (
              <span className="text-xs text-muted-foreground">{t('members.noRoles')}</span>
            )}
          </div>
        );
      },
    },
    {
      key: 'joinedAt',
      header: t('members.joinedAt'),
      cell: (row) => new Date(row.joinedAt).toLocaleDateString(),
    },
    {
      key: 'actions',
      header: t('members.actions'),
      className: 'w-[70px]',
      cell: (row) => (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" className="h-8 w-8">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={() => setRoleDialogMember(row)}>
              <Shield className="me-2 h-4 w-4" />
              {t('members.changeRole')}
            </DropdownMenuItem>
            {!row.isOwner && (
              <>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  className="text-destructive"
                  onClick={() => setRemoveTarget(row)}
                >
                  <Trash2 className="me-2 h-4 w-4" />
                  {t('members.removeMember')}
                </DropdownMenuItem>
              </>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{t('title')}</h1>
        <p className="text-muted-foreground">{t('description')}</p>
      </div>

      <Tabs defaultValue="members">
        <TabsList>
          <TabsTrigger value="members">
            <Users className="me-2 h-4 w-4" />
            {t('tabs.members')}
          </TabsTrigger>
          <TabsTrigger value="invitations">
            <UserPlus className="me-2 h-4 w-4" />
            {t('tabs.invitations')}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="members">
          <DataTableAdvanced
            columns={columns}
            data={data?.items ?? []}
            isLoading={isLoading}
            selectable
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            searchPlaceholder={t('members.searchPlaceholder')}
            searchValue={search}
            onSearchChange={(val) => {
              setSearch(val);
              setPage(1);
            }}
            page={page}
            totalPages={data?.totalPages}
            total={data?.totalCount}
            pageSize={pageSize}
            onPageChange={setPage}
            onPageSizeChange={setPageSize}
            onRefresh={() => refetch()}
            emptyVariant="team"
            emptyTitle={t('empty.members.title')}
            emptyDescription={t('empty.members.description')}
            emptyAction={
              <Button onClick={() => setInviteOpen(true)}>
                <UserPlus className="me-2 h-4 w-4" />
                {t('invite.title')}
              </Button>
            }
            actions={
              <Button onClick={() => setInviteOpen(true)}>
                <UserPlus className="me-2 h-4 w-4" />
                {t('invite.title')}
              </Button>
            }
            bulkActions={
              <Button
                variant="destructive"
                size="sm"
                onClick={() => setBulkRemoveOpen(true)}
              >
                <Trash2 className="me-2 h-4 w-4" />
                {t('members.removeSelected')}
              </Button>
            }
          />
        </TabsContent>

        <TabsContent value="invitations">
          <PendingInvitations onInvite={() => setInviteOpen(true)} />
        </TabsContent>
      </Tabs>

      {/* Dialogs */}
      <InviteMemberDialog open={inviteOpen} onOpenChange={setInviteOpen} />

      <ChangeMemberRoleDialog
        open={!!roleDialogMember}
        onOpenChange={(open) => !open && setRoleDialogMember(null)}
        member={roleDialogMember}
      />

      {/* Remove Member Confirmation */}
      <AlertDialog open={!!removeTarget} onOpenChange={() => setRemoveTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('members.removeMember')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('members.confirmRemove')}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleRemoveMember}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {t('members.removeMember')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Bulk Remove Confirmation */}
      <AlertDialog open={bulkRemoveOpen} onOpenChange={setBulkRemoveOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('members.removeSelected')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('members.confirmRemoveMultiple', { count: selectedIds.length })}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleBulkRemove}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {t('members.removeSelected')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { DataTableAdvanced, type Column } from '@/shared/components/data-table/DataTableAdvanced';
import { Badge } from '@/shared/components/ui/badge';
import { Button } from '@/shared/components/ui/button';
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import { MoreHorizontal, Trash2, Mail } from 'lucide-react';
import { useInvitations, useRevokeInvitation } from '../hooks';
import type { TenantInvitation } from '../types';

interface PendingInvitationsProps {
  onInvite: () => void;
}

export function PendingInvitations({ onInvite }: PendingInvitationsProps) {
  const { t } = useTranslation('team');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [revokeTarget, setRevokeTarget] = useState<TenantInvitation | null>(null);

  const { data, isLoading, refetch } = useInvitations({ page, pageSize });
  const revokeInvitation = useRevokeInvitation();

  const handleRevoke = async () => {
    if (!revokeTarget) return;
    await revokeInvitation.mutateAsync(revokeTarget.id);
    setRevokeTarget(null);
  };

  const columns: Column<TenantInvitation>[] = [
    {
      key: 'email',
      header: t('invitations.email'),
      cell: (row) => (
        <div className="flex items-center gap-2">
          <Mail className="h-4 w-4 text-muted-foreground" />
          <span className="font-medium">{row.email}</span>
        </div>
      ),
    },
    {
      key: 'invitedByName',
      header: t('invitations.invitedBy'),
    },
    {
      key: 'status',
      header: t('invitations.status'),
      cell: (row) => (
        <Badge variant={row.isExpired ? 'destructive' : 'warning'}>
          {row.isExpired ? t('invitations.expired') : t('invitations.pending')}
        </Badge>
      ),
    },
    {
      key: 'expiresAt',
      header: t('invitations.expires'),
      cell: (row) => new Date(row.expiresAt).toLocaleDateString(),
    },
    {
      key: 'actions',
      header: t('invitations.actions'),
      className: 'w-[70px]',
      cell: (row) => (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" className="h-8 w-8">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem
              className="text-destructive"
              onClick={() => setRevokeTarget(row)}
            >
              <Trash2 className="me-2 h-4 w-4" />
              {t('invitations.revoke')}
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      ),
    },
  ];

  return (
    <>
      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        page={page}
        totalPages={data?.totalPages}
        total={data?.totalCount}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        onRefresh={() => refetch()}
        emptyVariant="team"
        emptyTitle={t('empty.invitations.title')}
        emptyDescription={t('empty.invitations.description')}
        emptyAction={
          <Button onClick={onInvite}>
            {t('invite.title')}
          </Button>
        }
        actions={
          <Button onClick={onInvite}>
            {t('invite.title')}
          </Button>
        }
      />

      <AlertDialog open={!!revokeTarget} onOpenChange={() => setRevokeTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('invitations.revoke')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('invitations.confirmRevoke')}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleRevoke}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {t('invitations.revoke')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}

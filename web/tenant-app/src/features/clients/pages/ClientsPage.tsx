import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { DataTableAdvanced, type Column } from '@/shared/components/data-table/DataTableAdvanced';
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
import {
  MoreHorizontal,
  Plus,
  Pencil,
  Trash2,
  Mail,
  Phone,
  ToggleLeft,
  ToggleRight,
} from 'lucide-react';
import { usePermissions } from '@/features/auth/hooks/usePermissions';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { useClients, useUpdateClient, useDeleteClient } from '../hooks';
import { AddClientSheet } from '../components/AddClientSheet';
import { EditClientSheet } from '../components/EditClientSheet';
import type { ClientListDto } from '../types';

export function ClientsPage() {
  const { t } = useTranslation('clients');
  const { hasPermission } = usePermissions();

  // Table state
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  // Dialog state
  const [addOpen, setAddOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<ClientListDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<ClientListDto | null>(null);

  // Data
  const queryParams = useMemo(() => ({
    page,
    pageSize,
    search: search || undefined,
    sort: '-createdAt',
  }), [page, pageSize, search]);

  const { data, isLoading, refetch } = useClients(queryParams);
  const updateClient = useUpdateClient();
  const deleteClient = useDeleteClient();

  const handleDelete = async () => {
    if (!deleteTarget) return;
    await deleteClient.mutateAsync(deleteTarget.id);
    setDeleteTarget(null);
  };

  const handleToggleActive = async (item: ClientListDto) => {
    await updateClient.mutateAsync({
      id: item.id,
      data: { isActive: !item.isActive },
    });
  };

  const columns: Column<ClientListDto>[] = [
    {
      key: 'name',
      header: t('columns.name'),
      cell: (row) => (
        <div className="min-w-0">
          <p className="font-medium truncate">{row.nameEn}</p>
          {row.nameAr && (
            <p className="text-xs text-muted-foreground truncate" dir="rtl">
              {row.nameAr}
            </p>
          )}
        </div>
      ),
    },
    {
      key: 'nationalId',
      header: t('columns.nationalId'),
      cell: (row) => (
        <span className="text-sm">{row.nationalId || '—'}</span>
      ),
    },
    {
      key: 'contact',
      header: t('columns.contact'),
      cell: (row) => (
        <div className="flex flex-col gap-0.5 text-sm">
          {row.email ? (
            <span className="flex items-center gap-1.5 text-muted-foreground">
              <Mail className="h-3 w-3" />
              <span className="truncate max-w-[180px]">{row.email}</span>
            </span>
          ) : null}
          {row.phone ? (
            <span className="flex items-center gap-1.5 text-muted-foreground">
              <Phone className="h-3 w-3" />
              {row.phone}
            </span>
          ) : null}
          {!row.email && !row.phone && (
            <span className="text-xs text-muted-foreground">—</span>
          )}
        </div>
      ),
    },
    {
      key: 'city',
      header: t('columns.city'),
      cell: (row) => (
        <span className="text-sm">{row.city || '—'}</span>
      ),
    },
    {
      key: 'status',
      header: t('columns.status'),
      cell: (row) => (
        <Badge variant={row.isActive ? 'default' : 'secondary'}>
          {t(`status.${row.isActive ? 'Active' : 'Inactive'}`)}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: t('columns.actions'),
      className: 'w-[70px]',
      cell: (row) => (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" className="h-8 w-8">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            {hasPermission('clients.edit') && (
              <DropdownMenuItem onClick={() => setEditTarget(row)}>
                <Pencil className="me-2 h-4 w-4" />
                {t('actions.edit')}
              </DropdownMenuItem>
            )}
            {hasPermission('clients.edit') && (
              <DropdownMenuItem onClick={() => handleToggleActive(row)}>
                {row.isActive ? (
                  <>
                    <ToggleLeft className="me-2 h-4 w-4" />
                    {t('actions.deactivate')}
                  </>
                ) : (
                  <>
                    <ToggleRight className="me-2 h-4 w-4" />
                    {t('actions.activate')}
                  </>
                )}
              </DropdownMenuItem>
            )}
            {hasPermission('clients.delete') && (
              <>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  className="text-destructive"
                  onClick={() => setDeleteTarget(row)}
                >
                  <Trash2 className="me-2 h-4 w-4" />
                  {t('actions.delete')}
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

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        selectable
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        searchPlaceholder={t('searchPlaceholder')}
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
        emptyVariant="default"
        emptyTitle={t('empty.title')}
        emptyDescription={t('empty.description')}
        emptyAction={
          <PermissionGate permission="clients.create">
            <Button onClick={() => setAddOpen(true)}>
              <Plus className="me-2 h-4 w-4" />
              {t('addClient')}
            </Button>
          </PermissionGate>
        }
        actions={
          <PermissionGate permission="clients.create">
            <Button onClick={() => setAddOpen(true)}>
              <Plus className="me-2 h-4 w-4" />
              {t('addClient')}
            </Button>
          </PermissionGate>
        }
        bulkActions={
          <PermissionGate permission="clients.delete">
            <Button
              variant="destructive"
              size="sm"
              onClick={() => {
                const items = data?.items ?? [];
                const first = items.find((i) => selectedIds.includes(i.id));
                if (first) setDeleteTarget(first);
              }}
            >
              <Trash2 className="me-2 h-4 w-4" />
              {t('actions.delete')}
            </Button>
          </PermissionGate>
        }
      />

      {/* Add Client Sheet */}
      <AddClientSheet open={addOpen} onOpenChange={setAddOpen} />

      {/* Edit Client Sheet */}
      <EditClientSheet
        open={!!editTarget}
        onOpenChange={(open) => { if (!open) setEditTarget(null); }}
        client={editTarget}
      />

      {/* Delete Confirmation */}
      <AlertDialog open={!!deleteTarget} onOpenChange={() => setDeleteTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('delete.title')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('delete.description')}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {t('delete.confirm')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

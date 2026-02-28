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
  Ban,
  RotateCcw,
  Trash2,
  Mail,
  Phone,
} from 'lucide-react';
import { usePermissions } from '@/features/auth/hooks/usePermissions';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { useCountryRefs, getFlagEmoji } from '@/features/reference-data';
import { useSuppliers, useUnlinkSupplier, useUpdateTenantSupplier } from '../hooks';
import { AddSupplierSheet } from '../components/AddSupplierSheet';
import type { TenantSupplier } from '../types';

const statusVariant: Record<string, 'default' | 'secondary' | 'destructive'> = {
  Active: 'default',
  Suspended: 'secondary',
  Terminated: 'destructive',
};

export function SuppliersPage() {
  const { t } = useTranslation('suppliers');
  const { hasPermission } = usePermissions();

  // Table state
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  // Dialog state
  const [addOpen, setAddOpen] = useState(false);
  const [removeTarget, setRemoveTarget] = useState<TenantSupplier | null>(null);

  // Data
  const queryParams = useMemo(() => ({
    page,
    pageSize,
    search: search || undefined,
    sort: '-createdAt',
  }), [page, pageSize, search]);

  const { data, isLoading, refetch } = useSuppliers(queryParams);
  const { data: countries } = useCountryRefs();
  const unlinkSupplier = useUnlinkSupplier();
  const updateSupplier = useUpdateTenantSupplier();

  const getCountryName = (code?: string) => {
    if (!code) return '—';
    const country = countries?.find((c) => c.code === code);
    return country?.nameEn ?? code;
  };

  const handleRemove = async () => {
    if (!removeTarget) return;
    await unlinkSupplier.mutateAsync(removeTarget.id);
    setRemoveTarget(null);
  };

  const handleStatusChange = async (item: TenantSupplier, status: string) => {
    await updateSupplier.mutateAsync({ id: item.id, data: { status } });
  };

  const columns: Column<TenantSupplier>[] = [
    {
      key: 'name',
      header: t('columns.name'),
      cell: (row) => (
        <div className="min-w-0">
          <p className="font-medium truncate">{row.supplier?.nameEn ?? '—'}</p>
          {row.supplier?.nameAr && (
            <p className="text-xs text-muted-foreground truncate" dir="rtl">
              {row.supplier.nameAr}
            </p>
          )}
        </div>
      ),
    },
    {
      key: 'country',
      header: t('columns.country'),
      cell: (row) => (
        <div className="flex items-center gap-1.5">
          {row.supplier?.country && (
            <span className="text-lg leading-none">{getFlagEmoji(row.supplier.country)}</span>
          )}
          <span>{getCountryName(row.supplier?.country)}</span>
          {row.supplier?.city && (
            <span className="text-muted-foreground">/ {row.supplier.city}</span>
          )}
        </div>
      ),
    },
    {
      key: 'contact',
      header: t('columns.contact'),
      cell: (row) => (
        <div className="flex flex-col gap-0.5 text-sm">
          {row.supplier?.email ? (
            <span className="flex items-center gap-1.5 text-muted-foreground">
              <Mail className="h-3 w-3" />
              <span className="truncate max-w-[180px]">{row.supplier.email}</span>
            </span>
          ) : null}
          {row.supplier?.phone ? (
            <span className="flex items-center gap-1.5 text-muted-foreground">
              <Phone className="h-3 w-3" />
              {row.supplier.phone}
            </span>
          ) : null}
          {!row.supplier?.email && !row.supplier?.phone && (
            <span className="text-xs text-muted-foreground">{t('noContact')}</span>
          )}
        </div>
      ),
    },
    {
      key: 'status',
      header: t('columns.status'),
      cell: (row) => (
        <Badge variant={statusVariant[row.status] ?? 'secondary'}>
          {t(`status.${row.status}`)}
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
            {hasPermission('suppliers.manage') && row.status === 'Active' && (
              <DropdownMenuItem onClick={() => handleStatusChange(row, 'Suspended')}>
                <Ban className="me-2 h-4 w-4" />
                {t('actions.suspend')}
              </DropdownMenuItem>
            )}
            {hasPermission('suppliers.manage') && row.status === 'Suspended' && (
              <DropdownMenuItem onClick={() => handleStatusChange(row, 'Active')}>
                <RotateCcw className="me-2 h-4 w-4" />
                {t('actions.reactivate')}
              </DropdownMenuItem>
            )}
            {hasPermission('suppliers.delete') && (
              <>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  className="text-destructive"
                  onClick={() => setRemoveTarget(row)}
                >
                  <Trash2 className="me-2 h-4 w-4" />
                  {t('actions.remove')}
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
          <PermissionGate permission="suppliers.manage">
            <Button onClick={() => setAddOpen(true)}>
              <Plus className="me-2 h-4 w-4" />
              {t('addSupplier')}
            </Button>
          </PermissionGate>
        }
        actions={
          <PermissionGate permission="suppliers.manage">
            <Button onClick={() => setAddOpen(true)}>
              <Plus className="me-2 h-4 w-4" />
              {t('addSupplier')}
            </Button>
          </PermissionGate>
        }
        bulkActions={
          <PermissionGate permission="suppliers.delete">
            <Button
              variant="destructive"
              size="sm"
              onClick={() => {
                // bulk remove - pick first selected for confirmation
                const items = data?.items ?? [];
                const first = items.find((i) => selectedIds.includes(i.id));
                if (first) setRemoveTarget(first);
              }}
            >
              <Trash2 className="me-2 h-4 w-4" />
              {t('actions.remove')}
            </Button>
          </PermissionGate>
        }
      />

      {/* Add Supplier Sheet */}
      <AddSupplierSheet open={addOpen} onOpenChange={setAddOpen} />

      {/* Remove Confirmation */}
      <AlertDialog open={!!removeTarget} onOpenChange={() => setRemoveTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('remove.title')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('remove.description')}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleRemove}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {t('remove.confirm')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

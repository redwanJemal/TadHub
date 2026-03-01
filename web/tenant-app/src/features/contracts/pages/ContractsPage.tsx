import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { DataTableAdvanced, type Column, type Filter } from '@/shared/components/data-table/DataTableAdvanced';
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
import { MoreHorizontal, Trash2, Eye, RefreshCw, Plus, FileText } from 'lucide-react';
import { usePermissions } from '@/features/auth/hooks/usePermissions';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { useContracts, useDeleteContract } from '../hooks';
import { ContractStatusBadge } from '../components/ContractStatusBadge';
import { ContractTypeBadge } from '../components/ContractTypeBadge';
import { ContractStatusTransitionDialog } from '../components/ContractStatusTransitionDialog';
import { ALL_STATUSES, ALL_TYPES } from '../constants';
import type { ContractListDto } from '../types';

export function ContractsPage() {
  const { t } = useTranslation('contracts');
  const navigate = useNavigate();
  const { hasPermission } = usePermissions();

  // Table state
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [typeFilter, setTypeFilter] = useState<string | undefined>();

  // Dialog state
  const [deleteTarget, setDeleteTarget] = useState<ContractListDto | null>(null);
  const [transitionTarget, setTransitionTarget] = useState<ContractListDto | null>(null);

  // Data
  const queryParams = useMemo(() => ({
    page,
    pageSize,
    search: search || undefined,
    sort: '-createdAt',
    'filter[status]': statusFilter,
    'filter[type]': typeFilter,
  }), [page, pageSize, search, statusFilter, typeFilter]);

  const { data, isLoading, refetch } = useContracts(queryParams);
  const deleteMutation = useDeleteContract();

  const handleDelete = async () => {
    if (!deleteTarget) return;
    await deleteMutation.mutateAsync(deleteTarget.id);
    setDeleteTarget(null);
  };

  const filters: Filter[] = [
    {
      key: 'status',
      label: t('filters.status'),
      options: ALL_STATUSES.map((s) => ({ label: t(`status.${s}`), value: s })),
      value: statusFilter,
    },
    {
      key: 'type',
      label: t('filters.type'),
      options: ALL_TYPES.map((tp) => ({ label: t(`type.${tp}`), value: tp })),
      value: typeFilter,
    },
  ];

  const handleFilterChange = (key: string, value: string | undefined) => {
    if (key === 'status') setStatusFilter(value);
    if (key === 'type') setTypeFilter(value);
    setPage(1);
  };

  const columns: Column<ContractListDto>[] = [
    {
      key: 'contractCode',
      header: t('columns.contractCode'),
      cell: (row) => (
        <span className="font-mono text-sm">{row.contractCode}</span>
      ),
    },
    {
      key: 'type',
      header: t('columns.type'),
      cell: (row) => <ContractTypeBadge type={row.type} />,
    },
    {
      key: 'worker',
      header: t('columns.worker'),
      cell: (row) => (
        <div className="min-w-0">
          <p className="font-medium truncate">{row.worker?.fullNameEn ?? '—'}</p>
          {row.worker?.workerCode && (
            <p className="text-xs text-muted-foreground font-mono">{row.worker.workerCode}</p>
          )}
        </div>
      ),
    },
    {
      key: 'client',
      header: t('columns.client'),
      cell: (row) => row.client?.nameEn ?? '—',
    },
    {
      key: 'status',
      header: t('columns.status'),
      cell: (row) => <ContractStatusBadge status={row.status} />,
    },
    {
      key: 'startDate',
      header: t('columns.startDate'),
      cell: (row) => row.startDate ? new Date(row.startDate).toLocaleDateString() : '—',
    },
    {
      key: 'rate',
      header: t('columns.rate'),
      cell: (row) => (
        <span>
          {row.rate.toLocaleString()} {row.currency}/{t(`ratePeriod.${row.ratePeriod}`)}
        </span>
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
            <DropdownMenuItem onClick={() => navigate(`/contracts/${row.id}`)}>
              <Eye className="me-2 h-4 w-4" />
              {t('actions.view')}
            </DropdownMenuItem>
            <DropdownMenuItem
              onClick={() => {
                const params = new URLSearchParams({
                  contractId: row.id,
                  clientId: row.clientId,
                  ...(row.workerId ? { workerId: row.workerId } : {}),
                  contractCode: row.contractCode,
                  ...(row.client?.nameEn ? { clientName: row.client.nameEn } : {}),
                  ...(row.worker?.fullNameEn ? { workerName: row.worker.fullNameEn } : {}),
                });
                navigate(`/finance/invoices/new?${params.toString()}`);
              }}
            >
              <FileText className="me-2 h-4 w-4" />
              Create Invoice
            </DropdownMenuItem>
            {hasPermission('contracts.manage_status') && (
              <DropdownMenuItem onClick={() => setTransitionTarget(row)}>
                <RefreshCw className="me-2 h-4 w-4" />
                {t('actions.changeStatus')}
              </DropdownMenuItem>
            )}
            {hasPermission('contracts.delete') && (
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
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t('title')}</h1>
          <p className="text-muted-foreground">{t('description')}</p>
        </div>
        <PermissionGate permission="contracts.create">
          <Button onClick={() => navigate('/contracts/new')}>
            <Plus className="me-2 h-4 w-4" />
            {t('addContract')}
          </Button>
        </PermissionGate>
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
        filters={filters}
        onFilterChange={handleFilterChange}
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
      />

      {/* Status Transition Dialog */}
      {transitionTarget && (
        <ContractStatusTransitionDialog
          open={!!transitionTarget}
          onOpenChange={(open) => { if (!open) setTransitionTarget(null); }}
          contractId={transitionTarget.id}
          currentStatus={transitionTarget.status}
        />
      )}

      {/* Delete Confirmation */}
      <AlertDialog open={!!deleteTarget} onOpenChange={() => setDeleteTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('deleteDialog.title')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('deleteDialog.description')}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {t('deleteDialog.confirm')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

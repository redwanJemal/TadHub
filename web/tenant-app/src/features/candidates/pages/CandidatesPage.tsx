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
import { MoreHorizontal, Plus, Trash2, Eye, RefreshCw } from 'lucide-react';
import { useCountryRefs, getFlagEmoji } from '@/features/reference-data';
import { useCandidates, useDeleteCandidate } from '../hooks';
import { StatusBadge } from '../components/StatusBadge';
import { StatusTransitionDialog } from '../components/StatusTransitionDialog';
import { ALL_STATUSES, ALL_SOURCE_TYPES } from '../constants';
import type { CandidateListDto } from '../types';

export function CandidatesPage() {
  const { t } = useTranslation('candidates');
  const navigate = useNavigate();

  // Table state
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [sourceTypeFilter, setSourceTypeFilter] = useState<string | undefined>();

  // Dialog state
  const [deleteTarget, setDeleteTarget] = useState<CandidateListDto | null>(null);
  const [transitionTarget, setTransitionTarget] = useState<CandidateListDto | null>(null);

  // Data
  const queryParams = useMemo(() => ({
    page,
    pageSize,
    search: search || undefined,
    sort: '-createdAt',
    'filter[status]': statusFilter,
    'filter[sourceType]': sourceTypeFilter,
  }), [page, pageSize, search, statusFilter, sourceTypeFilter]);

  const { data, isLoading, refetch } = useCandidates(queryParams);
  const { data: countries } = useCountryRefs();
  const deleteMutation = useDeleteCandidate();

  const getCountryName = (code?: string) => {
    if (!code) return '—';
    const country = countries?.find((c) => c.code === code);
    return country?.nameEn ?? code;
  };

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
      key: 'sourceType',
      label: t('filters.sourceType'),
      options: ALL_SOURCE_TYPES.map((s) => ({ label: t(`sourceType.${s}`), value: s })),
      value: sourceTypeFilter,
    },
  ];

  const handleFilterChange = (key: string, value: string | undefined) => {
    if (key === 'status') setStatusFilter(value);
    if (key === 'sourceType') setSourceTypeFilter(value);
    setPage(1);
  };

  const columns: Column<CandidateListDto>[] = [
    {
      key: 'name',
      header: t('columns.name'),
      cell: (row) => (
        <div className="flex items-center gap-2 min-w-0">
          {row.photoUrl ? (
            <img src={row.photoUrl} alt="" className="h-8 w-8 rounded-full object-cover border shrink-0" />
          ) : (
            <div className="h-8 w-8 rounded-full bg-muted flex items-center justify-center text-xs font-medium shrink-0">
              {row.fullNameEn.charAt(0)}
            </div>
          )}
          <div className="min-w-0">
            <p className="font-medium truncate">{row.fullNameEn}</p>
            {row.fullNameAr && (
              <p className="text-xs text-muted-foreground truncate" dir="rtl">
                {row.fullNameAr}
              </p>
            )}
          </div>
        </div>
      ),
    },
    {
      key: 'nationality',
      header: t('columns.nationality'),
      cell: (row) => (
        <div className="flex items-center gap-1.5">
          {row.nationality && (
            <span className="text-lg leading-none">{getFlagEmoji(row.nationality)}</span>
          )}
          <span>{getCountryName(row.nationality)}</span>
        </div>
      ),
    },
    {
      key: 'sourceType',
      header: t('columns.sourceType'),
      cell: (row) => t(`sourceType.${row.sourceType}`),
    },
    {
      key: 'supplier',
      header: t('columns.supplier'),
      cell: (row) => row.supplier?.name ?? '—',
    },
    {
      key: 'jobCategory',
      header: t('columns.jobCategory'),
      cell: (row) => row.jobCategory?.name ?? '—',
    },
    {
      key: 'status',
      header: t('columns.status'),
      cell: (row) => <StatusBadge status={row.status} />,
    },
    {
      key: 'createdAt',
      header: t('columns.createdAt'),
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
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
            <DropdownMenuItem onClick={() => navigate(`/candidates/${row.id}`)}>
              <Eye className="me-2 h-4 w-4" />
              {t('actions.view')}
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => setTransitionTarget(row)}>
              <RefreshCw className="me-2 h-4 w-4" />
              {t('actions.changeStatus')}
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              className="text-destructive"
              onClick={() => setDeleteTarget(row)}
            >
              <Trash2 className="me-2 h-4 w-4" />
              {t('actions.delete')}
            </DropdownMenuItem>
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
        emptyAction={
          <Button onClick={() => navigate('/candidates/new')}>
            <Plus className="me-2 h-4 w-4" />
            {t('addCandidate')}
          </Button>
        }
        actions={
          <Button onClick={() => navigate('/candidates/new')}>
            <Plus className="me-2 h-4 w-4" />
            {t('addCandidate')}
          </Button>
        }
      />

      {/* Status Transition Dialog */}
      {transitionTarget && (
        <StatusTransitionDialog
          open={!!transitionTarget}
          onOpenChange={(open) => { if (!open) setTransitionTarget(null); }}
          candidateId={transitionTarget.id}
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

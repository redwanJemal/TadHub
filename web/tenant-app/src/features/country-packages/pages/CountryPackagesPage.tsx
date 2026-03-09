import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, MoreHorizontal, Eye, Pencil, Trash2, Star } from 'lucide-react';
import { toast } from 'sonner';
import { DataTableAdvanced, type Column, type Filter } from '@/shared/components/data-table';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { PermissionGate } from '@/shared/components/PermissionGate';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
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
import { useCountryPackages, useDeleteCountryPackage } from '../hooks';
import type { CountryPackageListDto } from '../types';

export function CountryPackagesPage() {
  const { t, i18n } = useTranslation('countryPackages');
  const navigate = useNavigate();

  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [activeFilter, setActiveFilter] = useState<string | undefined>();
  const [deleteTarget, setDeleteTarget] = useState<CountryPackageListDto | null>(null);

  const queryParams = useMemo(() => ({
    page,
    pageSize,
    search: search || undefined,
    sort: '-createdAt',
    'filter[isActive]': activeFilter,
  }), [page, pageSize, search, activeFilter]);

  const { data, isLoading, refetch } = useCountryPackages(queryParams);
  const deleteMutation = useDeleteCountryPackage();

  const isAr = i18n.language === 'ar';

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await deleteMutation.mutateAsync(deleteTarget.id);
      toast.success(t('deleteSuccess'));
    } catch {
      toast.error(t('deleteError'));
    }
    setDeleteTarget(null);
  };

  const handleFilterChange = (key: string, value: string | undefined) => {
    if (key === 'isActive') setActiveFilter(value);
    setPage(1);
  };

  const filters: Filter[] = [
    {
      key: 'isActive',
      label: t('filters.status'),
      options: [
        { label: t('common:active'), value: 'true' },
        { label: t('common:inactive'), value: 'false' },
      ],
      value: activeFilter,
    },
  ];

  const columns: Column<CountryPackageListDto>[] = [
    {
      key: 'name',
      header: t('columns.name'),
      cell: (row) => (
        <div className="flex items-center gap-2">
          <span className="font-medium">{row.name}</span>
          {row.isDefault && (
            <Star className="h-4 w-4 text-amber-500 fill-amber-500" />
          )}
        </div>
      ),
    },
    {
      key: 'country',
      header: t('columns.country'),
      cell: (row) => (
        <div className="flex items-center gap-2">
          <span className="text-xs font-mono text-muted-foreground">{row.countryCode}</span>
          <span>{isAr ? row.countryNameAr : row.countryNameEn}</span>
        </div>
      ),
    },
    {
      key: 'totalPackagePrice',
      header: t('columns.totalPrice'),
      cell: (row) => (
        <span className="font-mono">
          {row.totalPackagePrice.toLocaleString()} {row.currency}
        </span>
      ),
    },
    {
      key: 'guaranteePeriod',
      header: t('columns.guarantee'),
      cell: (row) => <span>{t(`guaranteePeriod.${row.defaultGuaranteePeriod}`)}</span>,
    },
    {
      key: 'effectiveFrom',
      header: t('columns.effective'),
      cell: (row) => (
        <span className="text-sm">
          {row.effectiveFrom}
          {row.effectiveTo ? ` — ${row.effectiveTo}` : ''}
        </span>
      ),
    },
    {
      key: 'isActive',
      header: t('columns.status'),
      cell: (row) => (
        <Badge variant={row.isActive ? 'default' : 'secondary'}>
          {row.isActive ? t('common:active') : t('common:inactive')}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: '',
      cell: (row) => (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="sm">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={() => navigate(`/country-packages/${row.id}`)}>
              <Eye className="me-2 h-4 w-4" />
              {t('common:edit')}
            </DropdownMenuItem>
            <PermissionGate permission="packages.edit">
              <DropdownMenuItem onClick={() => navigate(`/country-packages/${row.id}`)}>
                <Pencil className="me-2 h-4 w-4" />
                {t('common:edit')}
              </DropdownMenuItem>
            </PermissionGate>
            <PermissionGate permission="packages.delete">
              <DropdownMenuItem
                className="text-destructive"
                onClick={() => setDeleteTarget(row)}
              >
                <Trash2 className="me-2 h-4 w-4" />
                {t('common:delete')}
              </DropdownMenuItem>
            </PermissionGate>
          </DropdownMenuContent>
        </DropdownMenu>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t('title')}</h1>
          <p className="text-muted-foreground">{t('description')}</p>
        </div>
        <PermissionGate permission="packages.create">
          <Button onClick={() => navigate('/country-packages/new')}>
            <Plus className="me-2 h-4 w-4" />
            {t('addPackage')}
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
        onSearchChange={(val) => { setSearch(val); setPage(1); }}
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
      />

      <AlertDialog open={!!deleteTarget} onOpenChange={() => setDeleteTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('deleteConfirm.title')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('deleteConfirm.message', { name: deleteTarget?.name })}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {t('common:delete')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

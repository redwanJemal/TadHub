import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, RotateCcw, ShieldAlert } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { DataTableAdvanced } from '@/shared/components/data-table/DataTableAdvanced';
import type { Column } from '@/shared/components/data-table/DataTableAdvanced';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { ReturneeCaseStatusBadge } from '../components/ReturneeCaseStatusBadge';
import { useReturneeCases } from '../hooks';
import { ALL_STATUSES, RETURN_TYPES } from '../constants';
import type { ReturneeCaseListDto, ReturneeCaseStatus } from '../types';

export function ReturneeCasesListPage() {
  const { t } = useTranslation('returnees');
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [typeFilter, setTypeFilter] = useState<string | undefined>();

  const { data, isLoading, refetch } = useReturneeCases({
    page,
    pageSize,
    search: search || undefined,
    filter: {
      ...(statusFilter ? { status: statusFilter } : {}),
      ...(typeFilter ? { returnType: typeFilter } : {}),
    },
    sort: '-createdAt',
  });

  const columns: Column<ReturneeCaseListDto>[] = [
    {
      key: 'caseCode',
      header: t('code'),
      cell: (row) => (
        <button
          onClick={() => navigate(`/returnees/${row.id}`)}
          className="font-medium text-primary hover:underline"
        >
          {row.caseCode}
        </button>
      ),
      sortable: true,
    },
    {
      key: 'status',
      header: t('status'),
      cell: (row) => <ReturneeCaseStatusBadge status={row.status as ReturneeCaseStatus} />,
    },
    {
      key: 'returnType',
      header: t('return_type'),
      cell: (row) => (
        <Badge variant="outline">
          {row.returnType === 'ReturnToOffice' ? t('return_to_office') : t('return_to_country')}
        </Badge>
      ),
    },
    {
      key: 'worker',
      header: t('worker'),
      cell: (row) => row.worker?.fullNameEn || '—',
    },
    {
      key: 'client',
      header: t('client'),
      cell: (row) => row.client?.nameEn || '—',
    },
    {
      key: 'returnDate',
      header: t('return_date'),
      cell: (row) => row.returnDate,
      sortable: true,
    },
    {
      key: 'monthsWorked',
      header: t('months_worked'),
      cell: (row) => row.monthsWorked,
      sortable: true,
    },
    {
      key: 'isWithinGuarantee',
      header: t('guarantee'),
      cell: (row) => row.isWithinGuarantee ? (
        <Badge variant="warning" className="gap-1">
          <ShieldAlert className="h-3 w-3" />
          {t('within_guarantee')}
        </Badge>
      ) : (
        <span className="text-muted-foreground">—</span>
      ),
    },
    {
      key: 'refundAmount',
      header: t('refund'),
      cell: (row) => row.refundAmount != null
        ? `${row.refundAmount.toLocaleString()} AED`
        : '—',
    },
    {
      key: 'createdAt',
      header: t('created'),
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
      sortable: true,
    },
  ];

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">{t('title')}</h1>
          <p className="text-sm text-muted-foreground">{t('subtitle')}</p>
        </div>
        <PermissionGate permission="returnees.create">
          <Button onClick={() => navigate('/returnees/new')}>
            <Plus className="mr-2 h-4 w-4" />
            {t('create_case')}
          </Button>
        </PermissionGate>
      </div>

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('search_placeholder')}
        searchValue={search}
        onSearchChange={setSearch}
        filters={[
          {
            key: 'status',
            label: t('status'),
            options: ALL_STATUSES.map((s) => ({ label: s, value: s })),
            value: statusFilter,
          },
          {
            key: 'returnType',
            label: t('return_type'),
            options: RETURN_TYPES.map((rt) => ({ label: rt.label, value: rt.value })),
            value: typeFilter,
          },
        ]}
        onFilterChange={(key, value) => {
          if (key === 'status') setStatusFilter(value);
          if (key === 'returnType') setTypeFilter(value);
          setPage(1);
        }}
        page={page}
        totalPages={data?.totalPages ?? 1}
        total={data?.totalCount ?? 0}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={(ps) => { setPageSize(ps); setPage(1); }}
        onRefresh={() => refetch()}
        emptyIcon={RotateCcw}
        emptyTitle={t('empty_title')}
        emptyDescription={t('empty_description')}
      />
    </div>
  );
}

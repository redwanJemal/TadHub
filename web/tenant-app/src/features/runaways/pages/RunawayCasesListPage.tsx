import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, AlertTriangle, ShieldAlert } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { DataTableAdvanced } from '@/shared/components/data-table/DataTableAdvanced';
import type { Column } from '@/shared/components/data-table/DataTableAdvanced';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { RunawayCaseStatusBadge } from '../components/RunawayCaseStatusBadge';
import { useRunawayCases } from '../hooks';
import { ALL_STATUSES } from '../constants';
import type { RunawayCaseListDto, RunawayCaseStatus } from '../types';

export function RunawayCasesListPage() {
  const { t } = useTranslation('runaways');
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string | undefined>();

  const { data, isLoading, refetch } = useRunawayCases({
    page,
    pageSize,
    search: search || undefined,
    filter: {
      ...(statusFilter ? { status: statusFilter } : {}),
    },
    sort: '-createdAt',
  });

  const columns: Column<RunawayCaseListDto>[] = [
    {
      key: 'caseCode',
      header: t('code'),
      cell: (row) => (
        <button
          onClick={() => navigate(`/runaways/${row.id}`)}
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
      cell: (row) => <RunawayCaseStatusBadge status={row.status as RunawayCaseStatus} />,
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
      key: 'reportedDate',
      header: t('reported_date'),
      cell: (row) => new Date(row.reportedDate).toLocaleDateString(),
      sortable: true,
    },
    {
      key: 'policeReportNumber',
      header: t('police_report_number'),
      cell: (row) => row.policeReportNumber || '—',
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
        <PermissionGate permission="runaways.report">
          <Button onClick={() => navigate('/runaways/new')}>
            <Plus className="mr-2 h-4 w-4" />
            {t('report_case')}
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
        ]}
        onFilterChange={(key, value) => {
          if (key === 'status') setStatusFilter(value);
          setPage(1);
        }}
        page={page}
        totalPages={data?.totalPages ?? 1}
        total={data?.totalCount ?? 0}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={(ps) => { setPageSize(ps); setPage(1); }}
        onRefresh={() => refetch()}
        emptyIcon={AlertTriangle}
        emptyTitle={t('empty_title')}
        emptyDescription={t('empty_description')}
      />
    </div>
  );
}

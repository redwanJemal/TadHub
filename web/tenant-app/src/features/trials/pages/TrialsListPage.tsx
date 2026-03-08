import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, ClipboardCheck } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { DataTableAdvanced } from '@/shared/components/data-table/DataTableAdvanced';
import type { Column } from '@/shared/components/data-table/DataTableAdvanced';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { TrialStatusBadge } from '../components/TrialStatusBadge';
import { useTrials } from '../hooks';
import { ALL_STATUSES } from '../constants';
import type { TrialListDto, TrialStatus } from '../types';

export function TrialsListPage() {
  const { t } = useTranslation('trials');
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string | undefined>();

  const { data, isLoading, refetch } = useTrials({
    page,
    pageSize,
    search: search || undefined,
    filter: statusFilter ? { status: statusFilter } : undefined,
    sort: '-createdAt',
  });

  const columns: Column<TrialListDto>[] = [
    {
      key: 'trialCode',
      header: t('code'),
      cell: (row) => (
        <button
          onClick={() => navigate(`/trials/${row.id}`)}
          className="font-medium text-primary hover:underline"
        >
          {row.trialCode}
        </button>
      ),
      sortable: true,
    },
    {
      key: 'status',
      header: t('status'),
      cell: (row) => <TrialStatusBadge status={row.status as TrialStatus} />,
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
      key: 'startDate',
      header: t('start_date'),
      cell: (row) => row.startDate,
      sortable: true,
    },
    {
      key: 'endDate',
      header: t('end_date'),
      cell: (row) => row.endDate,
    },
    {
      key: 'daysRemaining',
      header: t('days_remaining'),
      cell: (row) => {
        if (row.status !== 'Active') return '—';
        return (
          <span className={row.daysRemaining <= 1 ? 'font-semibold text-destructive' : ''}>
            {row.daysRemaining}
          </span>
        );
      },
    },
    {
      key: 'outcome',
      header: t('outcome'),
      cell: (row) => row.outcome || '—',
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
        <PermissionGate permission="trials.create">
          <Button onClick={() => navigate('/trials/new')}>
            <Plus className="mr-2 h-4 w-4" />
            {t('create_trial')}
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
        emptyIcon={ClipboardCheck}
        emptyTitle={t('empty_title')}
        emptyDescription={t('empty_description')}
      />
    </div>
  );
}

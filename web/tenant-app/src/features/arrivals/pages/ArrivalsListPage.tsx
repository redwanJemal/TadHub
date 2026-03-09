import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plane, Plus, AlertTriangle } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { DataTableAdvanced } from '@/shared/components/data-table/DataTableAdvanced';
import type { Column, Filter } from '@/shared/components/data-table/DataTableAdvanced';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { useArrivals } from '../hooks';
import { ArrivalStatusBadge } from '../components/ArrivalStatusBadge';
import { ALL_STATUSES } from '../constants';
import type { ArrivalListDto } from '../types';

export function ArrivalsListPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string | undefined>();

  const { data, isLoading, refetch } = useArrivals({
    page,
    pageSize,
    search: search || undefined,
    filters: statusFilter ? [{ name: 'status', values: [statusFilter] }] : undefined,
  });

  const today = new Date().toISOString().split('T')[0];

  const isOverdue = (row: ArrivalListDto) => {
    return (
      (row.status === 'Scheduled' || row.status === 'InTransit') &&
      row.scheduledArrivalDate < today
    );
  };

  const columns: Column<ArrivalListDto>[] = [
    {
      key: 'arrivalCode',
      header: t('arrivals.code', 'Code'),
      sortable: true,
      cell: (row) => (
        <div className="flex items-center gap-2">
          <button
            onClick={() => navigate(`/arrivals/${row.id}`)}
            className="font-medium text-primary hover:underline"
          >
            {row.arrivalCode}
          </button>
          {isOverdue(row) && (
            <AlertTriangle className="h-4 w-4 text-destructive" />
          )}
        </div>
      ),
    },
    {
      key: 'status',
      header: t('status', 'Status'),
      cell: (row) => <ArrivalStatusBadge status={row.status} />,
    },
    {
      key: 'worker',
      header: t('arrivals.worker', 'Worker'),
      cell: (row) => row.worker?.fullNameEn || '-',
    },
    {
      key: 'flightNumber',
      header: t('arrivals.flight', 'Flight'),
      cell: (row) => row.flightNumber || '-',
    },
    {
      key: 'scheduledArrivalDate',
      header: t('arrivals.scheduledDate', 'Scheduled Date'),
      sortable: true,
      cell: (row) => (
        <span className={isOverdue(row) ? 'text-destructive font-medium' : ''}>
          {row.scheduledArrivalDate}
        </span>
      ),
    },
    {
      key: 'driverName',
      header: t('arrivals.driver', 'Driver'),
      cell: (row) => row.driverName || '-',
    },
    {
      key: 'createdAt',
      header: t('arrivals.createdAt', 'Created'),
      sortable: true,
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
    },
  ];

  const filters: Filter[] = [
    {
      key: 'status',
      label: t('status', 'Status'),
      options: [
        { label: t('allStatuses', 'All Statuses'), value: '' },
        ...ALL_STATUSES.map((s) => ({ label: s, value: s })),
      ],
      value: statusFilter ?? '',
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t('arrivals.title', 'Arrivals')}</h1>
          <p className="text-muted-foreground">
            {t('arrivals.subtitle', 'Manage maid arrivals and pickups')}
          </p>
        </div>
        <PermissionGate permission="arrivals.create">
          <Button onClick={() => navigate('/arrivals/new')}>
            <Plus className="mr-2 h-4 w-4" />
            {t('arrivals.schedule', 'Schedule Arrival')}
          </Button>
        </PermissionGate>
      </div>

      <DataTableAdvanced<ArrivalListDto>
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('arrivals.searchPlaceholder', 'Search by code, flight, or driver...')}
        searchValue={search}
        onSearchChange={setSearch}
        filters={filters}
        onFilterChange={(key, value) => {
          if (key === 'status') setStatusFilter(value || undefined);
          setPage(1);
        }}
        page={page}
        totalPages={data?.totalPages ?? 1}
        total={data?.totalCount}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={(size) => {
          setPageSize(size);
          setPage(1);
        }}
        onRefresh={() => refetch()}
        emptyIcon={Plane}
        emptyTitle={t('arrivals.empty', 'No arrivals yet')}
        emptyDescription={t('arrivals.emptyDescription', 'Schedule an arrival to get started.')}
        emptyAction={
          <PermissionGate permission="arrivals.create">
            <Button onClick={() => navigate('/arrivals/new')}>
              <Plus className="mr-2 h-4 w-4" />
              {t('arrivals.schedule', 'Schedule Arrival')}
            </Button>
          </PermissionGate>
        }
      />
    </div>
  );
}

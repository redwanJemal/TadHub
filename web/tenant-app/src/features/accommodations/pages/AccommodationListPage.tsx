import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Building2, Plus, Calendar, Users } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { DataTableAdvanced } from '@/shared/components/data-table/DataTableAdvanced';
import type { Column, Filter } from '@/shared/components/data-table/DataTableAdvanced';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { useAccommodations, useAccommodationCounts } from '../hooks';
import { AccommodationStatusBadge } from '../components/AccommodationStatusBadge';
import { ALL_STATUSES, DEPARTURE_REASON_CONFIG } from '../constants';
import type { AccommodationStayListDto, DepartureReason } from '../types';

export function AccommodationListPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [view, setView] = useState<'all' | 'current' | 'daily'>('current');
  const [selectedDate, setSelectedDate] = useState(
    new Date().toISOString().split('T')[0]
  );

  const { data: counts, isLoading: countsLoading } = useAccommodationCounts();

  const queryParams = {
    page,
    pageSize,
    search: search || undefined,
    filters: [
      ...(statusFilter ? [{ name: 'status', values: [statusFilter] }] : []),
      ...(view === 'current' ? [{ name: 'status', values: ['CheckedIn'] }] : []),
    ],
  };

  const { data, isLoading, refetch } = useAccommodations(
    view === 'all' || view === 'current' ? queryParams : { page, pageSize, search: search || undefined }
  );

  const columns: Column<AccommodationStayListDto>[] = [
    {
      key: 'stayCode',
      header: t('accommodations.code', 'Code'),
      sortable: true,
      cell: (row) => (
        <button
          onClick={() => navigate(`/accommodations/${row.id}`)}
          className="font-medium text-primary hover:underline"
        >
          {row.stayCode}
        </button>
      ),
    },
    {
      key: 'status',
      header: t('status', 'Status'),
      cell: (row) => <AccommodationStatusBadge status={row.status} />,
    },
    {
      key: 'worker',
      header: t('accommodations.worker', 'Worker'),
      cell: (row) => row.worker?.fullNameEn || '-',
    },
    {
      key: 'room',
      header: t('accommodations.room', 'Room'),
      cell: (row) => row.room || '-',
    },
    {
      key: 'location',
      header: t('accommodations.location', 'Location'),
      cell: (row) => row.location || '-',
    },
    {
      key: 'checkInDate',
      header: t('accommodations.checkInDate', 'Check-In'),
      sortable: true,
      cell: (row) => new Date(row.checkInDate).toLocaleDateString(),
    },
    {
      key: 'checkOutDate',
      header: t('accommodations.checkOutDate', 'Check-Out'),
      cell: (row) => row.checkOutDate ? new Date(row.checkOutDate).toLocaleDateString() : '-',
    },
    {
      key: 'departureReason',
      header: t('accommodations.departureReason', 'Departure Reason'),
      cell: (row) => {
        if (!row.departureReason) return '-';
        const config = DEPARTURE_REASON_CONFIG[row.departureReason as DepartureReason];
        return config?.label || row.departureReason;
      },
    },
  ];

  const filters: Filter[] = view === 'all' ? [
    {
      key: 'status',
      label: t('status', 'Status'),
      options: ALL_STATUSES.map((s) => ({ label: s === 'CheckedIn' ? 'Checked In' : 'Checked Out', value: s })),
      value: statusFilter ?? 'all',
    },
  ] : [];

  const checkedInCount = counts?.CheckedIn ?? 0;
  const checkedOutCount = counts?.CheckedOut ?? 0;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t('accommodations.title', 'Accommodation')}</h1>
          <p className="text-muted-foreground">
            {t('accommodations.subtitle', 'Manage maid accommodation stays')}
          </p>
        </div>
        <PermissionGate permission="accommodations.manage">
          <Button onClick={() => navigate('/accommodations/check-in')}>
            <Plus className="mr-2 h-4 w-4" />
            {t('accommodations.checkIn', 'Check In')}
          </Button>
        </PermissionGate>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card
          className={`cursor-pointer transition-colors ${view === 'current' ? 'border-primary' : ''}`}
          onClick={() => { setView('current'); setPage(1); setStatusFilter(undefined); }}
        >
          <CardContent className="flex items-center gap-4 pt-6">
            <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-green-100 dark:bg-green-900/20">
              <Users className="h-6 w-6 text-green-600 dark:text-green-400" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('accommodations.currentOccupants', 'Current Occupants')}</p>
              {countsLoading ? (
                <Skeleton className="h-8 w-16" />
              ) : (
                <p className="text-2xl font-bold">{checkedInCount}</p>
              )}
            </div>
          </CardContent>
        </Card>
        <Card
          className={`cursor-pointer transition-colors ${view === 'all' ? 'border-primary' : ''}`}
          onClick={() => { setView('all'); setPage(1); }}
        >
          <CardContent className="flex items-center gap-4 pt-6">
            <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-blue-100 dark:bg-blue-900/20">
              <Building2 className="h-6 w-6 text-blue-600 dark:text-blue-400" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('accommodations.allStays', 'All Stays')}</p>
              {countsLoading ? (
                <Skeleton className="h-8 w-16" />
              ) : (
                <p className="text-2xl font-bold">{checkedInCount + checkedOutCount}</p>
              )}
            </div>
          </CardContent>
        </Card>
        <Card
          className={`cursor-pointer transition-colors ${view === 'daily' ? 'border-primary' : ''}`}
          onClick={() => { setView('daily'); setPage(1); }}
        >
          <CardContent className="flex items-center gap-4 pt-6">
            <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-purple-100 dark:bg-purple-900/20">
              <Calendar className="h-6 w-6 text-purple-600 dark:text-purple-400" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('accommodations.dailyList', 'Daily List')}</p>
              <p className="text-sm text-muted-foreground">{t('accommodations.selectDate', 'Select a date')}</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Date picker for daily view */}
      {view === 'daily' && (
        <div className="flex items-center gap-4">
          <label className="text-sm font-medium">{t('accommodations.date', 'Date')}:</label>
          <input
            type="date"
            value={selectedDate}
            onChange={(e) => { setSelectedDate(e.target.value); setPage(1); }}
            className="rounded-md border px-3 py-2 text-sm"
          />
        </div>
      )}

      <DataTableAdvanced<AccommodationStayListDto>
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('accommodations.searchPlaceholder', 'Search by code, room, or location...')}
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
        emptyIcon={Building2}
        emptyTitle={t('accommodations.empty', 'No accommodation stays yet')}
        emptyDescription={t('accommodations.emptyDescription', 'Check in a maid to get started.')}
        emptyAction={
          <PermissionGate permission="accommodations.manage">
            <Button onClick={() => navigate('/accommodations/check-in')}>
              <Plus className="mr-2 h-4 w-4" />
              {t('accommodations.checkIn', 'Check In')}
            </Button>
          </PermissionGate>
        }
      />
    </div>
  );
}

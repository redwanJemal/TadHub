import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ChevronLeft } from 'lucide-react';
import { DataTableAdvanced, type Column, type Filter } from '@/shared/components/data-table';
import { useArrivalsReport } from '../hooks';
import { ExportCsvButton } from '../components/ExportCsvButton';
import { DateRangeFilter } from '../components/DateRangeFilter';
import { ARRIVAL_STATUSES } from '../constants';
import type { ArrivalReportItem } from '../types';

export function ArrivalsReportPage() {
  const { t } = useTranslation('reports');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [filterStatus, setFilterStatus] = useState<string>();
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [appliedFrom, setAppliedFrom] = useState('');
  const [appliedTo, setAppliedTo] = useState('');

  const params: Record<string, unknown> = { page, pageSize, search: search || undefined };
  if (filterStatus) params['filter[status]'] = filterStatus;
  if (appliedFrom) params['filter[from]'] = appliedFrom;
  if (appliedTo) params['filter[to]'] = appliedTo;

  const { data, isLoading } = useArrivalsReport(params);

  const columns: Column<ArrivalReportItem>[] = [
    { key: 'arrivalCode', header: t('reports.arrivals.arrivalCode') },
    { key: 'status', header: t('reports.arrivals.status') },
    { key: 'workerNameEn', header: t('reports.arrivals.worker'), cell: (row) => row.workerNameEn ?? '-' },
    { key: 'flightNumber', header: t('reports.arrivals.flight'), cell: (row) => row.flightNumber ?? '-' },
    { key: 'airportName', header: t('reports.arrivals.airport'), cell: (row) => row.airportName ?? '-' },
    { key: 'scheduledArrivalDate', header: t('reports.arrivals.scheduledDate') },
    { key: 'scheduledArrivalTime', header: t('reports.arrivals.scheduledTime'), cell: (row) => row.scheduledArrivalTime ?? '-' },
    { key: 'actualArrivalTime', header: t('reports.arrivals.actualTime'), cell: (row) => row.actualArrivalTime ?? '-' },
    { key: 'driverName', header: t('reports.arrivals.driver'), cell: (row) => row.driverName ?? '-' },
  ];

  const filters: Filter[] = [
    { key: 'status', label: t('reports.arrivals.status'), options: ARRIVAL_STATUSES.map((s) => ({ label: s, value: s })), value: filterStatus },
  ];

  const csvColumns = [
    { key: 'arrivalCode', label: 'Arrival Code' },
    { key: 'status', label: 'Status' },
    { key: 'workerNameEn', label: 'Worker' },
    { key: 'flightNumber', label: 'Flight' },
    { key: 'airportName', label: 'Airport' },
    { key: 'scheduledArrivalDate', label: 'Scheduled Date' },
    { key: 'scheduledArrivalTime', label: 'Scheduled Time' },
    { key: 'actualArrivalTime', label: 'Actual Time' },
    { key: 'driverName', label: 'Driver' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link to="/reports" className="text-muted-foreground hover:text-foreground">
            <ChevronLeft className="h-5 w-5" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">{t('reports.arrivals.title')}</h1>
            <p className="text-muted-foreground text-sm">{t('reports.arrivals.description')}</p>
          </div>
        </div>
        <ExportCsvButton data={data?.items ?? []} filename="arrivals-report" columns={csvColumns} />
      </div>

      <DateRangeFilter from={fromDate} to={toDate} onFromChange={setFromDate} onToChange={setToDate} onApply={() => { setAppliedFrom(fromDate); setAppliedTo(toDate); setPage(1); }} />

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('reports.arrivals.arrivalCode')}
        searchValue={search}
        onSearchChange={setSearch}
        filters={filters}
        onFilterChange={(key, value) => { if (key === 'status') setFilterStatus(value); }}
        page={page}
        totalPages={data?.totalPages ?? 0}
        total={data?.totalCount ?? 0}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
      />
    </div>
  );
}

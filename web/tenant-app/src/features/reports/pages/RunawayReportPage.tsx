import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ChevronLeft } from 'lucide-react';
import { DataTableAdvanced, type Column, type Filter } from '@/shared/components/data-table';
import { Badge } from '@/shared/components/ui/badge';
import { useRunawayReport } from '../hooks';
import { ExportCsvButton } from '../components/ExportCsvButton';
import { DateRangeFilter } from '../components/DateRangeFilter';
import { RUNAWAY_STATUSES } from '../constants';
import type { RunawayReportItem } from '../types';

export function RunawayReportPage() {
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

  const { data, isLoading } = useRunawayReport(params);

  const columns: Column<RunawayReportItem>[] = [
    { key: 'caseCode', header: t('reports.runaways.caseCode') },
    { key: 'status', header: t('reports.runaways.status') },
    { key: 'reportedDate', header: t('reports.runaways.reportedDate'), cell: (row) => row.reportedDate ? new Date(row.reportedDate).toLocaleDateString() : '-' },
    { key: 'workerNameEn', header: t('reports.runaways.worker'), cell: (row) => row.workerNameEn ?? '-' },
    { key: 'clientNameEn', header: t('reports.runaways.client'), cell: (row) => row.clientNameEn ?? '-' },
    { key: 'isWithinGuarantee', header: t('reports.runaways.guarantee'), cell: (row) => <Badge variant={row.isWithinGuarantee ? 'default' : 'secondary'}>{row.isWithinGuarantee ? 'Yes' : 'No'}</Badge> },
    { key: 'policeReportNumber', header: t('reports.runaways.policeReport'), cell: (row) => row.policeReportNumber ?? '-' },
    { key: 'totalExpenses', header: t('reports.runaways.totalExpenses'), cell: (row) => row.totalExpenses.toLocaleString() },
    { key: 'settledAt', header: t('reports.runaways.settledAt'), cell: (row) => row.settledAt ? new Date(row.settledAt).toLocaleDateString() : '-' },
  ];

  const filters: Filter[] = [
    { key: 'status', label: t('reports.runaways.status'), options: RUNAWAY_STATUSES.map((s) => ({ label: s, value: s })), value: filterStatus },
  ];

  const csvColumns = [
    { key: 'caseCode', label: 'Case Code' },
    { key: 'status', label: 'Status' },
    { key: 'reportedDate', label: 'Reported Date' },
    { key: 'workerNameEn', label: 'Worker' },
    { key: 'clientNameEn', label: 'Client' },
    { key: 'isWithinGuarantee', label: 'Within Guarantee' },
    { key: 'policeReportNumber', label: 'Police Report #' },
    { key: 'totalExpenses', label: 'Total Expenses' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link to="/reports" className="text-muted-foreground hover:text-foreground">
            <ChevronLeft className="h-5 w-5" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">{t('reports.runaways.title')}</h1>
            <p className="text-muted-foreground text-sm">{t('reports.runaways.description')}</p>
          </div>
        </div>
        <ExportCsvButton data={data?.items ?? []} filename="runaway-report" columns={csvColumns} />
      </div>

      <DateRangeFilter from={fromDate} to={toDate} onFromChange={setFromDate} onToChange={setToDate} onApply={() => { setAppliedFrom(fromDate); setAppliedTo(toDate); setPage(1); }} />

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('reports.runaways.caseCode')}
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

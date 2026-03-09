import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ChevronLeft } from 'lucide-react';
import { DataTableAdvanced, type Column, type Filter } from '@/shared/components/data-table';
import { Badge } from '@/shared/components/ui/badge';
import { useReturneeReport } from '../hooks';
import { ExportCsvButton } from '../components/ExportCsvButton';
import { DateRangeFilter } from '../components/DateRangeFilter';
import { RETURNEE_STATUSES } from '../constants';
import type { ReturneeReportItem } from '../types';

export function ReturneeReportPage() {
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

  const { data, isLoading } = useReturneeReport(params);

  const columns: Column<ReturneeReportItem>[] = [
    { key: 'caseCode', header: t('reports.returnees.caseCode') },
    { key: 'status', header: t('reports.returnees.status') },
    { key: 'returnType', header: t('reports.returnees.returnType'), cell: (row) => row.returnType ?? '-' },
    { key: 'returnDate', header: t('reports.returnees.returnDate'), cell: (row) => row.returnDate ?? '-' },
    { key: 'workerNameEn', header: t('reports.returnees.worker'), cell: (row) => row.workerNameEn ?? '-' },
    { key: 'clientNameEn', header: t('reports.returnees.client'), cell: (row) => row.clientNameEn ?? '-' },
    { key: 'refundAmount', header: t('reports.returnees.refundAmount'), cell: (row) => row.refundAmount?.toLocaleString() ?? '-' },
    { key: 'isWithinGuarantee', header: t('reports.returnees.guarantee'), cell: (row) => <Badge variant={row.isWithinGuarantee ? 'default' : 'secondary'}>{row.isWithinGuarantee ? 'Yes' : 'No'}</Badge> },
    { key: 'settledAt', header: t('reports.returnees.settledAt'), cell: (row) => row.settledAt ? new Date(row.settledAt).toLocaleDateString() : '-' },
  ];

  const filters: Filter[] = [
    { key: 'status', label: t('reports.returnees.status'), options: RETURNEE_STATUSES.map((s) => ({ label: s, value: s })), value: filterStatus },
  ];

  const csvColumns = [
    { key: 'caseCode', label: 'Case Code' },
    { key: 'status', label: 'Status' },
    { key: 'returnType', label: 'Return Type' },
    { key: 'returnDate', label: 'Return Date' },
    { key: 'workerNameEn', label: 'Worker' },
    { key: 'clientNameEn', label: 'Client' },
    { key: 'totalAmountPaid', label: 'Amount Paid' },
    { key: 'refundAmount', label: 'Refund Amount' },
    { key: 'isWithinGuarantee', label: 'Within Guarantee' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link to="/reports" className="text-muted-foreground hover:text-foreground">
            <ChevronLeft className="h-5 w-5" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">{t('reports.returnees.title')}</h1>
            <p className="text-muted-foreground text-sm">{t('reports.returnees.description')}</p>
          </div>
        </div>
        <ExportCsvButton data={data?.items ?? []} filename="returnee-report" columns={csvColumns} />
      </div>

      <DateRangeFilter from={fromDate} to={toDate} onFromChange={setFromDate} onToChange={setToDate} onApply={() => { setAppliedFrom(fromDate); setAppliedTo(toDate); setPage(1); }} />

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('reports.returnees.caseCode')}
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

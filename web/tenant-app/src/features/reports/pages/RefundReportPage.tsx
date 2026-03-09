import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ChevronLeft } from 'lucide-react';
import { DataTableAdvanced, type Column } from '@/shared/components/data-table';
import { useRefundReport } from '../hooks';
import { ExportCsvButton } from '../components/ExportCsvButton';
import { DateRangeFilter } from '../components/DateRangeFilter';
import type { RefundReportItem } from '../types';

type Row = RefundReportItem & { id: string };

export function RefundReportPage() {
  const { t } = useTranslation('reports');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [appliedFrom, setAppliedFrom] = useState('');
  const [appliedTo, setAppliedTo] = useState('');

  const params: Record<string, unknown> = { page, pageSize, search: search || undefined };
  if (appliedFrom) params['filter[from]'] = appliedFrom;
  if (appliedTo) params['filter[to]'] = appliedTo;

  const { data, isLoading } = useRefundReport(params);

  const items = useMemo(() =>
    (data?.items ?? []).map((item) => ({ ...item, id: item.paymentId })),
    [data?.items]
  );

  const columns: Column<Row>[] = [
    { key: 'paymentNumber', header: t('reports.refunds.paymentNumber') },
    { key: 'amount', header: t('reports.refunds.amount'), cell: (row) => row.amount.toLocaleString() },
    { key: 'refundAmount', header: t('reports.refunds.refundAmount'), cell: (row) => row.refundAmount?.toLocaleString() ?? '-' },
    { key: 'method', header: t('reports.refunds.method'), cell: (row) => row.method ?? '-' },
    { key: 'paymentDate', header: t('reports.refunds.paymentDate') },
    { key: 'clientNameEn', header: t('reports.refunds.client'), cell: (row) => row.clientNameEn ?? '-' },
    { key: 'invoiceNumber', header: t('reports.refunds.invoice'), cell: (row) => row.invoiceNumber ?? '-' },
  ];

  const csvColumns = [
    { key: 'paymentNumber', label: 'Payment #' },
    { key: 'amount', label: 'Amount' },
    { key: 'refundAmount', label: 'Refund Amount' },
    { key: 'method', label: 'Method' },
    { key: 'paymentDate', label: 'Payment Date' },
    { key: 'clientNameEn', label: 'Client' },
    { key: 'invoiceNumber', label: 'Invoice' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link to="/reports" className="text-muted-foreground hover:text-foreground">
            <ChevronLeft className="h-5 w-5" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">{t('reports.refunds.title')}</h1>
            <p className="text-muted-foreground text-sm">{t('reports.refunds.description')}</p>
          </div>
        </div>
        <ExportCsvButton data={items} filename="refund-report" columns={csvColumns} />
      </div>

      <DateRangeFilter from={fromDate} to={toDate} onFromChange={setFromDate} onToChange={setToDate} onApply={() => { setAppliedFrom(fromDate); setAppliedTo(toDate); setPage(1); }} />

      <DataTableAdvanced
        columns={columns}
        data={items}
        isLoading={isLoading}
        searchPlaceholder={t('reports.refunds.paymentNumber')}
        searchValue={search}
        onSearchChange={setSearch}
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

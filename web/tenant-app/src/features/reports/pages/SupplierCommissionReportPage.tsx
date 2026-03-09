import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ChevronLeft } from 'lucide-react';
import { DataTableAdvanced, type Column } from '@/shared/components/data-table';
import { useSupplierCommissions } from '../hooks';
import { ExportCsvButton } from '../components/ExportCsvButton';
import { DateRangeFilter } from '../components/DateRangeFilter';
import type { SupplierCommissionItem } from '../types';

type Row = SupplierCommissionItem & { id: string };

export function SupplierCommissionReportPage() {
  const { t } = useTranslation('reports');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [appliedFrom, setAppliedFrom] = useState('');
  const [appliedTo, setAppliedTo] = useState('');

  const params: Record<string, unknown> = { page, pageSize };
  if (appliedFrom) params['filter[from]'] = appliedFrom;
  if (appliedTo) params['filter[to]'] = appliedTo;

  const { data, isLoading } = useSupplierCommissions(params);

  const items = useMemo(() =>
    (data?.items ?? []).map((item) => ({ ...item, id: item.supplierId })),
    [data?.items]
  );

  const columns: Column<Row>[] = [
    { key: 'supplierNameEn', header: t('reports.supplierCommissions.supplier'), cell: (row) => row.supplierNameEn ?? '-' },
    { key: 'paymentCount', header: t('reports.supplierCommissions.paymentCount') },
    { key: 'totalPaid', header: t('reports.supplierCommissions.totalPaid'), cell: (row) => row.totalPaid.toLocaleString() },
    { key: 'totalPending', header: t('reports.supplierCommissions.totalPending'), cell: (row) => row.totalPending.toLocaleString() },
    { key: 'total', header: t('reports.supplierCommissions.total'), cell: (row) => (row.totalPaid + row.totalPending).toLocaleString() },
  ];

  const csvColumns = [
    { key: 'supplierNameEn', label: 'Supplier' },
    { key: 'paymentCount', label: 'Payment Count' },
    { key: 'totalPaid', label: 'Total Paid' },
    { key: 'totalPending', label: 'Total Pending' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link to="/reports" className="text-muted-foreground hover:text-foreground">
            <ChevronLeft className="h-5 w-5" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">{t('reports.supplierCommissions.title')}</h1>
            <p className="text-muted-foreground text-sm">{t('reports.supplierCommissions.description')}</p>
          </div>
        </div>
        <ExportCsvButton data={items} filename="supplier-commissions" columns={csvColumns} />
      </div>

      <DateRangeFilter from={fromDate} to={toDate} onFromChange={setFromDate} onToChange={setToDate} onApply={() => { setAppliedFrom(fromDate); setAppliedTo(toDate); setPage(1); }} />

      <DataTableAdvanced
        columns={columns}
        data={items}
        isLoading={isLoading}
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

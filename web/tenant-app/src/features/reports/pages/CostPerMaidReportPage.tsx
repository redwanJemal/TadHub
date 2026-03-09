import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ChevronLeft } from 'lucide-react';
import { DataTableAdvanced, type Column } from '@/shared/components/data-table';
import { useCostPerMaid } from '../hooks';
import { ExportCsvButton } from '../components/ExportCsvButton';
import type { CostPerMaidItem } from '../types';

type Row = CostPerMaidItem & { id: string };

export function CostPerMaidReportPage() {
  const { t } = useTranslation('reports');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');

  const params: Record<string, unknown> = { page, pageSize, search: search || undefined };
  const { data, isLoading } = useCostPerMaid(params);

  const items = useMemo(() =>
    (data?.items ?? []).map((item) => ({ ...item, id: item.workerId })),
    [data?.items]
  );

  const fmt = (n: number) => n > 0 ? n.toLocaleString() : '-';

  const columns: Column<Row>[] = [
    { key: 'workerCode', header: t('reports.costPerMaid.workerCode'), cell: (row) => row.workerCode ?? '-' },
    { key: 'workerNameEn', header: t('reports.costPerMaid.workerName'), cell: (row) => row.workerNameEn ?? '-' },
    { key: 'procurementCost', header: t('reports.costPerMaid.procurement'), cell: (row) => fmt(row.procurementCost) },
    { key: 'flightCost', header: t('reports.costPerMaid.flight'), cell: (row) => fmt(row.flightCost) },
    { key: 'medicalCost', header: t('reports.costPerMaid.medical'), cell: (row) => fmt(row.medicalCost) },
    { key: 'visaCost', header: t('reports.costPerMaid.visa'), cell: (row) => fmt(row.visaCost) },
    { key: 'insuranceCost', header: t('reports.costPerMaid.insurance'), cell: (row) => fmt(row.insuranceCost) },
    { key: 'accommodationCost', header: t('reports.costPerMaid.accommodation'), cell: (row) => fmt(row.accommodationCost) },
    { key: 'trainingCost', header: t('reports.costPerMaid.training'), cell: (row) => fmt(row.trainingCost) },
    { key: 'otherCost', header: t('reports.costPerMaid.other'), cell: (row) => fmt(row.otherCost) },
    { key: 'totalCost', header: t('reports.costPerMaid.total'), cell: (row) => row.totalCost.toLocaleString(), className: 'font-semibold' },
  ];

  const csvColumns = [
    { key: 'workerCode', label: 'Worker Code' },
    { key: 'workerNameEn', label: 'Worker Name' },
    { key: 'procurementCost', label: 'Procurement' },
    { key: 'flightCost', label: 'Flight' },
    { key: 'medicalCost', label: 'Medical' },
    { key: 'visaCost', label: 'Visa' },
    { key: 'insuranceCost', label: 'Insurance' },
    { key: 'accommodationCost', label: 'Accommodation' },
    { key: 'trainingCost', label: 'Training' },
    { key: 'otherCost', label: 'Other' },
    { key: 'totalCost', label: 'Total Cost' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link to="/reports" className="text-muted-foreground hover:text-foreground">
            <ChevronLeft className="h-5 w-5" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">{t('reports.costPerMaid.title')}</h1>
            <p className="text-muted-foreground text-sm">{t('reports.costPerMaid.description')}</p>
          </div>
        </div>
        <ExportCsvButton data={items} filename="cost-per-maid" columns={csvColumns} />
      </div>

      <DataTableAdvanced
        columns={columns}
        data={items}
        isLoading={isLoading}
        searchPlaceholder={t('reports.costPerMaid.workerCode')}
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

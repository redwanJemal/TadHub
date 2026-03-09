import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ChevronLeft } from 'lucide-react';
import { DataTableAdvanced, type Column } from '@/shared/components/data-table';
import { useDeployedReport } from '../hooks';
import { ExportCsvButton } from '../components/ExportCsvButton';
import type { DeployedReportItem } from '../types';

type Row = DeployedReportItem & { id: string };

export function DeployedReportPage() {
  const { t } = useTranslation('reports');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');

  const params: Record<string, unknown> = { page, pageSize, search: search || undefined };
  const { data, isLoading } = useDeployedReport(params);

  const items = useMemo(() =>
    (data?.items ?? []).map((item) => ({ ...item, id: item.workerId })),
    [data?.items]
  );

  const columns: Column<Row>[] = [
    { key: 'workerCode', header: t('reports.deployed.workerCode') },
    { key: 'fullNameEn', header: t('reports.deployed.workerName') },
    { key: 'nationality', header: t('reports.deployed.nationality') },
    { key: 'contractCode', header: t('reports.deployed.contractCode'), cell: (row) => row.contractCode ?? '-' },
    { key: 'clientNameEn', header: t('reports.deployed.client'), cell: (row) => row.clientNameEn ?? '-' },
    { key: 'startDate', header: t('reports.deployed.startDate') },
    { key: 'endDate', header: t('reports.deployed.endDate'), cell: (row) => row.endDate ?? '-' },
    { key: 'contractType', header: t('reports.deployed.contractType') },
    { key: 'rate', header: t('reports.deployed.rate'), cell: (row) => row.rate ? `${row.rate.toLocaleString()} / ${row.ratePeriod}` : '-' },
  ];

  const csvColumns = [
    { key: 'workerCode', label: 'Worker Code' },
    { key: 'fullNameEn', label: 'Worker Name' },
    { key: 'nationality', label: 'Nationality' },
    { key: 'contractCode', label: 'Contract Code' },
    { key: 'clientNameEn', label: 'Client' },
    { key: 'startDate', label: 'Start Date' },
    { key: 'endDate', label: 'End Date' },
    { key: 'contractType', label: 'Contract Type' },
    { key: 'rate', label: 'Rate' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link to="/reports" className="text-muted-foreground hover:text-foreground">
            <ChevronLeft className="h-5 w-5" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">{t('reports.deployed.title')}</h1>
            <p className="text-muted-foreground text-sm">{t('reports.deployed.description')}</p>
          </div>
        </div>
        <ExportCsvButton data={items} filename="deployed-report" columns={csvColumns} />
      </div>

      <DataTableAdvanced
        columns={columns}
        data={items}
        isLoading={isLoading}
        searchPlaceholder={t('reports.deployed.workerCode')}
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

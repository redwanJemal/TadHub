import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ChevronLeft } from 'lucide-react';
import { DataTableAdvanced, type Column, type Filter } from '@/shared/components/data-table';
import { useInventoryReport } from '../hooks';
import { ExportCsvButton } from '../components/ExportCsvButton';
import { INVENTORY_STATUSES } from '../constants';
import type { InventoryReportItem } from '../types';

export function InventoryReportPage() {
  const { t } = useTranslation('reports');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [filterStatus, setFilterStatus] = useState<string>();
  const [filterLocation, setFilterLocation] = useState<string>();
  const [filterNationality, setFilterNationality] = useState<string>();

  const params: Record<string, unknown> = { page, pageSize, search: search || undefined };
  if (filterStatus) params['filter[status]'] = filterStatus;
  if (filterLocation) params['filter[location]'] = filterLocation;
  if (filterNationality) params['filter[nationality]'] = filterNationality;

  const { data, isLoading } = useInventoryReport(params);

  const columns: Column<InventoryReportItem>[] = [
    { key: 'workerCode', header: t('reports.inventory.workerCode') },
    { key: 'fullNameEn', header: t('reports.inventory.name') },
    { key: 'nationality', header: t('reports.inventory.nationality') },
    { key: 'location', header: t('reports.inventory.location') },
    { key: 'status', header: t('reports.inventory.status') },
    { key: 'experienceYears', header: t('reports.inventory.experience') },
    { key: 'monthlySalary', header: t('reports.inventory.salary'), cell: (row) => row.monthlySalary?.toLocaleString() ?? '-' },
    { key: 'supplierNameEn', header: t('reports.inventory.supplier'), cell: (row) => row.supplierNameEn ?? '-' },
  ];

  const filters: Filter[] = [
    { key: 'status', label: t('reports.inventory.status'), options: INVENTORY_STATUSES.map((s) => ({ label: s, value: s })), value: filterStatus },
    { key: 'location', label: t('reports.inventory.location'), options: [{ label: 'Abroad', value: 'Abroad' }, { label: 'InCountry', value: 'InCountry' }], value: filterLocation },
  ];

  const csvColumns = [
    { key: 'workerCode', label: 'Worker Code' },
    { key: 'fullNameEn', label: 'Name (EN)' },
    { key: 'fullNameAr', label: 'Name (AR)' },
    { key: 'nationality', label: 'Nationality' },
    { key: 'location', label: 'Location' },
    { key: 'status', label: 'Status' },
    { key: 'experienceYears', label: 'Experience' },
    { key: 'monthlySalary', label: 'Salary' },
    { key: 'supplierNameEn', label: 'Supplier' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link to="/reports" className="text-muted-foreground hover:text-foreground">
            <ChevronLeft className="h-5 w-5" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">{t('reports.inventory.title')}</h1>
            <p className="text-muted-foreground text-sm">{t('reports.inventory.description')}</p>
          </div>
        </div>
        <ExportCsvButton data={data?.items ?? []} filename="inventory-report" columns={csvColumns} />
      </div>

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('reports.inventory.workerCode')}
        searchValue={search}
        onSearchChange={setSearch}
        filters={filters}
        onFilterChange={(key, value) => {
          if (key === 'status') setFilterStatus(value);
          if (key === 'location') setFilterLocation(value);
          if (key === 'nationality') setFilterNationality(value);
        }}
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

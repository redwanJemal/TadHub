import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plane, CheckCircle, XCircle } from 'lucide-react';
import { Badge } from '@/shared/components/ui/badge';
import { DataTableAdvanced, type Column } from '@/shared/components/data-table';
import { useSupplierArrivals } from '../hooks';
import { ARRIVAL_STATUS_CONFIG } from '../constants';
import type { SupplierArrivalListDto } from '../types';

export function SupplierArrivalsPage() {
  const { t } = useTranslation('supplierPortal');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const { data, isLoading } = useSupplierArrivals({
    page,
    pageSize,
    search: search || undefined,
  });

  const columns: Column<SupplierArrivalListDto>[] = [
    {
      key: 'workerNameEn',
      header: t('arrivals.worker'),
    },
    {
      key: 'flightNumber',
      header: t('arrivals.flight'),
      cell: (row) => <span className="font-mono text-sm">{row.flightNumber || '—'}</span>,
    },
    {
      key: 'arrivalDate',
      header: t('arrivals.arrivalDate'),
      cell: (row) => row.arrivalDate ? new Date(row.arrivalDate).toLocaleDateString() : '—',
    },
    {
      key: 'airportCode',
      header: t('arrivals.airport'),
    },
    {
      key: 'status',
      header: t('arrivals.status'),
      cell: (row) => {
        const config = row.status ? ARRIVAL_STATUS_CONFIG[row.status] : undefined;
        return config ? (
          <Badge variant={config.variant}>{config.label}</Badge>
        ) : (
          <span>{row.status || '—'}</span>
        );
      },
    },
    {
      key: 'hasPreTravelPhoto',
      header: t('arrivals.preTravelPhoto'),
      cell: (row) => row.hasPreTravelPhoto ? (
        <CheckCircle className="h-4 w-4 text-green-600" />
      ) : (
        <XCircle className="h-4 w-4 text-muted-foreground" />
      ),
    },
  ];

  return (
    <div className="space-y-4 p-6">
      <h1 className="text-2xl font-semibold">{t('arrivals.title')}</h1>

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('arrivals.searchPlaceholder')}
        searchValue={search}
        onSearchChange={setSearch}
        page={page}
        totalPages={data?.totalPages ?? 1}
        total={data?.totalCount ?? 0}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={(size) => { setPageSize(size); setPage(1); }}
        emptyIcon={Plane}
        emptyTitle={t('arrivals.emptyTitle')}
        emptyDescription={t('arrivals.emptyDescription')}
      />
    </div>
  );
}

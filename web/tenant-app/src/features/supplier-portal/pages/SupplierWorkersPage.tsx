import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { HardHat } from 'lucide-react';
import { Badge } from '@/shared/components/ui/badge';
import { DataTableAdvanced, type Column } from '@/shared/components/data-table';
import { useSupplierWorkers } from '../hooks';
import { WORKER_STATUS_CONFIG } from '../constants';
import type { SupplierWorkerListDto } from '../types';

export function SupplierWorkersPage() {
  const { t } = useTranslation('supplierPortal');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const { data, isLoading } = useSupplierWorkers({
    page,
    pageSize,
    search: search || undefined,
  });

  const columns: Column<SupplierWorkerListDto>[] = [
    {
      key: 'workerCode',
      header: t('workers.code'),
      cell: (row) => <span className="font-mono text-sm">{row.workerCode || '—'}</span>,
    },
    {
      key: 'fullNameEn',
      header: t('workers.name'),
      cell: (row) => (
        <div>
          <p className="font-medium">{row.fullNameEn || '—'}</p>
          {row.fullNameAr && <p className="text-xs text-muted-foreground">{row.fullNameAr}</p>}
        </div>
      ),
    },
    {
      key: 'nationality',
      header: t('workers.nationality'),
    },
    {
      key: 'status',
      header: t('workers.status'),
      cell: (row) => {
        const config = row.status ? WORKER_STATUS_CONFIG[row.status] : undefined;
        return config ? (
          <Badge variant={config.variant}>{config.label}</Badge>
        ) : (
          <span>{row.status || '—'}</span>
        );
      },
    },
    {
      key: 'createdAt',
      header: t('workers.createdAt'),
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
    },
  ];

  return (
    <div className="space-y-4 p-6">
      <h1 className="text-2xl font-semibold">{t('workers.title')}</h1>

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('workers.searchPlaceholder')}
        searchValue={search}
        onSearchChange={setSearch}
        page={page}
        totalPages={data?.totalPages ?? 1}
        total={data?.totalCount ?? 0}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={(size) => { setPageSize(size); setPage(1); }}
        emptyIcon={HardHat}
        emptyTitle={t('workers.emptyTitle')}
        emptyDescription={t('workers.emptyDescription')}
      />
    </div>
  );
}

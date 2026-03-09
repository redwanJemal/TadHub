import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { DollarSign } from 'lucide-react';
import { Badge } from '@/shared/components/ui/badge';
import { DataTableAdvanced, type Column, type Filter } from '@/shared/components/data-table';
import { useSupplierCommissions } from '../hooks';
import { COMMISSION_STATUS_CONFIG, ALL_COMMISSION_STATUSES } from '../constants';
import type { SupplierCommissionDto } from '../types';

export function SupplierCommissionsPage() {
  const { t } = useTranslation('supplierPortal');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [statusFilter, setStatusFilter] = useState<string | undefined>();

  const { data, isLoading } = useSupplierCommissions({
    page,
    pageSize,
    search: search || undefined,
    filters: statusFilter ? { status: statusFilter } : undefined,
  });

  const columns: Column<SupplierCommissionDto>[] = [
    {
      key: 'referenceNumber',
      header: t('commissions.reference'),
      cell: (row) => <span className="font-mono text-sm">{row.referenceNumber || '—'}</span>,
    },
    {
      key: 'workerNameEn',
      header: t('commissions.worker'),
    },
    {
      key: 'amount',
      header: t('commissions.amount'),
      cell: (row) => (
        <span className="font-mono">
          {row.currency || 'AED'} {row.amount.toLocaleString()}
        </span>
      ),
    },
    {
      key: 'status',
      header: t('commissions.status'),
      cell: (row) => {
        const config = row.status ? COMMISSION_STATUS_CONFIG[row.status] : undefined;
        return config ? (
          <Badge variant={config.variant}>{config.label}</Badge>
        ) : (
          <span>{row.status || '—'}</span>
        );
      },
    },
    {
      key: 'paymentDate',
      header: t('commissions.paymentDate'),
      cell: (row) => row.paymentDate ? new Date(row.paymentDate).toLocaleDateString() : '—',
    },
    {
      key: 'createdAt',
      header: t('commissions.createdAt'),
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
    },
  ];

  const filters: Filter[] = [
    {
      key: 'status',
      label: t('commissions.status'),
      options: ALL_COMMISSION_STATUSES.map((s) => ({
        label: COMMISSION_STATUS_CONFIG[s]?.label || s,
        value: s,
      })),
      value: statusFilter,
    },
  ];

  return (
    <div className="space-y-4 p-6">
      <h1 className="text-2xl font-semibold">{t('commissions.title')}</h1>

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('commissions.searchPlaceholder')}
        searchValue={search}
        onSearchChange={setSearch}
        filters={filters}
        onFilterChange={(key, value) => {
          if (key === 'status') setStatusFilter(value);
          setPage(1);
        }}
        page={page}
        totalPages={data?.totalPages ?? 1}
        total={data?.totalCount ?? 0}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={(size) => { setPageSize(size); setPage(1); }}
        emptyIcon={DollarSign}
        emptyTitle={t('commissions.emptyTitle')}
        emptyDescription={t('commissions.emptyDescription')}
      />
    </div>
  );
}

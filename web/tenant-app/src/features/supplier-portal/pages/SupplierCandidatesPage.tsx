import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { UserSearch, Plus } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { DataTableAdvanced, type Column, type Filter } from '@/shared/components/data-table';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { useSupplierCandidates } from '../hooks';
import { CANDIDATE_STATUS_CONFIG, ALL_CANDIDATE_STATUSES } from '../constants';
import type { SupplierCandidateListDto } from '../types';

export function SupplierCandidatesPage() {
  const { t } = useTranslation('supplierPortal');
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [statusFilter, setStatusFilter] = useState<string | undefined>();

  const { data, isLoading } = useSupplierCandidates({
    page,
    pageSize,
    search: search || undefined,
    filters: statusFilter ? { status: statusFilter } : undefined,
  });

  const columns: Column<SupplierCandidateListDto>[] = [
    {
      key: 'fullNameEn',
      header: t('candidates.name'),
      cell: (row) => (
        <div>
          <p className="font-medium">{row.fullNameEn || '—'}</p>
          {row.fullNameAr && <p className="text-xs text-muted-foreground">{row.fullNameAr}</p>}
        </div>
      ),
    },
    {
      key: 'nationality',
      header: t('candidates.nationality'),
    },
    {
      key: 'passportNumber',
      header: t('candidates.passport'),
    },
    {
      key: 'status',
      header: t('candidates.status'),
      cell: (row) => {
        const config = row.status ? CANDIDATE_STATUS_CONFIG[row.status] : undefined;
        return config ? (
          <Badge variant={config.variant}>{config.label}</Badge>
        ) : (
          <span>{row.status || '—'}</span>
        );
      },
    },
    {
      key: 'createdAt',
      header: t('candidates.registeredAt'),
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
    },
  ];

  const filters: Filter[] = [
    {
      key: 'status',
      label: t('candidates.status'),
      options: ALL_CANDIDATE_STATUSES.map((s) => ({
        label: CANDIDATE_STATUS_CONFIG[s]?.label || s,
        value: s,
      })),
      value: statusFilter,
    },
  ];

  return (
    <div className="space-y-4 p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">{t('candidates.title')}</h1>
        <PermissionGate permission="supplier_portal.create_candidate">
          <Button onClick={() => navigate('/candidates/new')}>
            <Plus className="mr-2 h-4 w-4" />
            {t('candidates.register')}
          </Button>
        </PermissionGate>
      </div>

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('candidates.searchPlaceholder')}
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
        emptyIcon={UserSearch}
        emptyTitle={t('candidates.emptyTitle')}
        emptyDescription={t('candidates.emptyDescription')}
      />
    </div>
  );
}

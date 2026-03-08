import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, FileText } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { DataTableAdvanced } from '@/shared/components/data-table/DataTableAdvanced';
import type { Column } from '@/shared/components/data-table/DataTableAdvanced';
import { useVisaApplications } from '../hooks';
import { STATUS_CONFIG, ALL_STATUSES, ALL_VISA_TYPES } from '../constants';
import type { VisaApplicationListDto, VisaApplicationStatus } from '../types';

export function VisaApplicationsListPage() {
  const { t } = useTranslation('visas');
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [typeFilter, setTypeFilter] = useState<string | undefined>();

  const { data, isLoading, refetch } = useVisaApplications({
    page,
    pageSize,
    search: search || undefined,
    filter: {
      ...(statusFilter ? { status: statusFilter } : {}),
      ...(typeFilter ? { visaType: typeFilter } : {}),
    },
    sort: '-createdAt',
  });

  const columns: Column<VisaApplicationListDto>[] = [
    {
      key: 'applicationCode',
      header: t('list.code'),
      cell: (row) => (
        <button
          onClick={() => navigate(`/visa-applications/${row.id}`)}
          className="font-mono font-medium text-primary hover:underline"
        >
          {row.applicationCode}
        </button>
      ),
      sortable: true,
    },
    {
      key: 'visaType',
      header: t('list.visaType'),
      cell: (row) => {
        const typeConfig = ALL_VISA_TYPES.find(vt => vt.value === row.visaType);
        return <span className="text-sm">{typeConfig?.label ?? row.visaType}</span>;
      },
    },
    {
      key: 'status',
      header: t('list.status'),
      cell: (row) => {
        const config = STATUS_CONFIG[row.status as VisaApplicationStatus];
        if (!config) return <span>{row.status}</span>;
        const Icon = config.icon;
        return (
          <Badge variant={config.variant} className="gap-1">
            <Icon className="h-3 w-3" />
            {config.shortLabel}
          </Badge>
        );
      },
    },
    {
      key: 'worker',
      header: t('list.worker'),
      cell: (row) => row.worker?.fullNameEn || '—',
    },
    {
      key: 'client',
      header: t('list.client'),
      cell: (row) => row.client?.nameEn || '—',
    },
    {
      key: 'applicationDate',
      header: t('list.applicationDate'),
      cell: (row) => row.applicationDate ?? '—',
    },
    {
      key: 'referenceNumber',
      header: t('list.referenceNumber'),
      cell: (row) => (
        <span className="font-mono text-sm">{row.referenceNumber ?? '—'}</span>
      ),
    },
    {
      key: 'documentCount',
      header: t('list.documents'),
      cell: (row) => row.documentCount,
    },
    {
      key: 'createdAt',
      header: t('list.createdAt'),
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
      sortable: true,
    },
  ];

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">{t('title')}</h1>
          <p className="text-sm text-muted-foreground">{t('subtitle')}</p>
        </div>
        <PermissionGate permission="visas.create">
          <Button onClick={() => navigate('/visa-applications/new')}>
            <Plus className="mr-2 h-4 w-4" />
            {t('actions.create')}
          </Button>
        </PermissionGate>
      </div>

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('list.searchPlaceholder')}
        searchValue={search}
        onSearchChange={(val) => { setSearch(val); setPage(1); }}
        filters={[
          {
            key: 'status',
            label: t('list.status'),
            options: ALL_STATUSES.map(s => ({ label: STATUS_CONFIG[s].label, value: s })),
            value: statusFilter,
          },
          {
            key: 'visaType',
            label: t('list.visaType'),
            options: ALL_VISA_TYPES.map(vt => ({ label: vt.label, value: vt.value })),
            value: typeFilter,
          },
        ]}
        onFilterChange={(key, value) => {
          if (key === 'status') setStatusFilter(value);
          if (key === 'visaType') setTypeFilter(value);
          setPage(1);
        }}
        page={page}
        totalPages={data?.totalPages ?? 1}
        total={data?.totalCount ?? 0}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={(ps) => { setPageSize(ps); setPage(1); }}
        onRefresh={() => refetch()}
        emptyIcon={FileText}
        emptyTitle={t('list.emptyTitle')}
        emptyDescription={t('list.emptyDescription')}
      />
    </div>
  );
}

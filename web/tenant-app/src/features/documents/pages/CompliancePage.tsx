import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';
import { DataTableAdvanced, type Column, type Filter } from '@/shared/components/data-table/DataTableAdvanced';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Button } from '@/shared/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import {
  FileText, CheckCircle, AlertTriangle, XCircle,
  MoreHorizontal, Eye, Download, Paperclip,
} from 'lucide-react';
import { useAllDocuments, useComplianceSummary } from '../hooks';
import { getWorkerDocument } from '../api';
import { DocumentStatusBadge } from '../components/DocumentStatusBadge';
import { DocumentTypeBadge } from '../components/DocumentTypeBadge';
import { ALL_DOCUMENT_TYPES, ALL_EFFECTIVE_STATUSES } from '../constants';
import type { WorkerDocumentListDto } from '../types';

function SummaryCard({
  title,
  value,
  icon: Icon,
  className,
  isLoading,
}: {
  title: string;
  value: number;
  icon: React.ElementType;
  className?: string;
  isLoading?: boolean;
}) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium">{title}</CardTitle>
        <Icon className={`h-4 w-4 ${className ?? 'text-muted-foreground'}`} />
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <Skeleton className="h-8 w-16" />
        ) : (
          <div className={`text-2xl font-bold ${className ?? ''}`}>{value}</div>
        )}
      </CardContent>
    </Card>
  );
}

export function CompliancePage() {
  const { t } = useTranslation('documents');
  const navigate = useNavigate();

  // Table state
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [typeFilter, setTypeFilter] = useState<string | undefined>();
  const [statusFilter, setStatusFilter] = useState<string | undefined>();

  // Data
  const { data: compliance, isLoading: complianceLoading } = useComplianceSummary();

  const queryParams = useMemo(() => ({
    page,
    pageSize,
    search: search || undefined,
    sort: 'expiresAt',
    'filter[documentType]': typeFilter,
    'filter[effectiveStatus]': statusFilter,
  }), [page, pageSize, search, typeFilter, statusFilter]);

  const { data, isLoading, refetch } = useAllDocuments(queryParams);

  const handleDownloadFile = async (workerId: string, docId: string) => {
    try {
      const doc = await getWorkerDocument(workerId, docId);
      if (doc.fileUrl) {
        window.open(doc.fileUrl, '_blank');
      } else {
        toast.error(t('toast.noFile'));
      }
    } catch {
      toast.error(t('toast.downloadError'));
    }
  };

  const filters: Filter[] = [
    {
      key: 'documentType',
      label: t('filters.documentType'),
      options: ALL_DOCUMENT_TYPES.map((dt) => ({ label: t(`documentType.${dt}`), value: dt })),
      value: typeFilter,
    },
    {
      key: 'effectiveStatus',
      label: t('filters.status'),
      options: ALL_EFFECTIVE_STATUSES.map((s) => ({ label: t(`effectiveStatus.${s}`), value: s })),
      value: statusFilter,
    },
  ];

  const handleFilterChange = (key: string, value: string | undefined) => {
    if (key === 'documentType') setTypeFilter(value);
    if (key === 'effectiveStatus') setStatusFilter(value);
    setPage(1);
  };

  const columns: Column<WorkerDocumentListDto>[] = [
    {
      key: 'worker',
      header: t('columns.worker'),
      cell: (row) => (
        <div className="min-w-0">
          <p className="font-medium truncate">{row.workerName ?? '—'}</p>
          {row.workerCode && (
            <p className="text-xs text-muted-foreground font-mono">{row.workerCode}</p>
          )}
        </div>
      ),
    },
    {
      key: 'documentType',
      header: t('columns.documentType'),
      cell: (row) => <DocumentTypeBadge type={row.documentType} />,
    },
    {
      key: 'documentNumber',
      header: t('columns.documentNumber'),
      cell: (row) => (
        <span className="font-mono text-sm">{row.documentNumber ?? '—'}</span>
      ),
    },
    {
      key: 'expiresAt',
      header: t('columns.expiresAt'),
      cell: (row) => row.expiresAt ? new Date(row.expiresAt).toLocaleDateString() : '—',
    },
    {
      key: 'effectiveStatus',
      header: t('columns.status'),
      cell: (row) => <DocumentStatusBadge status={row.effectiveStatus} />,
    },
    {
      key: 'daysUntilExpiry',
      header: t('columns.daysLeft'),
      cell: (row) => {
        if (row.daysUntilExpiry == null) return '—';
        const isNegative = row.daysUntilExpiry < 0;
        return (
          <span className={isNegative ? 'text-destructive font-medium' : ''}>
            {row.daysUntilExpiry}
          </span>
        );
      },
    },
    {
      key: 'hasFile',
      header: t('columns.file'),
      className: 'w-[60px]',
      cell: (row) => row.hasFile ? (
        <button
          onClick={() => handleDownloadFile(row.workerId, row.id)}
          className="inline-flex items-center text-primary hover:text-primary/80"
          title={t('actions.downloadFile')}
        >
          <Paperclip className="h-4 w-4" />
        </button>
      ) : (
        <span className="text-muted-foreground">—</span>
      ),
    },
    {
      key: 'actions',
      header: t('columns.actions'),
      className: 'w-[70px]',
      cell: (row) => (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" className="h-8 w-8">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={() => navigate(`/workers/${row.workerId}`)}>
              <Eye className="me-2 h-4 w-4" />
              {t('actions.viewWorker')}
            </DropdownMenuItem>
            {row.hasFile && (
              <DropdownMenuItem onClick={() => handleDownloadFile(row.workerId, row.id)}>
                <Download className="me-2 h-4 w-4" />
                {t('actions.downloadFile')}
              </DropdownMenuItem>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{t('compliance.title')}</h1>
        <p className="text-muted-foreground">{t('compliance.description')}</p>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <SummaryCard
          title={t('compliance.totalDocuments')}
          value={compliance?.totalDocuments ?? 0}
          icon={FileText}
          isLoading={complianceLoading}
        />
        <SummaryCard
          title={t('compliance.valid')}
          value={compliance?.valid ?? 0}
          icon={CheckCircle}
          className="text-emerald-600"
          isLoading={complianceLoading}
        />
        <SummaryCard
          title={t('compliance.expiringSoon')}
          value={compliance?.expiringSoon ?? 0}
          icon={AlertTriangle}
          className="text-amber-600"
          isLoading={complianceLoading}
        />
        <SummaryCard
          title={t('compliance.expired')}
          value={compliance?.expired ?? 0}
          icon={XCircle}
          className="text-destructive"
          isLoading={complianceLoading}
        />
      </div>

      {/* All Documents Table */}
      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        selectable
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        searchPlaceholder={t('compliance.searchPlaceholder')}
        searchValue={search}
        onSearchChange={(val) => {
          setSearch(val);
          setPage(1);
        }}
        filters={filters}
        onFilterChange={handleFilterChange}
        page={page}
        totalPages={data?.totalPages}
        total={data?.totalCount}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        onRefresh={() => refetch()}
        emptyVariant="default"
        emptyTitle={t('compliance.empty.title')}
        emptyDescription={t('compliance.empty.description')}
      />
    </div>
  );
}

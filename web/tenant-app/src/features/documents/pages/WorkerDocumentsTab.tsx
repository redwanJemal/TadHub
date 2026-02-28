import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';
import { DataTableAdvanced, type Column, type Filter } from '@/shared/components/data-table/DataTableAdvanced';
import { Button } from '@/shared/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';
import { MoreHorizontal, Trash2, Pencil, Plus, Paperclip, Upload, Download } from 'lucide-react';
import { useWorkerDocuments, useDeleteWorkerDocument, useUploadDocumentFile } from '../hooks';
import { getWorkerDocument } from '../api';
import { DocumentStatusBadge } from '../components/DocumentStatusBadge';
import { DocumentTypeBadge } from '../components/DocumentTypeBadge';
import { CreateDocumentDialog } from '../components/CreateDocumentDialog';
import { EditDocumentDialog } from '../components/EditDocumentDialog';
import { ALL_DOCUMENT_TYPES, ALL_EFFECTIVE_STATUSES } from '../constants';
import type { WorkerDocumentListDto, WorkerDocumentDto } from '../types';

interface WorkerDocumentsTabProps {
  workerId: string;
}

export function WorkerDocumentsTab({ workerId }: WorkerDocumentsTabProps) {
  const { t } = useTranslation('documents');

  // Table state
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [typeFilter, setTypeFilter] = useState<string | undefined>();
  const [statusFilter, setStatusFilter] = useState<string | undefined>();

  // Dialog state
  const [showCreate, setShowCreate] = useState(false);
  const [editTarget, setEditTarget] = useState<WorkerDocumentDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<WorkerDocumentListDto | null>(null);

  // Data
  const queryParams = useMemo(() => ({
    page,
    pageSize,
    search: search || undefined,
    sort: '-createdAt',
    'filter[documentType]': typeFilter,
    'filter[effectiveStatus]': statusFilter,
  }), [page, pageSize, search, typeFilter, statusFilter]);

  const { data, isLoading, refetch } = useWorkerDocuments(workerId, queryParams);
  const deleteMutation = useDeleteWorkerDocument(workerId);
  const uploadMutation = useUploadDocumentFile(workerId);

  const handleDelete = async () => {
    if (!deleteTarget) return;
    await deleteMutation.mutateAsync(deleteTarget.id);
    setDeleteTarget(null);
  };

  const handleFileUpload = async (docId: string) => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.pdf,.jpg,.jpeg,.png,.doc,.docx';
    input.onchange = async (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (file) {
        await uploadMutation.mutateAsync({ id: docId, file });
      }
    };
    input.click();
  };

  const handleDownloadFile = async (docId: string) => {
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
      key: 'issuedAt',
      header: t('columns.issuedAt'),
      cell: (row) => row.issuedAt ? new Date(row.issuedAt).toLocaleDateString() : '—',
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
          onClick={() => handleDownloadFile(row.id)}
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
            <DropdownMenuItem onClick={() => handleEditClick(row)}>
              <Pencil className="me-2 h-4 w-4" />
              {t('actions.edit')}
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => handleFileUpload(row.id)}>
              <Upload className="me-2 h-4 w-4" />
              {t('actions.uploadFile')}
            </DropdownMenuItem>
            {row.hasFile && (
              <DropdownMenuItem onClick={() => handleDownloadFile(row.id)}>
                <Download className="me-2 h-4 w-4" />
                {t('actions.downloadFile')}
              </DropdownMenuItem>
            )}
            <DropdownMenuSeparator />
            <DropdownMenuItem
              className="text-destructive"
              onClick={() => setDeleteTarget(row)}
            >
              <Trash2 className="me-2 h-4 w-4" />
              {t('actions.delete')}
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      ),
    },
  ];

  // To edit, we need the full DTO. We'll use a fetch-on-click pattern via the list data.
  const handleEditClick = async (row: WorkerDocumentListDto) => {
    // Build a WorkerDocumentDto from the list row for the edit dialog
    const docForEdit: WorkerDocumentDto = {
      id: row.id,
      tenantId: '',
      workerId: row.workerId,
      documentType: row.documentType,
      documentNumber: row.documentNumber,
      issuedAt: row.issuedAt,
      expiresAt: row.expiresAt,
      status: row.status,
      effectiveStatus: row.effectiveStatus,
      daysUntilExpiry: row.daysUntilExpiry,
      hasFile: row.hasFile,
      createdAt: row.createdAt,
      updatedAt: row.createdAt,
    } as WorkerDocumentDto;
    setEditTarget(docForEdit);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-end">
        <Button onClick={() => setShowCreate(true)}>
          <Plus className="me-2 h-4 w-4" />
          {t('addDocument')}
        </Button>
      </div>

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        selectable
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        searchPlaceholder={t('searchPlaceholder')}
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
        emptyTitle={t('empty.title')}
        emptyDescription={t('empty.description')}
      />

      {/* Create Document Dialog */}
      <CreateDocumentDialog
        open={showCreate}
        onOpenChange={setShowCreate}
        workerId={workerId}
      />

      {/* Edit Document Dialog */}
      {editTarget && (
        <EditDocumentDialog
          open={!!editTarget}
          onOpenChange={(open) => { if (!open) setEditTarget(null); }}
          workerId={workerId}
          document={editTarget}
        />
      )}

      {/* Delete Confirmation */}
      <AlertDialog open={!!deleteTarget} onOpenChange={() => setDeleteTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('deleteDialog.title')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('deleteDialog.description')}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {t('deleteDialog.confirm')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

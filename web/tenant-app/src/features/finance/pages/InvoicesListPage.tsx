import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { DataTableAdvanced, type Column, type Filter } from '@/shared/components/data-table/DataTableAdvanced';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
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
import {
  MoreHorizontal,
  Trash2,
  Eye,
  Plus,
  DollarSign,
  Clock,
  AlertCircle,
  TrendingUp,
} from 'lucide-react';
import { usePermissions } from '@/features/auth/hooks/usePermissions';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { useInvoices, useInvoiceSummary, useDeleteInvoice } from '../hooks';
import { InvoiceStatusBadge } from '../components/InvoiceStatusBadge';
import type { InvoiceListDto, InvoiceStatus } from '../types';

const ALL_STATUSES: InvoiceStatus[] = [
  'Draft', 'Issued', 'PartiallyPaid', 'Paid', 'Overdue', 'Cancelled', 'Refunded',
];

function SummaryCards() {
  const { data: summary, isLoading } = useInvoiceSummary();

  if (isLoading) {
    return (
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <Card key={i}>
            <CardHeader className="pb-2">
              <Skeleton className="h-4 w-24" />
            </CardHeader>
            <CardContent>
              <Skeleton className="h-8 w-32" />
            </CardContent>
          </Card>
        ))}
      </div>
    );
  }

  const cards = [
    {
      label: 'Total Revenue',
      value: `${(summary?.totalRevenue ?? 0).toLocaleString()} AED`,
      icon: TrendingUp,
      iconClass: 'text-blue-500',
    },
    {
      label: 'Total Paid',
      value: `${(summary?.totalPaid ?? 0).toLocaleString()} AED`,
      icon: DollarSign,
      iconClass: 'text-green-500',
    },
    {
      label: 'Outstanding',
      value: `${(summary?.totalOutstanding ?? 0).toLocaleString()} AED`,
      icon: Clock,
      iconClass: 'text-yellow-500',
    },
    {
      label: 'Overdue',
      value: `${(summary?.overdueAmount ?? 0).toLocaleString()} AED`,
      subValue: summary?.overdueCount ? `${summary.overdueCount} invoices` : undefined,
      icon: AlertCircle,
      iconClass: 'text-red-500',
    },
  ];

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {cards.map((card) => {
        const Icon = card.icon;
        return (
          <Card key={card.label}>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {card.label}
              </CardTitle>
              <Icon className={`h-4 w-4 ${card.iconClass}`} />
            </CardHeader>
            <CardContent>
              <p className="text-2xl font-bold">{card.value}</p>
              {card.subValue && (
                <p className="text-xs text-muted-foreground mt-1">{card.subValue}</p>
              )}
            </CardContent>
          </Card>
        );
      })}
    </div>
  );
}

export function InvoicesListPage() {
  useTranslation('finance');
  const navigate = useNavigate();
  const { hasPermission } = usePermissions();

  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [deleteTarget, setDeleteTarget] = useState<InvoiceListDto | null>(null);

  const queryParams = useMemo(() => ({
    page,
    pageSize,
    search: search || undefined,
    sort: '-createdAt',
    'filter[status]': statusFilter,
  }), [page, pageSize, search, statusFilter]);

  const { data, isLoading, refetch } = useInvoices(queryParams);
  const deleteMutation = useDeleteInvoice();

  const handleDelete = async () => {
    if (!deleteTarget) return;
    await deleteMutation.mutateAsync(deleteTarget.id);
    setDeleteTarget(null);
  };

  const filters: Filter[] = [
    {
      key: 'status',
      label: 'Status',
      options: ALL_STATUSES.map((s) => ({ label: s === 'PartiallyPaid' ? 'Partially Paid' : s, value: s })),
      value: statusFilter,
    },
  ];

  const handleFilterChange = (key: string, value: string | undefined) => {
    if (key === 'status') setStatusFilter(value);
    setPage(1);
  };

  const columns: Column<InvoiceListDto>[] = [
    {
      key: 'invoiceNumber',
      header: 'Invoice #',
      cell: (row) => (
        <span className="font-mono text-sm font-medium">{row.invoiceNumber}</span>
      ),
    },
    {
      key: 'type',
      header: 'Type',
      cell: (row) => (
        <span className="text-sm capitalize">{row.type === 'CreditNote' ? 'Credit Note' : row.type === 'ProformaDeposit' ? 'Proforma' : row.type}</span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      cell: (row) => <InvoiceStatusBadge status={row.status} />,
    },
    {
      key: 'clientId',
      header: 'Client',
      cell: (row) => (
        <span className="font-mono text-xs text-muted-foreground">{row.clientId}</span>
      ),
    },
    {
      key: 'issueDate',
      header: 'Issue Date',
      cell: (row) => new Date(row.issueDate).toLocaleDateString(),
    },
    {
      key: 'dueDate',
      header: 'Due Date',
      cell: (row) => new Date(row.dueDate).toLocaleDateString(),
    },
    {
      key: 'totalAmount',
      header: 'Total',
      cell: (row) => (
        <span className="font-medium tabular-nums">
          {row.totalAmount.toLocaleString()} {row.currency}
        </span>
      ),
    },
    {
      key: 'paidAmount',
      header: 'Paid',
      cell: (row) => (
        <span className="tabular-nums text-green-600 dark:text-green-400">
          {row.paidAmount.toLocaleString()} {row.currency}
        </span>
      ),
    },
    {
      key: 'balanceDue',
      header: 'Balance Due',
      cell: (row) => (
        <span className={`tabular-nums font-medium ${row.balanceDue > 0 ? 'text-red-600 dark:text-red-400' : 'text-muted-foreground'}`}>
          {row.balanceDue.toLocaleString()} {row.currency}
        </span>
      ),
    },
    {
      key: 'actions',
      header: '',
      className: 'w-[60px]',
      cell: (row) => (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" className="h-8 w-8">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={() => navigate(`/finance/invoices/${row.id}`)}>
              <Eye className="me-2 h-4 w-4" />
              View
            </DropdownMenuItem>
            {hasPermission('finance.invoices.delete') && (
              <>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  className="text-destructive"
                  onClick={() => setDeleteTarget(row)}
                >
                  <Trash2 className="me-2 h-4 w-4" />
                  Delete
                </DropdownMenuItem>
              </>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Invoices</h1>
          <p className="text-muted-foreground">Manage and track all invoices</p>
        </div>
        <PermissionGate permission="finance.invoices.create">
          <Button onClick={() => navigate('/finance/invoices/new')}>
            <Plus className="me-2 h-4 w-4" />
            New Invoice
          </Button>
        </PermissionGate>
      </div>

      <SummaryCards />

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        selectable
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        searchPlaceholder="Search invoices..."
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
        emptyTitle="No invoices found"
        emptyDescription="Create your first invoice to get started"
      />

      <AlertDialog open={!!deleteTarget} onOpenChange={() => setDeleteTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Invoice</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete invoice{' '}
              <span className="font-mono font-medium">{deleteTarget?.invoiceNumber}</span>?
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

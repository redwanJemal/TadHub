import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
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
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { Label } from '@/shared/components/ui/label';
import { Input } from '@/shared/components/ui/input';
import { MoreHorizontal, Eye, RotateCcw, Plus } from 'lucide-react';
import { usePermissions } from '@/features/auth/hooks/usePermissions';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { usePayments, useRefundPayment } from '../hooks';
import { PaymentStatusBadge } from '../components/PaymentStatusBadge';
import { PaymentMethodBadge } from '../components/PaymentMethodBadge';
import type { PaymentListDto, PaymentStatus, PaymentMethod } from '../types';

const ALL_STATUSES: PaymentStatus[] = ['Pending', 'Completed', 'Failed', 'Refunded', 'Cancelled'];
const ALL_METHODS: PaymentMethod[] = ['Cash', 'Card', 'BankTransfer', 'Cheque', 'EDirham', 'Online'];

export function PaymentsListPage() {
  useTranslation('finance');
  const navigate = useNavigate();
  const { hasPermission } = usePermissions();

  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [methodFilter, setMethodFilter] = useState<string | undefined>();

  const [refundTarget, setRefundTarget] = useState<PaymentListDto | null>(null);
  const [refundAmount, setRefundAmount] = useState('');
  const [refundReason, setRefundReason] = useState('');

  const queryParams = useMemo(() => ({
    page,
    pageSize,
    search: search || undefined,
    sort: '-createdAt',
    'filter[status]': statusFilter,
    'filter[method]': methodFilter,
  }), [page, pageSize, search, statusFilter, methodFilter]);

  const { data, isLoading, refetch } = usePayments(queryParams);
  const refundMutation = useRefundPayment();

  const handleRefund = async () => {
    if (!refundTarget || !refundReason) return;
    await refundMutation.mutateAsync({
      id: refundTarget.id,
      data: {
        amount: refundAmount ? parseFloat(refundAmount) : refundTarget.amount,
        reason: refundReason,
      },
    });
    setRefundTarget(null);
    setRefundAmount('');
    setRefundReason('');
  };

  const filters: Filter[] = [
    {
      key: 'status',
      label: 'Status',
      options: ALL_STATUSES.map((s) => ({ label: s, value: s })),
      value: statusFilter,
    },
    {
      key: 'method',
      label: 'Method',
      options: ALL_METHODS.map((m) => ({
        label: m === 'BankTransfer' ? 'Bank Transfer' : m === 'EDirham' ? 'E-Dirham' : m,
        value: m,
      })),
      value: methodFilter,
    },
  ];

  const handleFilterChange = (key: string, value: string | undefined) => {
    if (key === 'status') setStatusFilter(value);
    if (key === 'method') setMethodFilter(value);
    setPage(1);
  };

  const columns: Column<PaymentListDto>[] = [
    {
      key: 'paymentNumber',
      header: 'Payment #',
      cell: (row) => (
        <span className="font-mono text-sm font-medium">{row.paymentNumber}</span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      cell: (row) => <PaymentStatusBadge status={row.status} />,
    },
    {
      key: 'invoiceId',
      header: 'Invoice',
      cell: (row) => (
        <button
          className="font-mono text-xs text-blue-600 dark:text-blue-400 hover:underline"
          onClick={() => navigate(`/finance/invoices/${row.invoiceId}`)}
        >
          {row.invoiceId}
        </button>
      ),
    },
    {
      key: 'method',
      header: 'Method',
      cell: (row) => <PaymentMethodBadge method={row.method} />,
    },
    {
      key: 'referenceNumber',
      header: 'Reference',
      cell: (row) => (
        <span className="font-mono text-xs text-muted-foreground">
          {row.referenceNumber ?? '—'}
        </span>
      ),
    },
    {
      key: 'paymentDate',
      header: 'Payment Date',
      cell: (row) => new Date(row.paymentDate).toLocaleDateString(),
    },
    {
      key: 'amount',
      header: 'Amount',
      cell: (row) => (
        <span className="font-medium tabular-nums">
          {row.amount.toFixed(2)} {row.currency}
        </span>
      ),
    },
    {
      key: 'cashierName',
      header: 'Cashier',
      cell: (row) => row.cashierName ?? '—',
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
            <DropdownMenuItem onClick={() => navigate(`/finance/invoices/${row.invoiceId}`)}>
              <Eye className="me-2 h-4 w-4" />
              View Invoice
            </DropdownMenuItem>
            {hasPermission('finance.payments.refund') && row.status === 'Completed' && (
              <>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={() => setRefundTarget(row)}>
                  <RotateCcw className="me-2 h-4 w-4" />
                  Refund
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
          <h1 className="text-2xl font-bold tracking-tight">Payments</h1>
          <p className="text-muted-foreground">Track all payment transactions</p>
        </div>
        <PermissionGate permission="finance.payments.create">
          <Button onClick={() => navigate('/finance/payments/new')}>
            <Plus className="me-2 h-4 w-4" />
            Record Payment
          </Button>
        </PermissionGate>
      </div>

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder="Search payments..."
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
        emptyTitle="No payments found"
        emptyDescription="Record a payment against an invoice to get started"
      />

      {/* Refund Dialog */}
      <Dialog open={!!refundTarget} onOpenChange={(open) => { if (!open) setRefundTarget(null); }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Refund Payment</DialogTitle>
            <DialogDescription>
              Refund payment{' '}
              <span className="font-mono font-medium">{refundTarget?.paymentNumber}</span>{' '}
              ({refundTarget?.amount.toFixed(2)} {refundTarget?.currency})
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Refund Amount (leave blank for full amount)</Label>
              <Input
                type="number"
                min="0"
                step="0.01"
                placeholder={refundTarget?.amount.toFixed(2)}
                value={refundAmount}
                onChange={(e) => setRefundAmount(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>Reason *</Label>
              <Input
                placeholder="Reason for refund"
                value={refundReason}
                onChange={(e) => setRefundReason(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setRefundTarget(null)}>Cancel</Button>
            <Button
              onClick={handleRefund}
              disabled={!refundReason || refundMutation.isPending}
            >
              {refundMutation.isPending ? 'Processing...' : 'Refund Payment'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

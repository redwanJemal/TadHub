import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, RefreshCw } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { Card, CardContent } from '@/shared/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import { Skeleton } from '@/shared/components/ui/skeleton';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import { Label } from '@/shared/components/ui/label';
import { Input } from '@/shared/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { cn } from '@/shared/lib/cn';
import { usePermissions } from '@/features/auth/hooks/usePermissions';
import { PermissionGate } from '@/shared/components/PermissionGate';
import {
  useSupplierPayments,
  useCreateSupplierPayment,
  useTransitionSupplierPaymentStatus,
} from '../hooks';
import type { SupplierPaymentListDto, SupplierPaymentStatus } from '../types';

const STATUS_CLASSES: Record<SupplierPaymentStatus, string> = {
  Pending:       'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300',
  Paid:          'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
  PartiallyPaid: 'bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-300',
  Cancelled:     'bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-400',
};

const ALL_METHODS = ['Cash', 'Card', 'BankTransfer', 'Cheque', 'EDirham', 'Online'];
const ALL_TRANSITIONS: Record<SupplierPaymentStatus, SupplierPaymentStatus[]> = {
  Pending:       ['Paid', 'PartiallyPaid', 'Cancelled'],
  PartiallyPaid: ['Paid', 'Cancelled'],
  Paid:          [],
  Cancelled:     [],
};

export function SupplierPaymentsPage() {
  useTranslation('finance');
  const { hasPermission } = usePermissions();

  const [page, setPage] = useState(1);
  const pageSize = 20;
  const [statusFilter, setStatusFilter] = useState<string | undefined>();

  const queryParams = useMemo(() => ({
    page,
    pageSize,
    sort: '-createdAt',
    'filter[status]': statusFilter,
  }), [page, pageSize, statusFilter]);

  const { data, isLoading, refetch } = useSupplierPayments(queryParams);
  const createMutation = useCreateSupplierPayment();
  const transitionMutation = useTransitionSupplierPaymentStatus();

  // Create dialog
  const [showCreate, setShowCreate] = useState(false);
  const [supplierId, setSupplierId] = useState('');
  const [workerId, setWorkerId] = useState('');
  const [contractId, setContractId] = useState('');
  const [amount, setAmount] = useState('');
  const [currency, setCurrency] = useState('AED');
  const [method, setMethod] = useState('Cash');
  const [referenceNumber, setReferenceNumber] = useState('');
  const [paymentDate, setPaymentDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [createNotes, setCreateNotes] = useState('');

  // Transition dialog
  const [transitionTarget, setTransitionTarget] = useState<SupplierPaymentListDto | null>(null);
  const [transitionStatus, setTransitionStatus] = useState('');
  const [transitionReason, setTransitionReason] = useState('');

  const handleCreate = async () => {
    if (!supplierId || !amount || !method || !paymentDate) return;
    await createMutation.mutateAsync({
      supplierId,
      workerId: workerId || undefined,
      contractId: contractId || undefined,
      amount: parseFloat(amount),
      currency,
      method,
      referenceNumber: referenceNumber || undefined,
      paymentDate,
      notes: createNotes.trim() || undefined,
    });
    setShowCreate(false);
    setSupplierId(''); setWorkerId(''); setContractId('');
    setAmount(''); setReferenceNumber(''); setCreateNotes('');
  };

  const handleTransition = async () => {
    if (!transitionTarget || !transitionStatus) return;
    await transitionMutation.mutateAsync({
      id: transitionTarget.id,
      data: { status: transitionStatus, reason: transitionReason || undefined },
    });
    setTransitionTarget(null);
    setTransitionStatus('');
    setTransitionReason('');
  };

  const canCreate = supplierId.trim() && amount && parseFloat(amount) > 0 && method && paymentDate;

  const ALL_STATUSES: SupplierPaymentStatus[] = ['Pending', 'Paid', 'PartiallyPaid', 'Cancelled'];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Supplier Payments</h1>
          <p className="text-muted-foreground">Track supplier costs and outbound payments</p>
        </div>
        <div className="flex items-center gap-2">
          <Select
            value={statusFilter ?? 'all'}
            onValueChange={(v) => { setStatusFilter(v === 'all' ? undefined : v); setPage(1); }}
          >
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="All Statuses" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Statuses</SelectItem>
              {ALL_STATUSES.map((s) => (
                <SelectItem key={s} value={s}>{s === 'PartiallyPaid' ? 'Partially Paid' : s}</SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Button variant="outline" size="icon" onClick={() => refetch()}>
            <RefreshCw className="h-4 w-4" />
          </Button>
          <PermissionGate permission="finance.supplier_payments.create">
            <Button onClick={() => setShowCreate(true)}>
              <Plus className="me-2 h-4 w-4" />
              New Payment
            </Button>
          </PermissionGate>
        </div>
      </div>

      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Payment #</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Supplier</TableHead>
                <TableHead>Worker</TableHead>
                <TableHead>Method</TableHead>
                <TableHead>Payment Date</TableHead>
                <TableHead className="text-right">Amount</TableHead>
                {hasPermission('finance.supplier_payments.manage_status') && (
                  <TableHead className="w-[60px]" />
                )}
              </TableRow>
            </TableHeader>
            <TableBody>
              {isLoading ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <TableRow key={i}>
                    {Array.from({ length: 8 }).map((_, j) => (
                      <TableCell key={j}><Skeleton className="h-5 w-full" /></TableCell>
                    ))}
                  </TableRow>
                ))
              ) : (data?.items ?? []).length === 0 ? (
                <TableRow>
                  <TableCell colSpan={8} className="text-center py-12 text-muted-foreground">
                    No supplier payments found.
                  </TableCell>
                </TableRow>
              ) : (
                (data?.items ?? []).map((payment) => {
                  const availableTransitions = ALL_TRANSITIONS[payment.status] ?? [];
                  return (
                    <TableRow key={payment.id}>
                      <TableCell className="font-mono text-sm">{payment.paymentNumber}</TableCell>
                      <TableCell>
                        <Badge
                          variant="outline"
                          className={cn('border-transparent', STATUS_CLASSES[payment.status])}
                        >
                          {payment.status === 'PartiallyPaid' ? 'Partially Paid' : payment.status}
                        </Badge>
                      </TableCell>
                      <TableCell className="font-mono text-xs text-muted-foreground">
                        {payment.supplierId}
                      </TableCell>
                      <TableCell className="font-mono text-xs text-muted-foreground">
                        {payment.workerId ?? '—'}
                      </TableCell>
                      <TableCell>{payment.method}</TableCell>
                      <TableCell>{new Date(payment.paymentDate).toLocaleDateString()}</TableCell>
                      <TableCell className="text-right font-medium tabular-nums">
                        {payment.amount.toFixed(2)} {payment.currency}
                      </TableCell>
                      {hasPermission('finance.supplier_payments.manage_status') && (
                        <TableCell>
                          {availableTransitions.length > 0 && (
                            <DropdownMenu>
                              <DropdownMenuTrigger asChild>
                                <Button variant="ghost" size="icon" className="h-8 w-8">
                                  <RefreshCw className="h-4 w-4" />
                                </Button>
                              </DropdownMenuTrigger>
                              <DropdownMenuContent align="end">
                                {availableTransitions.map((s) => (
                                  <DropdownMenuItem
                                    key={s}
                                    onClick={() => {
                                      setTransitionTarget(payment);
                                      setTransitionStatus(s);
                                    }}
                                  >
                                    {s === 'PartiallyPaid' ? 'Mark Partially Paid' : `Mark as ${s}`}
                                  </DropdownMenuItem>
                                ))}
                              </DropdownMenuContent>
                            </DropdownMenu>
                          )}
                        </TableCell>
                      )}
                    </TableRow>
                  );
                })
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Pagination */}
      {(data?.totalPages ?? 1) > 1 && (
        <div className="flex items-center justify-end gap-2">
          <span className="text-sm text-muted-foreground">
            Page {page} of {data?.totalPages}
          </span>
          <Button variant="outline" size="sm" onClick={() => setPage((p) => p - 1)} disabled={page <= 1}>
            Previous
          </Button>
          <Button variant="outline" size="sm" onClick={() => setPage((p) => p + 1)} disabled={page >= (data?.totalPages ?? 1)}>
            Next
          </Button>
        </div>
      )}

      {/* Create Dialog */}
      <Dialog open={showCreate} onOpenChange={setShowCreate}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>New Supplier Payment</DialogTitle>
            <DialogDescription>Record a new outbound supplier payment</DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Supplier ID *</Label>
              <Input placeholder="Supplier ID" value={supplierId} onChange={(e) => setSupplierId(e.target.value)} />
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>Worker ID</Label>
                <Input placeholder="Optional worker" value={workerId} onChange={(e) => setWorkerId(e.target.value)} />
              </div>
              <div className="space-y-2">
                <Label>Contract ID</Label>
                <Input placeholder="Optional contract" value={contractId} onChange={(e) => setContractId(e.target.value)} />
              </div>
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>Amount *</Label>
                <Input type="number" min="0.01" step="0.01" value={amount} onChange={(e) => setAmount(e.target.value)} />
              </div>
              <div className="space-y-2">
                <Label>Currency</Label>
                <Input value={currency} onChange={(e) => setCurrency(e.target.value)} />
              </div>
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>Method *</Label>
                <Select value={method} onValueChange={setMethod}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {ALL_METHODS.map((m) => (
                      <SelectItem key={m} value={m}>
                        {m === 'BankTransfer' ? 'Bank Transfer' : m === 'EDirham' ? 'E-Dirham' : m}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>Payment Date *</Label>
                <Input type="date" value={paymentDate} onChange={(e) => setPaymentDate(e.target.value)} />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Reference Number</Label>
              <Input placeholder="Optional reference" value={referenceNumber} onChange={(e) => setReferenceNumber(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Notes</Label>
              <textarea
                className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                placeholder="Optional notes"
                value={createNotes}
                onChange={(e) => setCreateNotes(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowCreate(false)}>Cancel</Button>
            <Button onClick={handleCreate} disabled={!canCreate || createMutation.isPending}>
              {createMutation.isPending ? 'Saving...' : 'Create Payment'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Transition Dialog */}
      <Dialog
        open={!!transitionTarget}
        onOpenChange={(open) => { if (!open) setTransitionTarget(null); }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Change Payment Status</DialogTitle>
            <DialogDescription>
              Update status for {transitionTarget?.paymentNumber}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>New Status</Label>
              <Select value={transitionStatus} onValueChange={setTransitionStatus}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {transitionTarget && ALL_TRANSITIONS[transitionTarget.status].map((s) => (
                    <SelectItem key={s} value={s}>
                      {s === 'PartiallyPaid' ? 'Partially Paid' : s}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Reason</Label>
              <Input
                placeholder="Optional reason"
                value={transitionReason}
                onChange={(e) => setTransitionReason(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setTransitionTarget(null)}>Cancel</Button>
            <Button onClick={handleTransition} disabled={!transitionStatus || transitionMutation.isPending}>
              {transitionMutation.isPending ? 'Updating...' : 'Update Status'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

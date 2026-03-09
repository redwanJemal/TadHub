import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, RefreshCw } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
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
import { usePermissions } from '@/features/auth/hooks/usePermissions';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { SupplierDebitStatusBadge } from '../components/SupplierDebitStatusBadge';
import {
  useSupplierDebits,
  useCreateSupplierDebit,
  useTransitionSupplierDebitStatus,
} from '../hooks';
import type { SupplierDebitListDto, SupplierDebitStatus, SupplierDebitType } from '../types';

const ALL_DEBIT_TYPES: SupplierDebitType[] = [
  'CommissionRefund', 'TicketCost', 'VisaCost', 'TransportationCost',
  'MedicalCost', 'AccommodationCost', 'Other',
];

const DEBIT_TYPE_LABELS: Record<SupplierDebitType, string> = {
  CommissionRefund: 'Commission Refund',
  TicketCost: 'Ticket Cost',
  VisaCost: 'Visa Cost',
  TransportationCost: 'Transportation Cost',
  MedicalCost: 'Medical Cost',
  AccommodationCost: 'Accommodation Cost',
  Other: 'Other',
};

const ALL_TRANSITIONS: Record<SupplierDebitStatus, SupplierDebitStatus[]> = {
  Outstanding:   ['PartiallyPaid', 'Settled', 'Waived', 'Cancelled'],
  PartiallyPaid: ['Settled', 'Waived', 'Cancelled'],
  Settled:       [],
  Waived:        [],
  Cancelled:     [],
};

const ALL_STATUSES: SupplierDebitStatus[] = ['Outstanding', 'PartiallyPaid', 'Settled', 'Waived', 'Cancelled'];

export function SupplierDebitsPage() {
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

  const { data, isLoading, refetch } = useSupplierDebits(queryParams);
  const createMutation = useCreateSupplierDebit();
  const transitionMutation = useTransitionSupplierDebitStatus();

  // Create dialog
  const [showCreate, setShowCreate] = useState(false);
  const [supplierId, setSupplierId] = useState('');
  const [workerId, setWorkerId] = useState('');
  const [contractId, setContractId] = useState('');
  const [debitType, setDebitType] = useState<string>('CommissionRefund');
  const [description, setDescription] = useState('');
  const [amount, setAmount] = useState('');
  const [createNotes, setCreateNotes] = useState('');

  // Transition dialog
  const [transitionTarget, setTransitionTarget] = useState<SupplierDebitListDto | null>(null);
  const [transitionStatus, setTransitionStatus] = useState('');
  const [transitionReason, setTransitionReason] = useState('');

  const handleCreate = async () => {
    if (!supplierId || !amount || !description || !debitType) return;
    await createMutation.mutateAsync({
      supplierId,
      workerId: workerId || undefined,
      contractId: contractId || undefined,
      debitType,
      description,
      amount: parseFloat(amount),
      notes: createNotes.trim() || undefined,
    });
    setShowCreate(false);
    setSupplierId(''); setWorkerId(''); setContractId('');
    setAmount(''); setDescription(''); setCreateNotes('');
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

  const canCreate = supplierId.trim() && amount && parseFloat(amount) > 0 && description.trim() && debitType;

  // Summary cards
  const outstandingTotal = useMemo(() => {
    if (!data?.items) return 0;
    return data.items
      .filter((d) => d.status === 'Outstanding' || d.status === 'PartiallyPaid')
      .reduce((sum, d) => sum + d.amount, 0);
  }, [data?.items]);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Supplier Debits</h1>
          <p className="text-muted-foreground">Track outstanding debits and cost recovery from suppliers</p>
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
          <PermissionGate permission="supplier_debits.create">
            <Button onClick={() => setShowCreate(true)}>
              <Plus className="me-2 h-4 w-4" />
              New Debit
            </Button>
          </PermissionGate>
        </div>
      </div>

      {/* Summary */}
      <div className="grid gap-4 sm:grid-cols-3">
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">Total Debits</p>
            <p className="text-2xl font-bold">
              {isLoading ? <Skeleton className="h-8 w-16" /> : data?.totalCount ?? 0}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">Outstanding Amount</p>
            <p className="text-2xl font-bold text-red-600">
              {isLoading ? <Skeleton className="h-8 w-24" /> : `${outstandingTotal.toFixed(2)} AED`}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">Current Page</p>
            <p className="text-2xl font-bold">
              {isLoading ? <Skeleton className="h-8 w-16" /> : `${data?.items?.length ?? 0} items`}
            </p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Debit #</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Description</TableHead>
                <TableHead>Case</TableHead>
                <TableHead className="text-right">Amount</TableHead>
                {hasPermission('supplier_debits.manage_status') && (
                  <TableHead className="w-[60px]" />
                )}
              </TableRow>
            </TableHeader>
            <TableBody>
              {isLoading ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <TableRow key={i}>
                    {Array.from({ length: 7 }).map((_, j) => (
                      <TableCell key={j}><Skeleton className="h-5 w-full" /></TableCell>
                    ))}
                  </TableRow>
                ))
              ) : (data?.items ?? []).length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center py-12 text-muted-foreground">
                    No supplier debits found.
                  </TableCell>
                </TableRow>
              ) : (
                (data?.items ?? []).map((debit) => {
                  const availableTransitions = ALL_TRANSITIONS[debit.status] ?? [];
                  return (
                    <TableRow key={debit.id}>
                      <TableCell className="font-mono text-sm">{debit.debitNumber}</TableCell>
                      <TableCell>
                        <SupplierDebitStatusBadge status={debit.status} />
                      </TableCell>
                      <TableCell>
                        {DEBIT_TYPE_LABELS[debit.debitType] ?? debit.debitType}
                      </TableCell>
                      <TableCell className="max-w-[200px] truncate">{debit.description}</TableCell>
                      <TableCell className="text-sm text-muted-foreground">
                        {debit.caseType ?? '—'}
                      </TableCell>
                      <TableCell className="text-right font-medium tabular-nums">
                        {debit.amount.toFixed(2)} {debit.currency}
                      </TableCell>
                      {hasPermission('supplier_debits.manage_status') && (
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
                                      setTransitionTarget(debit);
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
            <DialogTitle>New Supplier Debit</DialogTitle>
            <DialogDescription>Record a debit against a supplier</DialogDescription>
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
                <Label>Debit Type *</Label>
                <Select value={debitType} onValueChange={setDebitType}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {ALL_DEBIT_TYPES.map((t) => (
                      <SelectItem key={t} value={t}>{DEBIT_TYPE_LABELS[t]}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>Amount *</Label>
                <Input type="number" min="0.01" step="0.01" value={amount} onChange={(e) => setAmount(e.target.value)} />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Description *</Label>
              <Input placeholder="Debit description" value={description} onChange={(e) => setDescription(e.target.value)} />
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
              {createMutation.isPending ? 'Saving...' : 'Create Debit'}
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
            <DialogTitle>Change Debit Status</DialogTitle>
            <DialogDescription>
              Update status for {transitionTarget?.debitNumber}
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

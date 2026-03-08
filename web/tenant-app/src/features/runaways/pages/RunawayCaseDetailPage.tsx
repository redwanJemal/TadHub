import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Trash2, CheckCircle2, Banknote, Lock, Plus, ShieldAlert } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Label } from '@/shared/components/ui/label';
import { Input } from '@/shared/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/shared/components/ui/dialog';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/shared/components/ui/alert-dialog';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { RunawayCaseStatusBadge } from '../components/RunawayCaseStatusBadge';
import {
  useRunawayCase,
  useConfirmRunawayCase,
  useSettleRunawayCase,
  useCloseRunawayCase,
  useAddRunawayExpense,
  useDeleteRunawayCase,
} from '../hooks';
import { EXPENSE_TYPES, PAID_BY_OPTIONS } from '../constants';
import type { RunawayCaseStatus } from '../types';

function InfoItem({ label, value }: { label: string; value?: string | number | null }) {
  return (
    <div>
      <dt className="text-sm text-muted-foreground">{label}</dt>
      <dd className="mt-0.5 text-sm font-medium">{value ?? '—'}</dd>
    </div>
  );
}

function DetailSkeleton() {
  return (
    <div className="space-y-6 p-6">
      <div>
        <Skeleton className="mb-2 h-4 w-32" />
        <div className="flex items-center justify-between">
          <div>
            <div className="flex items-center gap-3">
              <Skeleton className="h-8 w-40" />
              <Skeleton className="h-6 w-20 rounded-full" />
            </div>
            <Skeleton className="mt-1 h-4 w-56" />
          </div>
          <div className="flex items-center gap-2">
            <Skeleton className="h-9 w-28" />
            <Skeleton className="h-9 w-28" />
          </div>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 6 }).map((_, i) => (
          <Card key={i}>
            <CardHeader className="pb-3"><Skeleton className="h-5 w-28" /></CardHeader>
            <CardContent className="space-y-3">
              {Array.from({ length: 3 }).map((_, j) => (
                <div key={j}>
                  <Skeleton className="mb-1 h-3 w-20" />
                  <Skeleton className="h-4 w-36" />
                </div>
              ))}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}

export function RunawayCaseDetailPage() {
  const { t } = useTranslation('runaways');
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: rc, isLoading } = useRunawayCase(id!);
  const confirmMutation = useConfirmRunawayCase();
  const settleMutation = useSettleRunawayCase();
  const closeMutation = useCloseRunawayCase();
  const expenseMutation = useAddRunawayExpense();
  const deleteMutation = useDeleteRunawayCase();

  const [showConfirm, setShowConfirm] = useState(false);
  const [showSettle, setShowSettle] = useState(false);
  const [showClose, setShowClose] = useState(false);
  const [showAddExpense, setShowAddExpense] = useState(false);

  const [confirmNotes, setConfirmNotes] = useState('');
  const [settlementNotes, setSettlementNotes] = useState('');
  const [closeNotes, setCloseNotes] = useState('');

  const [expenseType, setExpenseType] = useState('');
  const [expenseAmount, setExpenseAmount] = useState('');
  const [expenseDescription, setExpenseDescription] = useState('');
  const [expensePaidBy, setExpensePaidBy] = useState('');

  if (isLoading) return <DetailSkeleton />;

  if (!rc) {
    return (
      <div className="p-6">
        <p>{t('not_found')}</p>
        <Link to="/runaways" className="text-primary underline">{t('back_to_list')}</Link>
      </div>
    );
  }

  const canConfirm = rc.status === 'Reported' || rc.status === 'UnderInvestigation';
  const canSettle = rc.status === 'Confirmed';
  const canClose = rc.status === 'Settled';

  const handleConfirm = () => {
    confirmMutation.mutate(
      { id: id!, data: { notes: confirmNotes || undefined } },
      { onSuccess: () => setShowConfirm(false) }
    );
  };

  const handleSettle = () => {
    settleMutation.mutate(
      { id: id!, data: { notes: settlementNotes || undefined } },
      { onSuccess: () => setShowSettle(false) }
    );
  };

  const handleClose = () => {
    closeMutation.mutate(
      { id: id!, data: { notes: closeNotes || undefined } },
      { onSuccess: () => setShowClose(false) }
    );
  };

  const handleAddExpense = () => {
    if (!expenseType || !expenseAmount || !expensePaidBy) return;
    expenseMutation.mutate(
      {
        caseId: id!,
        data: {
          expenseType,
          amount: parseFloat(expenseAmount),
          description: expenseDescription || undefined,
          paidBy: expensePaidBy,
        },
      },
      {
        onSuccess: () => {
          setShowAddExpense(false);
          setExpenseType('');
          setExpenseAmount('');
          setExpenseDescription('');
          setExpensePaidBy('');
        },
      }
    );
  };

  const handleDelete = () => {
    deleteMutation.mutate(id!, { onSuccess: () => navigate('/runaways') });
  };

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <Link
            to="/runaways"
            className="mb-2 inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="h-4 w-4" />
            {t('back_to_list')}
          </Link>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold">{rc.caseCode}</h1>
            <RunawayCaseStatusBadge status={rc.status as RunawayCaseStatus} />
            {rc.isWithinGuarantee && (
              <Badge variant="warning" className="gap-1">
                <ShieldAlert className="h-3 w-3" />
                {t('within_guarantee')}
              </Badge>
            )}
          </div>
          <p className="mt-1 text-sm text-muted-foreground">
            {rc.worker?.fullNameEn || t('unknown_worker')} — {rc.client?.nameEn || t('unknown_client')}
          </p>
        </div>
        <div className="flex items-center gap-2">
          {canConfirm && (
            <PermissionGate permission="runaways.manage">
              <Button size="sm" onClick={() => setShowConfirm(true)}>
                <CheckCircle2 className="mr-1 h-4 w-4" />
                {t('confirm')}
              </Button>
            </PermissionGate>
          )}
          {canSettle && (
            <PermissionGate permission="runaways.settle">
              <Button size="sm" variant="outline" onClick={() => setShowSettle(true)}>
                <Banknote className="mr-1 h-4 w-4" />
                {t('settle')}
              </Button>
            </PermissionGate>
          )}
          {canClose && (
            <PermissionGate permission="runaways.manage">
              <Button size="sm" variant="outline" onClick={() => setShowClose(true)}>
                <Lock className="mr-1 h-4 w-4" />
                {t('close')}
              </Button>
            </PermissionGate>
          )}
          <PermissionGate permission="runaways.delete">
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button size="sm" variant="destructive">
                  <Trash2 className="mr-1 h-4 w-4" />
                  {t('common:delete')}
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>{t('delete_title')}</AlertDialogTitle>
                  <AlertDialogDescription>{t('delete_description')}</AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
                  <AlertDialogAction onClick={handleDelete}>{t('common:delete')}</AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          </PermissionGate>
        </div>
      </div>

      {/* Supplier Liability Warning */}
      {rc.isWithinGuarantee && (
        <div className="rounded-md border border-orange-200 bg-orange-50 p-4">
          <div className="flex items-start gap-2">
            <ShieldAlert className="mt-0.5 h-5 w-5 text-orange-600" />
            <div>
              <p className="font-medium text-orange-800">{t('supplier_liability')}</p>
              <p className="mt-1 text-sm text-orange-700">{t('supplier_liable_message')}</p>
            </div>
          </div>
        </div>
      )}

      {/* Info Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {/* Worker */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">{t('worker')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <InfoItem label={t('name')} value={rc.worker?.fullNameEn} />
            <InfoItem label={t('worker_code')} value={rc.worker?.workerCode} />
            <Link to={`/workers/${rc.workerId}`} className="mt-2 inline-block text-sm text-primary underline">
              {t('view_worker')}
            </Link>
          </CardContent>
        </Card>

        {/* Client */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">{t('client')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <InfoItem label={t('name')} value={rc.client?.nameEn} />
          </CardContent>
        </Card>

        {/* Contract */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">{t('contract')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <InfoItem label={t('contract_id')} value={rc.contractId} />
            <Link to={`/contracts/${rc.contractId}`} className="mt-2 inline-block text-sm text-primary underline">
              {t('view_contract')}
            </Link>
          </CardContent>
        </Card>

        {/* Case Details */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">{t('case_details')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <InfoItem label={t('reported_date')} value={new Date(rc.reportedDate).toLocaleDateString()} />
            <InfoItem label={t('reported_by')} value={rc.reportedBy} />
            <InfoItem label={t('last_known_location')} value={rc.lastKnownLocation} />
            <InfoItem label={t('guarantee_period')} value={rc.guaranteePeriodType || '—'} />
            {rc.notes && <InfoItem label={t('notes')} value={rc.notes} />}
          </CardContent>
        </Card>

        {/* Police Report */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">{t('police_info')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <InfoItem label={t('police_report_number')} value={rc.policeReportNumber} />
            <InfoItem label={t('police_report_date')} value={rc.policeReportDate ? new Date(rc.policeReportDate).toLocaleDateString() : null} />
          </CardContent>
        </Card>

        {/* Timeline Timestamps */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">{t('audit')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <InfoItem label={t('created')} value={new Date(rc.createdAt).toLocaleString()} />
            {rc.confirmedAt && <InfoItem label={t('confirmed_at')} value={new Date(rc.confirmedAt).toLocaleString()} />}
            {rc.settledAt && <InfoItem label={t('settled_at')} value={new Date(rc.settledAt).toLocaleString()} />}
            {rc.closedAt && <InfoItem label={t('closed_at')} value={new Date(rc.closedAt).toLocaleString()} />}
            <InfoItem label={t('updated')} value={new Date(rc.updatedAt).toLocaleString()} />
          </CardContent>
        </Card>
      </div>

      {/* Expenses */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between pb-3">
          <CardTitle className="text-base">{t('expenses')}</CardTitle>
          {rc.status !== 'Closed' && (
            <PermissionGate permission="runaways.manage">
              <Button size="sm" variant="outline" onClick={() => setShowAddExpense(true)}>
                <Plus className="mr-1 h-4 w-4" />
                {t('add_expense')}
              </Button>
            </PermissionGate>
          )}
        </CardHeader>
        <CardContent>
          {rc.expenses && rc.expenses.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-muted-foreground">
                    <th className="pb-2 text-start font-medium">{t('expense_type')}</th>
                    <th className="pb-2 text-start font-medium">{t('description')}</th>
                    <th className="pb-2 text-end font-medium">{t('amount')}</th>
                    <th className="pb-2 text-start font-medium">{t('paid_by')}</th>
                  </tr>
                </thead>
                <tbody>
                  {rc.expenses.map((exp) => (
                    <tr key={exp.id} className="border-b last:border-0">
                      <td className="py-2">{exp.expenseType}</td>
                      <td className="py-2">{exp.description || '—'}</td>
                      <td className="py-2 text-end font-medium">{exp.amount.toLocaleString()} {exp.currency}</td>
                      <td className="py-2"><Badge variant="outline">{exp.paidBy}</Badge></td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr className="font-semibold">
                    <td className="pt-2" colSpan={2}>{t('total')}</td>
                    <td className="pt-2 text-end">
                      {rc.expenses.reduce((sum, e) => sum + e.amount, 0).toLocaleString()} AED
                    </td>
                    <td />
                  </tr>
                </tfoot>
              </table>
            </div>
          ) : (
            <p className="text-sm text-muted-foreground">{t('no_expenses')}</p>
          )}
        </CardContent>
      </Card>

      {/* Status History */}
      {rc.statusHistory && rc.statusHistory.length > 0 && (
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">{t('status_history')}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {rc.statusHistory.map((h) => (
                <div key={h.id} className="flex items-start gap-3 border-b pb-3 last:border-0">
                  <div className="mt-0.5 h-2 w-2 rounded-full bg-primary" />
                  <div className="flex-1">
                    <div className="flex items-center gap-2">
                      {h.fromStatus && (
                        <>
                          <RunawayCaseStatusBadge status={h.fromStatus as RunawayCaseStatus} showIcon={false} />
                          <span className="text-xs text-muted-foreground">&rarr;</span>
                        </>
                      )}
                      <RunawayCaseStatusBadge status={h.toStatus as RunawayCaseStatus} showIcon={false} />
                    </div>
                    <p className="mt-1 text-xs text-muted-foreground">
                      {new Date(h.changedAt).toLocaleString()}
                      {h.reason && ` — ${h.reason}`}
                    </p>
                    {h.notes && <p className="mt-0.5 text-xs">{h.notes}</p>}
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Confirm Dialog */}
      <Dialog open={showConfirm} onOpenChange={setShowConfirm}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t('confirm_case')}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <p className="text-sm text-muted-foreground">{t('confirm_message')}</p>
            <div className="space-y-2">
              <Label>{t('notes')}</Label>
              <textarea
                className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={confirmNotes}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setConfirmNotes(e.target.value)}
                rows={2}
                placeholder={t('notes_placeholder')}
              />
            </div>
          </div>
          <DialogFooter className="gap-2 sm:space-x-reverse">
            <Button variant="outline" onClick={() => setShowConfirm(false)}>{t('common:cancel')}</Button>
            <Button onClick={handleConfirm} disabled={confirmMutation.isPending}>
              {confirmMutation.isPending ? t('confirming') : t('confirm_confirm')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Settle Dialog */}
      <Dialog open={showSettle} onOpenChange={setShowSettle}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t('settle_case')}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <p className="text-sm text-muted-foreground">{t('settle_message')}</p>
            <div className="space-y-2">
              <Label>{t('settlement_notes')}</Label>
              <textarea
                className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={settlementNotes}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setSettlementNotes(e.target.value)}
                rows={2}
                placeholder={t('settlement_notes_placeholder')}
              />
            </div>
          </div>
          <DialogFooter className="gap-2 sm:space-x-reverse">
            <Button variant="outline" onClick={() => setShowSettle(false)}>{t('common:cancel')}</Button>
            <Button onClick={handleSettle} disabled={settleMutation.isPending}>
              {settleMutation.isPending ? t('settling') : t('confirm_settle')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Close Dialog */}
      <Dialog open={showClose} onOpenChange={setShowClose}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t('close_case')}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <p className="text-sm text-muted-foreground">{t('close_message')}</p>
            <div className="space-y-2">
              <Label>{t('notes')}</Label>
              <textarea
                className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={closeNotes}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setCloseNotes(e.target.value)}
                rows={2}
                placeholder={t('notes_placeholder')}
              />
            </div>
          </div>
          <DialogFooter className="gap-2 sm:space-x-reverse">
            <Button variant="outline" onClick={() => setShowClose(false)}>{t('common:cancel')}</Button>
            <Button onClick={handleClose} disabled={closeMutation.isPending}>
              {closeMutation.isPending ? t('closing') : t('confirm_close')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Add Expense Dialog */}
      <Dialog open={showAddExpense} onOpenChange={setShowAddExpense}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t('add_expense')}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label>{t('expense_type')} *</Label>
              <Select value={expenseType} onValueChange={setExpenseType}>
                <SelectTrigger>
                  <SelectValue placeholder={t('select_expense_type')} />
                </SelectTrigger>
                <SelectContent>
                  {EXPENSE_TYPES.map((et) => (
                    <SelectItem key={et.value} value={et.value}>{et.label}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>{t('amount')} *</Label>
              <Input
                type="number"
                value={expenseAmount}
                onChange={(e) => setExpenseAmount(e.target.value)}
                placeholder="0.00"
                min="0"
                step="0.01"
              />
            </div>
            <div className="space-y-2">
              <Label>{t('paid_by')} *</Label>
              <Select value={expensePaidBy} onValueChange={setExpensePaidBy}>
                <SelectTrigger>
                  <SelectValue placeholder={t('select_paid_by')} />
                </SelectTrigger>
                <SelectContent>
                  {PAID_BY_OPTIONS.map((p) => (
                    <SelectItem key={p.value} value={p.value}>{p.label}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>{t('description')}</Label>
              <Input
                value={expenseDescription}
                onChange={(e) => setExpenseDescription(e.target.value)}
                placeholder={t('expense_description_placeholder')}
              />
            </div>
          </div>
          <DialogFooter className="gap-2 sm:space-x-reverse">
            <Button variant="outline" onClick={() => setShowAddExpense(false)}>{t('common:cancel')}</Button>
            <Button
              onClick={handleAddExpense}
              disabled={!expenseType || !expenseAmount || !expensePaidBy || expenseMutation.isPending}
            >
              {expenseMutation.isPending ? t('adding') : t('add_expense')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

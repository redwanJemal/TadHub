import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Trash2, CheckCircle2, XCircle, Banknote, Plus, ShieldAlert } from 'lucide-react';
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
import { ReturneeCaseStatusBadge } from '../components/ReturneeCaseStatusBadge';
import {
  useReturneeCase,
  useApproveReturneeCase,
  useRejectReturneeCase,
  useSettleReturneeCase,
  useAddReturneeExpense,
  useDeleteReturneeCase,
  useRefundCalculation,
} from '../hooks';
import { EXPENSE_TYPES, PAID_BY_OPTIONS } from '../constants';
import type { ReturneeCaseStatus } from '../types';

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

      <Card>
        <CardContent className="py-6">
          <div className="grid grid-cols-4 gap-6">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="text-center">
                <Skeleton className="mx-auto mb-2 h-4 w-24" />
                <Skeleton className="mx-auto h-7 w-32" />
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 5 }).map((_, i) => (
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

export function ReturneeCaseDetailPage() {
  const { t } = useTranslation('returnees');
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: rc, isLoading } = useReturneeCase(id!);
  const { data: refundCalc } = useRefundCalculation(id!, !!rc);
  const approveMutation = useApproveReturneeCase();
  const rejectMutation = useRejectReturneeCase();
  const settleMutation = useSettleReturneeCase();
  const expenseMutation = useAddReturneeExpense();
  const deleteMutation = useDeleteReturneeCase();

  const [showApprove, setShowApprove] = useState(false);
  const [showReject, setShowReject] = useState(false);
  const [showSettle, setShowSettle] = useState(false);
  const [showAddExpense, setShowAddExpense] = useState(false);

  const [approveNotes, setApproveNotes] = useState('');
  const [rejectReason, setRejectReason] = useState('');
  const [rejectNotes, setRejectNotes] = useState('');
  const [settlementNotes, setSettlementNotes] = useState('');

  const [expenseType, setExpenseType] = useState('');
  const [expenseAmount, setExpenseAmount] = useState('');
  const [expenseDescription, setExpenseDescription] = useState('');
  const [expensePaidBy, setExpensePaidBy] = useState('');

  if (isLoading) return <DetailSkeleton />;

  if (!rc) {
    return (
      <div className="p-6">
        <p>{t('not_found')}</p>
        <Link to="/returnees" className="text-primary underline">{t('back_to_list')}</Link>
      </div>
    );
  }

  const canApproveReject = rc.status === 'Submitted' || rc.status === 'UnderReview';
  const canSettle = rc.status === 'Approved';

  const handleApprove = () => {
    approveMutation.mutate(
      { id: id!, data: { notes: approveNotes || undefined } },
      { onSuccess: () => setShowApprove(false) }
    );
  };

  const handleReject = () => {
    if (!rejectReason) return;
    rejectMutation.mutate(
      { id: id!, data: { reason: rejectReason, notes: rejectNotes || undefined } },
      { onSuccess: () => setShowReject(false) }
    );
  };

  const handleSettle = () => {
    settleMutation.mutate(
      { id: id!, data: { settlementNotes: settlementNotes || undefined } },
      { onSuccess: () => setShowSettle(false) }
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
    deleteMutation.mutate(id!, { onSuccess: () => navigate('/returnees') });
  };

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <Link
            to="/returnees"
            className="mb-2 inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="h-4 w-4" />
            {t('back_to_list')}
          </Link>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold">{rc.caseCode}</h1>
            <ReturneeCaseStatusBadge status={rc.status as ReturneeCaseStatus} />
            <Badge variant="outline">
              {rc.returnType === 'ReturnToOffice' ? t('return_to_office') : t('return_to_country')}
            </Badge>
            {rc.isWithinGuarantee && (
              <Badge variant="warning" className="gap-1">
                <ShieldAlert className="h-3 w-3" />
                {t('within_guarantee')}
              </Badge>
            )}
          </div>
          <p className="mt-1 text-sm text-muted-foreground">
            {rc.worker?.fullNameEn || t('unknown_worker')} → {rc.client?.nameEn || t('unknown_client')}
          </p>
        </div>
        <div className="flex items-center gap-2">
          {canApproveReject && (
            <PermissionGate permission="returnees.manage">
              <Button size="sm" onClick={() => setShowApprove(true)}>
                <CheckCircle2 className="mr-1 h-4 w-4" />
                {t('approve')}
              </Button>
              <Button size="sm" variant="outline" onClick={() => setShowReject(true)}>
                <XCircle className="mr-1 h-4 w-4" />
                {t('reject')}
              </Button>
            </PermissionGate>
          )}
          {canSettle && (
            <PermissionGate permission="returnees.settle">
              <Button size="sm" variant="outline" onClick={() => setShowSettle(true)}>
                <Banknote className="mr-1 h-4 w-4" />
                {t('settle')}
              </Button>
            </PermissionGate>
          )}
          <PermissionGate permission="returnees.delete">
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

      {/* Summary Card */}
      <Card>
        <CardContent className="py-6">
          <div className="grid grid-cols-2 gap-6 text-center md:grid-cols-4">
            <div>
              <p className="text-sm text-muted-foreground">{t('return_date')}</p>
              <p className="mt-1 text-xl font-semibold">{rc.returnDate}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('months_worked')}</p>
              <p className="mt-1 text-xl font-semibold">{rc.monthsWorked}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('total_paid')}</p>
              <p className="mt-1 text-xl font-semibold">
                {rc.totalAmountPaid != null ? `${rc.totalAmountPaid.toLocaleString()} ${rc.currency}` : '—'}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('refund_amount')}</p>
              <p className="mt-1 text-xl font-semibold text-green-600">
                {rc.refundAmount != null ? `${rc.refundAmount.toLocaleString()} ${rc.currency}` : '—'}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Refund Calculation Breakdown */}
      {refundCalc && (
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">{t('refund_breakdown')}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-4 md:grid-cols-3">
              <InfoItem label={t('total_contract_months')} value={refundCalc.totalContractMonths} />
              <InfoItem label={t('months_worked')} value={refundCalc.monthsWorked} />
              <InfoItem label={t('value_per_month')} value={`${refundCalc.valuePerMonth.toLocaleString()} ${refundCalc.currency}`} />
              <InfoItem label={t('total_paid')} value={`${refundCalc.totalAmountPaid.toLocaleString()} ${refundCalc.currency}`} />
              <InfoItem label={t('refund_amount')} value={`${refundCalc.refundAmount.toLocaleString()} ${refundCalc.currency}`} />
            </div>
          </CardContent>
        </Card>
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
            <InfoItem label={t('return_reason')} value={rc.returnReason} />
            <InfoItem label={t('guarantee_period')} value={rc.guaranteePeriodType || '—'} />
            {rc.notes && <InfoItem label={t('notes')} value={rc.notes} />}
          </CardContent>
        </Card>

        {/* Approval/Rejection Info */}
        {(rc.approvedAt || rc.rejectedReason) && (
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base">{rc.approvedAt ? t('approval_info') : t('rejection_info')}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {rc.approvedAt && (
                <InfoItem label={t('approved_at')} value={new Date(rc.approvedAt).toLocaleString()} />
              )}
              {rc.rejectedReason && (
                <InfoItem label={t('rejected_reason')} value={rc.rejectedReason} />
              )}
            </CardContent>
          </Card>
        )}

        {/* Settlement Info */}
        {rc.settledAt && (
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base">{t('settlement_info')}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <InfoItem label={t('settled_at')} value={new Date(rc.settledAt).toLocaleString()} />
              {rc.settlementNotes && <InfoItem label={t('settlement_notes')} value={rc.settlementNotes} />}
            </CardContent>
          </Card>
        )}

        {/* Audit */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">{t('audit')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <InfoItem label={t('created')} value={new Date(rc.createdAt).toLocaleString()} />
            <InfoItem label={t('updated')} value={new Date(rc.updatedAt).toLocaleString()} />
          </CardContent>
        </Card>
      </div>

      {/* Expenses */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between pb-3">
          <CardTitle className="text-base">{t('expenses')}</CardTitle>
          {rc.status !== 'Rejected' && rc.status !== 'Settled' && (
            <PermissionGate permission="returnees.manage">
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
                      {rc.expenses.reduce((sum, e) => sum + e.amount, 0).toLocaleString()} {rc.currency}
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
                          <ReturneeCaseStatusBadge status={h.fromStatus as ReturneeCaseStatus} showIcon={false} />
                          <span className="text-xs text-muted-foreground">→</span>
                        </>
                      )}
                      <ReturneeCaseStatusBadge status={h.toStatus as ReturneeCaseStatus} showIcon={false} />
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

      {/* Approve Dialog */}
      <Dialog open={showApprove} onOpenChange={setShowApprove}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t('approve_case')}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="rounded-md border border-green-200 bg-green-50 p-3 text-sm text-green-800">
              {rc.returnType === 'ReturnToOffice' ? t('approve_office_message') : t('approve_country_message')}
            </div>
            <div className="space-y-2">
              <Label>{t('notes')}</Label>
              <textarea
                className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={approveNotes}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setApproveNotes(e.target.value)}
                rows={2}
                placeholder={t('notes_placeholder')}
              />
            </div>
          </div>
          <DialogFooter className="gap-2 sm:space-x-reverse">
            <Button variant="outline" onClick={() => setShowApprove(false)}>{t('common:cancel')}</Button>
            <Button onClick={handleApprove} disabled={approveMutation.isPending}>
              {approveMutation.isPending ? t('approving') : t('confirm_approve')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Reject Dialog */}
      <Dialog open={showReject} onOpenChange={setShowReject}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t('reject_case')}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label>{t('reject_reason')} *</Label>
              <textarea
                className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={rejectReason}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setRejectReason(e.target.value)}
                rows={3}
                placeholder={t('reject_reason_placeholder')}
              />
            </div>
            <div className="space-y-2">
              <Label>{t('notes')}</Label>
              <textarea
                className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={rejectNotes}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setRejectNotes(e.target.value)}
                rows={2}
                placeholder={t('notes_placeholder')}
              />
            </div>
          </div>
          <DialogFooter className="gap-2 sm:space-x-reverse">
            <Button variant="outline" onClick={() => setShowReject(false)}>{t('common:cancel')}</Button>
            <Button variant="destructive" onClick={handleReject} disabled={!rejectReason || rejectMutation.isPending}>
              {rejectMutation.isPending ? t('rejecting') : t('confirm_reject')}
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
            <p className="text-sm text-muted-foreground">{t('settle_confirm_message')}</p>
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

import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Trash2, CheckCircle2, XCircle, Ban } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Label } from '@/shared/components/ui/label';
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
import { TrialStatusBadge } from '../components/TrialStatusBadge';
import { useTrial, useCompleteTrial, useCancelTrial, useDeleteTrial } from '../hooks';
import { OUTCOME_OPTIONS } from '../constants';
import type { TrialStatus, TrialOutcome } from '../types';

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
            <Skeleton className="h-9 w-32" />
            <Skeleton className="h-9 w-24" />
          </div>
        </div>
      </div>

      {/* Trial period card */}
      <Card>
        <CardContent className="py-6">
          <div className="flex items-center justify-between">
            <Skeleton className="h-16 w-48" />
            <Skeleton className="h-16 w-48" />
            <Skeleton className="h-16 w-48" />
          </div>
        </CardContent>
      </Card>

      {/* Info cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 4 }).map((_, i) => (
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

export function TrialDetailPage() {
  const { t } = useTranslation('trials');
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: trial, isLoading } = useTrial(id!);
  const completeMutation = useCompleteTrial();
  const cancelMutation = useCancelTrial();
  const deleteMutation = useDeleteTrial();

  const [showComplete, setShowComplete] = useState(false);
  const [showCancel, setShowCancel] = useState(false);
  const [outcome, setOutcome] = useState<TrialOutcome | ''>('');
  const [outcomeNotes, setOutcomeNotes] = useState('');
  const [cancelReason, setCancelReason] = useState('');

  if (isLoading) {
    return <DetailSkeleton />;
  }

  if (!trial) {
    return (
      <div className="p-6">
        <p>{t('not_found')}</p>
        <Link to="/trials" className="text-primary underline">{t('back_to_list')}</Link>
      </div>
    );
  }

  const isActive = trial.status === 'Active';

  const handleComplete = () => {
    if (!outcome) return;
    completeMutation.mutate(
      { id: id!, data: { outcome: outcome as TrialOutcome, outcomeNotes: outcomeNotes || undefined } },
      {
        onSuccess: (result) => {
          setShowComplete(false);
          if (result.outcome === 'ProceedToContract') {
            navigate(`/contracts/new?workerId=${trial.workerId}&clientId=${trial.clientId}`);
          }
        },
      }
    );
  };

  const handleCancel = () => {
    cancelMutation.mutate(
      { id: id!, data: { reason: cancelReason || undefined } },
      { onSuccess: () => setShowCancel(false) }
    );
  };

  const handleDelete = () => {
    deleteMutation.mutate(id!, {
      onSuccess: () => navigate('/trials'),
    });
  };

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <Link
            to="/trials"
            className="mb-2 inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="h-4 w-4" />
            {t('back_to_list')}
          </Link>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold">{trial.trialCode}</h1>
            <TrialStatusBadge status={trial.status as TrialStatus} />
          </div>
          <p className="mt-1 text-sm text-muted-foreground">
            {trial.worker?.fullNameEn || t('unknown_worker')} → {trial.client?.nameEn || t('unknown_client')}
          </p>
        </div>
        <div className="flex items-center gap-2">
          {isActive && (
            <PermissionGate permission="trials.manage">
              <Button size="sm" onClick={() => setShowComplete(true)}>
                <CheckCircle2 className="mr-1 h-4 w-4" />
                {t('complete')}
              </Button>
              <Button size="sm" variant="outline" onClick={() => setShowCancel(true)}>
                <Ban className="mr-1 h-4 w-4" />
                {t('cancel_trial')}
              </Button>
            </PermissionGate>
          )}
          {trial.status === 'Successful' && !trial.contractId && (
            <Button
              size="sm"
              variant="outline"
              onClick={() =>
                navigate(`/contracts/new?workerId=${trial.workerId}&clientId=${trial.clientId}`)
              }
            >
              {t('create_contract')}
            </Button>
          )}
          <PermissionGate permission="trials.delete">
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

      {/* Trial Period Summary Card */}
      <Card>
        <CardContent className="py-6">
          <div className="grid grid-cols-3 gap-6 text-center">
            <div>
              <p className="text-sm text-muted-foreground">{t('start_date')}</p>
              <p className="mt-1 text-xl font-semibold">{trial.startDate}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('end_date')}</p>
              <p className="mt-1 text-xl font-semibold">{trial.endDate}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">{t('days_remaining')}</p>
              <p className={`mt-1 text-xl font-semibold ${
                isActive && trial.daysRemaining <= 1 ? 'text-destructive' : ''
              }`}>
                {isActive ? trial.daysRemaining : '—'}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Info Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {/* Worker */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">{t('worker')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <InfoItem label={t('name')} value={trial.worker?.fullNameEn} />
            <InfoItem label={t('worker_code')} value={trial.worker?.workerCode} />
            <Link to={`/workers/${trial.workerId}`} className="mt-2 inline-block text-sm text-primary underline">
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
            <InfoItem label={t('name')} value={trial.client?.nameEn} />
          </CardContent>
        </Card>

        {/* Outcome */}
        {trial.outcome && (
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base">{t('outcome')}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <InfoItem label={t('result')} value={trial.outcome} />
              <InfoItem
                label={t('outcome_date')}
                value={trial.outcomeDate ? new Date(trial.outcomeDate).toLocaleString() : undefined}
              />
              {trial.outcomeNotes && <InfoItem label={t('notes')} value={trial.outcomeNotes} />}
              {trial.contractId && (
                <Link to={`/contracts/${trial.contractId}`} className="mt-2 inline-block text-sm text-primary underline">
                  {t('view_contract')}
                </Link>
              )}
            </CardContent>
          </Card>
        )}

        {/* Audit */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">{t('audit')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <InfoItem label={t('created')} value={new Date(trial.createdAt).toLocaleString()} />
            <InfoItem label={t('updated')} value={new Date(trial.updatedAt).toLocaleString()} />
          </CardContent>
        </Card>
      </div>

      {/* Status History */}
      {trial.statusHistory && trial.statusHistory.length > 0 && (
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">{t('status_history')}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {trial.statusHistory.map((h) => (
                <div key={h.id} className="flex items-start gap-3 border-b pb-3 last:border-0">
                  <div className="mt-0.5 h-2 w-2 rounded-full bg-primary" />
                  <div className="flex-1">
                    <div className="flex items-center gap-2">
                      {h.fromStatus && (
                        <>
                          <TrialStatusBadge status={h.fromStatus as TrialStatus} showIcon={false} />
                          <span className="text-xs text-muted-foreground">→</span>
                        </>
                      )}
                      <TrialStatusBadge status={h.toStatus as TrialStatus} showIcon={false} />
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

      {/* Complete Trial Dialog */}
      <Dialog open={showComplete} onOpenChange={setShowComplete}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t('complete_trial')}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label>{t('outcome')}</Label>
              <Select value={outcome} onValueChange={(v) => setOutcome(v as TrialOutcome)}>
                <SelectTrigger>
                  <SelectValue placeholder={t('select_outcome')} />
                </SelectTrigger>
                <SelectContent>
                  {OUTCOME_OPTIONS.map((o) => (
                    <SelectItem key={o.value} value={o.value}>
                      {o.value === 'ProceedToContract' ? (
                        <span className="flex items-center gap-2">
                          <CheckCircle2 className="h-4 w-4 text-green-600" />
                          {o.label}
                        </span>
                      ) : (
                        <span className="flex items-center gap-2">
                          <XCircle className="h-4 w-4 text-red-600" />
                          {o.label}
                        </span>
                      )}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>{t('outcome_notes')}</Label>
              <textarea
                className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={outcomeNotes}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setOutcomeNotes(e.target.value)}
                rows={3}
                placeholder={t('outcome_notes_placeholder')}
              />
            </div>
            {outcome === 'ProceedToContract' && (
              <div className="rounded-md border border-green-200 bg-green-50 p-3 text-sm text-green-800">
                {t('proceed_message')}
              </div>
            )}
            {outcome === 'ReturnToInventory' && (
              <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
                {t('return_message')}
              </div>
            )}
          </div>
          <DialogFooter className="gap-2 sm:space-x-reverse">
            <Button variant="outline" onClick={() => setShowComplete(false)}>{t('common:cancel')}</Button>
            <Button
              onClick={handleComplete}
              disabled={!outcome || completeMutation.isPending}
            >
              {completeMutation.isPending ? t('completing') : t('confirm_complete')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Cancel Trial Dialog */}
      <Dialog open={showCancel} onOpenChange={setShowCancel}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t('cancel_trial')}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <p className="text-sm text-muted-foreground">{t('cancel_confirm_message')}</p>
            <div className="space-y-2">
              <Label>{t('cancel_reason')}</Label>
              <textarea
                className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={cancelReason}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setCancelReason(e.target.value)}
                rows={2}
                placeholder={t('cancel_reason_placeholder')}
              />
            </div>
          </div>
          <DialogFooter className="gap-2 sm:space-x-reverse">
            <Button variant="outline" onClick={() => setShowCancel(false)}>{t('common:cancel')}</Button>
            <Button
              variant="destructive"
              onClick={handleCancel}
              disabled={cancelMutation.isPending}
            >
              {cancelMutation.isPending ? t('cancelling') : t('confirm_cancel')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

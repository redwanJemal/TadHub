import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Trash2, ExternalLink, FileText, CheckCircle2, Circle } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '@/shared/components/ui/dialog';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from '@/shared/components/ui/alert-dialog';
import { Label } from '@/shared/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { useVisaApplication, useTransitionVisaStatus, useDeleteVisaApplication } from '../hooks';
import { STATUS_CONFIG, ALLOWED_TRANSITIONS, REASON_REQUIRED_STATUSES, ALL_VISA_TYPES, VISA_DOCUMENT_TYPES, DOCUMENT_REQUIREMENTS } from '../constants';
import type { VisaApplicationStatus } from '../types';
import { toast } from 'sonner';

function InfoItem({ label, value }: { label: string; value?: string | null }) {
  return (
    <div className="space-y-1">
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="text-sm font-medium">{value || '-'}</p>
    </div>
  );
}

export function VisaApplicationDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation('visas');
  const navigate = useNavigate();
  const { data: visa, isLoading } = useVisaApplication(id!);
  const transitionMutation = useTransitionVisaStatus();
  const deleteMutation = useDeleteVisaApplication();

  const [transitionOpen, setTransitionOpen] = useState(false);
  const [selectedStatus, setSelectedStatus] = useState('');
  const [reason, setReason] = useState('');
  const [notes, setNotes] = useState('');

  if (isLoading) return <DetailSkeleton />;
  if (!visa) return <div className="p-4">{t('detail.notFound')}</div>;

  const statusConfig = STATUS_CONFIG[visa.status as VisaApplicationStatus];
  const StatusIcon = statusConfig?.icon ?? Circle;
  const allowedTransitions = ALLOWED_TRANSITIONS[visa.status as VisaApplicationStatus] ?? [];
  const typeConfig = ALL_VISA_TYPES.find(vt => vt.value === visa.visaType);

  const handleTransition = async () => {
    if (!selectedStatus) return;
    try {
      await transitionMutation.mutateAsync({
        id: visa.id,
        data: { status: selectedStatus, reason: reason || undefined, notes: notes || undefined },
      });
      toast.success(t('notifications.statusUpdated'));
      setTransitionOpen(false);
      setSelectedStatus('');
      setReason('');
      setNotes('');
    } catch {
      toast.error(t('notifications.statusUpdateFailed'));
    }
  };

  const handleDelete = async () => {
    try {
      await deleteMutation.mutateAsync(visa.id);
      toast.success(t('notifications.deleted'));
      navigate('/visa-applications');
    } catch {
      toast.error(t('notifications.deleteFailed'));
    }
  };

  // Determine document requirements
  const workerLocation = 'Outside'; // Default; in real app, fetch from worker data
  const requirementKey = `${visa.visaType}_${workerLocation}`;
  const requirements = DOCUMENT_REQUIREMENTS[requirementKey] ?? [];

  return (
    <div className="space-y-6 p-4">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/visa-applications')}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold">{visa.applicationCode}</h1>
            <Badge variant={statusConfig?.variant ?? 'default'} className="gap-1">
              <StatusIcon className="h-3 w-3" />
              {statusConfig?.label ?? visa.status}
            </Badge>
          </div>
          <p className="text-muted-foreground">{typeConfig?.label ?? visa.visaType}</p>
        </div>
        <div className="flex items-center gap-2">
          {allowedTransitions.length > 0 && (
            <PermissionGate permission="visas.manage">
              <Dialog open={transitionOpen} onOpenChange={setTransitionOpen}>
                <DialogTrigger asChild>
                  <Button>{t('actions.updateStatus')}</Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>{t('dialog.transitionTitle')}</DialogTitle>
                  </DialogHeader>
                  <div className="space-y-4">
                    <div className="space-y-2">
                      <Label>{t('dialog.newStatus')}</Label>
                      <Select value={selectedStatus} onValueChange={setSelectedStatus}>
                        <SelectTrigger>
                          <SelectValue placeholder={t('dialog.selectStatus')} />
                        </SelectTrigger>
                        <SelectContent>
                          {allowedTransitions.map(s => (
                            <SelectItem key={s} value={s}>
                              {STATUS_CONFIG[s]?.label ?? s}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                    {selectedStatus && REASON_REQUIRED_STATUSES.includes(selectedStatus as VisaApplicationStatus) && (
                      <div className="space-y-2">
                        <Label>{t('dialog.reason')}</Label>
                        <textarea
                          className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                          value={reason}
                          onChange={e => setReason(e.target.value)}
                          placeholder={t('dialog.reasonPlaceholder')}
                        />
                      </div>
                    )}
                    <div className="space-y-2">
                      <Label>{t('dialog.notes')}</Label>
                      <textarea
                        className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                        value={notes}
                        onChange={e => setNotes(e.target.value)}
                        placeholder={t('dialog.notesPlaceholder')}
                      />
                    </div>
                  </div>
                  <DialogFooter>
                    <Button variant="outline" onClick={() => setTransitionOpen(false)}>{t('common:cancel')}</Button>
                    <Button onClick={handleTransition} disabled={!selectedStatus || transitionMutation.isPending}>
                      {transitionMutation.isPending ? t('common:saving') : t('actions.confirm')}
                    </Button>
                  </DialogFooter>
                </DialogContent>
              </Dialog>
            </PermissionGate>
          )}
          <PermissionGate permission="visas.delete">
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button variant="destructive" size="icon">
                  <Trash2 className="h-4 w-4" />
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>{t('dialog.deleteTitle')}</AlertDialogTitle>
                  <AlertDialogDescription>{t('dialog.deleteDescription')}</AlertDialogDescription>
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

      {/* Info Cards */}
      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t('detail.workerInfo')}</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <InfoItem label={t('detail.workerName')} value={visa.worker?.fullNameEn} />
            <InfoItem label={t('detail.workerCode')} value={visa.worker?.workerCode} />
            {visa.worker && (
              <div className="col-span-2">
                <Link to={`/workers/${visa.workerId}`} className="inline-flex items-center gap-1 text-sm text-primary hover:underline">
                  {t('detail.viewWorker')} <ExternalLink className="h-3 w-3" />
                </Link>
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t('detail.clientInfo')}</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <InfoItem label={t('detail.clientName')} value={visa.client?.nameEn} />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t('detail.applicationInfo')}</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <InfoItem label={t('detail.applicationDate')} value={visa.applicationDate} />
            <InfoItem label={t('detail.approvalDate')} value={visa.approvalDate} />
            <InfoItem label={t('detail.issuanceDate')} value={visa.issuanceDate} />
            <InfoItem label={t('detail.expiryDate')} value={visa.expiryDate} />
            <InfoItem label={t('detail.referenceNumber')} value={visa.referenceNumber} />
            <InfoItem label={t('detail.visaNumber')} value={visa.visaNumber} />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t('detail.auditInfo')}</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <InfoItem label={t('detail.createdAt')} value={new Date(visa.createdAt).toLocaleString()} />
            <InfoItem label={t('detail.updatedAt')} value={new Date(visa.updatedAt).toLocaleString()} />
            {visa.notes && <InfoItem label={t('detail.notes')} value={visa.notes} />}
            {visa.rejectionReason && <InfoItem label={t('detail.rejectionReason')} value={visa.rejectionReason} />}
          </CardContent>
        </Card>
      </div>

      {/* Document Requirements */}
      {requirements.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t('detail.documentRequirements')}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {requirements.map((req) => {
                const uploaded = visa.documents?.some(d => d.documentType === req.type);
                const docLabel = VISA_DOCUMENT_TYPES.find(dt => dt.value === req.type)?.label ?? req.type;
                return (
                  <div key={req.type} className="flex items-center gap-3 rounded-md border p-3">
                    {uploaded ? (
                      <CheckCircle2 className="h-5 w-5 text-green-500" />
                    ) : (
                      <Circle className="h-5 w-5 text-muted-foreground" />
                    )}
                    <span className="flex-1 text-sm">{docLabel}</span>
                    <Badge variant={req.mandatory ? 'default' : 'outline'}>
                      {req.mandatory ? t('detail.mandatory') : t('detail.optional')}
                    </Badge>
                  </div>
                );
              })}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Uploaded Documents */}
      {visa.documents && visa.documents.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t('detail.uploadedDocuments')}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {visa.documents.map((doc) => {
                const docLabel = VISA_DOCUMENT_TYPES.find(dt => dt.value === doc.documentType)?.label ?? doc.documentType;
                return (
                  <div key={doc.id} className="flex items-center gap-3 rounded-md border p-3">
                    <FileText className="h-4 w-4 text-muted-foreground" />
                    <span className="flex-1 text-sm">{docLabel}</span>
                    <span className="text-xs text-muted-foreground">
                      {new Date(doc.uploadedAt).toLocaleDateString()}
                    </span>
                    {doc.isVerified && (
                      <Badge variant="success">{t('detail.verified')}</Badge>
                    )}
                  </div>
                );
              })}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Status History */}
      {visa.statusHistory && visa.statusHistory.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t('detail.statusHistory')}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {visa.statusHistory.map((entry, i) => {
                const toConfig = STATUS_CONFIG[entry.toStatus as VisaApplicationStatus];
                const ToIcon = toConfig?.icon ?? Circle;
                return (
                  <div key={entry.id} className="flex gap-3">
                    <div className="flex flex-col items-center">
                      <div className="flex h-8 w-8 items-center justify-center rounded-full border">
                        <ToIcon className="h-4 w-4" />
                      </div>
                      {i < visa.statusHistory!.length - 1 && (
                        <div className="w-px flex-1 bg-border" />
                      )}
                    </div>
                    <div className="flex-1 pb-4">
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-sm">
                          {entry.fromStatus ? `${STATUS_CONFIG[entry.fromStatus as VisaApplicationStatus]?.label ?? entry.fromStatus} → ` : ''}
                          {toConfig?.label ?? entry.toStatus}
                        </span>
                      </div>
                      <p className="text-xs text-muted-foreground">
                        {new Date(entry.changedAt).toLocaleString()}
                      </p>
                      {entry.reason && (
                        <p className="mt-1 text-sm text-muted-foreground">{entry.reason}</p>
                      )}
                      {entry.notes && (
                        <p className="mt-1 text-sm text-muted-foreground">{entry.notes}</p>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

function DetailSkeleton() {
  return (
    <div className="space-y-6 p-4">
      <div className="flex items-center gap-4">
        <Skeleton className="h-10 w-10" />
        <div className="space-y-2">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-4 w-32" />
        </div>
        <div className="ml-auto flex gap-2">
          <Skeleton className="h-10 w-32" />
          <Skeleton className="h-10 w-10" />
        </div>
      </div>
      <div className="grid gap-4 md:grid-cols-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <Card key={i}>
            <CardHeader><Skeleton className="h-5 w-32" /></CardHeader>
            <CardContent className="grid grid-cols-2 gap-4">
              {Array.from({ length: 4 }).map((_, j) => (
                <div key={j} className="space-y-1">
                  <Skeleton className="h-4 w-24" />
                  <Skeleton className="h-5 w-40" />
                </div>
              ))}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}

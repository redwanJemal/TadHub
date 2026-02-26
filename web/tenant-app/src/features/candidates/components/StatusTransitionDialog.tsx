import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { Button } from '@/shared/components/ui/button';
import { Label } from '@/shared/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { ALLOWED_TRANSITIONS } from '../constants';
import { StatusBadge } from './StatusBadge';
import { useTransitionStatus } from '../hooks';
import type { CandidateStatus } from '../types';

const REASON_REQUIRED_STATUSES: CandidateStatus[] = [
  'Rejected',
  'Cancelled',
  'FailedMedicalAbroad',
  'VisaDenied',
  'ReturnedAfterArrival',
];

interface StatusTransitionDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  candidateId: string;
  currentStatus: CandidateStatus;
}

export function StatusTransitionDialog({
  open,
  onOpenChange,
  candidateId,
  currentStatus,
}: StatusTransitionDialogProps) {
  const { t } = useTranslation('candidates');
  const [targetStatus, setTargetStatus] = useState('');
  const [reason, setReason] = useState('');
  const [notes, setNotes] = useState('');
  const transition = useTransitionStatus();

  const needsReason = REASON_REQUIRED_STATUSES.includes(targetStatus as CandidateStatus);
  const canSubmit = targetStatus && (!needsReason || reason.trim());

  const availableStatuses = ALLOWED_TRANSITIONS[currentStatus] ?? [];

  const handleSubmit = async () => {
    if (!canSubmit) return;
    await transition.mutateAsync({
      id: candidateId,
      data: {
        status: targetStatus,
        reason: reason.trim() || undefined,
        notes: notes.trim() || undefined,
      },
    });
    setTargetStatus('');
    setReason('');
    setNotes('');
    onOpenChange(false);
  };

  const handleOpenChange = (value: boolean) => {
    if (!value) {
      setTargetStatus('');
      setReason('');
      setNotes('');
    }
    onOpenChange(value);
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('transition.title')}</DialogTitle>
          <DialogDescription>{t('transition.description')}</DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label>{t('transition.targetStatus')}</Label>
            <Select value={targetStatus} onValueChange={setTargetStatus}>
              <SelectTrigger>
                <SelectValue placeholder={t('transition.targetStatusPlaceholder')} />
              </SelectTrigger>
              <SelectContent>
                {availableStatuses.map((status) => (
                  <SelectItem key={status} value={status}>
                    <StatusBadge status={status} />
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label>
              {t('transition.reason')}
              {needsReason && <span className="text-destructive"> *</span>}
            </Label>
            <textarea
              className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              placeholder={t('transition.reasonPlaceholder')}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
            />
            {needsReason && !reason.trim() && (
              <p className="text-sm text-destructive">{t('transition.reasonRequired')}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label>{t('transition.notes')}</Label>
            <textarea
              className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              placeholder={t('transition.notesPlaceholder')}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => handleOpenChange(false)}>
            {t('common:cancel')}
          </Button>
          <Button onClick={handleSubmit} disabled={!canSubmit || transition.isPending}>
            {transition.isPending ? t('transition.submitting') : t('transition.submit')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

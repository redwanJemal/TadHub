import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/shared/components/ui/dialog';
import { Button } from '@/shared/components/ui/button';
import { Label } from '@/shared/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import { PlacementStatusBadge } from './PlacementStatusBadge';
import { ALLOWED_TRANSITIONS, REASON_REQUIRED_STATUSES, STATUS_CONFIG } from '../constants';
import type { PlacementStatus } from '../types';

interface PlacementTransitionDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  currentStatus: PlacementStatus;
  onTransition: (status: string, reason?: string, notes?: string) => void;
  isPending?: boolean;
}

export function PlacementTransitionDialog({
  open,
  onOpenChange,
  currentStatus,
  onTransition,
  isPending,
}: PlacementTransitionDialogProps) {
  const [targetStatus, setTargetStatus] = useState('');
  const [reason, setReason] = useState('');
  const [notes, setNotes] = useState('');

  const allowedTargets = ALLOWED_TRANSITIONS[currentStatus] || [];
  const requiresReason = REASON_REQUIRED_STATUSES.includes(targetStatus as PlacementStatus);

  useEffect(() => {
    if (!open) {
      setTargetStatus('');
      setReason('');
      setNotes('');
    }
  }, [open]);

  const canSubmit = targetStatus && (!requiresReason || reason.trim());

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Advance Pipeline Stage</DialogTitle>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div>
            <Label className="text-sm text-muted-foreground">Current Status</Label>
            <div className="mt-1">
              <PlacementStatusBadge status={currentStatus} />
            </div>
          </div>

          <div className="space-y-2">
            <Label>Target Status</Label>
            <Select value={targetStatus} onValueChange={setTargetStatus}>
              <SelectTrigger>
                <SelectValue placeholder="Select next stage..." />
              </SelectTrigger>
              <SelectContent>
                {allowedTargets.map((s) => (
                  <SelectItem key={s} value={s}>
                    {STATUS_CONFIG[s]?.label || s}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {requiresReason && (
            <div className="space-y-2">
              <Label>Reason *</Label>
              <textarea
                className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={reason}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setReason(e.target.value)}
                placeholder="Provide reason for cancellation..."
                rows={3}
              />
            </div>
          )}

          <div className="space-y-2">
            <Label>Notes (optional)</Label>
            <textarea
              className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              value={notes}
              onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setNotes(e.target.value)}
              placeholder="Additional notes..."
              rows={2}
            />
          </div>
        </div>

        <DialogFooter className="gap-2 sm:space-x-reverse">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={() => onTransition(targetStatus, reason || undefined, notes || undefined)}
            disabled={!canSubmit || isPending}
          >
            {isPending ? 'Advancing...' : 'Advance'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

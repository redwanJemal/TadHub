import { useState, useEffect } from 'react';
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
import { Input } from '@/shared/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { ALL_STATUSES } from '../constants';
import { useUpdateWorkerDocument } from '../hooks';
import type { WorkerDocumentDto } from '../types';

interface EditDocumentDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  workerId: string;
  document: WorkerDocumentDto;
}

export function EditDocumentDialog({ open, onOpenChange, workerId, document }: EditDocumentDialogProps) {
  const { t } = useTranslation('documents');
  const updateMutation = useUpdateWorkerDocument(workerId);

  const [documentNumber, setDocumentNumber] = useState(document.documentNumber ?? '');
  const [issuedAt, setIssuedAt] = useState(document.issuedAt?.slice(0, 10) ?? '');
  const [expiresAt, setExpiresAt] = useState(document.expiresAt?.slice(0, 10) ?? '');
  const [status, setStatus] = useState(document.status);
  const [issuingAuthority, setIssuingAuthority] = useState(document.issuingAuthority ?? '');
  const [notes, setNotes] = useState(document.notes ?? '');

  useEffect(() => {
    setDocumentNumber(document.documentNumber ?? '');
    setIssuedAt(document.issuedAt?.slice(0, 10) ?? '');
    setExpiresAt(document.expiresAt?.slice(0, 10) ?? '');
    setStatus(document.status);
    setIssuingAuthority(document.issuingAuthority ?? '');
    setNotes(document.notes ?? '');
  }, [document]);

  const handleSubmit = async () => {
    await updateMutation.mutateAsync({
      id: document.id,
      data: {
        documentNumber: documentNumber || undefined,
        issuedAt: issuedAt || undefined,
        expiresAt: expiresAt || undefined,
        status,
        issuingAuthority: issuingAuthority || undefined,
        notes: notes.trim() || undefined,
      },
    });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('edit.title')}</DialogTitle>
          <DialogDescription>
            {t(`documentType.${document.documentType}`)}
            {document.documentNumber ? ` - ${document.documentNumber}` : ''}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label>{t('edit.documentNumber')}</Label>
            <Input
              value={documentNumber}
              onChange={(e) => setDocumentNumber(e.target.value)}
              placeholder={t('create.documentNumberPlaceholder')}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>{t('edit.issuedAt')}</Label>
              <Input
                type="date"
                value={issuedAt}
                onChange={(e) => setIssuedAt(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <Label>{t('edit.expiresAt')}</Label>
              <Input
                type="date"
                value={expiresAt}
                onChange={(e) => setExpiresAt(e.target.value)}
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label>{t('edit.status')}</Label>
            <Select value={status} onValueChange={(v) => setStatus(v as typeof status)}>
              <SelectTrigger>
                <SelectValue placeholder={t('create.statusPlaceholder')} />
              </SelectTrigger>
              <SelectContent>
                {ALL_STATUSES.map((s) => (
                  <SelectItem key={s} value={s}>
                    {t(`status.${s}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label>{t('edit.issuingAuthority')}</Label>
            <Input
              value={issuingAuthority}
              onChange={(e) => setIssuingAuthority(e.target.value)}
              placeholder={t('create.issuingAuthorityPlaceholder')}
            />
          </div>

          <div className="space-y-2">
            <Label>{t('edit.notes')}</Label>
            <textarea
              className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              placeholder={t('create.notesPlaceholder')}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            {t('common:cancel')}
          </Button>
          <Button onClick={handleSubmit} disabled={updateMutation.isPending}>
            {updateMutation.isPending ? t('edit.submitting') : t('edit.submit')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

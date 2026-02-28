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
import { Input } from '@/shared/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { ALL_DOCUMENT_TYPES, ALL_STATUSES } from '../constants';
import { useCreateWorkerDocument } from '../hooks';

interface CreateDocumentDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  workerId: string;
}

export function CreateDocumentDialog({ open, onOpenChange, workerId }: CreateDocumentDialogProps) {
  const { t } = useTranslation('documents');
  const createMutation = useCreateWorkerDocument(workerId);

  const [documentType, setDocumentType] = useState('');
  const [documentNumber, setDocumentNumber] = useState('');
  const [issuedAt, setIssuedAt] = useState('');
  const [expiresAt, setExpiresAt] = useState('');
  const [status, setStatus] = useState('');
  const [issuingAuthority, setIssuingAuthority] = useState('');
  const [notes, setNotes] = useState('');

  const resetForm = () => {
    setDocumentType('');
    setDocumentNumber('');
    setIssuedAt('');
    setExpiresAt('');
    setStatus('');
    setIssuingAuthority('');
    setNotes('');
  };

  const handleSubmit = async () => {
    if (!documentType) return;

    await createMutation.mutateAsync({
      documentType,
      documentNumber: documentNumber || undefined,
      issuedAt: issuedAt || undefined,
      expiresAt: expiresAt || undefined,
      status: status || undefined,
      issuingAuthority: issuingAuthority || undefined,
      notes: notes.trim() || undefined,
    });

    resetForm();
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={(val) => { if (!val) resetForm(); onOpenChange(val); }}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('create.title')}</DialogTitle>
          <DialogDescription>{t('create.description')}</DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label>{t('create.documentType')}</Label>
            <Select value={documentType} onValueChange={setDocumentType}>
              <SelectTrigger>
                <SelectValue placeholder={t('create.documentTypePlaceholder')} />
              </SelectTrigger>
              <SelectContent>
                {ALL_DOCUMENT_TYPES.map((dt) => (
                  <SelectItem key={dt} value={dt}>
                    {t(`documentType.${dt}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label>{t('create.documentNumber')}</Label>
            <Input
              value={documentNumber}
              onChange={(e) => setDocumentNumber(e.target.value)}
              placeholder={t('create.documentNumberPlaceholder')}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>{t('create.issuedAt')}</Label>
              <Input
                type="date"
                value={issuedAt}
                onChange={(e) => setIssuedAt(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <Label>{t('create.expiresAt')}</Label>
              <Input
                type="date"
                value={expiresAt}
                onChange={(e) => setExpiresAt(e.target.value)}
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label>{t('create.status')}</Label>
            <Select value={status} onValueChange={setStatus}>
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
            <Label>{t('create.issuingAuthority')}</Label>
            <Input
              value={issuingAuthority}
              onChange={(e) => setIssuingAuthority(e.target.value)}
              placeholder={t('create.issuingAuthorityPlaceholder')}
            />
          </div>

          <div className="space-y-2">
            <Label>{t('create.notes')}</Label>
            <textarea
              className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              placeholder={t('create.notesPlaceholder')}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => { resetForm(); onOpenChange(false); }}>
            {t('common:cancel')}
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={!documentType || createMutation.isPending}
          >
            {createMutation.isPending ? t('create.submitting') : t('create.submit')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

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
import { ALL_LOCATIONS } from '../constants';
import { useUpdateWorker } from '../hooks';
import type { WorkerDto, WorkerLocation } from '../types';

interface WorkerEditDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  worker: WorkerDto;
}

export function WorkerEditDialog({ open, onOpenChange, worker }: WorkerEditDialogProps) {
  const { t } = useTranslation('workers');
  const updateMutation = useUpdateWorker();

  const [location, setLocation] = useState<WorkerLocation>(worker.location);
  const [procurementPaidAt, setProcurementPaidAt] = useState(worker.procurementPaidAt?.slice(0, 10) ?? '');
  const [flightDate, setFlightDate] = useState(worker.flightDate?.slice(0, 10) ?? '');
  const [arrivedAt, setArrivedAt] = useState(worker.arrivedAt?.slice(0, 10) ?? '');
  const [notes, setNotes] = useState(worker.notes ?? '');

  useEffect(() => {
    setLocation(worker.location);
    setProcurementPaidAt(worker.procurementPaidAt?.slice(0, 10) ?? '');
    setFlightDate(worker.flightDate?.slice(0, 10) ?? '');
    setArrivedAt(worker.arrivedAt?.slice(0, 10) ?? '');
    setNotes(worker.notes ?? '');
  }, [worker]);

  const handleSubmit = async () => {
    await updateMutation.mutateAsync({
      id: worker.id,
      data: {
        location,
        procurementPaidAt: procurementPaidAt || undefined,
        flightDate: flightDate || undefined,
        arrivedAt: arrivedAt || undefined,
        notes: notes.trim() || undefined,
      },
    });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('editDialog.title')}</DialogTitle>
          <DialogDescription>{worker.fullNameEn} ({worker.workerCode})</DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label>{t('editDialog.location')}</Label>
            <Select value={location} onValueChange={(v) => setLocation(v as WorkerLocation)}>
              <SelectTrigger>
                <SelectValue placeholder={t('editDialog.locationPlaceholder')} />
              </SelectTrigger>
              <SelectContent>
                {ALL_LOCATIONS.map((loc) => (
                  <SelectItem key={loc} value={loc}>
                    {t(`location.${loc}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label>{t('editDialog.procurementPaidAt')}</Label>
            <Input
              type="date"
              value={procurementPaidAt}
              onChange={(e) => setProcurementPaidAt(e.target.value)}
            />
          </div>

          <div className="space-y-2">
            <Label>{t('editDialog.flightDate')}</Label>
            <Input
              type="date"
              value={flightDate}
              onChange={(e) => setFlightDate(e.target.value)}
            />
          </div>

          <div className="space-y-2">
            <Label>{t('editDialog.arrivedAt')}</Label>
            <Input
              type="date"
              value={arrivedAt}
              onChange={(e) => setArrivedAt(e.target.value)}
            />
          </div>

          <div className="space-y-2">
            <Label>{t('editDialog.notes')}</Label>
            <textarea
              className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              placeholder={t('editDialog.notesPlaceholder')}
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
            {updateMutation.isPending ? t('editDialog.submitting') : t('editDialog.submit')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

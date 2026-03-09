import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { useWorkers } from '@/features/workers/hooks';
import { useCheckIn } from '../hooks';

export function CheckInPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const checkInMutation = useCheckIn();

  const [workerId, setWorkerId] = useState('');
  const [workerSearch, setWorkerSearch] = useState('');
  const [room, setRoom] = useState('');
  const [location, setLocation] = useState('');

  const { data: workersData } = useWorkers({ pageSize: 5, search: workerSearch || undefined });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!workerId) return;

    checkInMutation.mutate(
      {
        workerId: workerId.trim(),
        room: room.trim() || undefined,
        location: location.trim() || undefined,
      },
      {
        onSuccess: (data) => {
          toast.success(t('accommodations.checkedInSuccess', 'Maid checked in successfully'));
          navigate(`/accommodations/${data.id}`);
        },
        onError: () => toast.error(t('accommodations.checkInFailed', 'Failed to check in')),
      }
    );
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate('/accommodations')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('back', 'Back')}
        </Button>
      </div>

      <div>
        <h1 className="text-2xl font-bold">{t('accommodations.checkInTitle', 'Check In Maid')}</h1>
        <p className="text-muted-foreground">
          {t('accommodations.checkInSubtitle', 'Register a maid arriving at accommodation.')}
        </p>
      </div>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle>{t('accommodations.checkInDetails', 'Check-In Details')}</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <Label>{t('accommodations.workerId', 'Worker')} *</Label>
              <Input
                className="mt-1"
                placeholder={t('accommodations.searchWorker', 'Search workers...')}
                value={workerSearch}
                onChange={(e) => { setWorkerSearch(e.target.value); if (workerId) { setWorkerId(''); } }}
              />
              {workersData && workersData.items.length > 0 && workerSearch && !workerId && (
                <div className="border rounded-md max-h-40 overflow-y-auto mt-1">
                  {workersData.items.map((w) => (
                    <button key={w.id} type="button" className="w-full text-start px-3 py-2 hover:bg-muted text-sm"
                      onClick={() => { setWorkerId(w.id); setWorkerSearch(`${w.fullNameEn} (${w.workerCode})`); }}>
                      <span className="font-medium">{w.fullNameEn}</span>
                      <span className="text-muted-foreground ms-2 font-mono text-xs">{w.workerCode}</span>
                    </button>
                  ))}
                </div>
              )}
              {workerId && (
                <button type="button" className="text-xs text-muted-foreground hover:text-foreground mt-1"
                  onClick={() => { setWorkerId(''); setWorkerSearch(''); }}>
                  {t('cancel', 'Clear')}
                </button>
              )}
            </div>

            <div>
              <label className="text-sm font-medium">
                {t('accommodations.room', 'Room')}
              </label>
              <input
                type="text"
                value={room}
                onChange={(e) => setRoom(e.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                placeholder={t('accommodations.roomPlaceholder', 'e.g., Room 101')}
              />
            </div>

            <div>
              <label className="text-sm font-medium">
                {t('accommodations.location', 'Location')}
              </label>
              <input
                type="text"
                value={location}
                onChange={(e) => setLocation(e.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                placeholder={t('accommodations.locationPlaceholder', 'e.g., Building A, Floor 2')}
              />
            </div>

            <div className="flex justify-end gap-3 pt-4">
              <Button type="button" variant="outline" onClick={() => navigate('/accommodations')}>
                {t('cancel', 'Cancel')}
              </Button>
              <Button type="submit" disabled={!workerId || checkInMutation.isPending}>
                {checkInMutation.isPending
                  ? t('accommodations.checkingIn', 'Checking in...')
                  : t('accommodations.checkIn', 'Check In')}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}

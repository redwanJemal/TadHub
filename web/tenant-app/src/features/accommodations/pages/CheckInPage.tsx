import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { useCheckIn } from '../hooks';

export function CheckInPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const checkInMutation = useCheckIn();

  const [workerId, setWorkerId] = useState('');
  const [room, setRoom] = useState('');
  const [location, setLocation] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!workerId.trim()) return;

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
              <label className="text-sm font-medium">
                {t('accommodations.workerId', 'Worker ID')} *
              </label>
              <input
                type="text"
                value={workerId}
                onChange={(e) => setWorkerId(e.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                placeholder={t('accommodations.workerIdPlaceholder', 'Enter worker ID (UUID)')}
                required
              />
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
              <Button type="submit" disabled={!workerId.trim() || checkInMutation.isPending}>
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

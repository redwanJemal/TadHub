import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';
import { ArrowLeft, Plane } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { useWorkers } from '@/features/workers/hooks';
import { usePlacements } from '@/features/placements/hooks';
import { useScheduleArrival } from '../hooks';

export function ScheduleArrivalPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const scheduleMutation = useScheduleArrival();

  const [placementId, setPlacementId] = useState(searchParams.get('placementId') || '');
  const [workerId, setWorkerId] = useState(searchParams.get('workerId') || '');
  const [flightNumber, setFlightNumber] = useState('');
  const [airportCode, setAirportCode] = useState('');
  const [airportName, setAirportName] = useState('');
  const [scheduledArrivalDate, setScheduledArrivalDate] = useState('');
  const [scheduledArrivalTime, setScheduledArrivalTime] = useState('');
  const [notes, setNotes] = useState('');

  const [workerSearch, setWorkerSearch] = useState('');
  const [placementSearch, setPlacementSearch] = useState('');

  const { data: workersData } = useWorkers({ pageSize: 5, search: workerSearch || undefined });
  const { data: placementsData } = usePlacements({ pageSize: 5, search: placementSearch || undefined });

  const canSubmit = placementId.trim() && workerId.trim() && scheduledArrivalDate;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!canSubmit) return;

    scheduleMutation.mutate(
      {
        placementId,
        workerId,
        flightNumber: flightNumber || undefined,
        airportCode: airportCode || undefined,
        airportName: airportName || undefined,
        scheduledArrivalDate,
        scheduledArrivalTime: scheduledArrivalTime || undefined,
        notes: notes || undefined,
      },
      {
        onSuccess: (data) => {
          toast.success(t('arrivals.scheduled', 'Arrival scheduled successfully'));
          navigate(`/arrivals/${data.id}`);
        },
        onError: () => {
          toast.error(t('arrivals.scheduleFailed', 'Failed to schedule arrival'));
        },
      }
    );
  };

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate('/arrivals')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('back', 'Back')}
        </Button>
      </div>

      <div>
        <h1 className="text-2xl font-bold">{t('arrivals.scheduleTitle', 'Schedule Arrival')}</h1>
        <p className="text-muted-foreground">
          {t('arrivals.scheduleSubtitle', 'Create a new arrival record for a maid arriving from abroad.')}
        </p>
      </div>

      <form onSubmit={handleSubmit}>
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Plane className="h-5 w-5" />
              {t('arrivals.arrivalDetails', 'Arrival Details')}
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>{t('arrivals.placementId', 'Placement')} *</Label>
                <Input
                  className="mt-1"
                  placeholder={t('arrivals.searchPlacement', 'Search placements...')}
                  value={placementSearch}
                  onChange={(e) => { setPlacementSearch(e.target.value); if (placementId) { setPlacementId(''); } }}
                />
                {placementsData && placementsData.items.length > 0 && placementSearch && !placementId && (
                  <div className="border rounded-md max-h-40 overflow-y-auto mt-1">
                    {placementsData.items.map((p) => (
                      <button
                        key={p.id}
                        type="button"
                        className="w-full text-start px-3 py-2 hover:bg-muted text-sm"
                        onClick={() => {
                          setPlacementId(p.id);
                          const label = p.candidate?.fullNameEn
                            ? `${p.placementCode} — ${p.candidate.fullNameEn}`
                            : p.placementCode;
                          setPlacementSearch(label);
                        }}
                      >
                        <span className="font-medium font-mono text-xs">{p.placementCode}</span>
                        {p.candidate?.fullNameEn && (
                          <span className="text-muted-foreground ms-2 text-xs">{p.candidate.fullNameEn}</span>
                        )}
                        {p.client?.nameEn && (
                          <span className="text-muted-foreground ms-2 text-xs">({p.client.nameEn})</span>
                        )}
                      </button>
                    ))}
                  </div>
                )}
                {placementId && (
                  <button
                    type="button"
                    className="text-xs text-muted-foreground hover:text-foreground mt-1"
                    onClick={() => { setPlacementId(''); setPlacementSearch(''); }}
                  >
                    {t('cancel', 'Clear')}
                  </button>
                )}
              </div>
              <div>
                <Label>{t('arrivals.workerId', 'Worker')} *</Label>
                <Input
                  className="mt-1"
                  placeholder={t('arrivals.searchWorker', 'Search workers...')}
                  value={workerSearch}
                  onChange={(e) => { setWorkerSearch(e.target.value); if (workerId) { setWorkerId(''); } }}
                />
                {workersData && workersData.items.length > 0 && workerSearch && !workerId && (
                  <div className="border rounded-md max-h-40 overflow-y-auto mt-1">
                    {workersData.items.map((w) => (
                      <button
                        key={w.id}
                        type="button"
                        className="w-full text-start px-3 py-2 hover:bg-muted text-sm"
                        onClick={() => { setWorkerId(w.id); setWorkerSearch(`${w.fullNameEn} (${w.workerCode})`); }}
                      >
                        <span className="font-medium">{w.fullNameEn}</span>
                        <span className="text-muted-foreground ms-2 font-mono text-xs">{w.workerCode}</span>
                      </button>
                    ))}
                  </div>
                )}
                {workerId && (
                  <button
                    type="button"
                    className="text-xs text-muted-foreground hover:text-foreground mt-1"
                    onClick={() => { setWorkerId(''); setWorkerSearch(''); }}
                  >
                    {t('cancel', 'Clear')}
                  </button>
                )}
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium">
                  {t('arrivals.scheduledDate', 'Scheduled Date')} *
                </label>
                <input
                  type="date"
                  value={scheduledArrivalDate}
                  onChange={(e) => setScheduledArrivalDate(e.target.value)}
                  className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                  required
                />
              </div>
              <div>
                <label className="text-sm font-medium">
                  {t('arrivals.scheduledTime', 'Scheduled Time')}
                </label>
                <input
                  type="time"
                  value={scheduledArrivalTime}
                  onChange={(e) => setScheduledArrivalTime(e.target.value)}
                  className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                />
              </div>
            </div>

            <div className="grid grid-cols-3 gap-4">
              <div>
                <label className="text-sm font-medium">
                  {t('arrivals.flightNumber', 'Flight Number')}
                </label>
                <input
                  type="text"
                  value={flightNumber}
                  onChange={(e) => setFlightNumber(e.target.value)}
                  className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                  placeholder="EK 123"
                />
              </div>
              <div>
                <label className="text-sm font-medium">
                  {t('arrivals.airportCode', 'Airport Code')}
                </label>
                <input
                  type="text"
                  value={airportCode}
                  onChange={(e) => setAirportCode(e.target.value)}
                  className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                  placeholder="DXB"
                />
              </div>
              <div>
                <label className="text-sm font-medium">
                  {t('arrivals.airportName', 'Airport Name')}
                </label>
                <input
                  type="text"
                  value={airportName}
                  onChange={(e) => setAirportName(e.target.value)}
                  className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                  placeholder="Dubai International"
                />
              </div>
            </div>

            <div>
              <label className="text-sm font-medium">{t('arrivals.notes', 'Notes')}</label>
              <textarea
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                rows={3}
              />
            </div>
          </CardContent>
        </Card>

        <div className="mt-6 flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate('/arrivals')}>
            {t('cancel', 'Cancel')}
          </Button>
          <Button type="submit" disabled={!canSubmit || scheduleMutation.isPending}>
            {scheduleMutation.isPending
              ? t('loading', 'Loading...')
              : t('arrivals.schedule', 'Schedule Arrival')}
          </Button>
        </div>
      </form>
    </div>
  );
}

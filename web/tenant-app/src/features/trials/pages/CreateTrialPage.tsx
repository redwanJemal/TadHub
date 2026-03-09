import { useState } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { useWorkers } from '@/features/workers/hooks';
import { useClients } from '@/features/clients/hooks';
import { useCreateTrial } from '../hooks';
import type { CreateTrialRequest } from '../types';

export function CreateTrialPage() {
  const { t } = useTranslation('trials');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const createMutation = useCreateTrial();

  const [workerId, setWorkerId] = useState(searchParams.get('workerId') || '');
  const [clientId, setClientId] = useState(searchParams.get('clientId') || '');
  const [startDate, setStartDate] = useState(new Date().toISOString().split('T')[0]);
  const [notes, setNotes] = useState('');

  const [workerSearch, setWorkerSearch] = useState('');
  const [clientSearch, setClientSearch] = useState('');

  const { data: workersData } = useWorkers({
    pageSize: 5,
    search: workerSearch || undefined,
  });

  const { data: clientsData } = useClients({
    pageSize: 5,
    search: clientSearch || undefined,
  });

  const endDate = startDate
    ? (() => {
        const d = new Date(startDate);
        d.setDate(d.getDate() + 5);
        return d.toISOString().split('T')[0];
      })()
    : '';

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!workerId || !clientId || !startDate) return;

    const data: CreateTrialRequest = {
      workerId,
      clientId,
      startDate,
      placementId: searchParams.get('placementId') || undefined,
      notes: notes || undefined,
    };

    createMutation.mutate(data, {
      onSuccess: (result) => {
        navigate(`/trials/${result.id}`);
      },
    });
  };

  return (
    <div className="mx-auto max-w-2xl space-y-6 p-6">
      <div>
        <Link
          to="/trials"
          className="mb-2 inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" />
          {t('back_to_list')}
        </Link>
        <h1 className="text-2xl font-semibold">{t('create_trial')}</h1>
        <p className="text-sm text-muted-foreground">{t('create_description')}</p>
      </div>

      <form onSubmit={handleSubmit}>
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t('trial_details')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>{t('worker_id')} *</Label>
              <Input
                placeholder={t('worker_id_placeholder')}
                value={workerSearch}
                onChange={(e) => {
                  setWorkerSearch(e.target.value);
                  if (workerId) {
                    setWorkerId('');
                  }
                }}
              />
              {workersData && workersData.items.length > 0 && workerSearch && !workerId && (
                <div className="border rounded-md max-h-40 overflow-y-auto">
                  {workersData.items.map((w) => (
                    <button
                      key={w.id}
                      type="button"
                      className="w-full text-start px-3 py-2 hover:bg-muted text-sm"
                      onClick={() => {
                        setWorkerId(w.id);
                        setWorkerSearch(`${w.fullNameEn} (${w.workerCode})`);
                      }}
                    >
                      <span className="font-medium">{w.fullNameEn}</span>
                      <span className="text-muted-foreground ms-2 font-mono text-xs">
                        {w.workerCode}
                      </span>
                    </button>
                  ))}
                </div>
              )}
              {workerId && (
                <button
                  type="button"
                  className="text-xs text-muted-foreground hover:text-foreground"
                  onClick={() => {
                    setWorkerId('');
                    setWorkerSearch('');
                  }}
                >
                  {t('common:cancel')}
                </button>
              )}
            </div>

            <div className="space-y-2">
              <Label>{t('client_id')} *</Label>
              <Input
                placeholder={t('client_id_placeholder')}
                value={clientSearch}
                onChange={(e) => {
                  setClientSearch(e.target.value);
                  if (clientId) {
                    setClientId('');
                  }
                }}
              />
              {clientsData && clientsData.items.length > 0 && clientSearch && !clientId && (
                <div className="border rounded-md max-h-40 overflow-y-auto">
                  {clientsData.items.map((c) => (
                    <button
                      key={c.id}
                      type="button"
                      className="w-full text-start px-3 py-2 hover:bg-muted text-sm"
                      onClick={() => {
                        setClientId(c.id);
                        setClientSearch(
                          c.nameAr ? `${c.nameEn} (${c.nameAr})` : c.nameEn
                        );
                      }}
                    >
                      <span className="font-medium">{c.nameEn}</span>
                      {c.nameAr && (
                        <span className="text-muted-foreground ms-2 text-xs">
                          {c.nameAr}
                        </span>
                      )}
                    </button>
                  ))}
                </div>
              )}
              {clientId && (
                <button
                  type="button"
                  className="text-xs text-muted-foreground hover:text-foreground"
                  onClick={() => {
                    setClientId('');
                    setClientSearch('');
                  }}
                >
                  {t('common:cancel')}
                </button>
              )}
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>{t('start_date')}</Label>
                <Input
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  required
                />
              </div>
              <div className="space-y-2">
                <Label>{t('end_date')}</Label>
                <Input
                  type="date"
                  value={endDate}
                  disabled
                />
                <p className="text-xs text-muted-foreground">{t('end_date_auto')}</p>
              </div>
            </div>

            <div className="space-y-2">
              <Label>{t('notes')}</Label>
              <textarea
                className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={notes}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setNotes(e.target.value)}
                rows={3}
                placeholder={t('notes_placeholder')}
              />
            </div>
          </CardContent>
        </Card>

        <div className="mt-6 flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate('/trials')}>
            {t('common:cancel')}
          </Button>
          <Button
            type="submit"
            disabled={!workerId || !clientId || !startDate || createMutation.isPending}
          >
            {createMutation.isPending ? t('creating') : t('create_trial')}
          </Button>
        </div>
      </form>
    </div>
  );
}

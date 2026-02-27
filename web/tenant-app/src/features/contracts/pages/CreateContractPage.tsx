import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Label } from '@/shared/components/ui/label';
import { Input } from '@/shared/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { useWorkers } from '@/features/workers/hooks';
import { useClients } from '@/features/clients/hooks';
import { useCreateContract } from '../hooks';
import { ALL_TYPES, ALL_RATE_PERIODS } from '../constants';

export function CreateContractPage() {
  const { t } = useTranslation('contracts');
  const navigate = useNavigate();
  const createMutation = useCreateContract();

  // Form state
  const [workerId, setWorkerId] = useState('');
  const [clientId, setClientId] = useState('');
  const [type, setType] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [probationEndDate, setProbationEndDate] = useState('');
  const [guaranteeEndDate, setGuaranteeEndDate] = useState('');
  const [rate, setRate] = useState('');
  const [ratePeriod, setRatePeriod] = useState('Monthly');
  const [currency, setCurrency] = useState('AED');
  const [totalValue, setTotalValue] = useState('');
  const [notes, setNotes] = useState('');

  // Worker search
  const [workerSearch, setWorkerSearch] = useState('');
  const { data: workersData } = useWorkers({
    pageSize: 50,
    search: workerSearch || undefined,
    'filter[status]': 'Available',
  });

  // Client search
  const [clientSearch, setClientSearch] = useState('');
  const { data: clientsData } = useClients({
    pageSize: 50,
    search: clientSearch || undefined,
    'filter[isActive]': 'true',
  });

  const canSubmit = workerId && clientId && type && startDate && rate;

  const handleSubmit = async () => {
    if (!canSubmit) return;
    await createMutation.mutateAsync({
      workerId,
      clientId,
      type,
      startDate,
      endDate: endDate || undefined,
      probationEndDate: probationEndDate || undefined,
      guaranteeEndDate: guaranteeEndDate || undefined,
      rate: parseFloat(rate),
      ratePeriod,
      currency,
      totalValue: totalValue ? parseFloat(totalValue) : undefined,
      notes: notes.trim() || undefined,
    });
    navigate('/contracts');
  };

  return (
    <div className="space-y-6">
      <div>
        <Link
          to="/contracts"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-4"
        >
          <ArrowLeft className="h-4 w-4" />
          {t('create.backToList')}
        </Link>
        <h1 className="text-2xl font-bold tracking-tight">{t('create.title')}</h1>
        <p className="text-muted-foreground">{t('create.description')}</p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Parties */}
        <Card>
          <CardHeader><CardTitle>{t('detail.parties')}</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>{t('create.worker')} *</Label>
              <Input
                placeholder={t('create.workerPlaceholder')}
                value={workerSearch}
                onChange={(e) => { setWorkerSearch(e.target.value); if (workerId) { setWorkerId(''); } }}
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
                      <span className="text-muted-foreground ms-2 font-mono text-xs">{w.workerCode}</span>
                    </button>
                  ))}
                </div>
              )}
              {workerId && (
                <button
                  type="button"
                  className="text-xs text-muted-foreground hover:text-foreground"
                  onClick={() => { setWorkerId(''); setWorkerSearch(''); }}
                >
                  {t('common:cancel')}
                </button>
              )}
            </div>

            <div className="space-y-2">
              <Label>{t('create.client')} *</Label>
              <Input
                placeholder={t('create.clientPlaceholder')}
                value={clientSearch}
                onChange={(e) => { setClientSearch(e.target.value); if (clientId) { setClientId(''); } }}
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
                        setClientSearch(c.nameEn);
                      }}
                    >
                      <span className="font-medium">{c.nameEn}</span>
                      {c.nameAr && <span className="text-muted-foreground ms-2 text-xs" dir="rtl">{c.nameAr}</span>}
                    </button>
                  ))}
                </div>
              )}
              {clientId && (
                <button
                  type="button"
                  className="text-xs text-muted-foreground hover:text-foreground"
                  onClick={() => { setClientId(''); setClientSearch(''); }}
                >
                  {t('common:cancel')}
                </button>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Contract Details */}
        <Card>
          <CardHeader><CardTitle>{t('detail.overview')}</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>{t('create.type')} *</Label>
              <Select value={type} onValueChange={setType}>
                <SelectTrigger>
                  <SelectValue placeholder={t('create.typePlaceholder')} />
                </SelectTrigger>
                <SelectContent>
                  {ALL_TYPES.map((tp) => (
                    <SelectItem key={tp} value={tp}>{t(`type.${tp}`)}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>{t('create.startDate')} *</Label>
                <Input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} />
              </div>
              <div className="space-y-2">
                <Label>{t('create.endDate')}</Label>
                <Input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>{t('create.probationEndDate')}</Label>
                <Input type="date" value={probationEndDate} onChange={(e) => setProbationEndDate(e.target.value)} />
              </div>
              <div className="space-y-2">
                <Label>{t('create.guaranteeEndDate')}</Label>
                <Input type="date" value={guaranteeEndDate} onChange={(e) => setGuaranteeEndDate(e.target.value)} />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Financial */}
        <Card>
          <CardHeader><CardTitle>{t('detail.financial')}</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>{t('create.rate')} *</Label>
                <Input
                  type="number"
                  min="0"
                  step="0.01"
                  value={rate}
                  onChange={(e) => setRate(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <Label>{t('create.ratePeriod')}</Label>
                <Select value={ratePeriod} onValueChange={setRatePeriod}>
                  <SelectTrigger>
                    <SelectValue placeholder={t('create.ratePeriodPlaceholder')} />
                  </SelectTrigger>
                  <SelectContent>
                    {ALL_RATE_PERIODS.map((rp) => (
                      <SelectItem key={rp} value={rp}>{t(`ratePeriod.${rp}`)}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>{t('create.currency')}</Label>
                <Input value={currency} onChange={(e) => setCurrency(e.target.value)} />
              </div>
              <div className="space-y-2">
                <Label>{t('create.totalValue')}</Label>
                <Input
                  type="number"
                  min="0"
                  step="0.01"
                  value={totalValue}
                  onChange={(e) => setTotalValue(e.target.value)}
                />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Notes */}
        <Card>
          <CardHeader><CardTitle>{t('create.notes')}</CardTitle></CardHeader>
          <CardContent>
            <textarea
              className="flex min-h-[100px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
              placeholder={t('create.notesPlaceholder')}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </CardContent>
        </Card>
      </div>

      <div className="flex justify-end gap-3">
        <Button variant="outline" onClick={() => navigate('/contracts')}>
          {t('common:cancel')}
        </Button>
        <Button onClick={handleSubmit} disabled={!canSubmit || createMutation.isPending}>
          {createMutation.isPending ? t('create.submitting') : t('create.submit')}
        </Button>
      </div>
    </div>
  );
}

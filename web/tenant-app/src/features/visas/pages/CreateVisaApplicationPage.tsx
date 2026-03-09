import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import { Badge } from '@/shared/components/ui/badge';
import { Circle } from 'lucide-react';
import { useCreateVisaApplication } from '../hooks';
import { ALL_VISA_TYPES, DOCUMENT_REQUIREMENTS, VISA_DOCUMENT_TYPES } from '../constants';
import { toast } from 'sonner';
import { useWorkers } from '@/features/workers/hooks';
import { useClients } from '@/features/clients/hooks';
import { usePlacements } from '@/features/placements/hooks';

export function CreateVisaApplicationPage() {
  const { t } = useTranslation('visas');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const createMutation = useCreateVisaApplication();

  const [workerId, setWorkerId] = useState(searchParams.get('workerId') ?? '');
  const [clientId, setClientId] = useState(searchParams.get('clientId') ?? '');
  const [visaType, setVisaType] = useState<string>(searchParams.get('visaType') ?? '');
  const [applicationDate, setApplicationDate] = useState('');
  const [referenceNumber, setReferenceNumber] = useState('');
  const [notes, setNotes] = useState('');
  const [placementId, setPlacementId] = useState(searchParams.get('placementId') ?? '');

  const [workerSearch, setWorkerSearch] = useState('');
  const [clientSearch, setClientSearch] = useState('');
  const [placementSearch, setPlacementSearch] = useState('');

  const { data: workersData } = useWorkers({ pageSize: 5, search: workerSearch || undefined });
  const { data: clientsData } = useClients({ pageSize: 5, search: clientSearch || undefined });
  const { data: placementsData } = usePlacements({ pageSize: 5, search: placementSearch || undefined });

  // Determine document requirements based on visa type
  const workerLocation = 'Outside'; // Default; can be enhanced to check worker data
  const requirementKey = visaType ? `${visaType}_${workerLocation}` : '';
  const requirements = requirementKey ? (DOCUMENT_REQUIREMENTS[requirementKey] ?? []) : [];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!workerId || !clientId || !visaType) {
      toast.error(t('notifications.requiredFields'));
      return;
    }

    try {
      const result = await createMutation.mutateAsync({
        workerId,
        clientId,
        visaType,
        applicationDate: applicationDate || undefined,
        referenceNumber: referenceNumber || undefined,
        notes: notes || undefined,
        placementId: placementId || undefined,
      });
      toast.success(t('notifications.created'));
      navigate(`/visa-applications/${result.id}`);
    } catch {
      toast.error(t('notifications.createFailed'));
    }
  };

  return (
    <div className="space-y-6 p-4">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/visa-applications')}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <h1 className="text-2xl font-bold">{t('create.title')}</h1>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="grid gap-6 md:grid-cols-2">
          <Card className="md:col-span-2">
            <CardHeader>
              <CardTitle className="text-base">{t('create.applicationDetails')}</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label>{t('create.visaType')} *</Label>
                <Select value={visaType} onValueChange={setVisaType}>
                  <SelectTrigger>
                    <SelectValue placeholder={t('create.selectVisaType')} />
                  </SelectTrigger>
                  <SelectContent>
                    {ALL_VISA_TYPES.map(vt => (
                      <SelectItem key={vt.value} value={vt.value}>{vt.label}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>{t('create.workerId')} *</Label>
                <Input
                  placeholder={t('create.workerIdPlaceholder')}
                  value={workerSearch}
                  onChange={(e) => { setWorkerSearch(e.target.value); if (workerId) { setWorkerId(''); } }}
                />
                {workersData && workersData.items.length > 0 && workerSearch && !workerId && (
                  <div className="border rounded-md max-h-40 overflow-y-auto">
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
                  <button type="button" className="text-xs text-muted-foreground hover:text-foreground"
                    onClick={() => { setWorkerId(''); setWorkerSearch(''); }}>
                    {t('common:cancel')}
                  </button>
                )}
              </div>

              <div className="space-y-2">
                <Label>{t('create.clientId')} *</Label>
                <Input
                  placeholder={t('create.clientIdPlaceholder')}
                  value={clientSearch}
                  onChange={(e) => { setClientSearch(e.target.value); if (clientId) { setClientId(''); } }}
                />
                {clientsData && clientsData.items.length > 0 && clientSearch && !clientId && (
                  <div className="border rounded-md max-h-40 overflow-y-auto">
                    {clientsData.items.map((c) => (
                      <button key={c.id} type="button" className="w-full text-start px-3 py-2 hover:bg-muted text-sm"
                        onClick={() => { setClientId(c.id); setClientSearch(c.nameEn); }}>
                        <span className="font-medium">{c.nameEn}</span>
                        {c.nameAr && <span className="text-muted-foreground ms-2 text-xs">{c.nameAr}</span>}
                      </button>
                    ))}
                  </div>
                )}
                {clientId && (
                  <button type="button" className="text-xs text-muted-foreground hover:text-foreground"
                    onClick={() => { setClientId(''); setClientSearch(''); }}>
                    {t('common:cancel')}
                  </button>
                )}
              </div>

              <div className="space-y-2">
                <Label>{t('create.applicationDate')}</Label>
                <Input
                  type="date"
                  value={applicationDate}
                  onChange={e => setApplicationDate(e.target.value)}
                />
              </div>

              <div className="space-y-2">
                <Label>{t('create.referenceNumber')}</Label>
                <Input
                  value={referenceNumber}
                  onChange={e => setReferenceNumber(e.target.value)}
                  placeholder={t('create.referenceNumberPlaceholder')}
                />
              </div>

              <div className="space-y-2">
                <Label>{t('create.placementId')}</Label>
                <Input
                  placeholder={t('create.placementIdPlaceholder')}
                  value={placementSearch}
                  onChange={(e) => { setPlacementSearch(e.target.value); if (placementId) { setPlacementId(''); } }}
                />
                {placementsData && placementsData.items.length > 0 && placementSearch && !placementId && (
                  <div className="border rounded-md max-h-40 overflow-y-auto">
                    {placementsData.items.map((p) => (
                      <button key={p.id} type="button" className="w-full text-start px-3 py-2 hover:bg-muted text-sm"
                        onClick={() => { setPlacementId(p.id); setPlacementSearch(p.placementCode); }}>
                        <span className="font-medium font-mono">{p.placementCode}</span>
                        {p.candidate?.fullNameEn && (
                          <span className="text-muted-foreground ms-2 text-xs">{p.candidate.fullNameEn}</span>
                        )}
                      </button>
                    ))}
                  </div>
                )}
                {placementId && (
                  <button type="button" className="text-xs text-muted-foreground hover:text-foreground"
                    onClick={() => { setPlacementId(''); setPlacementSearch(''); }}>
                    {t('common:cancel')}
                  </button>
                )}
              </div>

              <div className="space-y-2 md:col-span-2">
                <Label>{t('create.notes')}</Label>
                <textarea
                  className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  value={notes}
                  onChange={e => setNotes(e.target.value)}
                  placeholder={t('create.notesPlaceholder')}
                />
              </div>
            </CardContent>
          </Card>

          {/* Document Requirements Preview */}
          {requirements.length > 0 && (
            <Card className="md:col-span-2">
              <CardHeader>
                <CardTitle className="text-base">{t('create.requiredDocuments')}</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="mb-3 text-sm text-muted-foreground">{t('create.documentsNote')}</p>
                <div className="space-y-2">
                  {requirements.map(req => {
                    const docLabel = VISA_DOCUMENT_TYPES.find(dt => dt.value === req.type)?.label ?? req.type;
                    return (
                      <div key={req.type} className="flex items-center gap-3 rounded-md border p-3">
                        <Circle className="h-5 w-5 text-muted-foreground" />
                        <span className="flex-1 text-sm">{docLabel}</span>
                        <Badge variant={req.mandatory ? 'default' : 'outline'}>
                          {req.mandatory ? t('detail.mandatory') : t('detail.optional')}
                        </Badge>
                      </div>
                    );
                  })}
                </div>
              </CardContent>
            </Card>
          )}
        </div>

        <div className="mt-6 flex justify-end gap-2">
          <Button type="button" variant="outline" onClick={() => navigate('/visa-applications')}>
            {t('common:cancel')}
          </Button>
          <Button type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? t('common:saving') : t('create.submit')}
          </Button>
        </div>
      </form>
    </div>
  );
}

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
                  value={workerId}
                  onChange={e => setWorkerId(e.target.value)}
                  placeholder={t('create.workerIdPlaceholder')}
                />
              </div>

              <div className="space-y-2">
                <Label>{t('create.clientId')} *</Label>
                <Input
                  value={clientId}
                  onChange={e => setClientId(e.target.value)}
                  placeholder={t('create.clientIdPlaceholder')}
                />
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
                  value={placementId}
                  onChange={e => setPlacementId(e.target.value)}
                  placeholder={t('create.placementIdPlaceholder')}
                />
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

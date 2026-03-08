import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import { useCreateReturneeCase } from '../hooks';
import { RETURN_TYPES } from '../constants';

export function CreateReturneeCasePage() {
  const { t } = useTranslation('returnees');
  const navigate = useNavigate();
  const createMutation = useCreateReturneeCase();

  const [workerId, setWorkerId] = useState('');
  const [contractId, setContractId] = useState('');
  const [clientId, setClientId] = useState('');
  const [supplierId, setSupplierId] = useState('');
  const [returnType, setReturnType] = useState('');
  const [returnDate, setReturnDate] = useState('');
  const [returnReason, setReturnReason] = useState('');
  const [notes, setNotes] = useState('');

  const isValid = workerId && contractId && clientId && returnType && returnDate && returnReason;

  const handleSubmit = () => {
    if (!isValid) return;
    createMutation.mutate(
      {
        workerId,
        contractId,
        clientId,
        supplierId: supplierId || undefined,
        returnType,
        returnDate,
        returnReason,
        notes: notes || undefined,
      },
      {
        onSuccess: (data) => {
          navigate(`/returnees/${data.id}`);
        },
      }
    );
  };

  return (
    <div className="space-y-6 p-6">
      <div>
        <Link
          to="/returnees"
          className="mb-2 inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" />
          {t('back_to_list')}
        </Link>
        <h1 className="text-2xl font-semibold">{t('create_case')}</h1>
        <p className="text-sm text-muted-foreground">{t('create_description')}</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        {/* Party Information */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t('party_info')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>{t('worker_id')} *</Label>
              <Input
                value={workerId}
                onChange={(e) => setWorkerId(e.target.value)}
                placeholder={t('worker_id_placeholder')}
              />
              <p className="text-xs text-muted-foreground">{t('worker_id_help')}</p>
            </div>
            <div className="space-y-2">
              <Label>{t('contract_id')} *</Label>
              <Input
                value={contractId}
                onChange={(e) => setContractId(e.target.value)}
                placeholder={t('contract_id_placeholder')}
              />
              <p className="text-xs text-muted-foreground">{t('contract_id_help')}</p>
            </div>
            <div className="space-y-2">
              <Label>{t('client_id')} *</Label>
              <Input
                value={clientId}
                onChange={(e) => setClientId(e.target.value)}
                placeholder={t('client_id_placeholder')}
              />
            </div>
            <div className="space-y-2">
              <Label>{t('supplier_id')}</Label>
              <Input
                value={supplierId}
                onChange={(e) => setSupplierId(e.target.value)}
                placeholder={t('supplier_id_placeholder')}
              />
            </div>
          </CardContent>
        </Card>

        {/* Return Details */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t('return_details')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>{t('return_type')} *</Label>
              <Select value={returnType} onValueChange={setReturnType}>
                <SelectTrigger>
                  <SelectValue placeholder={t('select_return_type')} />
                </SelectTrigger>
                <SelectContent>
                  {RETURN_TYPES.map((rt) => (
                    <SelectItem key={rt.value} value={rt.value}>{rt.label}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {returnType === 'ReturnToOffice' && (
                <p className="text-xs text-green-600">{t('return_office_hint')}</p>
              )}
              {returnType === 'ReturnToCountry' && (
                <p className="text-xs text-orange-600">{t('return_country_hint')}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label>{t('return_date')} *</Label>
              <Input
                type="date"
                value={returnDate}
                onChange={(e) => setReturnDate(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>{t('return_reason')} *</Label>
              <textarea
                className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={returnReason}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setReturnReason(e.target.value)}
                rows={3}
                placeholder={t('return_reason_placeholder')}
              />
            </div>
            <div className="space-y-2">
              <Label>{t('notes')}</Label>
              <textarea
                className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={notes}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setNotes(e.target.value)}
                rows={2}
                placeholder={t('notes_placeholder')}
              />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Actions */}
      <div className="flex items-center gap-3">
        <Button variant="outline" onClick={() => navigate('/returnees')}>
          {t('common:cancel')}
        </Button>
        <Button onClick={handleSubmit} disabled={!isValid || createMutation.isPending}>
          {createMutation.isPending ? t('creating') : t('submit_case')}
        </Button>
      </div>
    </div>
  );
}

import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { useReportRunawayCase } from '../hooks';

export function ReportRunawayCasePage() {
  const { t } = useTranslation('runaways');
  const navigate = useNavigate();
  const reportMutation = useReportRunawayCase();

  const [workerId, setWorkerId] = useState('');
  const [contractId, setContractId] = useState('');
  const [clientId, setClientId] = useState('');
  const [supplierId, setSupplierId] = useState('');
  const [reportedDate, setReportedDate] = useState('');
  const [reportedBy, setReportedBy] = useState('');
  const [lastKnownLocation, setLastKnownLocation] = useState('');
  const [policeReportNumber, setPoliceReportNumber] = useState('');
  const [policeReportDate, setPoliceReportDate] = useState('');
  const [notes, setNotes] = useState('');

  const isValid = workerId && contractId && clientId && reportedDate && reportedBy;

  const handleSubmit = () => {
    if (!isValid) return;
    reportMutation.mutate(
      {
        workerId,
        contractId,
        clientId,
        supplierId: supplierId || undefined,
        reportedDate: new Date(reportedDate).toISOString(),
        reportedBy,
        lastKnownLocation: lastKnownLocation || undefined,
        policeReportNumber: policeReportNumber || undefined,
        policeReportDate: policeReportDate ? new Date(policeReportDate).toISOString() : undefined,
        notes: notes || undefined,
      },
      {
        onSuccess: (data) => {
          navigate(`/runaways/${data.id}`);
        },
      }
    );
  };

  return (
    <div className="space-y-6 p-6">
      <div>
        <Link
          to="/runaways"
          className="mb-2 inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" />
          {t('back_to_list')}
        </Link>
        <h1 className="text-2xl font-semibold">{t('report_case')}</h1>
        <p className="text-sm text-muted-foreground">{t('report_description')}</p>
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

        {/* Case Details */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">{t('case_details')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>{t('reported_date')} *</Label>
              <Input
                type="date"
                value={reportedDate}
                onChange={(e) => setReportedDate(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>{t('reported_by')} *</Label>
              <Input
                value={reportedBy}
                onChange={(e) => setReportedBy(e.target.value)}
                placeholder={t('reported_by_placeholder')}
              />
            </div>
            <div className="space-y-2">
              <Label>{t('last_known_location')}</Label>
              <Input
                value={lastKnownLocation}
                onChange={(e) => setLastKnownLocation(e.target.value)}
                placeholder={t('location_placeholder')}
              />
            </div>
            <div className="space-y-2">
              <Label>{t('police_report_number')}</Label>
              <Input
                value={policeReportNumber}
                onChange={(e) => setPoliceReportNumber(e.target.value)}
                placeholder={t('police_number_placeholder')}
              />
            </div>
            <div className="space-y-2">
              <Label>{t('police_report_date')}</Label>
              <Input
                type="date"
                value={policeReportDate}
                onChange={(e) => setPoliceReportDate(e.target.value)}
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
        <Button variant="outline" onClick={() => navigate('/runaways')}>
          {t('common:cancel')}
        </Button>
        <Button onClick={handleSubmit} disabled={!isValid || reportMutation.isPending}>
          {reportMutation.isPending ? t('reporting') : t('submit_report')}
        </Button>
      </div>
    </div>
  );
}

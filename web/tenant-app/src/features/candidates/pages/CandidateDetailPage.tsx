import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, RefreshCw, Trash2 } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';
import { useCountryRefs, getFlagEmoji } from '@/features/reference-data';
import { useCandidate, useDeleteCandidate } from '../hooks';
import { StatusBadge } from '../components/StatusBadge';
import { StatusTransitionDialog } from '../components/StatusTransitionDialog';
import { StatusTimeline } from '../components/StatusTimeline';

function InfoItem({ label, value }: { label: string; value?: string | null }) {
  return (
    <div>
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="font-medium">{value || '—'}</p>
    </div>
  );
}

export function CandidateDetailPage() {
  const { t } = useTranslation('candidates');
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: candidate, isLoading } = useCandidate(id!);
  const { data: countries } = useCountryRefs();
  const deleteMutation = useDeleteCandidate();

  const [showTransition, setShowTransition] = useState(false);
  const [showDelete, setShowDelete] = useState(false);

  const getCountryDisplay = (code?: string) => {
    if (!code) return undefined;
    const country = countries?.find((c) => c.code === code || c.nameEn === code);
    if (country) return `${getFlagEmoji(country.code)} ${country.nameEn}`;
    return code;
  };

  const handleDelete = async () => {
    if (!id) return;
    await deleteMutation.mutateAsync(id);
    navigate('/candidates');
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[40vh]">
        <p className="text-muted-foreground">{t('common:loading')}</p>
      </div>
    );
  }

  if (!candidate) {
    return (
      <div className="flex items-center justify-center min-h-[40vh]">
        <p className="text-muted-foreground">{t('common:noResults')}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <Link
          to="/candidates"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-4"
        >
          <ArrowLeft className="h-4 w-4" />
          {t('detail.backToList')}
        </Link>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold tracking-tight">{candidate.fullNameEn}</h1>
            <StatusBadge status={candidate.status} />
          </div>
          <div className="flex items-center gap-2">
            <Button variant="outline" onClick={() => setShowTransition(true)}>
              <RefreshCw className="me-2 h-4 w-4" />
              {t('actions.changeStatus')}
            </Button>
            <Button
              variant="destructive"
              onClick={() => setShowDelete(true)}
            >
              <Trash2 className="me-2 h-4 w-4" />
              {t('actions.delete')}
            </Button>
          </div>
        </div>
      </div>

      {/* Info Cards */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* Personal */}
        <Card>
          <CardHeader>
            <CardTitle>{t('detail.personal')}</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <InfoItem label={t('detail.fullNameEn')} value={candidate.fullNameEn} />
            <InfoItem label={t('detail.fullNameAr')} value={candidate.fullNameAr} />
            <InfoItem label={t('detail.nationality')} value={getCountryDisplay(candidate.nationality)} />
            <InfoItem label={t('detail.dateOfBirth')} value={candidate.dateOfBirth} />
            <InfoItem label={t('detail.gender')} value={candidate.gender} />
            <InfoItem label={t('detail.passportNumber')} value={candidate.passportNumber} />
            <InfoItem label={t('detail.passportExpiry')} value={candidate.passportExpiry} />
          </CardContent>
        </Card>

        {/* Contact */}
        <Card>
          <CardHeader>
            <CardTitle>{t('detail.contact')}</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <InfoItem label={t('detail.phone')} value={candidate.phone} />
            <InfoItem label={t('detail.email')} value={candidate.email} />
          </CardContent>
        </Card>

        {/* Sourcing */}
        <Card>
          <CardHeader>
            <CardTitle>{t('detail.sourcing')}</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <InfoItem label={t('detail.sourceType')} value={t(`sourceType.${candidate.sourceType}`)} />
            <InfoItem label={t('detail.supplier')} value={candidate.tenantSupplierName} />
            <InfoItem label={t('detail.externalReference')} value={candidate.externalReference} />
          </CardContent>
        </Card>

        {/* Documents */}
        <Card>
          <CardHeader>
            <CardTitle>{t('detail.documents')}</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <InfoItem label={t('detail.medicalStatus')} value={candidate.medicalStatus} />
            <InfoItem label={t('detail.visaStatus')} value={candidate.visaStatus} />
          </CardContent>
        </Card>

        {/* Operational */}
        <Card>
          <CardHeader>
            <CardTitle>{t('detail.operational')}</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <InfoItem label={t('detail.expectedArrivalDate')} value={candidate.expectedArrivalDate} />
            <InfoItem label={t('detail.actualArrivalDate')} value={candidate.actualArrivalDate} />
            <InfoItem label={t('detail.createdAt')} value={new Date(candidate.createdAt).toLocaleString()} />
            <InfoItem label={t('detail.updatedAt')} value={new Date(candidate.updatedAt).toLocaleString()} />
            <div className="sm:col-span-2">
              <InfoItem label={t('detail.notes')} value={candidate.notes} />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Status Timeline */}
      {candidate.statusHistory && candidate.statusHistory.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>{t('detail.statusHistory')}</CardTitle>
          </CardHeader>
          <CardContent>
            <StatusTimeline history={candidate.statusHistory} />
          </CardContent>
        </Card>
      )}

      {/* Status Transition Dialog */}
      <StatusTransitionDialog
        open={showTransition}
        onOpenChange={setShowTransition}
        candidateId={candidate.id}
        currentStatus={candidate.status}
      />

      {/* Delete Confirmation */}
      <AlertDialog open={showDelete} onOpenChange={setShowDelete}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('deleteDialog.title')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('deleteDialog.description')}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {t('deleteDialog.confirm')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

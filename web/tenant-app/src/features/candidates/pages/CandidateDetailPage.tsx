import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, RefreshCw, Trash2, Pencil, ImageOff, FileText, VideoOff } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Badge } from '@/shared/components/ui/badge';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/components/ui/tabs';
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
import { ALLOWED_TRANSITIONS } from '../constants';

function InfoItem({ label, value }: { label: string; value?: string | number | null }) {
  return (
    <div>
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="font-medium">{value != null && value !== '' ? String(value) : '—'}</p>
    </div>
  );
}

function DetailSkeleton() {
  return (
    <div className="space-y-6">
      <div>
        <Skeleton className="h-4 w-32 mb-4" />
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Skeleton className="h-8 w-64" />
            <Skeleton className="h-6 w-24" />
          </div>
          <div className="flex items-center gap-2">
            <Skeleton className="h-9 w-32" />
            <Skeleton className="h-9 w-32" />
          </div>
        </div>
      </div>
      <Skeleton className="h-10 w-96" />
      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-40" />
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i}>
                <Skeleton className="h-4 w-24 mb-1" />
                <Skeleton className="h-5 w-40" />
              </div>
            ))}
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-36" />
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            {Array.from({ length: 2 }).map((_, i) => (
              <div key={i}>
                <Skeleton className="h-4 w-24 mb-1" />
                <Skeleton className="h-5 w-40" />
              </div>
            ))}
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-36" />
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            {Array.from({ length: 3 }).map((_, i) => (
              <div key={i}>
                <Skeleton className="h-4 w-24 mb-1" />
                <Skeleton className="h-5 w-40" />
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
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
    return <DetailSkeleton />;
  }

  if (!candidate) {
    return (
      <div className="flex items-center justify-center min-h-[40vh]">
        <p className="text-muted-foreground">{t('common:noResults')}</p>
      </div>
    );
  }

  const isEditable = candidate.status === 'Received' || candidate.status === 'UnderReview';
  const hasTransitions = (ALLOWED_TRANSITIONS[candidate.status]?.length ?? 0) > 0;

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
            {candidate.photoUrl && (
              <img
                src={candidate.photoUrl}
                alt={candidate.fullNameEn}
                className="h-12 w-12 rounded-full object-cover border"
              />
            )}
            <h1 className="text-2xl font-bold tracking-tight">{candidate.fullNameEn}</h1>
            <StatusBadge status={candidate.status} />
          </div>
          <div className="flex items-center gap-2">
            {isEditable && (
              <Button variant="outline" onClick={() => navigate(`/candidates/${id}/edit`)}>
                <Pencil className="me-2 h-4 w-4" />
                {t('actions.edit')}
              </Button>
            )}
            {hasTransitions && (
              <Button variant="outline" onClick={() => setShowTransition(true)}>
                <RefreshCw className="me-2 h-4 w-4" />
                {t('actions.changeStatus')}
              </Button>
            )}
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

      {/* Tabs */}
      <Tabs defaultValue="overview" className="space-y-6">
        <TabsList>
          <TabsTrigger value="overview">{t('tabs.overview')}</TabsTrigger>
          <TabsTrigger value="professional">{t('tabs.professional')}</TabsTrigger>
          <TabsTrigger value="documentsOperations">{t('tabs.documentsOperations')}</TabsTrigger>
          <TabsTrigger value="statusHistory">
            {t('tabs.statusHistory')}
            {candidate.statusHistory && (
              <Badge variant="secondary" className="ms-2">
                {candidate.statusHistory.length}
              </Badge>
            )}
          </TabsTrigger>
        </TabsList>

        {/* Overview Tab */}
        <TabsContent value="overview" className="space-y-6">
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
                <InfoItem label={t('detail.supplier')} value={candidate.supplier?.name} />
                <InfoItem label={t('detail.externalReference')} value={candidate.externalReference} />
              </CardContent>
            </Card>

            {/* Media & Documents */}
            <Card>
              <CardHeader>
                <CardTitle>{t('detail.media')}</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid gap-6 sm:grid-cols-3">
                  {/* Photo */}
                  <div>
                    <p className="text-sm text-muted-foreground mb-2">{t('detail.photo')}</p>
                    {candidate.photoUrl ? (
                      <img
                        src={candidate.photoUrl}
                        alt={candidate.fullNameEn}
                        className="h-40 w-40 rounded-lg object-cover border"
                      />
                    ) : (
                      <div className="h-40 w-40 rounded-lg border border-dashed flex items-center justify-center bg-muted/30">
                        <ImageOff className="h-8 w-8 text-muted-foreground/50" />
                      </div>
                    )}
                  </div>

                  {/* Video */}
                  <div>
                    <p className="text-sm text-muted-foreground mb-2">{t('detail.video')}</p>
                    {candidate.videoUrl ? (
                      <video
                        src={candidate.videoUrl}
                        controls
                        className="h-40 max-w-full rounded-lg border"
                      />
                    ) : (
                      <div className="h-40 w-40 rounded-lg border border-dashed flex items-center justify-center bg-muted/30">
                        <VideoOff className="h-8 w-8 text-muted-foreground/50" />
                      </div>
                    )}
                  </div>

                  {/* Passport Document */}
                  <div>
                    <p className="text-sm text-muted-foreground mb-2">{t('detail.passportDocument')}</p>
                    {candidate.passportDocumentUrl ? (
                      <a
                        href={candidate.passportDocumentUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="h-40 w-40 rounded-lg border flex flex-col items-center justify-center gap-2 hover:bg-muted/50 transition-colors"
                      >
                        <FileText className="h-10 w-10 text-primary" />
                        <span className="text-sm text-primary font-medium">{t('detail.viewPassportDocument')}</span>
                      </a>
                    ) : (
                      <div className="h-40 w-40 rounded-lg border border-dashed flex items-center justify-center bg-muted/30">
                        <FileText className="h-8 w-8 text-muted-foreground/50" />
                      </div>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Professional Profile Tab */}
        <TabsContent value="professional" className="space-y-6">
          <div className="grid gap-6 md:grid-cols-2">
            {/* Profile Details */}
            <Card>
              <CardHeader>
                <CardTitle>{t('detail.professionalProfile')}</CardTitle>
              </CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <InfoItem label={t('detail.religion')} value={candidate.religion ? t(`professional.religion.${candidate.religion}`, candidate.religion) : undefined} />
                <InfoItem label={t('detail.maritalStatus')} value={candidate.maritalStatus ? t(`professional.maritalStatus.${candidate.maritalStatus}`, candidate.maritalStatus) : undefined} />
                <InfoItem label={t('detail.educationLevel')} value={candidate.educationLevel ? t(`professional.educationLevel.${candidate.educationLevel}`, candidate.educationLevel) : undefined} />
                <InfoItem label={t('detail.jobCategory')} value={candidate.jobCategoryName} />
                <InfoItem label={t('detail.experienceYears')} value={candidate.experienceYears} />
                <InfoItem label={t('detail.monthlySalary')} value={candidate.monthlySalary ? `${candidate.monthlySalary} AED` : undefined} />
              </CardContent>
            </Card>

            {/* Skills */}
            <Card>
              <CardHeader>
                <CardTitle>{t('detail.skills')}</CardTitle>
              </CardHeader>
              <CardContent>
                {candidate.skills && candidate.skills.length > 0 ? (
                  <div className="flex flex-wrap gap-2">
                    {candidate.skills.map((skill) => (
                      <Badge key={skill.id} variant="outline">
                        {skill.skillName} — {t(`skills.proficiency.${skill.proficiencyLevel}`, skill.proficiencyLevel)}
                      </Badge>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">—</p>
                )}
              </CardContent>
            </Card>

            {/* Languages */}
            <Card>
              <CardHeader>
                <CardTitle>{t('detail.languages')}</CardTitle>
              </CardHeader>
              <CardContent>
                {candidate.languages && candidate.languages.length > 0 ? (
                  <div className="flex flex-wrap gap-2">
                    {candidate.languages.map((lang) => (
                      <Badge key={lang.id} variant="outline">
                        {lang.language} — {t(`languages.proficiency.${lang.proficiencyLevel}`, lang.proficiencyLevel)}
                      </Badge>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">—</p>
                )}
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Documents & Operations Tab */}
        <TabsContent value="documentsOperations" className="space-y-6">
          <div className="grid gap-6 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>{t('detail.operational')}</CardTitle>
              </CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <InfoItem label={t('detail.createdAt')} value={new Date(candidate.createdAt).toLocaleString()} />
                <InfoItem label={t('detail.updatedAt')} value={new Date(candidate.updatedAt).toLocaleString()} />
                <div className="sm:col-span-2">
                  <InfoItem label={t('detail.notes')} value={candidate.notes} />
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Status History Tab */}
        <TabsContent value="statusHistory" className="space-y-6">
          {candidate.statusHistory && candidate.statusHistory.length > 0 ? (
            <Card>
              <CardHeader>
                <CardTitle>{t('detail.statusHistory')}</CardTitle>
              </CardHeader>
              <CardContent>
                <StatusTimeline history={candidate.statusHistory} />
              </CardContent>
            </Card>
          ) : (
            <div className="flex items-center justify-center min-h-[20vh]">
              <p className="text-muted-foreground">{t('detail.notAvailable')}</p>
            </div>
          )}
        </TabsContent>
      </Tabs>

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

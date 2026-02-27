import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, RefreshCw, Trash2, FileText, ImageOff, VideoOff, Pencil } from 'lucide-react';
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
import { useWorker, useDeleteWorker } from '../hooks';
import { WorkerStatusBadge } from '../components/WorkerStatusBadge';
import { WorkerStatusTransitionDialog } from '../components/WorkerStatusTransitionDialog';
import { WorkerEditDialog } from '../components/WorkerEditDialog';
import { ALLOWED_TRANSITIONS, LOCATION_CONFIG } from '../constants';

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
        {Array.from({ length: 4 }).map((_, i) => (
          <Card key={i}>
            <CardHeader><Skeleton className="h-6 w-40" /></CardHeader>
            <CardContent className="grid gap-4 sm:grid-cols-2">
              {Array.from({ length: 4 }).map((_, j) => (
                <div key={j}>
                  <Skeleton className="h-4 w-24 mb-1" />
                  <Skeleton className="h-5 w-40" />
                </div>
              ))}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}

export function WorkerDetailPage() {
  const { t } = useTranslation('workers');
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: worker, isLoading } = useWorker(id!);
  const { data: countries } = useCountryRefs();
  const deleteMutation = useDeleteWorker();

  const [showTransition, setShowTransition] = useState(false);
  const [showDelete, setShowDelete] = useState(false);
  const [showEdit, setShowEdit] = useState(false);

  const getCountryDisplay = (code?: string) => {
    if (!code) return undefined;
    const country = countries?.find((c) => c.code === code || c.nameEn === code);
    if (country) return `${getFlagEmoji(country.code)} ${country.nameEn}`;
    return code;
  };

  const handleDelete = async () => {
    if (!id) return;
    await deleteMutation.mutateAsync(id);
    navigate('/workers');
  };

  if (isLoading) return <DetailSkeleton />;

  if (!worker) {
    return (
      <div className="flex items-center justify-center min-h-[40vh]">
        <p className="text-muted-foreground">{t('common:noResults')}</p>
      </div>
    );
  }

  const hasTransitions = (ALLOWED_TRANSITIONS[worker.status]?.length ?? 0) > 0;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <Link
          to="/workers"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-4"
        >
          <ArrowLeft className="h-4 w-4" />
          {t('detail.backToList')}
        </Link>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            {worker.photoUrl && (
              <img
                src={worker.photoUrl}
                alt={worker.fullNameEn}
                className="h-12 w-12 rounded-full object-cover border"
              />
            )}
            <div>
              <h1 className="text-2xl font-bold tracking-tight">{worker.fullNameEn}</h1>
              <p className="text-sm text-muted-foreground font-mono">{worker.workerCode}</p>
            </div>
            <WorkerStatusBadge status={worker.status} />
            {(() => {
              const locConfig = LOCATION_CONFIG[worker.location];
              const LocIcon = locConfig?.icon;
              return (
                <Badge variant={locConfig?.variant ?? 'outline'} className="gap-1">
                  {LocIcon && <LocIcon className="h-3 w-3" />}
                  {t(`location.${worker.location}`)}
                </Badge>
              );
            })()}
          </div>
          <div className="flex items-center gap-2">
            <Button variant="outline" onClick={() => setShowEdit(true)}>
              <Pencil className="me-2 h-4 w-4" />
              {t('actions.edit')}
            </Button>
            <Button variant="outline" onClick={() => navigate(`/workers/${id}/cv`)}>
              <FileText className="me-2 h-4 w-4" />
              {t('actions.viewCv')}
            </Button>
            {hasTransitions && (
              <Button variant="outline" onClick={() => setShowTransition(true)}>
                <RefreshCw className="me-2 h-4 w-4" />
                {t('actions.changeStatus')}
              </Button>
            )}
            <Button variant="destructive" onClick={() => setShowDelete(true)}>
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
          <TabsTrigger value="documents">{t('tabs.documents')}</TabsTrigger>
          <TabsTrigger value="statusHistory">
            {t('tabs.statusHistory')}
            {worker.statusHistory && (
              <Badge variant="secondary" className="ms-2">
                {worker.statusHistory.length}
              </Badge>
            )}
          </TabsTrigger>
        </TabsList>

        {/* Overview Tab */}
        <TabsContent value="overview" className="space-y-6">
          <div className="grid gap-6 md:grid-cols-2">
            <Card>
              <CardHeader><CardTitle>{t('detail.personal')}</CardTitle></CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <InfoItem label={t('detail.fullNameEn')} value={worker.fullNameEn} />
                <InfoItem label={t('detail.fullNameAr')} value={worker.fullNameAr} />
                <InfoItem label={t('detail.workerCode')} value={worker.workerCode} />
                <InfoItem label={t('detail.nationality')} value={getCountryDisplay(worker.nationality)} />
                <InfoItem label={t('detail.dateOfBirth')} value={worker.dateOfBirth} />
                <InfoItem label={t('detail.gender')} value={worker.gender} />
                <InfoItem label={t('detail.passportNumber')} value={worker.passportNumber} />
                <InfoItem label={t('detail.passportExpiry')} value={worker.passportExpiry} />
              </CardContent>
            </Card>

            <Card>
              <CardHeader><CardTitle>{t('detail.contact')}</CardTitle></CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <InfoItem label={t('detail.phone')} value={worker.phone} />
                <InfoItem label={t('detail.email')} value={worker.email} />
              </CardContent>
            </Card>

            <Card>
              <CardHeader><CardTitle>{t('detail.sourcing')}</CardTitle></CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <InfoItem label={t('detail.sourceType')} value={t(`sourceType.${worker.sourceType}`)} />
                <InfoItem label={t('detail.supplier')} value={worker.supplier?.name} />
                <InfoItem label={t('detail.activatedAt')} value={worker.activatedAt ? new Date(worker.activatedAt).toLocaleString() : undefined} />
                {worker.terminatedAt && (
                  <>
                    <InfoItem label={t('detail.terminatedAt')} value={new Date(worker.terminatedAt).toLocaleString()} />
                    <InfoItem label={t('detail.terminationReason')} value={worker.terminationReason} />
                  </>
                )}
              </CardContent>
            </Card>

            {(worker.procurementPaidAt || worker.flightDate || worker.arrivedAt) && (
              <Card>
                <CardHeader><CardTitle>{t('detail.travel')}</CardTitle></CardHeader>
                <CardContent className="grid gap-4 sm:grid-cols-2">
                  {worker.procurementPaidAt && (
                    <InfoItem label={t('detail.procurementPaidAt')} value={new Date(worker.procurementPaidAt).toLocaleDateString()} />
                  )}
                  {worker.flightDate && (
                    <InfoItem label={t('detail.flightDate')} value={new Date(worker.flightDate).toLocaleDateString()} />
                  )}
                  {worker.arrivedAt && (
                    <InfoItem label={t('detail.arrivedAt')} value={new Date(worker.arrivedAt).toLocaleDateString()} />
                  )}
                </CardContent>
              </Card>
            )}

            <Card>
              <CardHeader><CardTitle>{t('detail.media')}</CardTitle></CardHeader>
              <CardContent>
                <div className="grid gap-6 sm:grid-cols-3">
                  <div>
                    <p className="text-sm text-muted-foreground mb-2">{t('detail.photo')}</p>
                    {worker.photoUrl ? (
                      <img src={worker.photoUrl} alt={worker.fullNameEn} className="h-40 w-40 rounded-lg object-cover border" />
                    ) : (
                      <div className="h-40 w-40 rounded-lg border border-dashed flex items-center justify-center bg-muted/30">
                        <ImageOff className="h-8 w-8 text-muted-foreground/50" />
                      </div>
                    )}
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground mb-2">{t('detail.video')}</p>
                    {worker.videoUrl ? (
                      <video src={worker.videoUrl} controls className="h-40 max-w-full rounded-lg border" />
                    ) : (
                      <div className="h-40 w-40 rounded-lg border border-dashed flex items-center justify-center bg-muted/30">
                        <VideoOff className="h-8 w-8 text-muted-foreground/50" />
                      </div>
                    )}
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground mb-2">{t('detail.passportDocument')}</p>
                    {worker.passportDocumentUrl ? (
                      <a href={worker.passportDocumentUrl} target="_blank" rel="noopener noreferrer"
                        className="h-40 w-40 rounded-lg border flex flex-col items-center justify-center gap-2 hover:bg-muted/50 transition-colors">
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

        {/* Professional Tab */}
        <TabsContent value="professional" className="space-y-6">
          <div className="grid gap-6 md:grid-cols-2">
            <Card>
              <CardHeader><CardTitle>{t('detail.professionalProfile')}</CardTitle></CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <InfoItem label={t('detail.religion')} value={worker.religion ? t(`professional.religion.${worker.religion}`, worker.religion) : undefined} />
                <InfoItem label={t('detail.maritalStatus')} value={worker.maritalStatus ? t(`professional.maritalStatus.${worker.maritalStatus}`, worker.maritalStatus) : undefined} />
                <InfoItem label={t('detail.educationLevel')} value={worker.educationLevel ? t(`professional.educationLevel.${worker.educationLevel}`, worker.educationLevel) : undefined} />
                <InfoItem label={t('detail.jobCategory')} value={worker.jobCategory?.name} />
                <InfoItem label={t('detail.experienceYears')} value={worker.experienceYears} />
                <InfoItem label={t('detail.monthlySalary')} value={worker.monthlySalary ? `${worker.monthlySalary} AED` : undefined} />
              </CardContent>
            </Card>

            <Card>
              <CardHeader><CardTitle>{t('detail.skills')}</CardTitle></CardHeader>
              <CardContent>
                {worker.skills && worker.skills.length > 0 ? (
                  <div className="flex flex-wrap gap-2">
                    {worker.skills.map((skill) => (
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

            <Card>
              <CardHeader><CardTitle>{t('detail.languages')}</CardTitle></CardHeader>
              <CardContent>
                {worker.languages && worker.languages.length > 0 ? (
                  <div className="flex flex-wrap gap-2">
                    {worker.languages.map((lang) => (
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

            <Card>
              <CardHeader><CardTitle>{t('detail.operational')}</CardTitle></CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <InfoItem label={t('detail.createdAt')} value={new Date(worker.createdAt).toLocaleString()} />
                <InfoItem label={t('detail.updatedAt')} value={new Date(worker.updatedAt).toLocaleString()} />
                <div className="sm:col-span-2">
                  <InfoItem label={t('detail.notes')} value={worker.notes} />
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Documents Tab */}
        <TabsContent value="documents" className="space-y-6">
          <Card>
            <CardHeader><CardTitle>{t('detail.media')}</CardTitle></CardHeader>
            <CardContent>
              <div className="grid gap-6 sm:grid-cols-3">
                <div>
                  <p className="text-sm text-muted-foreground mb-2">{t('detail.photo')}</p>
                  {worker.photoUrl ? (
                    <img src={worker.photoUrl} alt={worker.fullNameEn} className="h-48 w-48 rounded-lg object-cover border" />
                  ) : (
                    <div className="h-48 w-48 rounded-lg border border-dashed flex items-center justify-center bg-muted/30">
                      <ImageOff className="h-8 w-8 text-muted-foreground/50" />
                    </div>
                  )}
                </div>
                <div>
                  <p className="text-sm text-muted-foreground mb-2">{t('detail.video')}</p>
                  {worker.videoUrl ? (
                    <video src={worker.videoUrl} controls className="h-48 max-w-full rounded-lg border" />
                  ) : (
                    <div className="h-48 w-48 rounded-lg border border-dashed flex items-center justify-center bg-muted/30">
                      <VideoOff className="h-8 w-8 text-muted-foreground/50" />
                    </div>
                  )}
                </div>
                <div>
                  <p className="text-sm text-muted-foreground mb-2">{t('detail.passportDocument')}</p>
                  {worker.passportDocumentUrl ? (
                    <a href={worker.passportDocumentUrl} target="_blank" rel="noopener noreferrer"
                      className="h-48 w-48 rounded-lg border flex flex-col items-center justify-center gap-2 hover:bg-muted/50 transition-colors">
                      <FileText className="h-12 w-12 text-primary" />
                      <span className="text-sm text-primary font-medium">{t('detail.viewPassportDocument')}</span>
                    </a>
                  ) : (
                    <div className="h-48 w-48 rounded-lg border border-dashed flex items-center justify-center bg-muted/30">
                      <FileText className="h-8 w-8 text-muted-foreground/50" />
                    </div>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Status History Tab */}
        <TabsContent value="statusHistory" className="space-y-6">
          {worker.statusHistory && worker.statusHistory.length > 0 ? (
            <Card>
              <CardHeader><CardTitle>{t('detail.statusHistory')}</CardTitle></CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {worker.statusHistory.map((entry) => (
                    <div key={entry.id} className="flex items-start gap-3 border-s-2 border-muted ps-4 pb-4">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-1">
                          {entry.fromStatus && (
                            <>
                              <WorkerStatusBadge status={entry.fromStatus} />
                              <span className="text-muted-foreground">→</span>
                            </>
                          )}
                          <WorkerStatusBadge status={entry.toStatus} />
                        </div>
                        <p className="text-xs text-muted-foreground">
                          {new Date(entry.changedAt).toLocaleString()}
                        </p>
                        {entry.reason && (
                          <p className="text-sm mt-1">{entry.reason}</p>
                        )}
                        {entry.notes && (
                          <p className="text-sm text-muted-foreground mt-1">{entry.notes}</p>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          ) : (
            <div className="flex items-center justify-center min-h-[20vh]">
              <p className="text-muted-foreground">{t('detail.notAvailable')}</p>
            </div>
          )}
        </TabsContent>
      </Tabs>

      {/* Edit Dialog */}
      <WorkerEditDialog
        open={showEdit}
        onOpenChange={setShowEdit}
        worker={worker}
      />

      {/* Status Transition Dialog */}
      <WorkerStatusTransitionDialog
        open={showTransition}
        onOpenChange={setShowTransition}
        workerId={worker.id}
        currentStatus={worker.status}
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

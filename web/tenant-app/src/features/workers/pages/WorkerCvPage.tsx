import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, Printer, ImageOff } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Badge } from '@/shared/components/ui/badge';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { useCountryRefs, getFlagEmoji } from '@/features/reference-data';
import { getWorkerCv } from '../api';

function CvSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-4 w-32 mb-4" />
      <Skeleton className="h-8 w-64" />
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

function InfoItem({ label, value }: { label: string; value?: string | number | null }) {
  return (
    <div>
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="font-medium">{value != null && value !== '' ? String(value) : '—'}</p>
    </div>
  );
}

export function WorkerCvPage() {
  const { t } = useTranslation('workers');
  const { id } = useParams<{ id: string }>();
  const { data: countries } = useCountryRefs();

  const { data: cv, isLoading } = useQuery({
    queryKey: ['workers', id, 'cv'],
    queryFn: () => getWorkerCv(id!),
    enabled: !!id,
  });

  const getCountryDisplay = (code?: string) => {
    if (!code) return undefined;
    const country = countries?.find((c) => c.code === code || c.nameEn === code);
    if (country) return `${getFlagEmoji(country.code)} ${country.nameEn}`;
    return code;
  };

  if (isLoading) return <CvSkeleton />;

  if (!cv) {
    return (
      <div className="flex items-center justify-center min-h-[40vh]">
        <p className="text-muted-foreground">{t('common:noResults')}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between print:hidden">
        <div>
          <Link
            to={`/workers/${id}`}
            className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-4"
          >
            <ArrowLeft className="h-4 w-4" />
            {t('cv.backToDetail')}
          </Link>
          <h1 className="text-2xl font-bold tracking-tight">{t('cv.title')}</h1>
          <p className="text-muted-foreground font-mono">{cv.workerCode}</p>
        </div>
        <Button variant="outline" onClick={() => window.print()}>
          <Printer className="me-2 h-4 w-4" />
          {t('cv.print')}
        </Button>
      </div>

      {/* CV Content */}
      <div className="grid gap-6 md:grid-cols-3 print:grid-cols-3">
        {/* Photo */}
        <div className="md:col-span-1 print:col-span-1 flex justify-center">
          {cv.photoUrl ? (
            <img src={cv.photoUrl} alt={cv.fullNameEn} className="h-48 w-48 rounded-lg object-cover border" />
          ) : (
            <div className="h-48 w-48 rounded-lg border border-dashed flex items-center justify-center bg-muted/30">
              <ImageOff className="h-12 w-12 text-muted-foreground/50" />
            </div>
          )}
        </div>

        {/* Personal Info */}
        <Card className="md:col-span-2 print:col-span-2">
          <CardHeader><CardTitle>{t('cv.personal')}</CardTitle></CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <InfoItem label={t('detail.fullNameEn')} value={cv.fullNameEn} />
            <InfoItem label={t('detail.fullNameAr')} value={cv.fullNameAr} />
            <InfoItem label={t('detail.nationality')} value={getCountryDisplay(cv.nationality)} />
            <InfoItem label={t('detail.dateOfBirth')} value={cv.dateOfBirth} />
            <InfoItem label={t('detail.gender')} value={cv.gender} />
            <InfoItem label={t('detail.passportNumber')} value={cv.passportNumber} />
            <InfoItem label={t('detail.phone')} value={cv.phone} />
            <InfoItem label={t('detail.email')} value={cv.email} />
          </CardContent>
        </Card>
      </div>

      {/* Professional Profile */}
      <Card>
        <CardHeader><CardTitle>{t('cv.professional')}</CardTitle></CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-3">
          <InfoItem label={t('detail.religion')} value={cv.religion ? t(`professional.religion.${cv.religion}`, cv.religion) : undefined} />
          <InfoItem label={t('detail.maritalStatus')} value={cv.maritalStatus ? t(`professional.maritalStatus.${cv.maritalStatus}`, cv.maritalStatus) : undefined} />
          <InfoItem label={t('detail.educationLevel')} value={cv.educationLevel ? t(`professional.educationLevel.${cv.educationLevel}`, cv.educationLevel) : undefined} />
          <InfoItem label={t('detail.jobCategory')} value={cv.jobCategoryName} />
          <InfoItem label={t('detail.experienceYears')} value={cv.experienceYears} />
          <InfoItem label={t('detail.monthlySalary')} value={cv.monthlySalary ? `${cv.monthlySalary} AED` : undefined} />
        </CardContent>
      </Card>

      {/* Skills & Languages */}
      <div className="grid gap-6 md:grid-cols-2 print:grid-cols-2">
        <Card>
          <CardHeader><CardTitle>{t('cv.skills')}</CardTitle></CardHeader>
          <CardContent>
            {cv.skills.length > 0 ? (
              <div className="flex flex-wrap gap-2">
                {cv.skills.map((skill) => (
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
          <CardHeader><CardTitle>{t('cv.languages')}</CardTitle></CardHeader>
          <CardContent>
            {cv.languages.length > 0 ? (
              <div className="flex flex-wrap gap-2">
                {cv.languages.map((lang) => (
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
    </div>
  );
}

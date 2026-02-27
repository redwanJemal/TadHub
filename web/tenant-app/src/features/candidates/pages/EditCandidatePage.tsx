import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { NationalitySelect, JobCategorySelect } from '@/features/reference-data';
import { useSuppliers } from '@/features/suppliers/hooks';
import { useCandidate, useUpdateCandidate, useUploadPhoto, useUploadVideo, useUploadPassport } from '../hooks';
import {
  ALL_SOURCE_TYPES,
  RELIGION_OPTIONS,
  MARITAL_STATUS_OPTIONS,
  EDUCATION_LEVEL_OPTIONS,
} from '../constants';
import { SkillsEditor } from '../components/SkillsEditor';
import { LanguagesEditor } from '../components/LanguagesEditor';
import { FileUpload } from '../components/FileUpload';
import type { CandidateSkillRequest, CandidateLanguageRequest } from '../types';

export function EditCandidatePage() {
  const { t } = useTranslation('candidates');
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: candidate, isLoading } = useCandidate(id!);
  const updateCandidate = useUpdateCandidate();
  const uploadPhoto = useUploadPhoto();
  const uploadVideo = useUploadVideo();
  const uploadPassport = useUploadPassport();

  const [form, setForm] = useState({
    fullNameEn: '',
    fullNameAr: '',
    nationality: '',
    dateOfBirth: '',
    gender: '',
    passportNumber: '',
    passportExpiry: '',
    phone: '',
    email: '',
    sourceType: '',
    tenantSupplierId: '',
    religion: '',
    maritalStatus: '',
    educationLevel: '',
    jobCategoryId: '',
    experienceYears: '',
    monthlySalary: '',
    notes: '',
    externalReference: '',
  });
  const [skills, setSkills] = useState<CandidateSkillRequest[]>([]);
  const [languages, setLanguages] = useState<CandidateLanguageRequest[]>([]);
  const [initialized, setInitialized] = useState(false);

  const isSupplierSource = form.sourceType === 'Supplier';
  const { data: suppliersData } = useSuppliers({ pageSize: 100 });

  // Pre-populate form when candidate loads
  useEffect(() => {
    if (candidate && !initialized) {
      setForm({
        fullNameEn: candidate.fullNameEn || '',
        fullNameAr: candidate.fullNameAr || '',
        nationality: candidate.nationality || '',
        dateOfBirth: candidate.dateOfBirth || '',
        gender: candidate.gender || '',
        passportNumber: candidate.passportNumber || '',
        passportExpiry: candidate.passportExpiry || '',
        phone: candidate.phone || '',
        email: candidate.email || '',
        sourceType: candidate.sourceType || '',
        tenantSupplierId: candidate.tenantSupplierId || '',
        religion: candidate.religion || '',
        maritalStatus: candidate.maritalStatus || '',
        educationLevel: candidate.educationLevel || '',
        jobCategoryId: candidate.jobCategory?.id || candidate.jobCategoryId || '',
        experienceYears: candidate.experienceYears?.toString() || '',
        monthlySalary: candidate.monthlySalary?.toString() || '',
        notes: candidate.notes || '',
        externalReference: candidate.externalReference || '',
      });
      setSkills(
        candidate.skills?.map((s) => ({
          skillName: s.skillName,
          proficiencyLevel: s.proficiencyLevel,
        })) ?? []
      );
      setLanguages(
        candidate.languages?.map((l) => ({
          language: l.language,
          proficiencyLevel: l.proficiencyLevel,
        })) ?? []
      );
      setInitialized(true);
    }
  }, [candidate, initialized]);

  const update = (field: string, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !form.fullNameEn.trim()) return;

    try {
      const validSkills = skills.filter((s) => s.skillName.trim());
      const validLanguages = languages.filter((l) => l.language.trim());

      await updateCandidate.mutateAsync({
        id,
        data: {
          fullNameEn: form.fullNameEn.trim(),
          fullNameAr: form.fullNameAr.trim() || undefined,
          nationality: form.nationality || undefined,
          dateOfBirth: form.dateOfBirth || undefined,
          gender: form.gender || undefined,
          passportNumber: form.passportNumber.trim() || undefined,
          passportExpiry: form.passportExpiry || undefined,
          phone: form.phone.trim() || undefined,
          email: form.email.trim() || undefined,
          sourceType: form.sourceType || undefined,
          tenantSupplierId: isSupplierSource && form.tenantSupplierId ? form.tenantSupplierId : undefined,
          religion: form.religion || undefined,
          maritalStatus: form.maritalStatus || undefined,
          educationLevel: form.educationLevel || undefined,
          jobCategoryId: form.jobCategoryId || undefined,
          experienceYears: form.experienceYears ? parseInt(form.experienceYears) : undefined,
          monthlySalary: form.monthlySalary ? parseFloat(form.monthlySalary) : undefined,
          skills: validSkills,
          languages: validLanguages,
          notes: form.notes.trim() || undefined,
          externalReference: form.externalReference.trim() || undefined,
        },
      });
      navigate(`/candidates/${id}`);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to update candidate';
      toast.error(message);
    }
  };

  if (isLoading || !initialized) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-4 w-32 mb-4" />
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-64 w-full" />
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

  const isEditable = candidate.status === 'Received' || candidate.status === 'UnderReview';
  if (!isEditable) {
    navigate(`/candidates/${id}`);
    return null;
  }

  return (
    <div className="space-y-6">
      <div>
        <Link
          to={`/candidates/${id}`}
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-4"
        >
          <ArrowLeft className="h-4 w-4" />
          {t('edit.backToDetail')}
        </Link>
        <h1 className="text-2xl font-bold tracking-tight">{t('edit.title')}</h1>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Personal Information */}
        <Card>
          <CardHeader>
            <CardTitle>{t('create.sections.personal')}</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="fullNameEn">
                {t('create.fullNameEn')} <span className="text-destructive">*</span>
              </Label>
              <Input
                id="fullNameEn"
                value={form.fullNameEn}
                onChange={(e) => update('fullNameEn', e.target.value)}
                placeholder={t('create.fullNameEnPlaceholder')}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="fullNameAr">{t('create.fullNameAr')}</Label>
              <Input
                id="fullNameAr"
                value={form.fullNameAr}
                onChange={(e) => update('fullNameAr', e.target.value)}
                placeholder={t('create.fullNameArPlaceholder')}
                dir="rtl"
              />
            </div>
            <div className="space-y-2">
              <Label>{t('create.nationality')}</Label>
              <NationalitySelect
                value={form.nationality}
                onChange={(value) => update('nationality', value)}
                placeholder={t('create.nationalityPlaceholder')}
                valueType="name"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="dateOfBirth">{t('create.dateOfBirth')}</Label>
              <Input
                id="dateOfBirth"
                type="date"
                value={form.dateOfBirth}
                onChange={(e) => update('dateOfBirth', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>{t('create.gender')}</Label>
              <Select value={form.gender} onValueChange={(v) => update('gender', v)}>
                <SelectTrigger>
                  <SelectValue placeholder={t('create.genderPlaceholder')} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Male">{t('create.genderOptions.Male')}</SelectItem>
                  <SelectItem value="Female">{t('create.genderOptions.Female')}</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="passportNumber">{t('create.passportNumber')}</Label>
              <Input
                id="passportNumber"
                value={form.passportNumber}
                onChange={(e) => update('passportNumber', e.target.value)}
                placeholder={t('create.passportNumberPlaceholder')}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="passportExpiry">{t('create.passportExpiry')}</Label>
              <Input
                id="passportExpiry"
                type="date"
                value={form.passportExpiry}
                onChange={(e) => update('passportExpiry', e.target.value)}
              />
            </div>
          </CardContent>
        </Card>

        {/* Contact Information */}
        <Card>
          <CardHeader>
            <CardTitle>{t('create.sections.contact')}</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="phone">{t('create.phone')}</Label>
              <Input
                id="phone"
                value={form.phone}
                onChange={(e) => update('phone', e.target.value)}
                placeholder={t('create.phonePlaceholder')}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">{t('create.email')}</Label>
              <Input
                id="email"
                type="email"
                value={form.email}
                onChange={(e) => update('email', e.target.value)}
                placeholder={t('create.emailPlaceholder')}
              />
            </div>
          </CardContent>
        </Card>

        {/* Sourcing */}
        <Card>
          <CardHeader>
            <CardTitle>{t('create.sections.sourcing')}</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label>{t('create.sourceType')}</Label>
              <Select value={form.sourceType} onValueChange={(v) => {
                update('sourceType', v);
                if (v !== 'Supplier') update('tenantSupplierId', '');
              }}>
                <SelectTrigger>
                  <SelectValue placeholder={t('create.sourceTypePlaceholder')} />
                </SelectTrigger>
                <SelectContent>
                  {ALL_SOURCE_TYPES.map((st) => (
                    <SelectItem key={st} value={st}>
                      {t(`sourceType.${st}`)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            {isSupplierSource && (
              <div className="space-y-2">
                <Label>{t('create.supplier')}</Label>
                <Select value={form.tenantSupplierId} onValueChange={(v) => update('tenantSupplierId', v)}>
                  <SelectTrigger>
                    <SelectValue placeholder={t('create.supplierPlaceholder')} />
                  </SelectTrigger>
                  <SelectContent>
                    {suppliersData?.items?.map((ts) => (
                      <SelectItem key={ts.id} value={ts.id}>
                        {ts.supplier?.nameEn ?? ts.id}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Professional Profile */}
        <Card>
          <CardHeader>
            <CardTitle>{t('create.sections.professional')}</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label>{t('create.religion')}</Label>
              <Select value={form.religion} onValueChange={(v) => update('religion', v)}>
                <SelectTrigger>
                  <SelectValue placeholder={t('create.religionPlaceholder')} />
                </SelectTrigger>
                <SelectContent>
                  {RELIGION_OPTIONS.map((r) => (
                    <SelectItem key={r} value={r}>
                      {t(`professional.religion.${r}`, r)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>{t('create.maritalStatus')}</Label>
              <Select value={form.maritalStatus} onValueChange={(v) => update('maritalStatus', v)}>
                <SelectTrigger>
                  <SelectValue placeholder={t('create.maritalStatusPlaceholder')} />
                </SelectTrigger>
                <SelectContent>
                  {MARITAL_STATUS_OPTIONS.map((m) => (
                    <SelectItem key={m} value={m}>
                      {t(`professional.maritalStatus.${m}`, m)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>{t('create.educationLevel')}</Label>
              <Select value={form.educationLevel} onValueChange={(v) => update('educationLevel', v)}>
                <SelectTrigger>
                  <SelectValue placeholder={t('create.educationLevelPlaceholder')} />
                </SelectTrigger>
                <SelectContent>
                  {EDUCATION_LEVEL_OPTIONS.map((e) => (
                    <SelectItem key={e} value={e}>
                      {t(`professional.educationLevel.${e}`, e)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>{t('create.jobCategory')}</Label>
              <JobCategorySelect
                value={form.jobCategoryId}
                onChange={(value) => update('jobCategoryId', value)}
                placeholder={t('create.jobCategoryPlaceholder')}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="experienceYears">{t('create.experienceYears')}</Label>
              <Input
                id="experienceYears"
                type="number"
                min="0"
                max="50"
                value={form.experienceYears}
                onChange={(e) => update('experienceYears', e.target.value)}
                placeholder={t('create.experienceYearsPlaceholder')}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="monthlySalary">{t('create.monthlySalary')}</Label>
              <Input
                id="monthlySalary"
                type="number"
                min="0"
                step="0.01"
                value={form.monthlySalary}
                onChange={(e) => update('monthlySalary', e.target.value)}
                placeholder={t('create.monthlySalaryPlaceholder')}
              />
            </div>
          </CardContent>
        </Card>

        {/* Skills */}
        <Card>
          <CardHeader>
            <CardTitle>{t('create.sections.skills')}</CardTitle>
          </CardHeader>
          <CardContent>
            <SkillsEditor skills={skills} onChange={setSkills} />
          </CardContent>
        </Card>

        {/* Languages */}
        <Card>
          <CardHeader>
            <CardTitle>{t('create.sections.languages')}</CardTitle>
          </CardHeader>
          <CardContent>
            <LanguagesEditor languages={languages} onChange={setLanguages} />
          </CardContent>
        </Card>

        {/* Media Uploads */}
        <Card>
          <CardHeader>
            <CardTitle>{t('edit.sections.media')}</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-6 sm:grid-cols-2">
            <div className="space-y-2">
              <Label>{t('edit.photo')}</Label>
              <FileUpload
                accept="image/jpeg,image/png,image/webp"
                maxSizeMB={5}
                currentUrl={candidate.photoUrl}
                onUpload={async (file) => {
                  await uploadPhoto.mutateAsync({ id: id!, file });
                }}
                isPending={uploadPhoto.isPending}
                type="photo"
              />
            </div>
            <div className="space-y-2">
              <Label>{t('edit.video')}</Label>
              <FileUpload
                accept="video/mp4,video/webm"
                maxSizeMB={50}
                currentUrl={candidate.videoUrl}
                onUpload={async (file) => {
                  await uploadVideo.mutateAsync({ id: id!, file });
                }}
                isPending={uploadVideo.isPending}
                type="video"
              />
            </div>
            <div className="space-y-2">
              <Label>{t('edit.passport')}</Label>
              <FileUpload
                accept="application/pdf,image/jpeg,image/png"
                maxSizeMB={10}
                currentUrl={candidate.passportDocumentUrl}
                onUpload={async (file) => {
                  await uploadPassport.mutateAsync({ id: id!, file });
                }}
                isPending={uploadPassport.isPending}
                type="document"
              />
            </div>
          </CardContent>
        </Card>

        {/* Additional */}
        <Card>
          <CardHeader>
            <CardTitle>{t('create.sections.additional')}</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="externalReference">{t('create.externalReference')}</Label>
              <Input
                id="externalReference"
                value={form.externalReference}
                onChange={(e) => update('externalReference', e.target.value)}
                placeholder={t('create.externalReferencePlaceholder')}
              />
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label htmlFor="notes">{t('create.notes')}</Label>
              <textarea
                id="notes"
                className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                value={form.notes}
                onChange={(e) => update('notes', e.target.value)}
                placeholder={t('create.notesPlaceholder')}
              />
            </div>
          </CardContent>
        </Card>

        {/* Submit */}
        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate(`/candidates/${id}`)}>
            {t('common:cancel')}
          </Button>
          <Button
            type="submit"
            disabled={!form.fullNameEn.trim() || updateCandidate.isPending}
          >
            {updateCandidate.isPending ? t('edit.submitting') : t('edit.submit')}
          </Button>
        </div>
      </form>
    </div>
  );
}

import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
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
import { NationalitySelect } from '@/features/reference-data';
import { useSuppliers } from '@/features/suppliers/hooks';
import { useCreateCandidate } from '../hooks';
import { ALL_SOURCE_TYPES } from '../constants';

const initialForm = {
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
  medicalStatus: '',
  visaStatus: '',
  expectedArrivalDate: '',
  notes: '',
  externalReference: '',
};

export function CreateCandidatePage() {
  const { t } = useTranslation('candidates');
  const navigate = useNavigate();
  const [form, setForm] = useState(initialForm);
  const createCandidate = useCreateCandidate();

  const isSupplierSource = form.sourceType === 'Supplier';
  const { data: suppliersData } = useSuppliers({ pageSize: 100 });

  const update = (field: string, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.fullNameEn.trim() || !form.sourceType) return;

    const result = await createCandidate.mutateAsync({
      fullNameEn: form.fullNameEn.trim(),
      fullNameAr: form.fullNameAr.trim() || undefined,
      nationality: form.nationality || undefined,
      dateOfBirth: form.dateOfBirth || undefined,
      gender: form.gender || undefined,
      passportNumber: form.passportNumber.trim() || undefined,
      passportExpiry: form.passportExpiry || undefined,
      phone: form.phone.trim() || undefined,
      email: form.email.trim() || undefined,
      sourceType: form.sourceType,
      tenantSupplierId: isSupplierSource && form.tenantSupplierId ? form.tenantSupplierId : undefined,
      medicalStatus: form.medicalStatus.trim() || undefined,
      visaStatus: form.visaStatus.trim() || undefined,
      expectedArrivalDate: form.expectedArrivalDate || undefined,
      notes: form.notes.trim() || undefined,
      externalReference: form.externalReference.trim() || undefined,
    });
    navigate(`/candidates/${result.id}`);
  };

  return (
    <div className="space-y-6">
      <div>
        <Link
          to="/candidates"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-4"
        >
          <ArrowLeft className="h-4 w-4" />
          {t('create.backToList')}
        </Link>
        <h1 className="text-2xl font-bold tracking-tight">{t('create.title')}</h1>
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
              <Label>
                {t('create.nationality')}
              </Label>
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
              <Label>
                {t('create.sourceType')} <span className="text-destructive">*</span>
              </Label>
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

        {/* Documents */}
        <Card>
          <CardHeader>
            <CardTitle>{t('create.sections.documents')}</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="medicalStatus">{t('create.medicalStatus')}</Label>
              <Input
                id="medicalStatus"
                value={form.medicalStatus}
                onChange={(e) => update('medicalStatus', e.target.value)}
                placeholder={t('create.medicalStatusPlaceholder')}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="visaStatus">{t('create.visaStatus')}</Label>
              <Input
                id="visaStatus"
                value={form.visaStatus}
                onChange={(e) => update('visaStatus', e.target.value)}
                placeholder={t('create.visaStatusPlaceholder')}
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
              <Label htmlFor="expectedArrivalDate">{t('create.expectedArrivalDate')}</Label>
              <Input
                id="expectedArrivalDate"
                type="date"
                value={form.expectedArrivalDate}
                onChange={(e) => update('expectedArrivalDate', e.target.value)}
              />
            </div>
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
          <Button type="button" variant="outline" onClick={() => navigate('/candidates')}>
            {t('common:cancel')}
          </Button>
          <Button
            type="submit"
            disabled={!form.fullNameEn.trim() || !form.sourceType || createCandidate.isPending}
          >
            {createCandidate.isPending ? t('create.submitting') : t('create.submit')}
          </Button>
        </div>
      </form>
    </div>
  );
}

import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Star, Pencil } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Badge } from '@/shared/components/ui/badge';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { Checkbox } from '@/shared/components/ui/checkbox';
import { Skeleton } from '@/shared/components/ui/skeleton';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { useCountryPackage, useUpdateCountryPackage } from '../hooks';
import { ALL_GUARANTEE_PERIODS, ALL_COMMISSION_TYPES } from '../constants';

export function CountryPackageDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { t, i18n } = useTranslation('countryPackages');
  const { data: pkg, isLoading } = useCountryPackage(id!);
  const updateMutation = useUpdateCountryPackage();

  const [editing, setEditing] = useState(false);
  const [editData, setEditData] = useState<Record<string, string | boolean>>({});

  const isAr = i18n.language === 'ar';

  if (isLoading) return <DetailSkeleton />;
  if (!pkg) return <div className="p-6 text-center text-muted-foreground">{t('notFound')}</div>;

  const startEditing = () => {
    setEditData({
      name: pkg.name,
      isDefault: pkg.isDefault,
      isActive: pkg.isActive,
      maidCost: String(pkg.maidCost),
      monthlyAccommodationCost: String(pkg.monthlyAccommodationCost),
      visaCost: String(pkg.visaCost),
      employmentVisaCost: String(pkg.employmentVisaCost),
      residenceVisaCost: String(pkg.residenceVisaCost),
      medicalCost: String(pkg.medicalCost),
      transportationCost: String(pkg.transportationCost),
      ticketCost: String(pkg.ticketCost),
      insuranceCost: String(pkg.insuranceCost),
      emiratesIdCost: String(pkg.emiratesIdCost),
      otherCosts: String(pkg.otherCosts),
      totalPackagePrice: String(pkg.totalPackagePrice),
      supplierCommission: String(pkg.supplierCommission),
      supplierCommissionType: pkg.supplierCommissionType,
      defaultGuaranteePeriod: pkg.defaultGuaranteePeriod,
      currency: pkg.currency,
      effectiveFrom: pkg.effectiveFrom,
      effectiveTo: pkg.effectiveTo || '',
      notes: pkg.notes || '',
    });
    setEditing(true);
  };

  const handleSave = async () => {
    const payload: Record<string, unknown> = {};
    if (editData.name !== pkg.name) payload.name = editData.name;
    if (editData.isDefault !== pkg.isDefault) payload.isDefault = editData.isDefault;
    if (editData.isActive !== pkg.isActive) payload.isActive = editData.isActive;

    const pkgAny = pkg as unknown as Record<string, unknown>;

    const numFields = [
      'maidCost', 'monthlyAccommodationCost', 'visaCost', 'employmentVisaCost',
      'residenceVisaCost', 'medicalCost', 'transportationCost', 'ticketCost',
      'insuranceCost', 'emiratesIdCost', 'otherCosts', 'totalPackagePrice', 'supplierCommission',
    ];
    for (const f of numFields) {
      const newVal = parseFloat(editData[f] as string) || 0;
      if (newVal !== pkgAny[f]) payload[f] = newVal;
    }

    const strFields = ['supplierCommissionType', 'defaultGuaranteePeriod', 'currency', 'effectiveFrom', 'effectiveTo', 'notes'];
    for (const f of strFields) {
      const newVal = editData[f] as string;
      const origVal = (pkgAny[f] as string) || '';
      if (newVal !== origVal) payload[f] = newVal || undefined;
    }

    if (Object.keys(payload).length === 0) {
      setEditing(false);
      return;
    }

    try {
      await updateMutation.mutateAsync({ id: id!, data: payload });
      toast.success(t('updateSuccess'));
      setEditing(false);
    } catch {
      toast.error(t('updateError'));
    }
  };

  const set = (key: string, value: string | boolean) =>
    setEditData((prev) => ({ ...prev, [key]: value }));

  const costFields = [
    'maidCost', 'monthlyAccommodationCost', 'visaCost', 'employmentVisaCost',
    'residenceVisaCost', 'medicalCost', 'transportationCost', 'ticketCost',
    'insuranceCost', 'emiratesIdCost', 'otherCosts',
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link to="/country-packages" className="text-muted-foreground hover:text-foreground">
            <ArrowLeft className="h-5 w-5" />
          </Link>
          <h1 className="text-2xl font-bold">{editing ? editData.name as string : pkg.name}</h1>
          {pkg.isDefault && <Star className="h-5 w-5 text-amber-500 fill-amber-500" />}
          <Badge variant={pkg.isActive ? 'default' : 'secondary'}>
            {pkg.isActive ? t('common:active') : t('common:inactive')}
          </Badge>
        </div>
        <PermissionGate permission="packages.edit">
          {editing ? (
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => setEditing(false)}>
                {t('common:cancel')}
              </Button>
              <Button onClick={handleSave} disabled={updateMutation.isPending}>
                {updateMutation.isPending ? t('common:loading') : t('common:save')}
              </Button>
            </div>
          ) : (
            <Button variant="outline" onClick={startEditing}>
              <Pencil className="me-2 h-4 w-4" />
              {t('common:edit')}
            </Button>
          )}
        </PermissionGate>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Package Info */}
        <Card>
          <CardHeader><CardTitle>{t('detail.packageInfo')}</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            {editing ? (
              <>
                <div className="space-y-2">
                  <Label>{t('create.name')}</Label>
                  <Input value={editData.name as string} onChange={(e) => set('name', e.target.value)} />
                </div>
                <div className="grid gap-4 sm:grid-cols-2">
                  <div className="space-y-2">
                    <Label>{t('create.effectiveFrom')}</Label>
                    <Input type="date" value={editData.effectiveFrom as string} onChange={(e) => set('effectiveFrom', e.target.value)} />
                  </div>
                  <div className="space-y-2">
                    <Label>{t('create.effectiveTo')}</Label>
                    <Input type="date" value={editData.effectiveTo as string} onChange={(e) => set('effectiveTo', e.target.value)} />
                  </div>
                </div>
                <div className="space-y-2">
                  <Label>{t('create.currency')}</Label>
                  <Input value={editData.currency as string} onChange={(e) => set('currency', e.target.value)} />
                </div>
                <div className="flex items-center gap-4">
                  <div className="flex items-center gap-2">
                    <Checkbox id="ed-default" checked={editData.isDefault as boolean} onCheckedChange={(v) => set('isDefault', v === true)} />
                    <Label htmlFor="ed-default">{t('create.isDefault')}</Label>
                  </div>
                  <div className="flex items-center gap-2">
                    <Checkbox id="ed-active" checked={editData.isActive as boolean} onCheckedChange={(v) => set('isActive', v === true)} />
                    <Label htmlFor="ed-active">{t('create.isActive')}</Label>
                  </div>
                </div>
              </>
            ) : (
              <div className="grid grid-cols-2 gap-4">
                <InfoItem label={t('detail.country')} value={`${pkg.countryCode} — ${isAr ? pkg.countryNameAr : pkg.countryNameEn}`} />
                <InfoItem label={t('create.name')} value={pkg.name} />
                <InfoItem label={t('create.effectiveFrom')} value={pkg.effectiveFrom} />
                <InfoItem label={t('create.effectiveTo')} value={pkg.effectiveTo || '—'} />
                <InfoItem label={t('create.currency')} value={pkg.currency} />
                <InfoItem label={t('detail.createdAt')} value={new Date(pkg.createdAt).toLocaleDateString()} />
              </div>
            )}
          </CardContent>
        </Card>

        {/* Commission & Guarantee */}
        <Card>
          <CardHeader><CardTitle>{t('create.commissionSettings')}</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            {editing ? (
              <>
                <div className="space-y-2">
                  <Label>{t('create.guaranteePeriod')}</Label>
                  <Select value={editData.defaultGuaranteePeriod as string} onValueChange={(v) => set('defaultGuaranteePeriod', v)}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      {ALL_GUARANTEE_PERIODS.map((gp) => (
                        <SelectItem key={gp} value={gp}>{t(`guaranteePeriod.${gp}`)}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label>{t('create.commissionType')}</Label>
                  <Select value={editData.supplierCommissionType as string} onValueChange={(v) => set('supplierCommissionType', v)}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      {ALL_COMMISSION_TYPES.map((ct) => (
                        <SelectItem key={ct} value={ct}>{t(`commissionType.${ct}`)}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label>{t('create.supplierCommission')}</Label>
                  <Input type="number" step="0.01" value={editData.supplierCommission as string} onChange={(e) => set('supplierCommission', e.target.value)} />
                </div>
              </>
            ) : (
              <div className="grid grid-cols-2 gap-4">
                <InfoItem label={t('create.guaranteePeriod')} value={t(`guaranteePeriod.${pkg.defaultGuaranteePeriod}`)} />
                <InfoItem label={t('create.commissionType')} value={t(`commissionType.${pkg.supplierCommissionType}`)} />
                <InfoItem
                  label={t('create.supplierCommission')}
                  value={
                    pkg.supplierCommissionType === 'Percentage'
                      ? `${pkg.supplierCommission}%`
                      : `${pkg.supplierCommission.toLocaleString()} ${pkg.currency}`
                  }
                />
              </div>
            )}
          </CardContent>
        </Card>

        {/* Cost Breakdown */}
        <Card className="md:col-span-2">
          <CardHeader><CardTitle>{t('create.costBreakdown')}</CardTitle></CardHeader>
          <CardContent>
            {editing ? (
              <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {costFields.map((key) => (
                  <div key={key} className="space-y-2">
                    <Label>{t(`costs.${key}`)}</Label>
                    <Input type="number" step="0.01" value={editData[key] as string} onChange={(e) => set(key, e.target.value)} />
                  </div>
                ))}
                <div className="space-y-2">
                  <Label className="font-semibold">{t('costs.totalPackagePrice')}</Label>
                  <Input type="number" step="0.01" value={editData.totalPackagePrice as string} onChange={(e) => set('totalPackagePrice', e.target.value)} className="font-semibold" />
                </div>
              </div>
            ) : (
              <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {costFields.map((key) => (
                  <InfoItem
                    key={key}
                    label={t(`costs.${key}`)}
                    value={`${((pkg as unknown as Record<string, unknown>)[key] as number).toLocaleString()} ${pkg.currency}`}
                  />
                ))}
                <div className="space-y-1">
                  <p className="text-sm text-muted-foreground font-semibold">{t('costs.totalPackagePrice')}</p>
                  <p className="text-lg font-bold">{pkg.totalPackagePrice.toLocaleString()} {pkg.currency}</p>
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Notes */}
        {(pkg.notes || editing) && (
          <Card className="md:col-span-2">
            <CardHeader><CardTitle>{t('create.notes')}</CardTitle></CardHeader>
            <CardContent>
              {editing ? (
                <textarea
                  className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                  value={editData.notes as string}
                  onChange={(e) => set('notes', e.target.value)}
                />
              ) : (
                <p className="text-sm whitespace-pre-wrap">{pkg.notes}</p>
              )}
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}

function InfoItem({ label, value }: { label: string; value: string }) {
  return (
    <div className="space-y-1">
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="text-sm font-medium">{value}</p>
    </div>
  );
}

function DetailSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-6 w-20" />
      </div>
      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><Skeleton className="h-5 w-32" /></CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="space-y-1">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-5 w-40" />
              </div>
            ))}
          </CardContent>
        </Card>
        <Card>
          <CardHeader><Skeleton className="h-5 w-40" /></CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            {Array.from({ length: 3 }).map((_, i) => (
              <div key={i} className="space-y-1">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-5 w-40" />
              </div>
            ))}
          </CardContent>
        </Card>
        <Card className="md:col-span-2">
          <CardHeader><Skeleton className="h-5 w-36" /></CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 12 }).map((_, i) => (
              <div key={i} className="space-y-1">
                <Skeleton className="h-4 w-28" />
                <Skeleton className="h-5 w-32" />
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

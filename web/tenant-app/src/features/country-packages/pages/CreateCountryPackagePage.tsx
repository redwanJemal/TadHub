import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { Checkbox } from '@/shared/components/ui/checkbox';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { useCountryRefs } from '@/features/reference-data';
import { useCreateCountryPackage } from '../hooks';
import { ALL_GUARANTEE_PERIODS, ALL_COMMISSION_TYPES } from '../constants';

export function CreateCountryPackagePage() {
  const { t, i18n } = useTranslation('countryPackages');
  const navigate = useNavigate();
  const createMutation = useCreateCountryPackage();
  const { data: countries } = useCountryRefs();

  const isAr = i18n.language === 'ar';

  // Form state
  const [countryId, setCountryId] = useState('');
  const [name, setName] = useState('');
  const [isDefault, setIsDefault] = useState(false);
  const [isActive, setIsActive] = useState(true);
  const [currency, setCurrency] = useState('AED');
  const [effectiveFrom, setEffectiveFrom] = useState('');
  const [effectiveTo, setEffectiveTo] = useState('');
  const [guaranteePeriod, setGuaranteePeriod] = useState('TwoYears');
  const [commissionType, setCommissionType] = useState('FixedAmount');
  const [supplierCommission, setSupplierCommission] = useState('');
  const [notes, setNotes] = useState('');

  // Cost fields
  const [maidCost, setMaidCost] = useState('');
  const [monthlyAccommodationCost, setMonthlyAccommodationCost] = useState('');
  const [visaCost, setVisaCost] = useState('');
  const [employmentVisaCost, setEmploymentVisaCost] = useState('');
  const [residenceVisaCost, setResidenceVisaCost] = useState('');
  const [medicalCost, setMedicalCost] = useState('');
  const [transportationCost, setTransportationCost] = useState('');
  const [ticketCost, setTicketCost] = useState('');
  const [insuranceCost, setInsuranceCost] = useState('');
  const [emiratesIdCost, setEmiratesIdCost] = useState('');
  const [otherCosts, setOtherCosts] = useState('');
  const [totalPackagePrice, setTotalPackagePrice] = useState('');

  const canSubmit = countryId && name && effectiveFrom;

  const parseNum = (v: string) => parseFloat(v) || 0;

  const calculateTotal = () => {
    const total = parseNum(maidCost)
      + parseNum(monthlyAccommodationCost)
      + parseNum(visaCost)
      + parseNum(medicalCost)
      + parseNum(transportationCost)
      + parseNum(ticketCost)
      + parseNum(insuranceCost)
      + parseNum(emiratesIdCost)
      + parseNum(otherCosts);
    setTotalPackagePrice(total.toFixed(2));
  };

  const handleSubmit = async () => {
    if (!canSubmit) return;
    try {
      await createMutation.mutateAsync({
        countryId,
        name,
        isDefault,
        isActive,
        currency,
        effectiveFrom,
        effectiveTo: effectiveTo || undefined,
        defaultGuaranteePeriod: guaranteePeriod,
        supplierCommissionType: commissionType,
        supplierCommission: parseNum(supplierCommission),
        maidCost: parseNum(maidCost),
        monthlyAccommodationCost: parseNum(monthlyAccommodationCost),
        visaCost: parseNum(visaCost),
        employmentVisaCost: parseNum(employmentVisaCost),
        residenceVisaCost: parseNum(residenceVisaCost),
        medicalCost: parseNum(medicalCost),
        transportationCost: parseNum(transportationCost),
        ticketCost: parseNum(ticketCost),
        insuranceCost: parseNum(insuranceCost),
        emiratesIdCost: parseNum(emiratesIdCost),
        otherCosts: parseNum(otherCosts),
        totalPackagePrice: parseNum(totalPackagePrice),
        notes: notes || undefined,
      });
      toast.success(t('createSuccess'));
      navigate('/country-packages');
    } catch {
      toast.error(t('createError'));
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link to="/country-packages" className="text-muted-foreground hover:text-foreground">
          <ArrowLeft className="h-5 w-5" />
        </Link>
        <h1 className="text-2xl font-bold">{t('create.title')}</h1>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Basic Info */}
        <Card>
          <CardHeader><CardTitle>{t('create.basicInfo')}</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>{t('create.country')} *</Label>
              <Select value={countryId} onValueChange={setCountryId}>
                <SelectTrigger>
                  <SelectValue placeholder={t('create.countryPlaceholder')} />
                </SelectTrigger>
                <SelectContent>
                  {countries?.map((c) => (
                    <SelectItem key={c.id} value={c.id}>
                      {c.code} — {isAr ? c.nameAr : c.nameEn}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>{t('create.name')} *</Label>
              <Input
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder={t('create.namePlaceholder')}
              />
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>{t('create.effectiveFrom')} *</Label>
                <Input type="date" value={effectiveFrom} onChange={(e) => setEffectiveFrom(e.target.value)} />
              </div>
              <div className="space-y-2">
                <Label>{t('create.effectiveTo')}</Label>
                <Input type="date" value={effectiveTo} onChange={(e) => setEffectiveTo(e.target.value)} />
              </div>
            </div>
            <div className="space-y-2">
              <Label>{t('create.currency')}</Label>
              <Input value={currency} onChange={(e) => setCurrency(e.target.value)} />
            </div>
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-2">
                <Checkbox
                  id="isDefault"
                  checked={isDefault}
                  onCheckedChange={(v) => setIsDefault(v === true)}
                />
                <Label htmlFor="isDefault">{t('create.isDefault')}</Label>
              </div>
              <div className="flex items-center gap-2">
                <Checkbox
                  id="isActive"
                  checked={isActive}
                  onCheckedChange={(v) => setIsActive(v === true)}
                />
                <Label htmlFor="isActive">{t('create.isActive')}</Label>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Commission & Guarantee */}
        <Card>
          <CardHeader><CardTitle>{t('create.commissionSettings')}</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>{t('create.guaranteePeriod')}</Label>
              <Select value={guaranteePeriod} onValueChange={setGuaranteePeriod}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {ALL_GUARANTEE_PERIODS.map((gp) => (
                    <SelectItem key={gp} value={gp}>
                      {t(`guaranteePeriod.${gp}`)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>{t('create.commissionType')}</Label>
              <Select value={commissionType} onValueChange={setCommissionType}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {ALL_COMMISSION_TYPES.map((ct) => (
                    <SelectItem key={ct} value={ct}>
                      {t(`commissionType.${ct}`)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>
                {t('create.supplierCommission')}
                {commissionType === 'Percentage' ? ' (%)' : ` (${currency})`}
              </Label>
              <Input
                type="number"
                step="0.01"
                value={supplierCommission}
                onChange={(e) => setSupplierCommission(e.target.value)}
                placeholder="0.00"
              />
            </div>
          </CardContent>
        </Card>

        {/* Cost Components */}
        <Card className="md:col-span-2">
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>{t('create.costBreakdown')}</CardTitle>
              <Button variant="outline" size="sm" onClick={calculateTotal}>
                {t('create.calculateTotal')}
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {[
                { key: 'maidCost', value: maidCost, set: setMaidCost },
                { key: 'monthlyAccommodationCost', value: monthlyAccommodationCost, set: setMonthlyAccommodationCost },
                { key: 'visaCost', value: visaCost, set: setVisaCost },
                { key: 'employmentVisaCost', value: employmentVisaCost, set: setEmploymentVisaCost },
                { key: 'residenceVisaCost', value: residenceVisaCost, set: setResidenceVisaCost },
                { key: 'medicalCost', value: medicalCost, set: setMedicalCost },
                { key: 'transportationCost', value: transportationCost, set: setTransportationCost },
                { key: 'ticketCost', value: ticketCost, set: setTicketCost },
                { key: 'insuranceCost', value: insuranceCost, set: setInsuranceCost },
                { key: 'emiratesIdCost', value: emiratesIdCost, set: setEmiratesIdCost },
                { key: 'otherCosts', value: otherCosts, set: setOtherCosts },
              ].map(({ key, value, set }) => (
                <div key={key} className="space-y-2">
                  <Label>{t(`costs.${key}`)}</Label>
                  <Input
                    type="number"
                    step="0.01"
                    value={value}
                    onChange={(e) => set(e.target.value)}
                    placeholder="0.00"
                  />
                </div>
              ))}
              <div className="space-y-2">
                <Label className="font-semibold">{t('costs.totalPackagePrice')}</Label>
                <Input
                  type="number"
                  step="0.01"
                  value={totalPackagePrice}
                  onChange={(e) => setTotalPackagePrice(e.target.value)}
                  placeholder="0.00"
                  className="font-semibold"
                />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Notes */}
        <Card className="md:col-span-2">
          <CardHeader><CardTitle>{t('create.notes')}</CardTitle></CardHeader>
          <CardContent>
            <textarea
              className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder={t('create.notesPlaceholder')}
            />
          </CardContent>
        </Card>
      </div>

      <div className="flex justify-end gap-3">
        <Button variant="outline" onClick={() => navigate('/country-packages')}>
          {t('common:cancel')}
        </Button>
        <Button onClick={handleSubmit} disabled={!canSubmit || createMutation.isPending}>
          {createMutation.isPending ? t('create.submitting') : t('create.submit')}
        </Button>
      </div>
    </div>
  );
}

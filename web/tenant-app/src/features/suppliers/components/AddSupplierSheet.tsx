import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetFooter,
  SheetHeader,
  SheetTitle,
} from '@/shared/components/ui/sheet';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { useCreateAndLinkSupplier } from '../hooks';

interface AddSupplierSheetProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const initialForm = {
  nameEn: '',
  nameAr: '',
  country: '',
  city: '',
  licenseNumber: '',
  phone: '',
  email: '',
  website: '',
  notes: '',
};

export function AddSupplierSheet({ open, onOpenChange }: AddSupplierSheetProps) {
  const { t } = useTranslation('suppliers');
  const [form, setForm] = useState(initialForm);
  const createAndLink = useCreateAndLinkSupplier();

  const update = (field: string, value: string) =>
    setForm((prev) => ({ ...prev, [field]: value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.nameEn.trim() || !form.country.trim()) return;

    await createAndLink.mutateAsync({
      nameEn: form.nameEn.trim(),
      nameAr: form.nameAr.trim() || undefined,
      country: form.country.trim(),
      city: form.city.trim() || undefined,
      licenseNumber: form.licenseNumber.trim() || undefined,
      phone: form.phone.trim() || undefined,
      email: form.email.trim() || undefined,
      website: form.website.trim() || undefined,
      notes: form.notes.trim() || undefined,
    });

    setForm(initialForm);
    onOpenChange(false);
  };

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="right" className="overflow-y-auto">
        <form onSubmit={handleSubmit} className="flex flex-col h-full">
          <SheetHeader>
            <SheetTitle>{t('add.title')}</SheetTitle>
            <SheetDescription>{t('add.description')}</SheetDescription>
          </SheetHeader>

          <div className="grid gap-4 py-6 flex-1">
            {/* Name (English) - Required */}
            <div className="grid gap-2">
              <Label htmlFor="nameEn">
                {t('add.nameEn')} <span className="text-destructive">*</span>
              </Label>
              <Input
                id="nameEn"
                placeholder={t('add.nameEnPlaceholder')}
                value={form.nameEn}
                onChange={(e) => update('nameEn', e.target.value)}
                required
              />
            </div>

            {/* Name (Arabic) */}
            <div className="grid gap-2">
              <Label htmlFor="nameAr">{t('add.nameAr')}</Label>
              <Input
                id="nameAr"
                placeholder={t('add.nameArPlaceholder')}
                value={form.nameAr}
                onChange={(e) => update('nameAr', e.target.value)}
                dir="rtl"
              />
            </div>

            {/* Country - Required */}
            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="country">
                  {t('add.country')} <span className="text-destructive">*</span>
                </Label>
                <Input
                  id="country"
                  placeholder={t('add.countryPlaceholder')}
                  value={form.country}
                  onChange={(e) => update('country', e.target.value)}
                  maxLength={10}
                  required
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="city">{t('add.city')}</Label>
                <Input
                  id="city"
                  placeholder={t('add.cityPlaceholder')}
                  value={form.city}
                  onChange={(e) => update('city', e.target.value)}
                />
              </div>
            </div>

            {/* License Number */}
            <div className="grid gap-2">
              <Label htmlFor="licenseNumber">{t('add.licenseNumber')}</Label>
              <Input
                id="licenseNumber"
                placeholder={t('add.licenseNumberPlaceholder')}
                value={form.licenseNumber}
                onChange={(e) => update('licenseNumber', e.target.value)}
              />
            </div>

            {/* Phone & Email */}
            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="phone">{t('add.phone')}</Label>
                <Input
                  id="phone"
                  type="tel"
                  placeholder={t('add.phonePlaceholder')}
                  value={form.phone}
                  onChange={(e) => update('phone', e.target.value)}
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="email">{t('add.email')}</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder={t('add.emailPlaceholder')}
                  value={form.email}
                  onChange={(e) => update('email', e.target.value)}
                />
              </div>
            </div>

            {/* Website */}
            <div className="grid gap-2">
              <Label htmlFor="website">{t('add.website')}</Label>
              <Input
                id="website"
                type="url"
                placeholder={t('add.websitePlaceholder')}
                value={form.website}
                onChange={(e) => update('website', e.target.value)}
              />
            </div>

            {/* Notes */}
            <div className="grid gap-2">
              <Label htmlFor="notes">{t('add.notes')}</Label>
              <textarea
                id="notes"
                className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                placeholder={t('add.notesPlaceholder')}
                value={form.notes}
                onChange={(e) => update('notes', e.target.value)}
                rows={3}
              />
            </div>
          </div>

          <SheetFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              {t('common:cancel')}
            </Button>
            <Button
              type="submit"
              disabled={createAndLink.isPending || !form.nameEn.trim() || !form.country.trim()}
            >
              {createAndLink.isPending ? t('add.submitting') : t('add.submit')}
            </Button>
          </SheetFooter>
        </form>
      </SheetContent>
    </Sheet>
  );
}

import { useState, useEffect } from 'react';
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
import { useUpdateClient } from '../hooks';
import type { ClientListDto } from '../types';

interface EditClientSheetProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  client: ClientListDto | null;
}

export function EditClientSheet({ open, onOpenChange, client }: EditClientSheetProps) {
  const { t } = useTranslation('clients');
  const updateClient = useUpdateClient();

  const [form, setForm] = useState({
    nameEn: '',
    nameAr: '',
    nationalId: '',
    phone: '',
    email: '',
    city: '',
  });

  useEffect(() => {
    if (client) {
      setForm({
        nameEn: client.nameEn || '',
        nameAr: client.nameAr || '',
        nationalId: client.nationalId || '',
        phone: client.phone || '',
        email: client.email || '',
        city: client.city || '',
      });
    }
  }, [client]);

  const update = (field: string, value: string) =>
    setForm((prev) => ({ ...prev, [field]: value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!client || !form.nameEn.trim()) return;

    await updateClient.mutateAsync({
      id: client.id,
      data: {
        nameEn: form.nameEn.trim(),
        nameAr: form.nameAr.trim() || undefined,
        nationalId: form.nationalId.trim() || undefined,
        phone: form.phone.trim() || undefined,
        email: form.email.trim() || undefined,
        city: form.city.trim() || undefined,
      },
    });

    onOpenChange(false);
  };

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="right" className="overflow-y-auto">
        <form onSubmit={handleSubmit} className="flex flex-col h-full">
          <SheetHeader>
            <SheetTitle>{t('edit.title')}</SheetTitle>
            <SheetDescription>{t('edit.description')}</SheetDescription>
          </SheetHeader>

          <div className="grid gap-4 py-6 flex-1">
            <div className="grid gap-2">
              <Label htmlFor="editNameEn">
                {t('add.nameEn')} <span className="text-destructive">*</span>
              </Label>
              <Input
                id="editNameEn"
                placeholder={t('add.nameEnPlaceholder')}
                value={form.nameEn}
                onChange={(e) => update('nameEn', e.target.value)}
                required
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="editNameAr">{t('add.nameAr')}</Label>
              <Input
                id="editNameAr"
                placeholder={t('add.nameArPlaceholder')}
                value={form.nameAr}
                onChange={(e) => update('nameAr', e.target.value)}
                dir="rtl"
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="editNationalId">{t('add.nationalId')}</Label>
              <Input
                id="editNationalId"
                placeholder={t('add.nationalIdPlaceholder')}
                value={form.nationalId}
                onChange={(e) => update('nationalId', e.target.value)}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="editPhone">{t('add.phone')}</Label>
                <Input
                  id="editPhone"
                  type="tel"
                  placeholder={t('add.phonePlaceholder')}
                  value={form.phone}
                  onChange={(e) => update('phone', e.target.value)}
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="editEmail">{t('add.email')}</Label>
                <Input
                  id="editEmail"
                  type="email"
                  placeholder={t('add.emailPlaceholder')}
                  value={form.email}
                  onChange={(e) => update('email', e.target.value)}
                />
              </div>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="editCity">{t('add.city')}</Label>
              <Input
                id="editCity"
                placeholder={t('add.cityPlaceholder')}
                value={form.city}
                onChange={(e) => update('city', e.target.value)}
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
              disabled={updateClient.isPending || !form.nameEn.trim()}
            >
              {updateClient.isPending ? t('edit.submitting') : t('edit.submit')}
            </Button>
          </SheetFooter>
        </form>
      </SheetContent>
    </Sheet>
  );
}

import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Save, RefreshCw } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Label } from '@/shared/components/ui/label';
import { Input } from '@/shared/components/ui/input';
import { cn } from '@/shared/lib/cn';
import { usePermissions } from '@/features/auth/hooks/usePermissions';
import { useFinancialSettings, useUpdateFinancialSettings } from '../hooks';
import type { TenantFinancialSettings, PaymentMethod } from '../types';

const ALL_PAYMENT_METHODS: PaymentMethod[] = ['Cash', 'Card', 'BankTransfer', 'Cheque', 'EDirham', 'Online'];

const METHOD_LABELS: Record<PaymentMethod, string> = {
  Cash:         'Cash',
  Card:         'Card',
  BankTransfer: 'Bank Transfer',
  Cheque:       'Cheque',
  EDirham:      'E-Dirham',
  Online:       'Online',
};

function SettingsSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <Skeleton className="h-7 w-48 mb-2" />
          <Skeleton className="h-4 w-64" />
        </div>
        <Skeleton className="h-9 w-24" />
      </div>
      <div className="grid gap-6 md:grid-cols-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <Card key={i}>
            <CardHeader><Skeleton className="h-6 w-40" /></CardHeader>
            <CardContent className="space-y-4">
              {Array.from({ length: 3 }).map((_, j) => (
                <div key={j} className="space-y-2">
                  <Skeleton className="h-4 w-24" />
                  <Skeleton className="h-9 w-full" />
                </div>
              ))}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}

export function FinancialSettingsPage() {
  useTranslation('finance');
  const { hasPermission } = usePermissions();

  const { data: settings, isLoading, refetch } = useFinancialSettings();
  const updateMutation = useUpdateFinancialSettings();

  const canEdit = hasPermission('finance.settings.manage');

  // Form state — mirror TenantFinancialSettings
  const [vatRate, setVatRate] = useState('5');
  const [vatEnabled, setVatEnabled] = useState(true);
  const [taxRegistrationNumber, setTaxRegistrationNumber] = useState('');
  const [defaultCurrency, setDefaultCurrency] = useState('AED');
  const [invoicePrefix, setInvoicePrefix] = useState('INV-');
  const [paymentPrefix, setPaymentPrefix] = useState('PAY-');
  const [invoiceDueDays, setInvoiceDueDays] = useState('30');
  const [requireDepositOnBooking, setRequireDepositOnBooking] = useState(false);
  const [depositPercentage, setDepositPercentage] = useState('20');
  const [enableInstallments, setEnableInstallments] = useState(false);
  const [maxInstallments, setMaxInstallments] = useState('3');
  const [paymentMethods, setPaymentMethods] = useState<string[]>(['Cash', 'Card', 'BankTransfer']);
  const [invoiceFooterText, setInvoiceFooterText] = useState('');
  const [invoiceFooterTextAr, setInvoiceFooterTextAr] = useState('');
  const [invoiceTerms, setInvoiceTerms] = useState('');
  const [invoiceTermsAr, setInvoiceTermsAr] = useState('');
  const [autoGenerateInvoiceOnConfirm, setAutoGenerateInvoiceOnConfirm] = useState(false);

  // Invoice Template
  const [templatePrimaryColor, setTemplatePrimaryColor] = useState('#1a365d');
  const [templateAccentColor, setTemplateAccentColor] = useState('#2b6cb0');
  const [templateShowLogo, setTemplateShowLogo] = useState(true);
  const [templateShowArabic, setTemplateShowArabic] = useState(true);
  const [templateCompanyAddress, setTemplateCompanyAddress] = useState('');
  const [templateCompanyAddressAr, setTemplateCompanyAddressAr] = useState('');

  const [isDirty, setIsDirty] = useState(false);

  // Populate form when settings load
  useEffect(() => {
    if (!settings) return;
    setVatRate(String(settings.vatRate));
    setVatEnabled(settings.vatEnabled);
    setTaxRegistrationNumber(settings.taxRegistrationNumber ?? '');
    setDefaultCurrency(settings.defaultCurrency);
    setInvoicePrefix(settings.invoicePrefix);
    setPaymentPrefix(settings.paymentPrefix);
    setInvoiceDueDays(String(settings.invoiceDueDays));
    setRequireDepositOnBooking(settings.requireDepositOnBooking);
    setDepositPercentage(String(settings.depositPercentage));
    setEnableInstallments(settings.enableInstallments);
    setMaxInstallments(String(settings.maxInstallments));
    setPaymentMethods(settings.paymentMethods);
    setInvoiceFooterText(settings.invoiceFooterText ?? '');
    setInvoiceFooterTextAr(settings.invoiceFooterTextAr ?? '');
    setInvoiceTerms(settings.invoiceTerms ?? '');
    setInvoiceTermsAr(settings.invoiceTermsAr ?? '');
    setAutoGenerateInvoiceOnConfirm(settings.autoGenerateInvoiceOnConfirm);
    // Invoice Template
    const tpl = settings.invoiceTemplate;
    if (tpl) {
      setTemplatePrimaryColor(tpl.primaryColor ?? '#1a365d');
      setTemplateAccentColor(tpl.accentColor ?? '#2b6cb0');
      setTemplateShowLogo(tpl.showLogo ?? true);
      setTemplateShowArabic(tpl.showArabicText ?? true);
      setTemplateCompanyAddress(tpl.companyAddress ?? '');
      setTemplateCompanyAddressAr(tpl.companyAddressAr ?? '');
    }
    setIsDirty(false);
  }, [settings]);

  const toggleMethod = (method: string) => {
    setPaymentMethods((prev) =>
      prev.includes(method) ? prev.filter((m) => m !== method) : [...prev, method]
    );
    setIsDirty(true);
  };

  const handleSave = async () => {
    const payload: TenantFinancialSettings = {
      vatRate: parseFloat(vatRate) || 0,
      vatEnabled,
      taxRegistrationNumber: taxRegistrationNumber || undefined,
      defaultCurrency,
      invoicePrefix,
      paymentPrefix,
      invoiceDueDays: parseInt(invoiceDueDays, 10) || 30,
      requireDepositOnBooking,
      depositPercentage: parseFloat(depositPercentage) || 0,
      enableInstallments,
      maxInstallments: parseInt(maxInstallments, 10) || 1,
      paymentMethods,
      invoiceFooterText: invoiceFooterText || undefined,
      invoiceFooterTextAr: invoiceFooterTextAr || undefined,
      invoiceTerms: invoiceTerms || undefined,
      invoiceTermsAr: invoiceTermsAr || undefined,
      autoGenerateInvoiceOnConfirm,
      invoiceTemplate: {
        primaryColor: templatePrimaryColor,
        accentColor: templateAccentColor,
        showLogo: templateShowLogo,
        showArabicText: templateShowArabic,
        companyAddress: templateCompanyAddress || undefined,
        companyAddressAr: templateCompanyAddressAr || undefined,
      },
    };
    await updateMutation.mutateAsync(payload);
    setIsDirty(false);
  };

  if (isLoading) return <SettingsSkeleton />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Financial Settings</h1>
          <p className="text-muted-foreground">Configure VAT, invoicing, and payment settings</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="icon" onClick={() => refetch()}>
            <RefreshCw className="h-4 w-4" />
          </Button>
          {canEdit && (
            <Button onClick={handleSave} disabled={!isDirty || updateMutation.isPending}>
              <Save className="me-2 h-4 w-4" />
              {updateMutation.isPending ? 'Saving...' : 'Save Changes'}
            </Button>
          )}
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* VAT Settings */}
        <Card>
          <CardHeader><CardTitle>VAT Settings</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="vatEnabled"
                checked={vatEnabled}
                onChange={(e) => { setVatEnabled(e.target.checked); setIsDirty(true); }}
                disabled={!canEdit}
                className="h-4 w-4"
              />
              <Label htmlFor="vatEnabled">Enable VAT</Label>
            </div>
            <div className="space-y-2">
              <Label>VAT Rate (%)</Label>
              <Input
                type="number"
                min="0"
                max="100"
                step="0.01"
                value={vatRate}
                onChange={(e) => { setVatRate(e.target.value); setIsDirty(true); }}
                disabled={!canEdit || !vatEnabled}
              />
            </div>
            <div className="space-y-2">
              <Label>Tax Registration Number (TRN)</Label>
              <Input
                placeholder="e.g. 100123456789003"
                value={taxRegistrationNumber}
                onChange={(e) => { setTaxRegistrationNumber(e.target.value); setIsDirty(true); }}
                disabled={!canEdit}
              />
            </div>
          </CardContent>
        </Card>

        {/* Invoice Settings */}
        <Card>
          <CardHeader><CardTitle>Invoice Settings</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>Invoice Prefix</Label>
                <Input
                  value={invoicePrefix}
                  onChange={(e) => { setInvoicePrefix(e.target.value); setIsDirty(true); }}
                  disabled={!canEdit}
                />
              </div>
              <div className="space-y-2">
                <Label>Payment Prefix</Label>
                <Input
                  value={paymentPrefix}
                  onChange={(e) => { setPaymentPrefix(e.target.value); setIsDirty(true); }}
                  disabled={!canEdit}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Invoice Due Days</Label>
              <Input
                type="number"
                min="0"
                value={invoiceDueDays}
                onChange={(e) => { setInvoiceDueDays(e.target.value); setIsDirty(true); }}
                disabled={!canEdit}
              />
            </div>
            <div className="space-y-2">
              <Label>Default Currency</Label>
              <Input
                value={defaultCurrency}
                onChange={(e) => { setDefaultCurrency(e.target.value); setIsDirty(true); }}
                disabled={!canEdit}
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="autoGenerate"
                checked={autoGenerateInvoiceOnConfirm}
                onChange={(e) => { setAutoGenerateInvoiceOnConfirm(e.target.checked); setIsDirty(true); }}
                disabled={!canEdit}
                className="h-4 w-4"
              />
              <Label htmlFor="autoGenerate">Auto-generate invoice on contract confirmation</Label>
            </div>
          </CardContent>
        </Card>

        {/* Deposit & Installments */}
        <Card>
          <CardHeader><CardTitle>Deposit & Installments</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="requireDeposit"
                checked={requireDepositOnBooking}
                onChange={(e) => { setRequireDepositOnBooking(e.target.checked); setIsDirty(true); }}
                disabled={!canEdit}
                className="h-4 w-4"
              />
              <Label htmlFor="requireDeposit">Require deposit on booking</Label>
            </div>
            <div className="space-y-2">
              <Label>Deposit Percentage (%)</Label>
              <Input
                type="number"
                min="0"
                max="100"
                step="0.01"
                value={depositPercentage}
                onChange={(e) => { setDepositPercentage(e.target.value); setIsDirty(true); }}
                disabled={!canEdit || !requireDepositOnBooking}
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="enableInstallments"
                checked={enableInstallments}
                onChange={(e) => { setEnableInstallments(e.target.checked); setIsDirty(true); }}
                disabled={!canEdit}
                className="h-4 w-4"
              />
              <Label htmlFor="enableInstallments">Enable installment payments</Label>
            </div>
            <div className="space-y-2">
              <Label>Max Installments</Label>
              <Input
                type="number"
                min="1"
                value={maxInstallments}
                onChange={(e) => { setMaxInstallments(e.target.value); setIsDirty(true); }}
                disabled={!canEdit || !enableInstallments}
              />
            </div>
          </CardContent>
        </Card>

        {/* Payment Methods */}
        <Card>
          <CardHeader><CardTitle>Accepted Payment Methods</CardTitle></CardHeader>
          <CardContent>
            <div className="flex flex-wrap gap-2">
              {ALL_PAYMENT_METHODS.map((method) => {
                const isActive = paymentMethods.includes(method);
                return (
                  <button
                    key={method}
                    type="button"
                    onClick={() => canEdit && toggleMethod(method)}
                    disabled={!canEdit}
                    className={cn(
                      'rounded-full px-3 py-1 text-sm font-medium border transition-colors',
                      isActive
                        ? 'bg-primary text-primary-foreground border-primary'
                        : 'bg-background text-muted-foreground border-border hover:border-primary hover:text-foreground',
                      !canEdit && 'cursor-not-allowed opacity-60'
                    )}
                  >
                    {METHOD_LABELS[method]}
                  </button>
                );
              })}
            </div>
            <p className="text-xs text-muted-foreground mt-3">
              {paymentMethods.length} method{paymentMethods.length !== 1 ? 's' : ''} enabled
            </p>
          </CardContent>
        </Card>

        {/* Invoice PDF Template */}
        <Card className="md:col-span-2">
          <CardHeader><CardTitle>Invoice PDF Template</CardTitle></CardHeader>
          <CardContent className="grid gap-4 md:grid-cols-2">
            <div className="space-y-4">
              <div className="space-y-2">
                <Label>Primary Color</Label>
                <div className="flex items-center gap-2">
                  <input
                    type="color"
                    value={templatePrimaryColor}
                    onChange={(e) => { setTemplatePrimaryColor(e.target.value); setIsDirty(true); }}
                    disabled={!canEdit}
                    className="h-9 w-12 rounded border border-input cursor-pointer disabled:cursor-not-allowed disabled:opacity-60"
                  />
                  <Input
                    value={templatePrimaryColor}
                    onChange={(e) => { setTemplatePrimaryColor(e.target.value); setIsDirty(true); }}
                    disabled={!canEdit}
                    className="font-mono"
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label>Accent Color</Label>
                <div className="flex items-center gap-2">
                  <input
                    type="color"
                    value={templateAccentColor}
                    onChange={(e) => { setTemplateAccentColor(e.target.value); setIsDirty(true); }}
                    disabled={!canEdit}
                    className="h-9 w-12 rounded border border-input cursor-pointer disabled:cursor-not-allowed disabled:opacity-60"
                  />
                  <Input
                    value={templateAccentColor}
                    onChange={(e) => { setTemplateAccentColor(e.target.value); setIsDirty(true); }}
                    disabled={!canEdit}
                    className="font-mono"
                  />
                </div>
              </div>
              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="templateShowLogo"
                  checked={templateShowLogo}
                  onChange={(e) => { setTemplateShowLogo(e.target.checked); setIsDirty(true); }}
                  disabled={!canEdit}
                  className="h-4 w-4"
                />
                <Label htmlFor="templateShowLogo">Show company logo on invoice</Label>
              </div>
              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="templateShowArabic"
                  checked={templateShowArabic}
                  onChange={(e) => { setTemplateShowArabic(e.target.checked); setIsDirty(true); }}
                  disabled={!canEdit}
                  className="h-4 w-4"
                />
                <Label htmlFor="templateShowArabic">Show Arabic text on invoice</Label>
              </div>
            </div>
            <div className="space-y-4">
              <div className="space-y-2">
                <Label>Company Address (EN)</Label>
                <textarea
                  className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:opacity-60 disabled:cursor-not-allowed"
                  placeholder="Company address for invoice header..."
                  value={templateCompanyAddress}
                  onChange={(e) => { setTemplateCompanyAddress(e.target.value); setIsDirty(true); }}
                  disabled={!canEdit}
                />
              </div>
              <div className="space-y-2">
                <Label>Company Address (AR)</Label>
                <textarea
                  className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:opacity-60 disabled:cursor-not-allowed"
                  placeholder="عنوان الشركة للفاتورة..."
                  dir="rtl"
                  value={templateCompanyAddressAr}
                  onChange={(e) => { setTemplateCompanyAddressAr(e.target.value); setIsDirty(true); }}
                  disabled={!canEdit}
                />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Invoice Terms */}
        <Card className="md:col-span-2">
          <CardHeader><CardTitle>Invoice Terms & Footer</CardTitle></CardHeader>
          <CardContent className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <Label>Invoice Terms (EN)</Label>
              <textarea
                className="flex min-h-[100px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:opacity-60 disabled:cursor-not-allowed"
                placeholder="Payment terms and conditions..."
                value={invoiceTerms}
                onChange={(e) => { setInvoiceTerms(e.target.value); setIsDirty(true); }}
                disabled={!canEdit}
              />
            </div>
            <div className="space-y-2">
              <Label>Invoice Terms (AR)</Label>
              <textarea
                className="flex min-h-[100px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:opacity-60 disabled:cursor-not-allowed"
                placeholder="شروط وأحكام الدفع..."
                dir="rtl"
                value={invoiceTermsAr}
                onChange={(e) => { setInvoiceTermsAr(e.target.value); setIsDirty(true); }}
                disabled={!canEdit}
              />
            </div>
            <div className="space-y-2">
              <Label>Invoice Footer (EN)</Label>
              <textarea
                className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:opacity-60 disabled:cursor-not-allowed"
                placeholder="Thank you for your business..."
                value={invoiceFooterText}
                onChange={(e) => { setInvoiceFooterText(e.target.value); setIsDirty(true); }}
                disabled={!canEdit}
              />
            </div>
            <div className="space-y-2">
              <Label>Invoice Footer (AR)</Label>
              <textarea
                className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:opacity-60 disabled:cursor-not-allowed"
                placeholder="شكراً لتعاملكم معنا..."
                dir="rtl"
                value={invoiceFooterTextAr}
                onChange={(e) => { setInvoiceFooterTextAr(e.target.value); setIsDirty(true); }}
                disabled={!canEdit}
              />
            </div>
          </CardContent>
        </Card>
      </div>

      {isDirty && canEdit && (
        <div className="sticky bottom-4 flex justify-end">
          <div className="bg-background border rounded-lg shadow-lg p-3 flex items-center gap-3">
            <span className="text-sm text-muted-foreground">You have unsaved changes</span>
            <Button
              variant="outline"
              size="sm"
              onClick={() => {
                refetch();
                setIsDirty(false);
              }}
            >
              Discard
            </Button>
            <Button size="sm" onClick={handleSave} disabled={updateMutation.isPending}>
              <Save className="me-2 h-4 w-4" />
              {updateMutation.isPending ? 'Saving...' : 'Save'}
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}

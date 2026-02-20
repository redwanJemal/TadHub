import { useTranslation } from 'react-i18next';
import { CreditCard, Receipt, ArrowUpRight } from 'lucide-react';

export function BillingPage() {
  const { t } = useTranslation();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">{t('nav.billing')}</h1>
        <p className="text-muted-foreground mt-1">Manage your subscription and billing</p>
      </div>

      {/* Current plan */}
      <div className="rounded-xl bg-card border border-border p-6">
        <div className="flex items-start justify-between">
          <div>
            <p className="text-sm text-muted-foreground">Current Plan</p>
            <h2 className="text-2xl font-bold text-foreground mt-1">Free Trial</h2>
            <p className="text-sm text-muted-foreground mt-2">Your trial ends in 14 days</p>
          </div>
          <button className="flex items-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90">
            Upgrade Plan
            <ArrowUpRight className="h-4 w-4" />
          </button>
        </div>
      </div>

      {/* Payment method */}
      <div className="rounded-xl bg-card border border-border p-6">
        <h3 className="font-semibold text-foreground mb-4">Payment Method</h3>
        <div className="flex items-center gap-4 p-4 border border-border rounded-lg">
          <CreditCard className="h-8 w-8 text-muted-foreground" />
          <p className="text-muted-foreground">No payment method added</p>
        </div>
        <button className="mt-4 rounded-lg border border-border px-4 py-2 text-sm font-medium hover:bg-muted">
          Add Payment Method
        </button>
      </div>

      {/* Billing history */}
      <div className="rounded-xl bg-card border border-border p-6">
        <h3 className="font-semibold text-foreground mb-4">Billing History</h3>
        <div className="text-center py-8">
          <Receipt className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
          <p className="text-muted-foreground">No invoices yet</p>
        </div>
      </div>
    </div>
  );
}

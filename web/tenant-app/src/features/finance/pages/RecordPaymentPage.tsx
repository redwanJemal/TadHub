import { useState } from 'react';
import { useNavigate, Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Label } from '@/shared/components/ui/label';
import { Input } from '@/shared/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { useRecordPayment } from '../hooks';
import type { PaymentMethod } from '../types';

const PAYMENT_METHODS: PaymentMethod[] = ['Cash', 'Card', 'BankTransfer', 'Cheque', 'EDirham', 'Online'];

const METHOD_LABELS: Record<PaymentMethod, string> = {
  Cash:         'Cash',
  Card:         'Card',
  BankTransfer: 'Bank Transfer',
  Cheque:       'Cheque',
  EDirham:      'E-Dirham',
  Online:       'Online',
};

export function RecordPaymentPage() {
  useTranslation('finance');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const recordMutation = useRecordPayment();

  // Form state
  const [invoiceId, setInvoiceId] = useState(searchParams.get('invoiceId') ?? '');
  const [clientId, setClientId] = useState('');
  const [amount, setAmount] = useState('');
  const [currency, setCurrency] = useState('AED');
  const [method, setMethod] = useState<string>('Cash');
  const [referenceNumber, setReferenceNumber] = useState('');
  const [paymentDate, setPaymentDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [gatewayProvider, setGatewayProvider] = useState('');
  const [cashierName, setCashierName] = useState('');
  const [notes, setNotes] = useState('');

  const showReference = method !== 'Cash';
  const showGateway = method === 'Online' || method === 'Card';

  const canSubmit =
    invoiceId.trim() &&
    clientId.trim() &&
    amount &&
    parseFloat(amount) > 0 &&
    method &&
    paymentDate;

  const handleSubmit = async () => {
    if (!canSubmit) return;
    await recordMutation.mutateAsync({
      invoiceId,
      clientId,
      amount: parseFloat(amount),
      currency,
      method,
      referenceNumber: referenceNumber || undefined,
      paymentDate,
      gatewayProvider: gatewayProvider || undefined,
      cashierName: cashierName || undefined,
      notes: notes.trim() || undefined,
    });
    navigate('/finance/payments');
  };

  return (
    <div className="space-y-6">
      <div>
        <Link
          to="/finance/payments"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-4"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Payments
        </Link>
        <h1 className="text-2xl font-bold tracking-tight">Record Payment</h1>
        <p className="text-muted-foreground">Record a payment against an invoice</p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Invoice & Client */}
        <Card>
          <CardHeader><CardTitle>Invoice & Client</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>Invoice ID *</Label>
              <Input
                placeholder="Enter invoice ID"
                value={invoiceId}
                onChange={(e) => setInvoiceId(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>Client ID *</Label>
              <Input
                placeholder="Enter client ID"
                value={clientId}
                onChange={(e) => setClientId(e.target.value)}
              />
            </div>
          </CardContent>
        </Card>

        {/* Payment Details */}
        <Card>
          <CardHeader><CardTitle>Payment Details</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>Amount *</Label>
                <Input
                  type="number"
                  min="0.01"
                  step="0.01"
                  placeholder="0.00"
                  value={amount}
                  onChange={(e) => setAmount(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <Label>Currency</Label>
                <Input value={currency} onChange={(e) => setCurrency(e.target.value)} />
              </div>
            </div>

            <div className="space-y-2">
              <Label>Payment Method *</Label>
              <Select value={method} onValueChange={setMethod}>
                <SelectTrigger>
                  <SelectValue placeholder="Select method" />
                </SelectTrigger>
                <SelectContent>
                  {PAYMENT_METHODS.map((m) => (
                    <SelectItem key={m} value={m}>{METHOD_LABELS[m]}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Payment Date *</Label>
              <Input
                type="date"
                value={paymentDate}
                onChange={(e) => setPaymentDate(e.target.value)}
              />
            </div>

            {showReference && (
              <div className="space-y-2">
                <Label>Reference Number</Label>
                <Input
                  placeholder="Transaction / cheque reference"
                  value={referenceNumber}
                  onChange={(e) => setReferenceNumber(e.target.value)}
                />
              </div>
            )}

            {showGateway && (
              <div className="space-y-2">
                <Label>Gateway Provider</Label>
                <Input
                  placeholder="e.g. Stripe, PayFort"
                  value={gatewayProvider}
                  onChange={(e) => setGatewayProvider(e.target.value)}
                />
              </div>
            )}
          </CardContent>
        </Card>

        {/* Cashier & Notes */}
        <Card>
          <CardHeader><CardTitle>Cashier & Notes</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>Cashier Name</Label>
              <Input
                placeholder="Name of the cashier (optional)"
                value={cashierName}
                onChange={(e) => setCashierName(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>Notes</Label>
              <textarea
                className="flex min-h-[100px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                placeholder="Optional payment notes..."
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
              />
            </div>
          </CardContent>
        </Card>

        {/* Summary */}
        <Card>
          <CardHeader><CardTitle>Summary</CardTitle></CardHeader>
          <CardContent>
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Invoice</span>
                <span className="font-mono">{invoiceId || '—'}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Client</span>
                <span className="font-mono">{clientId || '—'}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Method</span>
                <span>{METHOD_LABELS[method as PaymentMethod] ?? method}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Date</span>
                <span>{paymentDate ? new Date(paymentDate).toLocaleDateString() : '—'}</span>
              </div>
              <div className="flex justify-between border-t pt-2 font-bold text-base">
                <span>Amount</span>
                <span className="tabular-nums">
                  {amount ? parseFloat(amount).toFixed(2) : '0.00'} {currency}
                </span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="flex justify-end gap-3">
        <Button variant="outline" onClick={() => navigate('/finance/payments')}>
          Cancel
        </Button>
        <Button onClick={handleSubmit} disabled={!canSubmit || recordMutation.isPending}>
          {recordMutation.isPending ? 'Recording...' : 'Record Payment'}
        </Button>
      </div>
    </div>
  );
}

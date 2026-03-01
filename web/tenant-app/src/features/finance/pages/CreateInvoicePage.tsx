import { useState } from 'react';
import { useNavigate, Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Plus, Trash2 } from 'lucide-react';
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
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import { useCreateInvoice } from '../hooks';
import type { CreateInvoiceLineItemRequest } from '../types';

const INVOICE_TYPES = ['Standard', 'CreditNote', 'ProformaDeposit'] as const;
const MILESTONE_TYPES = ['AdvanceDeposit', 'ActivationBalance', 'Installment', 'FullPayment'] as const;
const VAT_RATE = 0.05;

interface LineItemRow extends CreateInvoiceLineItemRequest {
  _key: string;
}

function computeLineTotal(item: CreateInvoiceLineItemRequest) {
  return item.quantity * item.unitPrice - (item.discountAmount ?? 0);
}

function computeTotals(lineItems: LineItemRow[], vatRate: number) {
  const subtotal = lineItems.reduce((sum, li) => sum + computeLineTotal(li), 0);
  const vatAmount = subtotal * vatRate;
  const total = subtotal + vatAmount;
  return { subtotal, vatAmount, total };
}

export function CreateInvoicePage() {
  useTranslation('finance');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const createMutation = useCreateInvoice();

  // Form state
  const [contractId, setContractId] = useState(searchParams.get('contractId') ?? '');
  const [clientId, setClientId] = useState(searchParams.get('clientId') ?? '');
  const [workerId, setWorkerId] = useState('');
  const [type, setType] = useState<string>('Standard');
  const [milestoneType, setMilestoneType] = useState('');
  const [issueDate, setIssueDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [dueDate, setDueDate] = useState(() => {
    const d = new Date();
    d.setDate(d.getDate() + 30);
    return d.toISOString().slice(0, 10);
  });
  const [currency, setCurrency] = useState('AED');
  const [tenantTrn, setTenantTrn] = useState('');
  const [clientTrn, setClientTrn] = useState('');
  const [notes, setNotes] = useState('');
  const [applyVat, setApplyVat] = useState(true);

  const [lineItems, setLineItems] = useState<LineItemRow[]>([
    {
      _key: crypto.randomUUID(),
      description: '',
      quantity: 1,
      unitPrice: 0,
      discountAmount: 0,
    },
  ]);

  const addLineItem = () => {
    setLineItems((prev) => [
      ...prev,
      {
        _key: crypto.randomUUID(),
        description: '',
        quantity: 1,
        unitPrice: 0,
        discountAmount: 0,
      },
    ]);
  };

  const removeLineItem = (key: string) => {
    setLineItems((prev) => prev.filter((li) => li._key !== key));
  };

  const updateLineItem = (key: string, field: keyof CreateInvoiceLineItemRequest, value: string | number) => {
    setLineItems((prev) =>
      prev.map((li) => (li._key === key ? { ...li, [field]: value } : li))
    );
  };

  const { subtotal, vatAmount, total } = computeTotals(lineItems, applyVat ? VAT_RATE : 0);

  const canSubmit =
    contractId.trim() &&
    clientId.trim() &&
    issueDate &&
    dueDate &&
    lineItems.length > 0 &&
    lineItems.every((li) => li.description.trim() && li.quantity > 0);

  const handleSubmit = async () => {
    if (!canSubmit) return;
    await createMutation.mutateAsync({
      contractId,
      clientId,
      workerId: workerId || undefined,
      type,
      issueDate,
      dueDate,
      milestoneType: milestoneType || undefined,
      currency,
      tenantTrn: tenantTrn || undefined,
      clientTrn: clientTrn || undefined,
      notes: notes.trim() || undefined,
      lineItems: lineItems.map(({ _key, ...li }) => li),
    });
    navigate('/finance/invoices');
  };

  return (
    <div className="space-y-6">
      <div>
        <Link
          to="/finance/invoices"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-4"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Invoices
        </Link>
        <h1 className="text-2xl font-bold tracking-tight">New Invoice</h1>
        <p className="text-muted-foreground">Create a new invoice for a contract</p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Contract & Client */}
        <Card>
          <CardHeader><CardTitle>Contract & Client</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>Contract ID *</Label>
              <Input
                placeholder="Enter contract ID"
                value={contractId}
                onChange={(e) => setContractId(e.target.value)}
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
            <div className="space-y-2">
              <Label>Worker ID</Label>
              <Input
                placeholder="Optional worker ID"
                value={workerId}
                onChange={(e) => setWorkerId(e.target.value)}
              />
            </div>
          </CardContent>
        </Card>

        {/* Invoice Details */}
        <Card>
          <CardHeader><CardTitle>Invoice Details</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>Invoice Type</Label>
              <Select value={type} onValueChange={setType}>
                <SelectTrigger>
                  <SelectValue placeholder="Select type" />
                </SelectTrigger>
                <SelectContent>
                  {INVOICE_TYPES.map((tp) => (
                    <SelectItem key={tp} value={tp}>
                      {tp === 'CreditNote' ? 'Credit Note' : tp === 'ProformaDeposit' ? 'Proforma Deposit' : tp}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Milestone Type</Label>
              <Select value={milestoneType || 'none'} onValueChange={(v) => setMilestoneType(v === 'none' ? '' : v)}>
                <SelectTrigger>
                  <SelectValue placeholder="Select milestone (optional)" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">None</SelectItem>
                  {MILESTONE_TYPES.map((mt) => (
                    <SelectItem key={mt} value={mt}>{mt}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>Issue Date *</Label>
                <Input type="date" value={issueDate} onChange={(e) => setIssueDate(e.target.value)} />
              </div>
              <div className="space-y-2">
                <Label>Due Date *</Label>
                <Input type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Currency</Label>
              <Input value={currency} onChange={(e) => setCurrency(e.target.value)} />
            </div>
          </CardContent>
        </Card>

        {/* Tax Info */}
        <Card>
          <CardHeader><CardTitle>Tax & Reference</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>Tenant TRN</Label>
              <Input
                placeholder="Tax Registration Number"
                value={tenantTrn}
                onChange={(e) => setTenantTrn(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>Client TRN</Label>
              <Input
                placeholder="Client Tax Registration Number"
                value={clientTrn}
                onChange={(e) => setClientTrn(e.target.value)}
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="applyVat"
                checked={applyVat}
                onChange={(e) => setApplyVat(e.target.checked)}
                className="h-4 w-4"
              />
              <Label htmlFor="applyVat">Apply VAT (5%)</Label>
            </div>
          </CardContent>
        </Card>

        {/* Notes */}
        <Card>
          <CardHeader><CardTitle>Notes</CardTitle></CardHeader>
          <CardContent>
            <textarea
              className="flex min-h-[120px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
              placeholder="Optional notes for the invoice..."
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </CardContent>
        </Card>
      </div>

      {/* Line Items */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Line Items</CardTitle>
            <Button variant="outline" size="sm" onClick={addLineItem}>
              <Plus className="me-2 h-4 w-4" />
              Add Row
            </Button>
          </div>
        </CardHeader>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Description *</TableHead>
                <TableHead>Arabic Description</TableHead>
                <TableHead>Item Code</TableHead>
                <TableHead className="text-right w-20">Qty *</TableHead>
                <TableHead className="text-right w-28">Unit Price *</TableHead>
                <TableHead className="text-right w-28">Discount</TableHead>
                <TableHead className="text-right w-28">Line Total</TableHead>
                <TableHead className="w-10" />
              </TableRow>
            </TableHeader>
            <TableBody>
              {lineItems.map((item) => (
                <TableRow key={item._key}>
                  <TableCell>
                    <Input
                      placeholder="Description"
                      value={item.description}
                      onChange={(e) => updateLineItem(item._key, 'description', e.target.value)}
                    />
                  </TableCell>
                  <TableCell>
                    <Input
                      placeholder="وصف"
                      dir="rtl"
                      value={item.descriptionAr ?? ''}
                      onChange={(e) => updateLineItem(item._key, 'descriptionAr', e.target.value)}
                    />
                  </TableCell>
                  <TableCell>
                    <Input
                      placeholder="Code"
                      value={item.itemCode ?? ''}
                      onChange={(e) => updateLineItem(item._key, 'itemCode', e.target.value)}
                    />
                  </TableCell>
                  <TableCell>
                    <Input
                      type="number"
                      min="0.01"
                      step="0.01"
                      className="text-right"
                      value={item.quantity}
                      onChange={(e) => updateLineItem(item._key, 'quantity', parseFloat(e.target.value) || 0)}
                    />
                  </TableCell>
                  <TableCell>
                    <Input
                      type="number"
                      min="0"
                      step="0.01"
                      className="text-right"
                      value={item.unitPrice}
                      onChange={(e) => updateLineItem(item._key, 'unitPrice', parseFloat(e.target.value) || 0)}
                    />
                  </TableCell>
                  <TableCell>
                    <Input
                      type="number"
                      min="0"
                      step="0.01"
                      className="text-right"
                      value={item.discountAmount ?? 0}
                      onChange={(e) => updateLineItem(item._key, 'discountAmount', parseFloat(e.target.value) || 0)}
                    />
                  </TableCell>
                  <TableCell className="text-right font-medium tabular-nums">
                    {computeLineTotal(item).toFixed(2)}
                  </TableCell>
                  <TableCell>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-destructive hover:text-destructive"
                      onClick={() => removeLineItem(item._key)}
                      disabled={lineItems.length === 1}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* VAT Preview */}
      <Card>
        <CardHeader><CardTitle>Invoice Summary</CardTitle></CardHeader>
        <CardContent>
          <div className="space-y-2 max-w-xs ms-auto text-sm">
            <div className="flex justify-between">
              <span className="text-muted-foreground">Subtotal</span>
              <span className="tabular-nums font-medium">{subtotal.toFixed(2)} {currency}</span>
            </div>
            {applyVat && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">VAT (5%)</span>
                <span className="tabular-nums">{vatAmount.toFixed(2)} {currency}</span>
              </div>
            )}
            <div className="flex justify-between border-t pt-2 font-bold text-base">
              <span>Total</span>
              <span className="tabular-nums">{total.toFixed(2)} {currency}</span>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="flex justify-end gap-3">
        <Button variant="outline" onClick={() => navigate('/finance/invoices')}>
          Cancel
        </Button>
        <Button onClick={handleSubmit} disabled={!canSubmit || createMutation.isPending}>
          {createMutation.isPending ? 'Creating...' : 'Create Invoice'}
        </Button>
      </div>
    </div>
  );
}

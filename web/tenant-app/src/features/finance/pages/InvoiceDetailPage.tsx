import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  ArrowLeft,
  RefreshCw,
  Trash2,
  FileText,
  Tag,
  Plus,
  Download,
} from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';
import { Label } from '@/shared/components/ui/label';
import { Input } from '@/shared/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { usePermissions } from '@/features/auth/hooks/usePermissions';
import { PermissionGate } from '@/shared/components/PermissionGate';
import {
  useInvoice,
  useDeleteInvoice,
  useTransitionInvoiceStatus,
  useCreateCreditNote,
  useApplyDiscount,
  useDiscountPrograms,
} from '../hooks';
import { downloadInvoicePdf } from '../api';
import { InvoiceStatusBadge } from '../components/InvoiceStatusBadge';
import { PaymentStatusBadge } from '../components/PaymentStatusBadge';
import { PaymentMethodBadge } from '../components/PaymentMethodBadge';
import type { InvoiceStatus } from '../types';


const ALLOWED_TRANSITIONS: Partial<Record<InvoiceStatus, InvoiceStatus[]>> = {
  Draft:         ['Issued', 'Cancelled'],
  Issued:        ['PartiallyPaid', 'Paid', 'Overdue', 'Cancelled'],
  PartiallyPaid: ['Paid', 'Overdue', 'Cancelled'],
  Overdue:       ['Paid', 'Cancelled'],
  Paid:          ['Refunded'],
};

function InfoItem({ label, value }: { label: string; value?: string | number | null }) {
  return (
    <div>
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="font-medium">{value != null && value !== '' ? String(value) : '—'}</p>
    </div>
  );
}

function DetailSkeleton() {
  return (
    <div className="space-y-6">
      <div>
        <Skeleton className="h-4 w-32 mb-4" />
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Skeleton className="h-8 w-48" />
            <Skeleton className="h-6 w-20" />
          </div>
          <div className="flex items-center gap-2">
            <Skeleton className="h-9 w-32" />
            <Skeleton className="h-9 w-32" />
          </div>
        </div>
      </div>
      <div className="grid gap-6 md:grid-cols-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <Card key={i}>
            <CardHeader><Skeleton className="h-6 w-32" /></CardHeader>
            <CardContent className="grid gap-4 sm:grid-cols-2">
              {Array.from({ length: 4 }).map((_, j) => (
                <div key={j}>
                  <Skeleton className="h-4 w-20 mb-1" />
                  <Skeleton className="h-5 w-36" />
                </div>
              ))}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}

export function InvoiceDetailPage() {
  useTranslation('finance');
  const { invoiceId } = useParams<{ invoiceId: string }>();
  const navigate = useNavigate();
  const { hasPermission } = usePermissions();

  const { data: invoice, isLoading } = useInvoice(invoiceId!);
  const deleteMutation = useDeleteInvoice();
  const transitionMutation = useTransitionInvoiceStatus();
  const creditNoteMutation = useCreateCreditNote();
  const applyDiscountMutation = useApplyDiscount();

  const { data: discountPrograms } = useDiscountPrograms({ pageSize: 100 });

  const [showTransition, setShowTransition] = useState(false);
  const [transitionStatus, setTransitionStatus] = useState('');
  const [transitionReason, setTransitionReason] = useState('');

  const [showCreditNote, setShowCreditNote] = useState(false);
  const [creditReason, setCreditReason] = useState('');
  const [creditAmount, setCreditAmount] = useState('');

  const [showDiscount, setShowDiscount] = useState(false);
  const [discountProgramId, setDiscountProgramId] = useState('');
  const [discountCardNumber, setDiscountCardNumber] = useState('');

  const [showDelete, setShowDelete] = useState(false);
  const [downloading, setDownloading] = useState(false);

  const availableTransitions = invoice ? (ALLOWED_TRANSITIONS[invoice.status] ?? []) : [];

  const handleTransition = async () => {
    if (!invoiceId || !transitionStatus) return;
    await transitionMutation.mutateAsync({
      id: invoiceId,
      data: { status: transitionStatus, reason: transitionReason || undefined },
    });
    setShowTransition(false);
    setTransitionStatus('');
    setTransitionReason('');
  };

  const handleCreditNote = async () => {
    if (!invoiceId || !creditReason) return;
    await creditNoteMutation.mutateAsync({
      id: invoiceId,
      data: {
        reason: creditReason,
        amount: creditAmount ? parseFloat(creditAmount) : undefined,
      },
    });
    setShowCreditNote(false);
    setCreditReason('');
    setCreditAmount('');
  };

  const handleApplyDiscount = async () => {
    if (!invoiceId || !discountProgramId) return;
    await applyDiscountMutation.mutateAsync({
      id: invoiceId,
      data: {
        discountProgramId,
        cardNumber: discountCardNumber || undefined,
      },
    });
    setShowDiscount(false);
    setDiscountProgramId('');
    setDiscountCardNumber('');
  };

  const handleDownloadPdf = async () => {
    if (!invoiceId) return;
    setDownloading(true);
    try {
      const blob = await downloadInvoicePdf(invoiceId);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Invoice-${invoice?.invoiceNumber ?? invoiceId}.pdf`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch {
      // silently fail
    } finally {
      setDownloading(false);
    }
  };

  const handleDelete = async () => {
    if (!invoiceId) return;
    await deleteMutation.mutateAsync(invoiceId);
    navigate('/finance/invoices');
  };

  if (isLoading) return <DetailSkeleton />;

  if (!invoice) {
    return (
      <div className="flex items-center justify-center min-h-[40vh]">
        <p className="text-muted-foreground">Invoice not found.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <Link
          to="/finance/invoices"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-4"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Invoices
        </Link>
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="flex items-center gap-3 flex-wrap">
            <div>
              <h1 className="text-2xl font-bold tracking-tight font-mono">{invoice.invoiceNumber}</h1>
              <p className="text-sm text-muted-foreground">
                {invoice.type === 'CreditNote' ? 'Credit Note' : invoice.type === 'ProformaDeposit' ? 'Proforma Deposit' : 'Standard Invoice'}
              </p>
            </div>
            <InvoiceStatusBadge status={invoice.status} />
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <Button variant="outline" onClick={handleDownloadPdf} disabled={downloading}>
              <Download className="me-2 h-4 w-4" />
              {downloading ? 'Downloading...' : 'Download PDF'}
            </Button>
            {availableTransitions.length > 0 && hasPermission('finance.invoices.manage_status') && (
              <Button variant="outline" onClick={() => setShowTransition(true)}>
                <RefreshCw className="me-2 h-4 w-4" />
                Change Status
              </Button>
            )}
            {invoice.status === 'Issued' || invoice.status === 'PartiallyPaid' ? (
              <Button variant="outline" onClick={() => setShowCreditNote(true)}>
                <FileText className="me-2 h-4 w-4" />
                Credit Note
              </Button>
            ) : null}
            {(invoice.status === 'Draft' || invoice.status === 'Issued') && hasPermission('finance.invoices.apply_discount') && (
              <Button variant="outline" onClick={() => setShowDiscount(true)}>
                <Tag className="me-2 h-4 w-4" />
                Apply Discount
              </Button>
            )}
            {hasPermission('finance.invoices.delete') && (
              <Button variant="destructive" onClick={() => setShowDelete(true)}>
                <Trash2 className="me-2 h-4 w-4" />
                Delete
              </Button>
            )}
          </div>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 sm:grid-cols-3">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Total Amount</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold tabular-nums">
              {invoice.totalAmount.toFixed(2)} {invoice.currency}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Paid Amount</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold tabular-nums text-green-600 dark:text-green-400">
              {invoice.paidAmount.toFixed(2)} {invoice.currency}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Balance Due</CardTitle>
          </CardHeader>
          <CardContent>
            <p className={`text-2xl font-bold tabular-nums ${invoice.balanceDue > 0 ? 'text-red-600 dark:text-red-400' : 'text-muted-foreground'}`}>
              {invoice.balanceDue.toFixed(2)} {invoice.currency}
            </p>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Invoice Details */}
        <Card>
          <CardHeader><CardTitle>Invoice Details</CardTitle></CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <InfoItem label="Invoice Number" value={invoice.invoiceNumber} />
            <InfoItem label="Type" value={invoice.type} />
            <InfoItem label="Issue Date" value={new Date(invoice.issueDate).toLocaleDateString()} />
            <InfoItem label="Due Date" value={new Date(invoice.dueDate).toLocaleDateString()} />
            <InfoItem label="Milestone Type" value={invoice.milestoneType} />
            <InfoItem label="Currency" value={invoice.currency} />
            {invoice.tenantTrn && <InfoItem label="Tenant TRN" value={invoice.tenantTrn} />}
            {invoice.clientTrn && <InfoItem label="Client TRN" value={invoice.clientTrn} />}
          </CardContent>
        </Card>

        {/* Financials */}
        <Card>
          <CardHeader><CardTitle>Financials</CardTitle></CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <InfoItem label="Subtotal" value={`${invoice.subtotal?.toFixed(2)} ${invoice.currency}`} />
            <InfoItem label="Discount" value={invoice.discountAmount ? `${invoice.discountAmount.toFixed(2)} ${invoice.currency}` : '—'} />
            <InfoItem label="Taxable Amount" value={`${invoice.taxableAmount?.toFixed(2)} ${invoice.currency}`} />
            <InfoItem label="VAT Rate" value={invoice.vatRate ? `${invoice.vatRate}%` : '—'} />
            <InfoItem label="VAT Amount" value={`${invoice.vatAmount?.toFixed(2)} ${invoice.currency}`} />
            <InfoItem label="Total Amount" value={`${invoice.totalAmount.toFixed(2)} ${invoice.currency}`} />
            {invoice.discountProgramName && (
              <InfoItem label="Discount Program" value={`${invoice.discountProgramName} (${invoice.discountPercentage}%)`} />
            )}
          </CardContent>
        </Card>

        {/* Notes */}
        {invoice.notes && (
          <Card className="md:col-span-2">
            <CardHeader><CardTitle>Notes</CardTitle></CardHeader>
            <CardContent>
              <p className="text-sm">{invoice.notes}</p>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Line Items */}
      {invoice.lineItems && invoice.lineItems.length > 0 && (
        <Card>
          <CardHeader><CardTitle>Line Items</CardTitle></CardHeader>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>#</TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead>Code</TableHead>
                  <TableHead className="text-right">Qty</TableHead>
                  <TableHead className="text-right">Unit Price</TableHead>
                  <TableHead className="text-right">Discount</TableHead>
                  <TableHead className="text-right">Line Total</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {invoice.lineItems.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell className="text-muted-foreground">{item.lineNumber}</TableCell>
                    <TableCell>
                      <div>
                        <p>{item.description}</p>
                        {item.descriptionAr && (
                          <p className="text-xs text-muted-foreground" dir="rtl">{item.descriptionAr}</p>
                        )}
                      </div>
                    </TableCell>
                    <TableCell className="font-mono text-xs text-muted-foreground">{item.itemCode ?? '—'}</TableCell>
                    <TableCell className="text-right tabular-nums">{item.quantity}</TableCell>
                    <TableCell className="text-right tabular-nums">{item.unitPrice.toFixed(2)}</TableCell>
                    <TableCell className="text-right tabular-nums text-muted-foreground">
                      {item.discountAmount > 0 ? item.discountAmount.toFixed(2) : '—'}
                    </TableCell>
                    <TableCell className="text-right tabular-nums font-medium">{item.lineTotal.toFixed(2)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {/* Payment History */}
      {invoice.payments && invoice.payments.length > 0 && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Payment History</CardTitle>
              <PermissionGate permission="finance.payments.create">
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => navigate(`/finance/payments/new?invoiceId=${invoice.id}`)}
                >
                  <Plus className="me-2 h-4 w-4" />
                  Record Payment
                </Button>
              </PermissionGate>
            </div>
          </CardHeader>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Payment #</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Method</TableHead>
                  <TableHead>Reference</TableHead>
                  <TableHead>Date</TableHead>
                  <TableHead className="text-right">Amount</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {invoice.payments.map((payment) => (
                  <TableRow key={payment.id}>
                    <TableCell className="font-mono text-sm">{payment.paymentNumber}</TableCell>
                    <TableCell><PaymentStatusBadge status={payment.status} /></TableCell>
                    <TableCell><PaymentMethodBadge method={payment.method} /></TableCell>
                    <TableCell className="font-mono text-xs text-muted-foreground">
                      {payment.referenceNumber ?? '—'}
                    </TableCell>
                    <TableCell>{new Date(payment.paymentDate).toLocaleDateString()}</TableCell>
                    <TableCell className="text-right tabular-nums font-medium">
                      {payment.amount.toFixed(2)} {payment.currency}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {/* Status Transition Dialog */}
      <Dialog open={showTransition} onOpenChange={setShowTransition}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Change Invoice Status</DialogTitle>
            <DialogDescription>
              Current status: <InvoiceStatusBadge status={invoice.status} />
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>New Status *</Label>
              <Select value={transitionStatus} onValueChange={setTransitionStatus}>
                <SelectTrigger>
                  <SelectValue placeholder="Select new status" />
                </SelectTrigger>
                <SelectContent>
                  {availableTransitions.map((s) => (
                    <SelectItem key={s} value={s}>
                      {s === 'PartiallyPaid' ? 'Partially Paid' : s}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Reason</Label>
              <Input
                placeholder="Optional reason for status change"
                value={transitionReason}
                onChange={(e) => setTransitionReason(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowTransition(false)}>Cancel</Button>
            <Button
              onClick={handleTransition}
              disabled={!transitionStatus || transitionMutation.isPending}
            >
              {transitionMutation.isPending ? 'Updating...' : 'Update Status'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Credit Note Dialog */}
      <Dialog open={showCreditNote} onOpenChange={setShowCreditNote}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Credit Note</DialogTitle>
            <DialogDescription>
              Create a credit note for invoice {invoice.invoiceNumber}.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Reason *</Label>
              <Input
                placeholder="Reason for credit note"
                value={creditReason}
                onChange={(e) => setCreditReason(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>Amount (leave blank for full amount)</Label>
              <Input
                type="number"
                min="0"
                step="0.01"
                placeholder="Optional partial amount"
                value={creditAmount}
                onChange={(e) => setCreditAmount(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowCreditNote(false)}>Cancel</Button>
            <Button
              onClick={handleCreditNote}
              disabled={!creditReason || creditNoteMutation.isPending}
            >
              {creditNoteMutation.isPending ? 'Creating...' : 'Create Credit Note'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Apply Discount Dialog */}
      <Dialog open={showDiscount} onOpenChange={setShowDiscount}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Apply Discount</DialogTitle>
            <DialogDescription>
              Apply a discount program to invoice {invoice.invoiceNumber}.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Discount Program *</Label>
              <Select value={discountProgramId} onValueChange={setDiscountProgramId}>
                <SelectTrigger>
                  <SelectValue placeholder="Select discount program" />
                </SelectTrigger>
                <SelectContent>
                  {discountPrograms?.items?.map((dp) => (
                    <SelectItem key={dp.id} value={dp.id}>
                      {dp.name} ({dp.discountPercentage}%)
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Card Number</Label>
              <Input
                placeholder="Optional card/membership number"
                value={discountCardNumber}
                onChange={(e) => setDiscountCardNumber(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowDiscount(false)}>Cancel</Button>
            <Button
              onClick={handleApplyDiscount}
              disabled={!discountProgramId || applyDiscountMutation.isPending}
            >
              {applyDiscountMutation.isPending ? 'Applying...' : 'Apply Discount'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation */}
      <AlertDialog open={showDelete} onOpenChange={setShowDelete}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Invoice</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete invoice{' '}
              <span className="font-mono font-medium">{invoice.invoiceNumber}</span>?
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

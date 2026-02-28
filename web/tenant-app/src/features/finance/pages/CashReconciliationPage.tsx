import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Lock, RefreshCw } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import { Skeleton } from '@/shared/components/ui/skeleton';
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
import { usePermissions } from '@/features/auth/hooks/usePermissions';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { useXReports, useGenerateXReport, useCloseXReport } from '../hooks';
import type { CashReconciliationListDto } from '../types';

export function CashReconciliationPage() {
  useTranslation('finance');
  const { hasPermission } = usePermissions();

  const [page, setPage] = useState(1);
  const pageSize = 20;

  const queryParams = useMemo(() => ({ page, pageSize, sort: '-reportDate' }), [page, pageSize]);
  const { data, isLoading, refetch } = useXReports(queryParams);

  const generateMutation = useGenerateXReport();
  const closeMutation = useCloseXReport();

  const [showGenerate, setShowGenerate] = useState(false);
  const [generateDate, setGenerateDate] = useState(() => new Date().toISOString().slice(0, 10));

  const [closeTarget, setCloseTarget] = useState<CashReconciliationListDto | null>(null);

  const handleGenerate = async () => {
    await generateMutation.mutateAsync(generateDate || undefined);
    setShowGenerate(false);
    setGenerateDate(new Date().toISOString().slice(0, 10));
  };

  const handleClose = async () => {
    if (!closeTarget) return;
    await closeMutation.mutateAsync(closeTarget.id);
    setCloseTarget(null);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Cash Reconciliation</h1>
          <p className="text-muted-foreground">Generate and manage X-Reports for daily cash reconciliation</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="icon" onClick={() => refetch()}>
            <RefreshCw className="h-4 w-4" />
          </Button>
          <PermissionGate permission="finance.cash_reconciliation.create">
            <Button onClick={() => setShowGenerate(true)}>
              <Plus className="me-2 h-4 w-4" />
              Generate X-Report
            </Button>
          </PermissionGate>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>X-Reports ({data?.totalCount ?? 0})</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Report Date</TableHead>
                <TableHead>Cashier</TableHead>
                <TableHead className="text-right">Transactions</TableHead>
                <TableHead className="text-right">Grand Total</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Generated At</TableHead>
                {hasPermission('finance.cash_reconciliation.close') && (
                  <TableHead className="w-[80px]" />
                )}
              </TableRow>
            </TableHeader>
            <TableBody>
              {isLoading ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <TableRow key={i}>
                    {Array.from({ length: 7 }).map((_, j) => (
                      <TableCell key={j}><Skeleton className="h-5 w-full" /></TableCell>
                    ))}
                  </TableRow>
                ))
              ) : (data?.items ?? []).length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center py-12 text-muted-foreground">
                    No X-Reports found. Generate one to start reconciliation.
                  </TableCell>
                </TableRow>
              ) : (
                (data?.items ?? []).map((report) => (
                  <TableRow key={report.id}>
                    <TableCell className="font-mono">
                      {new Date(report.reportDate).toLocaleDateString()}
                    </TableCell>
                    <TableCell>{report.cashierName ?? '—'}</TableCell>
                    <TableCell className="text-right tabular-nums">
                      {report.transactionCount}
                    </TableCell>
                    <TableCell className="text-right font-bold tabular-nums">
                      {report.grandTotal.toLocaleString()} AED
                    </TableCell>
                    <TableCell>
                      <Badge variant={report.isClosed ? 'secondary' : 'success'}>
                        {report.isClosed ? 'Closed' : 'Open'}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-muted-foreground text-sm">
                      {new Date(report.createdAt).toLocaleString()}
                    </TableCell>
                    {hasPermission('finance.cash_reconciliation.close') && (
                      <TableCell>
                        {!report.isClosed && (
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => setCloseTarget(report)}
                            className="gap-1"
                          >
                            <Lock className="h-3 w-3" />
                            Close
                          </Button>
                        )}
                      </TableCell>
                    )}
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Pagination */}
      {(data?.totalPages ?? 1) > 1 && (
        <div className="flex items-center justify-end gap-2">
          <span className="text-sm text-muted-foreground">
            Page {page} of {data?.totalPages}
          </span>
          <Button variant="outline" size="sm" onClick={() => setPage((p) => p - 1)} disabled={page <= 1}>
            Previous
          </Button>
          <Button variant="outline" size="sm" onClick={() => setPage((p) => p + 1)} disabled={page >= (data?.totalPages ?? 1)}>
            Next
          </Button>
        </div>
      )}

      {/* Generate Dialog */}
      <Dialog open={showGenerate} onOpenChange={setShowGenerate}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Generate X-Report</DialogTitle>
            <DialogDescription>
              Generate a cash reconciliation report (X-Report) for the selected date.
              This will summarize all cash transactions for that day.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Report Date</Label>
              <Input
                type="date"
                value={generateDate}
                onChange={(e) => setGenerateDate(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowGenerate(false)}>Cancel</Button>
            <Button onClick={handleGenerate} disabled={generateMutation.isPending}>
              {generateMutation.isPending ? 'Generating...' : 'Generate Report'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Close Confirmation */}
      <AlertDialog open={!!closeTarget} onOpenChange={() => setCloseTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Close X-Report</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to close the X-Report for{' '}
              <span className="font-medium">
                {closeTarget ? new Date(closeTarget.reportDate).toLocaleDateString() : ''}
              </span>?
              Once closed, this report cannot be reopened.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleClose}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Close Report
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

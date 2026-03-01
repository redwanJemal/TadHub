import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, RefreshCw, Trash2, Download, FileText } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Badge } from '@/shared/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/components/ui/tabs';
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
import { useContract, useDeleteContract } from '../hooks';
import { downloadContractPdf } from '../api';
import { ContractStatusBadge } from '../components/ContractStatusBadge';
import { ContractTypeBadge } from '../components/ContractTypeBadge';
import { ContractStatusTransitionDialog } from '../components/ContractStatusTransitionDialog';
import { ALLOWED_TRANSITIONS } from '../constants';

function InfoItem({ label, value }: { label: string; value?: string | number | boolean | null }) {
  const display = typeof value === 'boolean'
    ? (value ? 'Yes' : 'No')
    : value;
  return (
    <div>
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="font-medium">{display != null && display !== '' ? String(display) : '—'}</p>
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
            <Skeleton className="h-8 w-64" />
            <Skeleton className="h-6 w-24" />
          </div>
          <div className="flex items-center gap-2">
            <Skeleton className="h-9 w-32" />
            <Skeleton className="h-9 w-32" />
          </div>
        </div>
      </div>
      <Skeleton className="h-10 w-96" />
      <div className="grid gap-6 md:grid-cols-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <Card key={i}>
            <CardHeader><Skeleton className="h-6 w-40" /></CardHeader>
            <CardContent className="grid gap-4 sm:grid-cols-2">
              {Array.from({ length: 4 }).map((_, j) => (
                <div key={j}>
                  <Skeleton className="h-4 w-24 mb-1" />
                  <Skeleton className="h-5 w-40" />
                </div>
              ))}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}

export function ContractDetailPage() {
  const { t } = useTranslation('contracts');
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: contract, isLoading } = useContract(id!);
  const deleteMutation = useDeleteContract();

  const [showTransition, setShowTransition] = useState(false);
  const [showDelete, setShowDelete] = useState(false);
  const [downloading, setDownloading] = useState(false);

  const handleDownloadPdf = async () => {
    if (!id) return;
    setDownloading(true);
    try {
      const blob = await downloadContractPdf(id);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Contract-${contract?.contractCode ?? id}.pdf`;
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
    if (!id) return;
    await deleteMutation.mutateAsync(id);
    navigate('/contracts');
  };

  if (isLoading) return <DetailSkeleton />;

  if (!contract) {
    return (
      <div className="flex items-center justify-center min-h-[40vh]">
        <p className="text-muted-foreground">{t('common:noResults')}</p>
      </div>
    );
  }

  const hasTransitions = (ALLOWED_TRANSITIONS[contract.status]?.length ?? 0) > 0;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <Link
          to="/contracts"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-4"
        >
          <ArrowLeft className="h-4 w-4" />
          {t('detail.backToList')}
        </Link>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div>
              <h1 className="text-2xl font-bold tracking-tight">{contract.contractCode}</h1>
              {contract.worker && (
                <p className="text-sm text-muted-foreground">
                  {contract.worker.fullNameEn} — {contract.client?.nameEn}
                </p>
              )}
            </div>
            <ContractStatusBadge status={contract.status} />
            <ContractTypeBadge type={contract.type} />
          </div>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              onClick={() => {
                const params = new URLSearchParams({
                  contractId: contract.id,
                  clientId: contract.clientId,
                  ...(contract.workerId ? { workerId: contract.workerId } : {}),
                  contractCode: contract.contractCode,
                  ...(contract.client?.nameEn ? { clientName: contract.client.nameEn } : {}),
                  ...(contract.worker?.fullNameEn ? { workerName: contract.worker.fullNameEn } : {}),
                });
                navigate(`/finance/invoices/new?${params.toString()}`);
              }}
            >
              <FileText className="me-2 h-4 w-4" />
              Create Invoice
            </Button>
            <Button variant="outline" onClick={handleDownloadPdf} disabled={downloading}>
              <Download className="me-2 h-4 w-4" />
              {downloading ? t('actions.downloading') : t('actions.downloadPdf')}
            </Button>
            {hasTransitions && (
              <Button variant="outline" onClick={() => setShowTransition(true)}>
                <RefreshCw className="me-2 h-4 w-4" />
                {t('actions.changeStatus')}
              </Button>
            )}
            <Button variant="destructive" onClick={() => setShowDelete(true)}>
              <Trash2 className="me-2 h-4 w-4" />
              {t('actions.delete')}
            </Button>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="overview" className="space-y-6">
        <TabsList>
          <TabsTrigger value="overview">{t('tabs.overview')}</TabsTrigger>
          <TabsTrigger value="statusHistory">
            {t('tabs.statusHistory')}
            {contract.statusHistory && (
              <Badge variant="secondary" className="ms-2">
                {contract.statusHistory.length}
              </Badge>
            )}
          </TabsTrigger>
        </TabsList>

        {/* Overview Tab */}
        <TabsContent value="overview" className="space-y-6">
          <div className="grid gap-6 md:grid-cols-2">
            <Card>
              <CardHeader><CardTitle>{t('detail.parties')}</CardTitle></CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <InfoItem label={t('detail.contractCode')} value={contract.contractCode} />
                <InfoItem label={t('detail.type')} value={t(`type.${contract.type}`)} />
                <InfoItem label={t('detail.worker')} value={contract.worker ? `${contract.worker.fullNameEn} (${contract.worker.workerCode})` : undefined} />
                <InfoItem label={t('detail.client')} value={contract.client?.nameEn} />
              </CardContent>
            </Card>

            <Card>
              <CardHeader><CardTitle>{t('detail.dates')}</CardTitle></CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <InfoItem label={t('detail.startDate')} value={contract.startDate ? new Date(contract.startDate).toLocaleDateString() : undefined} />
                <InfoItem label={t('detail.endDate')} value={contract.endDate ? new Date(contract.endDate).toLocaleDateString() : undefined} />
                <InfoItem label={t('detail.probationEndDate')} value={contract.probationEndDate ? new Date(contract.probationEndDate).toLocaleDateString() : undefined} />
                <InfoItem label={t('detail.guaranteeEndDate')} value={contract.guaranteeEndDate ? new Date(contract.guaranteeEndDate).toLocaleDateString() : undefined} />
                <InfoItem label={t('detail.probationPassed')} value={contract.probationPassed} />
              </CardContent>
            </Card>

            <Card>
              <CardHeader><CardTitle>{t('detail.financial')}</CardTitle></CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <InfoItem label={t('detail.rate')} value={`${contract.rate.toLocaleString()} ${contract.currency}`} />
                <InfoItem label={t('detail.ratePeriod')} value={t(`ratePeriod.${contract.ratePeriod}`)} />
                <InfoItem label={t('detail.totalValue')} value={contract.totalValue ? `${contract.totalValue.toLocaleString()} ${contract.currency}` : undefined} />
              </CardContent>
            </Card>

            {contract.terminatedAt && (
              <Card>
                <CardHeader><CardTitle>{t('detail.termination')}</CardTitle></CardHeader>
                <CardContent className="grid gap-4 sm:grid-cols-2">
                  <InfoItem label={t('detail.terminatedAt')} value={new Date(contract.terminatedAt).toLocaleString()} />
                  <InfoItem label={t('detail.terminationReason')} value={contract.terminationReason} />
                  <InfoItem label={t('detail.terminatedBy')} value={contract.terminatedBy} />
                </CardContent>
              </Card>
            )}

            <Card>
              <CardHeader><CardTitle>{t('detail.operational')}</CardTitle></CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <InfoItem label={t('detail.createdAt')} value={new Date(contract.createdAt).toLocaleString()} />
                <InfoItem label={t('detail.updatedAt')} value={new Date(contract.updatedAt).toLocaleString()} />
                <div className="sm:col-span-2">
                  <InfoItem label={t('detail.notes')} value={contract.notes} />
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Status History Tab */}
        <TabsContent value="statusHistory" className="space-y-6">
          {contract.statusHistory && contract.statusHistory.length > 0 ? (
            <Card>
              <CardHeader><CardTitle>{t('detail.statusHistory')}</CardTitle></CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {contract.statusHistory.map((entry) => (
                    <div key={entry.id} className="flex items-start gap-3 border-s-2 border-muted ps-4 pb-4">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-1">
                          {entry.fromStatus && (
                            <>
                              <ContractStatusBadge status={entry.fromStatus} />
                              <span className="text-muted-foreground">→</span>
                            </>
                          )}
                          <ContractStatusBadge status={entry.toStatus} />
                        </div>
                        <p className="text-xs text-muted-foreground">
                          {new Date(entry.changedAt).toLocaleString()}
                        </p>
                        {entry.reason && (
                          <p className="text-sm mt-1">{entry.reason}</p>
                        )}
                        {entry.notes && (
                          <p className="text-sm text-muted-foreground mt-1">{entry.notes}</p>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          ) : (
            <div className="flex items-center justify-center min-h-[20vh]">
              <p className="text-muted-foreground">{t('detail.notAvailable')}</p>
            </div>
          )}
        </TabsContent>
      </Tabs>

      {/* Status Transition Dialog */}
      <ContractStatusTransitionDialog
        open={showTransition}
        onOpenChange={setShowTransition}
        contractId={contract.id}
        currentStatus={contract.status}
      />

      {/* Delete Confirmation */}
      <AlertDialog open={showDelete} onOpenChange={setShowDelete}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('deleteDialog.title')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('deleteDialog.description')}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {t('deleteDialog.confirm')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

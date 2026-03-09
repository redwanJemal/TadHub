import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';
import {
  ArrowLeft,
  Plane,
  MapPin,
  Truck,
  Building2,
  UserCheck,
  AlertTriangle,
  Trash2,
  Clock,
  Camera,
  User,
} from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Badge } from '@/shared/components/ui/badge';
import { Skeleton } from '@/shared/components/ui/skeleton';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
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
import { PermissionGate } from '@/shared/components/PermissionGate';
import {
  useArrival,
  useAssignDriver,
  useConfirmArrival,
  useConfirmPickup,
  useConfirmAccommodation,
  useConfirmCustomerPickup,
  useReportNoShow,
  useDeleteArrival,
} from '../hooks';
import { ArrivalStatusBadge } from '../components/ArrivalStatusBadge';
import { STATUS_CONFIG } from '../constants';
import type { ArrivalStatus } from '../types';

export function ArrivalDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { data: arrival, isLoading, isError } = useArrival(id!);

  const assignDriverMutation = useAssignDriver();
  const confirmArrivalMutation = useConfirmArrival();
  const confirmPickupMutation = useConfirmPickup();
  const confirmAccommodationMutation = useConfirmAccommodation();
  const confirmCustomerPickupMutation = useConfirmCustomerPickup();
  const reportNoShowMutation = useReportNoShow();
  const deleteMutation = useDeleteArrival();

  const [showAssignDriver, setShowAssignDriver] = useState(false);
  const [showConfirmArrival, setShowConfirmArrival] = useState(false);
  const [showConfirmPickup, setShowConfirmPickup] = useState(false);
  const [showConfirmAccommodation, setShowConfirmAccommodation] = useState(false);
  const [showConfirmCustomerPickup, setShowConfirmCustomerPickup] = useState(false);
  const [showReportNoShow, setShowReportNoShow] = useState(false);
  const [showDelete, setShowDelete] = useState(false);

  const [driverName, setDriverName] = useState('');
  const [notes, setNotes] = useState('');
  const [noShowReason, setNoShowReason] = useState('');
  const [accommodationConfirmedBy, setAccommodationConfirmedBy] = useState('');

  if (isLoading) return <DetailSkeleton />;
  if (isError || !arrival) {
    return (
      <div className="flex flex-col items-center justify-center gap-4 py-12">
        <AlertTriangle className="h-12 w-12 text-muted-foreground" />
        <p className="text-muted-foreground">{t('arrivals.notFound', 'Arrival not found')}</p>
        <Button variant="outline" onClick={() => navigate('/arrivals')}>
          {t('back', 'Back')}
        </Button>
      </div>
    );
  }

  const today = new Date().toISOString().split('T')[0];
  const isOverdue =
    (arrival.status === 'Scheduled' || arrival.status === 'InTransit') &&
    arrival.scheduledArrivalDate < today;

  const handleAssignDriver = () => {
    assignDriverMutation.mutate(
      { id: arrival.id, data: { driverId: crypto.randomUUID(), driverName } },
      {
        onSuccess: () => {
          toast.success(t('arrivals.driverAssigned', 'Driver assigned'));
          setShowAssignDriver(false);
          setDriverName('');
        },
        onError: () => toast.error(t('error', 'Error')),
      }
    );
  };

  const handleConfirmArrival = () => {
    confirmArrivalMutation.mutate(
      { id: arrival.id, data: { notes: notes || undefined } },
      {
        onSuccess: () => {
          toast.success(t('arrivals.arrivalConfirmed', 'Arrival confirmed'));
          setShowConfirmArrival(false);
          setNotes('');
        },
        onError: () => toast.error(t('error', 'Error')),
      }
    );
  };

  const handleConfirmPickup = () => {
    confirmPickupMutation.mutate(
      { id: arrival.id, data: { notes: notes || undefined } },
      {
        onSuccess: () => {
          toast.success(t('arrivals.pickupConfirmed', 'Pickup confirmed'));
          setShowConfirmPickup(false);
          setNotes('');
        },
        onError: () => toast.error(t('error', 'Error')),
      }
    );
  };

  const handleConfirmAccommodation = () => {
    confirmAccommodationMutation.mutate(
      {
        id: arrival.id,
        data: {
          confirmedBy: accommodationConfirmedBy || undefined,
          notes: notes || undefined,
        },
      },
      {
        onSuccess: () => {
          toast.success(t('arrivals.accommodationConfirmed', 'Accommodation confirmed'));
          setShowConfirmAccommodation(false);
          setNotes('');
          setAccommodationConfirmedBy('');
        },
        onError: () => toast.error(t('error', 'Error')),
      }
    );
  };

  const handleConfirmCustomerPickup = () => {
    confirmCustomerPickupMutation.mutate(
      { id: arrival.id, data: { notes: notes || undefined } },
      {
        onSuccess: () => {
          toast.success(t('arrivals.customerPickupConfirmed', 'Customer pickup confirmed'));
          setShowConfirmCustomerPickup(false);
          setNotes('');
        },
        onError: () => toast.error(t('error', 'Error')),
      }
    );
  };

  const handleReportNoShow = () => {
    reportNoShowMutation.mutate(
      { id: arrival.id, data: { reason: noShowReason || undefined, notes: notes || undefined } },
      {
        onSuccess: () => {
          toast.success(t('arrivals.noShowReported', 'No-show reported'));
          setShowReportNoShow(false);
          setNotes('');
          setNoShowReason('');
        },
        onError: () => toast.error(t('error', 'Error')),
      }
    );
  };

  const handleDelete = () => {
    deleteMutation.mutate(arrival.id, {
      onSuccess: () => {
        toast.success(t('arrivals.deleted', 'Arrival deleted'));
        navigate('/arrivals');
      },
      onError: () => toast.error(t('error', 'Error')),
    });
  };

  const statusSteps: ArrivalStatus[] = ['Scheduled', 'InTransit', 'Arrived', 'PickedUp', 'AtAccommodation'];
  const currentStepIndex = statusSteps.indexOf(arrival.status);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate('/arrivals')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('back', 'Back')}
        </Button>
      </div>

      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <h1 className="text-2xl font-bold">{arrival.arrivalCode}</h1>
          <ArrivalStatusBadge status={arrival.status} />
          {isOverdue && (
            <Badge variant="destructive">
              <AlertTriangle className="mr-1 h-3 w-3" />
              {t('arrivals.overdue', 'Overdue')}
            </Badge>
          )}
        </div>
        <div className="flex items-center gap-2">
          <PermissionGate permission="arrivals.manage">
            {(arrival.status === 'Scheduled' || arrival.status === 'InTransit') && (
              <Button variant="outline" onClick={() => setShowAssignDriver(true)}>
                <User className="mr-2 h-4 w-4" />
                {t('arrivals.assignDriver', 'Assign Driver')}
              </Button>
            )}
            {(arrival.status === 'Scheduled' || arrival.status === 'InTransit') && (
              <Button onClick={() => setShowConfirmArrival(true)}>
                <MapPin className="mr-2 h-4 w-4" />
                {t('arrivals.confirmArrival', 'Confirm Arrival')}
              </Button>
            )}
            {arrival.status === 'Arrived' && (
              <>
                <Button variant="outline" onClick={() => setShowConfirmCustomerPickup(true)}>
                  <UserCheck className="mr-2 h-4 w-4" />
                  {t('arrivals.customerPickup', 'Customer Pickup')}
                </Button>
                <Button onClick={() => setShowConfirmAccommodation(true)}>
                  <Building2 className="mr-2 h-4 w-4" />
                  {t('arrivals.confirmAccommodation', 'Confirm Accommodation')}
                </Button>
              </>
            )}
            {arrival.status === 'PickedUp' && (
              <Button onClick={() => setShowConfirmAccommodation(true)}>
                <Building2 className="mr-2 h-4 w-4" />
                {t('arrivals.confirmAccommodation', 'Confirm Accommodation')}
              </Button>
            )}
            {arrival.status !== 'AtAccommodation' &&
              arrival.status !== 'NoShow' &&
              arrival.status !== 'Cancelled' && (
                <Button variant="destructive" onClick={() => setShowReportNoShow(true)}>
                  <AlertTriangle className="mr-2 h-4 w-4" />
                  {t('arrivals.reportNoShow', 'Report No-Show')}
                </Button>
              )}
          </PermissionGate>
          <PermissionGate permission="arrivals.driver_actions">
            {arrival.status === 'Arrived' && (
              <Button onClick={() => setShowConfirmPickup(true)}>
                <Truck className="mr-2 h-4 w-4" />
                {t('arrivals.confirmPickup', 'Confirm Pickup')}
              </Button>
            )}
          </PermissionGate>
          <PermissionGate permission="arrivals.delete">
            <Button variant="ghost" size="sm" onClick={() => setShowDelete(true)}>
              <Trash2 className="h-4 w-4 text-destructive" />
            </Button>
          </PermissionGate>
        </div>
      </div>

      {/* Status Stepper */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            {statusSteps.map((step, i) => {
              const config = STATUS_CONFIG[step];
              const Icon = config.icon;
              const isActive = i <= currentStepIndex && currentStepIndex >= 0;
              const isCurrent = step === arrival.status;
              return (
                <div key={step} className="flex flex-1 items-center">
                  <div className="flex flex-col items-center gap-1">
                    <div
                      className={`flex h-10 w-10 items-center justify-center rounded-full border-2 ${
                        isCurrent
                          ? 'border-primary bg-primary text-primary-foreground'
                          : isActive
                          ? 'border-primary/50 bg-primary/10 text-primary'
                          : 'border-muted bg-muted text-muted-foreground'
                      }`}
                    >
                      <Icon className="h-5 w-5" />
                    </div>
                    <span className={`text-xs ${isCurrent ? 'font-medium' : 'text-muted-foreground'}`}>
                      {config.shortLabel}
                    </span>
                  </div>
                  {i < statusSteps.length - 1 && (
                    <div
                      className={`mx-2 h-0.5 flex-1 ${
                        i < currentStepIndex ? 'bg-primary' : 'bg-muted'
                      }`}
                    />
                  )}
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Flight Info */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Plane className="h-5 w-5" />
              {t('arrivals.flightInfo', 'Flight Information')}
            </CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <InfoItem label={t('arrivals.flight', 'Flight')} value={arrival.flightNumber} />
            <InfoItem label={t('arrivals.airport', 'Airport')} value={arrival.airportName || arrival.airportCode} />
            <InfoItem label={t('arrivals.scheduledDate', 'Scheduled Date')} value={arrival.scheduledArrivalDate} />
            <InfoItem label={t('arrivals.scheduledTime', 'Scheduled Time')} value={arrival.scheduledArrivalTime} />
            <InfoItem
              label={t('arrivals.actualArrival', 'Actual Arrival')}
              value={arrival.actualArrivalTime ? new Date(arrival.actualArrivalTime).toLocaleString() : undefined}
            />
          </CardContent>
        </Card>

        {/* Driver & Pickup */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Truck className="h-5 w-5" />
              {t('arrivals.driverPickup', 'Driver & Pickup')}
            </CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <InfoItem label={t('arrivals.driver', 'Driver')} value={arrival.driverName} />
            <InfoItem
              label={t('arrivals.pickupConfirmedAt', 'Pickup Confirmed')}
              value={arrival.driverConfirmedPickupAt ? new Date(arrival.driverConfirmedPickupAt).toLocaleString() : undefined}
            />
            <InfoItem
              label={t('arrivals.customerPickedUp', 'Customer Picked Up')}
              value={arrival.customerPickedUp ? t('yes', 'Yes') : t('no', 'No')}
            />
            {arrival.customerPickupConfirmedAt && (
              <InfoItem
                label={t('arrivals.customerPickupAt', 'Customer Pickup At')}
                value={new Date(arrival.customerPickupConfirmedAt).toLocaleString()}
              />
            )}
          </CardContent>
        </Card>

        {/* Worker */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <UserCheck className="h-5 w-5" />
              {t('arrivals.workerInfo', 'Worker Information')}
            </CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <InfoItem label={t('arrivals.workerName', 'Worker Name')} value={arrival.worker?.fullNameEn} />
            <InfoItem label={t('arrivals.workerCode', 'Worker Code')} value={arrival.worker?.workerCode} />
            <InfoItem
              label={t('arrivals.placement', 'Placement')}
              value={
                <button
                  onClick={() => navigate(`/placements/${arrival.placementId}`)}
                  className="text-primary hover:underline"
                >
                  {t('arrivals.viewPlacement', 'View Placement')}
                </button>
              }
            />
          </CardContent>
        </Card>

        {/* Accommodation */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              {t('arrivals.accommodation', 'Accommodation')}
            </CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <InfoItem
              label={t('arrivals.confirmedAt', 'Confirmed At')}
              value={arrival.accommodationConfirmedAt ? new Date(arrival.accommodationConfirmedAt).toLocaleString() : undefined}
            />
            <InfoItem
              label={t('arrivals.confirmedBy', 'Confirmed By')}
              value={arrival.accommodationConfirmedBy}
            />
          </CardContent>
        </Card>

        {/* Photos */}
        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Camera className="h-5 w-5" />
              {t('arrivals.photos', 'Photos')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-3 gap-4">
              <div>
                <p className="mb-2 text-sm font-medium text-muted-foreground">
                  {t('arrivals.preTravelPhoto', 'Pre-Travel Photo')}
                </p>
                {arrival.preTravelPhotoUrl ? (
                  <img
                    src={arrival.preTravelPhotoUrl}
                    alt="Pre-travel"
                    className="h-48 w-full rounded-lg border object-cover"
                  />
                ) : (
                  <div className="flex h-48 items-center justify-center rounded-lg border bg-muted">
                    <Camera className="h-8 w-8 text-muted-foreground" />
                  </div>
                )}
              </div>
              <div>
                <p className="mb-2 text-sm font-medium text-muted-foreground">
                  {t('arrivals.arrivalPhoto', 'Arrival Photo')}
                </p>
                {arrival.arrivalPhotoUrl ? (
                  <img
                    src={arrival.arrivalPhotoUrl}
                    alt="Arrival"
                    className="h-48 w-full rounded-lg border object-cover"
                  />
                ) : (
                  <div className="flex h-48 items-center justify-center rounded-lg border bg-muted">
                    <Camera className="h-8 w-8 text-muted-foreground" />
                  </div>
                )}
              </div>
              <div>
                <p className="mb-2 text-sm font-medium text-muted-foreground">
                  {t('arrivals.pickupPhoto', 'Pickup Photo')}
                </p>
                {arrival.driverPickupPhotoUrl ? (
                  <img
                    src={arrival.driverPickupPhotoUrl}
                    alt="Pickup"
                    className="h-48 w-full rounded-lg border object-cover"
                  />
                ) : (
                  <div className="flex h-48 items-center justify-center rounded-lg border bg-muted">
                    <Camera className="h-8 w-8 text-muted-foreground" />
                  </div>
                )}
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Notes */}
        {arrival.notes && (
          <Card className="md:col-span-2">
            <CardHeader>
              <CardTitle>{t('arrivals.notes', 'Notes')}</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground whitespace-pre-wrap">{arrival.notes}</p>
            </CardContent>
          </Card>
        )}

        {/* Status History */}
        {arrival.statusHistory && arrival.statusHistory.length > 0 && (
          <Card className="md:col-span-2">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Clock className="h-5 w-5" />
                {t('arrivals.statusHistory', 'Status History')}
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {arrival.statusHistory.map((entry) => (
                  <div key={entry.id} className="flex items-start gap-4 border-b pb-4 last:border-b-0">
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted">
                      <Clock className="h-4 w-4 text-muted-foreground" />
                    </div>
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        {entry.fromStatus && (
                          <>
                            <Badge variant="outline">{entry.fromStatus}</Badge>
                            <span className="text-muted-foreground">→</span>
                          </>
                        )}
                        <Badge>{entry.toStatus}</Badge>
                      </div>
                      {entry.reason && (
                        <p className="mt-1 text-sm text-muted-foreground">
                          {t('arrivals.reason', 'Reason')}: {entry.reason}
                        </p>
                      )}
                      {entry.notes && (
                        <p className="mt-1 text-sm text-muted-foreground">{entry.notes}</p>
                      )}
                      <p className="mt-1 text-xs text-muted-foreground">
                        {new Date(entry.changedAt).toLocaleString()}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Assign Driver Dialog */}
      <Dialog open={showAssignDriver} onOpenChange={setShowAssignDriver}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t('arrivals.assignDriver', 'Assign Driver')}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium">{t('arrivals.driverName', 'Driver Name')}</label>
              <input
                type="text"
                value={driverName}
                onChange={(e) => setDriverName(e.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                placeholder={t('arrivals.driverNamePlaceholder', 'Enter driver name')}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowAssignDriver(false)}>
              {t('cancel', 'Cancel')}
            </Button>
            <Button onClick={handleAssignDriver} disabled={!driverName.trim() || assignDriverMutation.isPending}>
              {t('confirm', 'Confirm')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Confirm Arrival Dialog */}
      <ActionDialog
        open={showConfirmArrival}
        onOpenChange={setShowConfirmArrival}
        title={t('arrivals.confirmArrival', 'Confirm Arrival')}
        notes={notes}
        onNotesChange={setNotes}
        onConfirm={handleConfirmArrival}
        isPending={confirmArrivalMutation.isPending}
      />

      {/* Confirm Pickup Dialog */}
      <ActionDialog
        open={showConfirmPickup}
        onOpenChange={setShowConfirmPickup}
        title={t('arrivals.confirmPickup', 'Confirm Pickup')}
        notes={notes}
        onNotesChange={setNotes}
        onConfirm={handleConfirmPickup}
        isPending={confirmPickupMutation.isPending}
      />

      {/* Confirm Accommodation Dialog */}
      <Dialog open={showConfirmAccommodation} onOpenChange={setShowConfirmAccommodation}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t('arrivals.confirmAccommodation', 'Confirm Accommodation')}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium">{t('arrivals.confirmedBy', 'Confirmed By')}</label>
              <input
                type="text"
                value={accommodationConfirmedBy}
                onChange={(e) => setAccommodationConfirmedBy(e.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
              />
            </div>
            <div>
              <label className="text-sm font-medium">{t('arrivals.notes', 'Notes')}</label>
              <textarea
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                rows={3}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowConfirmAccommodation(false)}>
              {t('cancel', 'Cancel')}
            </Button>
            <Button onClick={handleConfirmAccommodation} disabled={confirmAccommodationMutation.isPending}>
              {t('confirm', 'Confirm')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Customer Pickup Dialog */}
      <ActionDialog
        open={showConfirmCustomerPickup}
        onOpenChange={setShowConfirmCustomerPickup}
        title={t('arrivals.customerPickup', 'Customer Pickup')}
        notes={notes}
        onNotesChange={setNotes}
        onConfirm={handleConfirmCustomerPickup}
        isPending={confirmCustomerPickupMutation.isPending}
      />

      {/* Report No-Show Dialog */}
      <Dialog open={showReportNoShow} onOpenChange={setShowReportNoShow}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t('arrivals.reportNoShow', 'Report No-Show')}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium">{t('arrivals.reason', 'Reason')}</label>
              <textarea
                value={noShowReason}
                onChange={(e) => setNoShowReason(e.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                rows={2}
              />
            </div>
            <div>
              <label className="text-sm font-medium">{t('arrivals.notes', 'Notes')}</label>
              <textarea
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                rows={3}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowReportNoShow(false)}>
              {t('cancel', 'Cancel')}
            </Button>
            <Button variant="destructive" onClick={handleReportNoShow} disabled={reportNoShowMutation.isPending}>
              {t('arrivals.reportNoShow', 'Report No-Show')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation */}
      <AlertDialog open={showDelete} onOpenChange={setShowDelete}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('arrivals.deleteTitle', 'Delete Arrival?')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('arrivals.deleteDescription', 'This action cannot be undone.')}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('cancel', 'Cancel')}</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete}>{t('delete', 'Delete')}</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

function InfoItem({ label, value }: { label: string; value?: string | React.ReactNode }) {
  return (
    <div className="space-y-1">
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="text-sm font-medium">{value || '-'}</p>
    </div>
  );
}

function ActionDialog({
  open,
  onOpenChange,
  title,
  notes,
  onNotesChange,
  onConfirm,
  isPending,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  notes: string;
  onNotesChange: (notes: string) => void;
  onConfirm: () => void;
  isPending: boolean;
}) {
  const { t } = useTranslation();
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>
        <div>
          <label className="text-sm font-medium">{t('arrivals.notes', 'Notes')}</label>
          <textarea
            value={notes}
            onChange={(e) => onNotesChange(e.target.value)}
            className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
            rows={3}
          />
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            {t('cancel', 'Cancel')}
          </Button>
          <Button onClick={onConfirm} disabled={isPending}>
            {t('confirm', 'Confirm')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function DetailSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-6 w-20" />
      </div>
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="flex flex-1 items-center">
                <Skeleton className="h-10 w-10 rounded-full" />
                {i < 4 && <Skeleton className="mx-2 h-0.5 flex-1" />}
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
      <div className="grid gap-6 md:grid-cols-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <Card key={i}>
            <CardHeader>
              <Skeleton className="h-5 w-32" />
            </CardHeader>
            <CardContent className="grid grid-cols-2 gap-4">
              {Array.from({ length: 4 }).map((_, j) => (
                <div key={j} className="space-y-1">
                  <Skeleton className="h-4 w-24" />
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

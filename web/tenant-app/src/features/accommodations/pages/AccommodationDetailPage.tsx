import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';
import {
  ArrowLeft,
  Building2,
  MapPin,
  User,
  Clock,
  LogOut as LogOutIcon,
  Trash2,
  AlertTriangle,
} from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
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
import { useAccommodation, useCheckOut, useDeleteStay } from '../hooks';
import { AccommodationStatusBadge } from '../components/AccommodationStatusBadge';
import { ALL_DEPARTURE_REASONS, DEPARTURE_REASON_CONFIG } from '../constants';
import type { DepartureReason } from '../types';

export function AccommodationDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { data: stay, isLoading, isError } = useAccommodation(id!);

  const checkOutMutation = useCheckOut();
  const deleteMutation = useDeleteStay();

  const [showCheckOut, setShowCheckOut] = useState(false);
  const [showDelete, setShowDelete] = useState(false);
  const [departureReason, setDepartureReason] = useState<string>('');
  const [departureNotes, setDepartureNotes] = useState('');

  if (isLoading) return <DetailSkeleton />;
  if (isError || !stay) {
    return (
      <div className="flex flex-col items-center justify-center gap-4 py-12">
        <AlertTriangle className="h-12 w-12 text-muted-foreground" />
        <p className="text-muted-foreground">{t('accommodations.notFound', 'Stay not found')}</p>
        <Button variant="outline" onClick={() => navigate('/accommodations')}>
          {t('back', 'Back')}
        </Button>
      </div>
    );
  }

  const handleCheckOut = () => {
    if (!departureReason) return;
    checkOutMutation.mutate(
      {
        id: stay.id,
        data: {
          departureReason,
          departureNotes: departureNotes || undefined,
        },
      },
      {
        onSuccess: () => {
          toast.success(t('accommodations.checkedOut', 'Checked out successfully'));
          setShowCheckOut(false);
          setDepartureReason('');
          setDepartureNotes('');
        },
        onError: () => toast.error(t('error', 'Error')),
      }
    );
  };

  const handleDelete = () => {
    deleteMutation.mutate(stay.id, {
      onSuccess: () => {
        toast.success(t('accommodations.deleted', 'Stay deleted'));
        navigate('/accommodations');
      },
      onError: () => toast.error(t('error', 'Error')),
    });
  };

  const daysStayed = stay.checkOutDate
    ? Math.ceil((new Date(stay.checkOutDate).getTime() - new Date(stay.checkInDate).getTime()) / (1000 * 60 * 60 * 24))
    : Math.ceil((Date.now() - new Date(stay.checkInDate).getTime()) / (1000 * 60 * 60 * 24));

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate('/accommodations')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('back', 'Back')}
        </Button>
      </div>

      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <h1 className="text-2xl font-bold">{stay.stayCode}</h1>
          <AccommodationStatusBadge status={stay.status} />
        </div>
        <div className="flex items-center gap-2">
          <PermissionGate permission="accommodations.manage">
            {stay.status === 'CheckedIn' && (
              <Button onClick={() => setShowCheckOut(true)}>
                <LogOutIcon className="mr-2 h-4 w-4" />
                {t('accommodations.checkOut', 'Check Out')}
              </Button>
            )}
            <Button variant="ghost" size="sm" onClick={() => setShowDelete(true)}>
              <Trash2 className="h-4 w-4 text-destructive" />
            </Button>
          </PermissionGate>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Worker Info */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <User className="h-5 w-5" />
              {t('accommodations.workerInfo', 'Worker Information')}
            </CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <InfoItem label={t('accommodations.workerName', 'Worker Name')} value={stay.worker?.fullNameEn} />
            <InfoItem label={t('accommodations.workerCode', 'Worker Code')} value={stay.worker?.workerCode} />
            {stay.placementId && (
              <InfoItem
                label={t('accommodations.placement', 'Placement')}
                value={
                  <button
                    onClick={() => navigate(`/placements/${stay.placementId}`)}
                    className="text-primary hover:underline"
                  >
                    {t('accommodations.viewPlacement', 'View Placement')}
                  </button>
                }
              />
            )}
            {stay.arrivalId && (
              <InfoItem
                label={t('accommodations.arrival', 'Arrival')}
                value={
                  <button
                    onClick={() => navigate(`/arrivals/${stay.arrivalId}`)}
                    className="text-primary hover:underline"
                  >
                    {t('accommodations.viewArrival', 'View Arrival')}
                  </button>
                }
              />
            )}
          </CardContent>
        </Card>

        {/* Stay Details */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              {t('accommodations.stayDetails', 'Stay Details')}
            </CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <InfoItem label={t('accommodations.room', 'Room')} value={stay.room} />
            <InfoItem label={t('accommodations.location', 'Location')} value={stay.location} />
            <InfoItem
              label={t('accommodations.daysStayed', 'Days Stayed')}
              value={`${daysStayed} ${t('accommodations.days', 'days')}`}
            />
            <InfoItem label={t('accommodations.checkedInBy', 'Checked In By')} value={stay.checkedInBy} />
          </CardContent>
        </Card>

        {/* Check-in/out Times */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Clock className="h-5 w-5" />
              {t('accommodations.timing', 'Timing')}
            </CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <InfoItem
              label={t('accommodations.checkInDate', 'Check-In Date')}
              value={new Date(stay.checkInDate).toLocaleString()}
            />
            <InfoItem
              label={t('accommodations.checkOutDate', 'Check-Out Date')}
              value={stay.checkOutDate ? new Date(stay.checkOutDate).toLocaleString() : undefined}
            />
            <InfoItem label={t('accommodations.createdAt', 'Created')} value={new Date(stay.createdAt).toLocaleString()} />
            <InfoItem label={t('accommodations.updatedAt', 'Updated')} value={new Date(stay.updatedAt).toLocaleString()} />
          </CardContent>
        </Card>

        {/* Departure Info (if checked out) */}
        {stay.status === 'CheckedOut' && (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <MapPin className="h-5 w-5" />
                {t('accommodations.departureInfo', 'Departure Information')}
              </CardTitle>
            </CardHeader>
            <CardContent className="grid grid-cols-2 gap-4">
              <InfoItem
                label={t('accommodations.departureReason', 'Departure Reason')}
                value={
                  stay.departureReason
                    ? DEPARTURE_REASON_CONFIG[stay.departureReason as DepartureReason]?.label || stay.departureReason
                    : undefined
                }
              />
              <InfoItem label={t('accommodations.checkedOutBy', 'Checked Out By')} value={stay.checkedOutBy} />
              {stay.departureNotes && (
                <div className="col-span-2 space-y-1">
                  <p className="text-sm text-muted-foreground">{t('accommodations.departureNotes', 'Departure Notes')}</p>
                  <p className="text-sm font-medium whitespace-pre-wrap">{stay.departureNotes}</p>
                </div>
              )}
            </CardContent>
          </Card>
        )}
      </div>

      {/* Check-Out Dialog */}
      <Dialog open={showCheckOut} onOpenChange={setShowCheckOut}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t('accommodations.checkOut', 'Check Out')}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium">
                {t('accommodations.departureReason', 'Departure Reason')} *
              </label>
              <select
                value={departureReason}
                onChange={(e) => setDepartureReason(e.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
              >
                <option value="">{t('accommodations.selectReason', 'Select a reason...')}</option>
                {ALL_DEPARTURE_REASONS.map((reason) => (
                  <option key={reason} value={reason}>
                    {DEPARTURE_REASON_CONFIG[reason].label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="text-sm font-medium">{t('accommodations.departureNotes', 'Departure Notes')}</label>
              <textarea
                value={departureNotes}
                onChange={(e) => setDepartureNotes(e.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
                rows={3}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowCheckOut(false)}>
              {t('cancel', 'Cancel')}
            </Button>
            <Button onClick={handleCheckOut} disabled={!departureReason || checkOutMutation.isPending}>
              {t('accommodations.checkOut', 'Check Out')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation */}
      <AlertDialog open={showDelete} onOpenChange={setShowDelete}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('accommodations.deleteTitle', 'Delete Stay?')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('accommodations.deleteDescription', 'This action cannot be undone.')}
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

function DetailSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-6 w-20" />
      </div>
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

import { useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';
import { Card, CardContent } from '@/shared/components/ui/card';
import { Badge } from '@/shared/components/ui/badge';
import { Button } from '@/shared/components/ui/button';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { ArrivalStatusBadge } from '../components/ArrivalStatusBadge';
import { useMyPickups, useConfirmPickup, useUploadPickupPhoto } from '../hooks';
import type { ArrivalListDto } from '../types';
import {
  Plane,
  MapPin,
  Clock,
  Camera,
  CheckCircle2,
  User,
  RefreshCw,
} from 'lucide-react';

export function DriverDashboardPage() {
  const { t, i18n } = useTranslation('driver');
  const isAr = i18n.language === 'ar';
  const { data, isLoading, refetch, isRefetching } = useMyPickups({
    sort: 'scheduledArrivalDate',
    pageSize: 50,
  });
  const confirmPickup = useConfirmPickup();
  const uploadPhoto = useUploadPickupPhoto();

  const pickups = data?.items ?? [];

  const activePickups = pickups.filter(
    (p) => p.status === 'Scheduled' || p.status === 'InTransit' || p.status === 'Arrived'
  );
  const completedPickups = pickups.filter(
    (p) => p.status === 'PickedUp' || p.status === 'AtAccommodation'
  );

  if (isLoading) {
    return <DashboardSkeleton />;
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t('title')}</h1>
          <p className="text-sm text-muted-foreground">{t('subtitle')}</p>
        </div>
        <Button
          variant="outline"
          size="icon"
          onClick={() => refetch()}
          disabled={isRefetching}
        >
          <RefreshCw className={`h-4 w-4 ${isRefetching ? 'animate-spin' : ''}`} />
        </Button>
      </div>

      {/* Summary */}
      <div className="grid grid-cols-2 gap-3">
        <Card>
          <CardContent className="p-4 text-center">
            <p className="text-3xl font-bold">{activePickups.length}</p>
            <p className="text-sm text-muted-foreground">{t('activePickups')}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <p className="text-3xl font-bold">{completedPickups.length}</p>
            <p className="text-sm text-muted-foreground">{t('completedPickups')}</p>
          </CardContent>
        </Card>
      </div>

      {/* Active pickups */}
      {activePickups.length > 0 && (
        <div className="space-y-3">
          <h2 className="text-lg font-semibold">{t('pendingPickups')}</h2>
          {activePickups.map((pickup) => (
            <PickupCard
              key={pickup.id}
              pickup={pickup}
              isAr={isAr}
              onConfirm={(id) => {
                confirmPickup.mutate(
                  { id, data: {} },
                  {
                    onSuccess: () => toast.success(t('pickupConfirmedSuccess')),
                    onError: () => toast.error(t('pickupConfirmedError')),
                  }
                );
              }}
              onUploadPhoto={(id, file) => {
                uploadPhoto.mutate(
                  { id, file },
                  {
                    onSuccess: () => toast.success(t('photoUploaded')),
                    onError: () => toast.error(t('photoUploadError')),
                  }
                );
              }}
              isConfirming={confirmPickup.isPending}
              isUploading={uploadPhoto.isPending}
            />
          ))}
        </div>
      )}

      {/* Completed pickups */}
      {completedPickups.length > 0 && (
        <div className="space-y-3">
          <h2 className="text-lg font-semibold">{t('completedPickups')}</h2>
          {completedPickups.map((pickup) => (
            <PickupCard
              key={pickup.id}
              pickup={pickup}
              isAr={isAr}
              isCompleted
            />
          ))}
        </div>
      )}

      {/* Empty state */}
      {pickups.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center">
            <Plane className="mx-auto h-12 w-12 text-muted-foreground/50" />
            <p className="mt-4 text-lg font-medium text-muted-foreground">
              {t('noPickups')}
            </p>
            <p className="mt-1 text-sm text-muted-foreground">
              {t('noPickupsDescription')}
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

interface PickupCardProps {
  pickup: ArrivalListDto;
  isAr: boolean;
  isCompleted?: boolean;
  onConfirm?: (id: string) => void;
  onUploadPhoto?: (id: string, file: File) => void;
  isConfirming?: boolean;
  isUploading?: boolean;
}

function PickupCard({
  pickup,
  isAr,
  isCompleted,
  onConfirm,
  onUploadPhoto,
  isConfirming,
  isUploading,
}: PickupCardProps) {
  const { t } = useTranslation('driver');
  const fileInputRef = useRef<HTMLInputElement>(null);

  const workerName = isAr
    ? pickup.worker?.fullNameAr || pickup.worker?.fullNameEn || '—'
    : pickup.worker?.fullNameEn || '—';

  const isArrived = pickup.status === 'Arrived';

  return (
    <Card className={isCompleted ? 'opacity-75' : ''}>
      <CardContent className="p-4 space-y-3">
        {/* Worker name and status */}
        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-2 min-w-0">
            {pickup.worker?.photoUrl ? (
              <img
                src={pickup.worker.photoUrl}
                alt=""
                className="h-10 w-10 rounded-full object-cover shrink-0"
              />
            ) : (
              <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-primary shrink-0">
                <User className="h-5 w-5" />
              </div>
            )}
            <div className="min-w-0">
              <p className="font-semibold truncate">{workerName}</p>
              <p className="text-xs text-muted-foreground">{pickup.arrivalCode}</p>
            </div>
          </div>
          <ArrivalStatusBadge status={pickup.status} />
        </div>

        {/* Flight and airport info */}
        <div className="grid grid-cols-2 gap-2 text-sm">
          {pickup.flightNumber && (
            <div className="flex items-center gap-1.5">
              <Plane className="h-4 w-4 text-muted-foreground shrink-0" />
              <span>{pickup.flightNumber}</span>
            </div>
          )}
          {pickup.airportCode && (
            <div className="flex items-center gap-1.5">
              <MapPin className="h-4 w-4 text-muted-foreground shrink-0" />
              <span>{pickup.airportCode}</span>
            </div>
          )}
          <div className="flex items-center gap-1.5 col-span-2">
            <Clock className="h-4 w-4 text-muted-foreground shrink-0" />
            <span>
              {pickup.scheduledArrivalDate}
              {pickup.scheduledArrivalTime && ` ${pickup.scheduledArrivalTime}`}
            </span>
          </div>
        </div>

        {/* Actions for non-completed pickups */}
        {!isCompleted && (
          <div className="flex gap-2 pt-1">
            {isArrived && onConfirm && (
              <Button
                className="flex-1 h-12 text-base"
                onClick={() => onConfirm(pickup.id)}
                disabled={isConfirming}
              >
                <CheckCircle2 className="mr-2 h-5 w-5" />
                {t('confirmPickup')}
              </Button>
            )}
            {isArrived && onUploadPhoto && (
              <>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/*"
                  capture="environment"
                  className="hidden"
                  onChange={(e) => {
                    const file = e.target.files?.[0];
                    if (file) {
                      onUploadPhoto(pickup.id, file);
                      e.target.value = '';
                    }
                  }}
                />
                <Button
                  variant="outline"
                  className="h-12 text-base"
                  onClick={() => fileInputRef.current?.click()}
                  disabled={isUploading}
                >
                  <Camera className="mr-2 h-5 w-5" />
                  {t('uploadPhoto')}
                </Button>
              </>
            )}
            {!isArrived && (
              <Badge variant="secondary" className="py-2 px-3">
                {t('waitingForArrival')}
              </Badge>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function DashboardSkeleton() {
  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <Skeleton className="h-8 w-48" />
          <Skeleton className="mt-1 h-4 w-64" />
        </div>
        <Skeleton className="h-10 w-10" />
      </div>
      <div className="grid grid-cols-2 gap-3">
        <Card>
          <CardContent className="p-4 text-center">
            <Skeleton className="mx-auto h-9 w-12" />
            <Skeleton className="mx-auto mt-1 h-4 w-24" />
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <Skeleton className="mx-auto h-9 w-12" />
            <Skeleton className="mx-auto mt-1 h-4 w-24" />
          </CardContent>
        </Card>
      </div>
      <div className="space-y-3">
        <Skeleton className="h-6 w-36" />
        {Array.from({ length: 3 }).map((_, i) => (
          <Card key={i}>
            <CardContent className="p-4 space-y-3">
              <div className="flex items-center gap-3">
                <Skeleton className="h-10 w-10 rounded-full" />
                <div className="flex-1">
                  <Skeleton className="h-5 w-32" />
                  <Skeleton className="mt-1 h-3 w-20" />
                </div>
                <Skeleton className="h-6 w-20" />
              </div>
              <div className="grid grid-cols-2 gap-2">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-4 w-20" />
                <Skeleton className="h-4 w-40 col-span-2" />
              </div>
              <Skeleton className="h-12 w-full" />
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}

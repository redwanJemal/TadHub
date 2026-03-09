export type ArrivalStatus =
  | 'Scheduled'
  | 'InTransit'
  | 'Arrived'
  | 'PickedUp'
  | 'AtAccommodation'
  | 'NoShow'
  | 'Cancelled';

export interface ArrivalWorkerRefDto {
  id: string;
  fullNameEn: string;
  fullNameAr?: string;
  workerCode?: string;
  photoUrl?: string;
}

export interface ArrivalStatusHistoryDto {
  id: string;
  arrivalId: string;
  fromStatus?: string;
  toStatus: string;
  changedAt: string;
  changedBy?: string;
  reason?: string;
  notes?: string;
}

export interface ArrivalDto {
  id: string;
  tenantId: string;
  arrivalCode: string;
  status: ArrivalStatus;
  statusChangedAt: string;
  workerId: string;
  placementId: string;
  supplierId?: string;
  worker?: ArrivalWorkerRefDto;
  flightNumber?: string;
  airportCode?: string;
  airportName?: string;
  scheduledArrivalDate: string;
  scheduledArrivalTime?: string;
  actualArrivalTime?: string;
  preTravelPhotoUrl?: string;
  arrivalPhotoUrl?: string;
  driverPickupPhotoUrl?: string;
  driverId?: string;
  driverName?: string;
  driverConfirmedPickupAt?: string;
  accommodationConfirmedAt?: string;
  accommodationConfirmedBy?: string;
  customerPickedUp: boolean;
  customerPickupConfirmedAt?: string;
  notes?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt: string;
  updatedAt: string;
  statusHistory?: ArrivalStatusHistoryDto[];
}

export interface ArrivalListDto {
  id: string;
  arrivalCode: string;
  status: ArrivalStatus;
  statusChangedAt: string;
  workerId: string;
  placementId: string;
  supplierId?: string;
  worker?: ArrivalWorkerRefDto;
  flightNumber?: string;
  airportCode?: string;
  scheduledArrivalDate: string;
  scheduledArrivalTime?: string;
  actualArrivalTime?: string;
  driverId?: string;
  driverName?: string;
  customerPickedUp: boolean;
  createdAt: string;
}

export interface ScheduleArrivalRequest {
  placementId: string;
  workerId: string;
  supplierId?: string;
  flightNumber?: string;
  airportCode?: string;
  airportName?: string;
  scheduledArrivalDate: string;
  scheduledArrivalTime?: string;
  notes?: string;
}

export interface UpdateArrivalRequest {
  flightNumber?: string;
  airportCode?: string;
  airportName?: string;
  scheduledArrivalDate?: string;
  scheduledArrivalTime?: string;
  notes?: string;
}

export interface AssignDriverRequest {
  driverId: string;
  driverName: string;
}

export interface ConfirmArrivalRequest {
  actualArrivalTime?: string;
  notes?: string;
}

export interface ConfirmPickupRequest {
  notes?: string;
}

export interface ConfirmAccommodationRequest {
  confirmedBy?: string;
  notes?: string;
}

export interface ConfirmCustomerPickupRequest {
  notes?: string;
}

export interface ReportNoShowRequest {
  reason?: string;
  notes?: string;
}

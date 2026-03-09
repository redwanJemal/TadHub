export type AccommodationStayStatus = 'CheckedIn' | 'CheckedOut';

export type DepartureReason =
  | 'DeployedToCustomer'
  | 'Runaway'
  | 'ReturnedToCountry'
  | 'Transferred'
  | 'MedicalReason'
  | 'Other';

export interface AccommodationWorkerRefDto {
  id: string;
  fullNameEn: string;
  fullNameAr?: string;
  workerCode?: string;
  photoUrl?: string;
}

export interface AccommodationStayDto {
  id: string;
  tenantId: string;
  stayCode: string;
  status: AccommodationStayStatus;
  statusChangedAt: string;
  workerId: string;
  placementId?: string;
  arrivalId?: string;
  checkInDate: string;
  checkOutDate?: string;
  room?: string;
  location?: string;
  departureReason?: DepartureReason;
  departureNotes?: string;
  checkedInBy: string;
  checkedOutBy?: string;
  worker?: AccommodationWorkerRefDto;
  createdBy?: string;
  updatedBy?: string;
  createdAt: string;
  updatedAt: string;
}

export interface AccommodationStayListDto {
  id: string;
  stayCode: string;
  status: AccommodationStayStatus;
  workerId: string;
  checkInDate: string;
  checkOutDate?: string;
  room?: string;
  location?: string;
  departureReason?: DepartureReason;
  checkedInBy: string;
  worker?: AccommodationWorkerRefDto;
  createdAt: string;
}

export interface CheckInRequest {
  workerId: string;
  placementId?: string;
  arrivalId?: string;
  room?: string;
  location?: string;
}

export interface CheckOutRequest {
  departureReason: string;
  departureNotes?: string;
}

export interface UpdateStayRequest {
  room?: string;
  location?: string;
}

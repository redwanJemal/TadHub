export interface SupplierUserDto {
  id: string;
  userId: string;
  supplierId: string;
  isActive: boolean;
  displayName?: string;
  email?: string;
  phone?: string;
  supplierNameEn?: string;
  supplierNameAr?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSupplierUserRequest {
  userId: string;
  supplierId: string;
  displayName?: string;
  email?: string;
  phone?: string;
}

export interface UpdateSupplierUserRequest {
  isActive?: boolean;
  displayName?: string;
  email?: string;
  phone?: string;
}

export interface SupplierDashboardDto {
  totalCandidates: number;
  pendingCandidates: number;
  approvedCandidates: number;
  rejectedCandidates: number;
  totalWorkers: number;
  activeWorkers: number;
  deployedWorkers: number;
  totalCommissions: number;
  pendingCommissions: number;
  paidCommissions: number;
}

export interface SupplierCandidateListDto {
  id: string;
  fullNameEn?: string;
  fullNameAr?: string;
  nationality?: string;
  status?: string;
  photoUrl?: string;
  passportNumber?: string;
  createdAt: string;
}

export interface SupplierWorkerListDto {
  id: string;
  workerCode?: string;
  fullNameEn?: string;
  fullNameAr?: string;
  nationality?: string;
  status?: string;
  photoUrl?: string;
  createdAt: string;
}

export interface SupplierCommissionDto {
  id: string;
  referenceNumber?: string;
  amount: number;
  currency?: string;
  status?: string;
  workerNameEn?: string;
  notes?: string;
  paymentDate?: string;
  createdAt: string;
}

export interface SupplierArrivalListDto {
  id: string;
  workerNameEn?: string;
  flightNumber?: string;
  arrivalDate?: string;
  status?: string;
  airportCode?: string;
  hasPreTravelPhoto: boolean;
  createdAt: string;
}

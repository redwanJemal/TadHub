export type PlacementStatus =
  | 'Booked'
  | 'InTrial'
  | 'TrialSuccessful'
  | 'ContractCreated'
  | 'StatusChanged'
  | 'EmploymentVisaProcessing'
  | 'TicketArranged'
  | 'InTransit'
  | 'Arrived'
  | 'MedicalInProgress'
  | 'MedicalCleared'
  | 'GovtProcessing'
  | 'GovtCleared'
  | 'Training'
  | 'ReadyForPlacement'
  | 'Deployed'
  | 'Placed'
  | 'FullPaymentReceived'
  | 'ResidenceVisaProcessing'
  | 'EmiratesIdProcessing'
  | 'Completed'
  | 'Cancelled';

export type PlacementFlowType = 'OutsideCountry' | 'InsideCountry';

export type PlacementCostType =
  | 'Procurement'
  | 'Flight'
  | 'Medical'
  | 'Visa'
  | 'EmiratesId'
  | 'Insurance'
  | 'Accommodation'
  | 'Training'
  | 'Other';

export type PlacementCostStatus = 'Pending' | 'Paid' | 'Cancelled';

export type ChecklistStepStatus = 'Pending' | 'InProgress' | 'Completed';

export interface PlacementCandidateDto {
  id: string;
  fullNameEn: string;
  fullNameAr?: string;
  nationality?: string;
  photoUrl?: string;
}

export interface PlacementClientDto {
  id: string;
  nameEn: string;
  nameAr?: string;
}

export interface PlacementWorkerDto {
  id: string;
  fullNameEn: string;
  fullNameAr?: string;
  workerCode: string;
}

export interface PlacementCostItemDto {
  id: string;
  placementId: string;
  costType: string;
  description: string;
  amount: number;
  currency: string;
  status: string;
  costDate?: string;
  paidAt?: string;
  referenceNumber?: string;
  notes?: string;
  createdAt: string;
}

export interface PlacementStatusHistoryDto {
  id: string;
  placementId: string;
  fromStatus?: string;
  toStatus: string;
  changedAt: string;
  changedBy?: string;
  reason?: string;
  notes?: string;
}

export interface PlacementChecklistStepDto {
  stepNumber: number;
  status: string;
  stepStatus: ChecklistStepStatus;
  label: string;
  description: string;
  completedAt?: string;
  actionLabel?: string;
  linkedEntityId?: string;
  linkedEntityType?: string;
}

export interface PlacementChecklistDto {
  steps: PlacementChecklistStepDto[];
  currentStepNumber: number;
  totalSteps: number;
  progressPercent: number;
  flowType: PlacementFlowType;
}

export interface PlacementDto {
  id: string;
  tenantId: string;
  placementCode: string;
  status: PlacementStatus;
  statusChangedAt: string;
  statusReason?: string;
  flowType: PlacementFlowType;
  candidateId: string;
  clientId: string;
  workerId?: string;
  contractId?: string;
  trialId?: string;
  employmentVisaApplicationId?: string;
  residenceVisaApplicationId?: string;
  emiratesIdApplicationId?: string;
  arrivalId?: string;
  candidate?: PlacementCandidateDto;
  client?: PlacementClientDto;
  worker?: PlacementWorkerDto;
  bookedBy?: string;
  bookedByName?: string;
  bookedAt: string;
  bookingNotes?: string;
  contractCreatedAt?: string;
  employmentVisaStartedAt?: string;
  ticketDate?: string;
  flightDetails?: string;
  expectedArrivalDate?: string;
  arrivedAt?: string;
  deployedAt?: string;
  fullPaymentReceivedAt?: string;
  residenceVisaStartedAt?: string;
  emiratesIdStartedAt?: string;
  trialStartedAt?: string;
  trialSucceededAt?: string;
  statusChangedStepAt?: string;
  medicalClearedAt?: string;
  govtClearedAt?: string;
  placedAt?: string;
  completedAt?: string;
  cancelledAt?: string;
  cancellationReason?: string;
  currency: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt: string;
  updatedAt: string;
  costItems?: PlacementCostItemDto[];
  statusHistory?: PlacementStatusHistoryDto[];
  totalCost?: number;
  checklist?: PlacementChecklistDto;
}

export interface PlacementListDto {
  id: string;
  placementCode: string;
  status: PlacementStatus;
  statusChangedAt: string;
  flowType: PlacementFlowType;
  candidateId: string;
  clientId: string;
  workerId?: string;
  contractId?: string;
  candidate?: PlacementCandidateDto;
  client?: PlacementClientDto;
  bookedAt: string;
  expectedArrivalDate?: string;
  totalCost: number;
  createdAt: string;
  currentStep: number;
  totalSteps: number;
}

export interface CreatePlacementRequest {
  candidateId: string;
  clientId: string;
  bookingNotes?: string;
  initialCostItems?: CreatePlacementCostItemRequest[];
}

export interface UpdatePlacementRequest {
  ticketDate?: string;
  flightDetails?: string;
  expectedArrivalDate?: string;
  bookingNotes?: string;
  contractId?: string;
  trialId?: string;
  employmentVisaApplicationId?: string;
  residenceVisaApplicationId?: string;
  emiratesIdApplicationId?: string;
  arrivalId?: string;
}

export interface TransitionPlacementStatusRequest {
  status: string;
  reason?: string;
  notes?: string;
}

export interface AdvancePlacementStepRequest {
  notes?: string;
}

export interface CreatePlacementCostItemRequest {
  costType: string;
  description: string;
  amount: number;
  currency?: string;
  status?: string;
  costDate?: string;
  referenceNumber?: string;
  notes?: string;
}

export interface UpdatePlacementCostItemRequest {
  costType?: string;
  description?: string;
  amount?: number;
  currency?: string;
  status?: string;
  costDate?: string;
  paidAt?: string;
  referenceNumber?: string;
  notes?: string;
}

export interface PlacementBoardDto {
  statusCounts: Record<string, number>;
  columns: Record<string, PlacementListDto[]>;
}

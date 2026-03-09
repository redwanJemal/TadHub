import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type {
  ScheduleArrivalRequest,
  UpdateArrivalRequest,
  AssignDriverRequest,
  ConfirmArrivalRequest,
  ConfirmPickupRequest,
  ConfirmAccommodationRequest,
  ConfirmCustomerPickupRequest,
  ReportNoShowRequest,
} from './types';

const ARRIVALS_KEY = 'arrivals';

export function useArrivals(params?: QueryParams) {
  return useQuery({
    queryKey: [ARRIVALS_KEY, params],
    queryFn: () => api.listArrivals(params),
  });
}

export function useArrival(id: string) {
  return useQuery({
    queryKey: [ARRIVALS_KEY, id],
    queryFn: () => api.getArrival(id, { include: 'statusHistory' }),
    enabled: !!id,
  });
}

export function useArrivalStatusHistory(id: string) {
  return useQuery({
    queryKey: [ARRIVALS_KEY, id, 'status-history'],
    queryFn: () => api.getArrivalStatusHistory(id),
    enabled: !!id,
  });
}

export function useScheduleArrival() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: ScheduleArrivalRequest) => api.scheduleArrival(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ARRIVALS_KEY] });
    },
  });
}

export function useUpdateArrival() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateArrivalRequest }) =>
      api.updateArrival(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ARRIVALS_KEY] });
    },
  });
}

export function useAssignDriver() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AssignDriverRequest }) =>
      api.assignDriver(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ARRIVALS_KEY] });
    },
  });
}

export function useConfirmArrival() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ConfirmArrivalRequest }) =>
      api.confirmArrival(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ARRIVALS_KEY] });
    },
  });
}

export function useConfirmPickup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ConfirmPickupRequest }) =>
      api.confirmPickup(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ARRIVALS_KEY] });
    },
  });
}

export function useConfirmAccommodation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ConfirmAccommodationRequest }) =>
      api.confirmAccommodation(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ARRIVALS_KEY] });
    },
  });
}

export function useConfirmCustomerPickup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ConfirmCustomerPickupRequest }) =>
      api.confirmCustomerPickup(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ARRIVALS_KEY] });
    },
  });
}

export function useReportNoShow() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ReportNoShowRequest }) =>
      api.reportNoShow(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ARRIVALS_KEY] });
    },
  });
}

export function useDeleteArrival() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteArrival(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ARRIVALS_KEY] });
    },
  });
}

// Driver-scoped hooks
const MY_PICKUPS_KEY = 'my-pickups';

export function useMyPickups(params?: QueryParams) {
  return useQuery({
    queryKey: [MY_PICKUPS_KEY, params],
    queryFn: () => api.listMyPickups(params),
  });
}

export function useUploadPickupPhoto() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) =>
      api.uploadPickupPhoto(id, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ARRIVALS_KEY] });
      queryClient.invalidateQueries({ queryKey: [MY_PICKUPS_KEY] });
    },
  });
}

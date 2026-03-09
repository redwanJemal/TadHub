import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ArrowLeft, ChevronRight, Plus, Trash2, CheckCircle2, Circle, Loader2, ExternalLink } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Badge } from '@/shared/components/ui/badge';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
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
  AlertDialogTrigger,
} from '@/shared/components/ui/alert-dialog';
import { PermissionGate } from '@/shared/components/PermissionGate';
import { PlacementStatusBadge } from '../components/PlacementStatusBadge';
import { PlacementTransitionDialog } from '../components/PlacementTransitionDialog';
import {
  usePlacement,
  useTransitionPlacementStatus,
  useAdvancePlacementStep,
  useAddPlacementCostItem,
  useDeletePlacementCostItem,
  useDeletePlacement,
} from '../hooks';
import { STATUS_CONFIG, PIPELINE_STATUSES, COST_TYPES, ALLOWED_TRANSITIONS } from '../constants';
import type { PlacementStatus, CreatePlacementCostItemRequest, PlacementChecklistStepDto } from '../types';

function InfoItem({ label, value }: { label: string; value?: string | number | null }) {
  return (
    <div>
      <dt className="text-sm text-muted-foreground">{label}</dt>
      <dd className="mt-0.5 text-sm font-medium">{value ?? '—'}</dd>
    </div>
  );
}

function StepIcon({ stepStatus }: { stepStatus: string }) {
  if (stepStatus === 'Completed') {
    return <CheckCircle2 className="h-5 w-5 text-green-600" />;
  }
  if (stepStatus === 'InProgress') {
    return <Loader2 className="h-5 w-5 animate-spin text-primary" />;
  }
  return <Circle className="h-5 w-5 text-muted-foreground/40" />;
}

function ChecklistStep({ step, isLast }: { step: PlacementChecklistStepDto; isLast: boolean }) {
  const config = STATUS_CONFIG[step.status as PlacementStatus];
  const Icon = config?.icon;

  return (
    <div className="flex gap-3">
      <div className="flex flex-col items-center">
        <StepIcon stepStatus={step.stepStatus} />
        {!isLast && (
          <div className={`mt-1 w-0.5 flex-1 ${step.stepStatus === 'Completed' ? 'bg-green-600' : 'bg-border'}`} />
        )}
      </div>
      <div className={`flex-1 pb-6 ${isLast ? 'pb-0' : ''}`}>
        <div className="flex items-center gap-2">
          <span className="text-xs font-medium text-muted-foreground">Step {step.stepNumber}</span>
          {Icon && <Icon className="h-3.5 w-3.5 text-muted-foreground" />}
          <Badge
            variant={step.stepStatus === 'Completed' ? 'success' : step.stepStatus === 'InProgress' ? 'default' : 'outline'}
            className="text-xs"
          >
            {step.stepStatus === 'InProgress' ? 'In Progress' : step.stepStatus}
          </Badge>
        </div>
        <h4 className="mt-1 text-sm font-semibold">{step.label}</h4>
        <p className="text-xs text-muted-foreground">{step.description}</p>
        {step.completedAt && (
          <p className="mt-0.5 text-xs text-muted-foreground">
            Completed: {new Date(step.completedAt).toLocaleDateString()}
          </p>
        )}
        {step.linkedEntityId && step.linkedEntityType && (
          <Link
            to={`/${step.linkedEntityType === 'Contract' ? 'contracts' : step.linkedEntityType === 'VisaApplication' ? 'visas' : 'arrivals'}/${step.linkedEntityId}`}
            className="mt-1 inline-flex items-center gap-1 text-xs text-primary hover:underline"
          >
            <ExternalLink className="h-3 w-3" />
            View {step.linkedEntityType}
          </Link>
        )}
      </div>
    </div>
  );
}

function DetailSkeleton() {
  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div>
        <Skeleton className="mb-2 h-4 w-32" />
        <div className="flex items-center justify-between">
          <div>
            <div className="flex items-center gap-3">
              <Skeleton className="h-8 w-40" />
              <Skeleton className="h-6 w-20 rounded-full" />
            </div>
            <Skeleton className="mt-1 h-4 w-56" />
          </div>
          <div className="flex items-center gap-2">
            <Skeleton className="h-9 w-32" />
            <Skeleton className="h-9 w-24" />
          </div>
        </div>
      </div>

      {/* Progress Bar */}
      <Card>
        <CardContent className="py-4">
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <Skeleton className="h-4 w-32" />
              <Skeleton className="h-4 w-16" />
            </div>
            <Skeleton className="h-3 w-full rounded-full" />
          </div>
        </CardContent>
      </Card>

      {/* Checklist & Info */}
      <div className="grid gap-4 lg:grid-cols-3">
        <Card className="lg:col-span-1">
          <CardHeader><Skeleton className="h-5 w-28" /></CardHeader>
          <CardContent className="space-y-4">
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="flex gap-3">
                <Skeleton className="h-5 w-5 rounded-full" />
                <div className="flex-1 space-y-1">
                  <Skeleton className="h-4 w-24" />
                  <Skeleton className="h-3 w-40" />
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
        <div className="space-y-4 lg:col-span-2">
          {Array.from({ length: 3 }).map((_, i) => (
            <Card key={i}>
              <CardHeader className="pb-3"><Skeleton className="h-5 w-28" /></CardHeader>
              <CardContent className="space-y-3">
                {Array.from({ length: 3 }).map((_, j) => (
                  <div key={j}>
                    <Skeleton className="mb-1 h-3 w-20" />
                    <Skeleton className="h-4 w-36" />
                  </div>
                ))}
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </div>
  );
}

export function PlacementDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: placement, isLoading } = usePlacement(id!);
  const transitionMutation = useTransitionPlacementStatus();
  const advanceStepMutation = useAdvancePlacementStep();
  const addCostMutation = useAddPlacementCostItem();
  const deleteCostMutation = useDeletePlacementCostItem();
  const deleteMutation = useDeletePlacement();

  const [showTransition, setShowTransition] = useState(false);
  const [showAddCost, setShowAddCost] = useState(false);

  // Cost item form state
  const [costType, setCostType] = useState('');
  const [costDescription, setCostDescription] = useState('');
  const [costAmount, setCostAmount] = useState('');
  const [costNotes, setCostNotes] = useState('');

  if (isLoading) {
    return <DetailSkeleton />;
  }

  if (!placement) {
    return (
      <div className="p-6">
        <p>Placement not found.</p>
        <Link to="/placements" className="text-primary underline">Back to pipeline</Link>
      </div>
    );
  }

  const currentStatus = placement.status as PlacementStatus;
  const hasTransitions = (ALLOWED_TRANSITIONS[currentStatus] || []).length > 0;
  const currentStepIndex = PIPELINE_STATUSES.indexOf(currentStatus);
  const checklist = placement.checklist;

  const handleTransition = (status: string, reason?: string, notes?: string) => {
    transitionMutation.mutate(
      { id: id!, data: { status, reason, notes } },
      { onSuccess: () => setShowTransition(false) }
    );
  };

  const handleAdvanceStep = () => {
    advanceStepMutation.mutate(
      { id: id!, data: {} },
    );
  };

  const handleAddCost = () => {
    if (!costType || !costDescription || !costAmount) return;
    const data: CreatePlacementCostItemRequest = {
      costType,
      description: costDescription,
      amount: parseFloat(costAmount),
      notes: costNotes || undefined,
    };
    addCostMutation.mutate(
      { placementId: id!, data },
      {
        onSuccess: () => {
          setShowAddCost(false);
          setCostType('');
          setCostDescription('');
          setCostAmount('');
          setCostNotes('');
        },
      }
    );
  };

  const handleDelete = () => {
    deleteMutation.mutate(id!, {
      onSuccess: () => navigate('/placements'),
    });
  };

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <Link to="/placements" className="mb-2 inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
            <ArrowLeft className="h-4 w-4" />
            Back to Pipeline
          </Link>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold">{placement.placementCode}</h1>
            <PlacementStatusBadge status={currentStatus} />
          </div>
          <p className="mt-1 text-sm text-muted-foreground">
            {placement.candidate?.fullNameEn || 'Unknown candidate'} → {placement.client?.nameEn || 'Unknown client'}
          </p>
        </div>
        <div className="flex items-center gap-2">
          {/* Advance Step button — primary action for outside-country flow */}
          {checklist && currentStepIndex >= 0 && currentStepIndex < PIPELINE_STATUSES.length - 1 && (
            <PermissionGate permission="placements.manage">
              <Button
                size="sm"
                onClick={handleAdvanceStep}
                disabled={advanceStepMutation.isPending}
              >
                <ChevronRight className="mr-1 h-4 w-4" />
                {advanceStepMutation.isPending ? 'Advancing...' : 'Advance Step'}
              </Button>
            </PermissionGate>
          )}
          {hasTransitions && (
            <PermissionGate permission="placements.manage">
              <Button size="sm" variant="outline" onClick={() => setShowTransition(true)}>
                Change Status
              </Button>
            </PermissionGate>
          )}
          <PermissionGate permission="placements.delete">
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button size="sm" variant="destructive">
                  <Trash2 className="mr-1 h-4 w-4" />
                  Delete
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Delete Placement</AlertDialogTitle>
                  <AlertDialogDescription>
                    Are you sure you want to delete this placement? This action cannot be undone.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction onClick={handleDelete}>Delete</AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          </PermissionGate>
        </div>
      </div>

      {/* Progress Bar */}
      {checklist && (
        <Card>
          <CardContent className="py-4">
            <div className="space-y-2">
              <div className="flex items-center justify-between text-sm">
                <span className="font-medium">
                  Outside Country Flow — Step {checklist.currentStepNumber} of {checklist.totalSteps}
                </span>
                <span className="text-muted-foreground">{checklist.progressPercent}%</span>
              </div>
              <div className="h-3 w-full overflow-hidden rounded-full bg-muted">
                <div
                  className="h-full rounded-full bg-primary transition-all duration-500"
                  style={{ width: `${checklist.progressPercent}%` }}
                />
              </div>
              {/* Step indicators */}
              <div className="flex items-center justify-between pt-1">
                {PIPELINE_STATUSES.map((status, i) => {
                  const config = STATUS_CONFIG[status];
                  const Icon = config.icon;
                  const isActive = status === currentStatus;
                  const isPast = currentStepIndex > i;
                  return (
                    <div
                      key={status}
                      className={`flex flex-col items-center gap-1 ${
                        isActive ? 'text-primary' : isPast ? 'text-green-600' : 'text-muted-foreground/40'
                      }`}
                      title={config.label}
                    >
                      <div className={`flex h-7 w-7 items-center justify-center rounded-full border-2 ${
                        isActive
                          ? 'border-primary bg-primary text-primary-foreground'
                          : isPast
                            ? 'border-green-600 bg-green-50 text-green-600'
                            : 'border-muted-foreground/30'
                      }`}>
                        {isPast ? (
                          <CheckCircle2 className="h-4 w-4" />
                        ) : (
                          <Icon className="h-3.5 w-3.5" />
                        )}
                      </div>
                      <span className="text-[10px] font-medium leading-tight">{config.shortLabel}</span>
                    </div>
                  );
                })}
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Main Content: Checklist + Info Cards */}
      <div className="grid gap-4 lg:grid-cols-3">
        {/* Checklist */}
        {checklist && (
          <Card className="lg:col-span-1">
            <CardHeader className="pb-3">
              <CardTitle className="text-base">Deployment Checklist</CardTitle>
            </CardHeader>
            <CardContent>
              {checklist.steps.map((step, i) => (
                <ChecklistStep
                  key={step.stepNumber}
                  step={step}
                  isLast={i === checklist.steps.length - 1}
                />
              ))}
            </CardContent>
          </Card>
        )}

        {/* Info Cards */}
        <div className={`space-y-4 ${checklist ? 'lg:col-span-2' : 'lg:col-span-3'}`}>
          <div className="grid gap-4 md:grid-cols-2">
            {/* Candidate */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-base">Candidate</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <InfoItem label="Name" value={placement.candidate?.fullNameEn} />
                <InfoItem label="Nationality" value={placement.candidate?.nationality} />
              </CardContent>
            </Card>

            {/* Client */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-base">Client</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <InfoItem label="Name" value={placement.client?.nameEn} />
              </CardContent>
            </Card>

            {/* Worker (if created) */}
            {placement.worker && (
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-base">Worker</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <InfoItem label="Name" value={placement.worker.fullNameEn} />
                  <InfoItem label="Code" value={placement.worker.workerCode} />
                  <Link to={`/workers/${placement.workerId}`} className="mt-2 inline-block text-sm text-primary underline">
                    View Worker
                  </Link>
                </CardContent>
              </Card>
            )}

            {/* Booking */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-base">Booking</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <InfoItem label="Booked By" value={placement.bookedByName} />
                <InfoItem label="Booked At" value={new Date(placement.bookedAt).toLocaleDateString()} />
                <InfoItem label="Notes" value={placement.bookingNotes} />
              </CardContent>
            </Card>

            {/* Pipeline Dates */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-base">Pipeline Dates</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <InfoItem label="Contract Created" value={placement.contractCreatedAt ? new Date(placement.contractCreatedAt).toLocaleDateString() : undefined} />
                <InfoItem label="Employment Visa Started" value={placement.employmentVisaStartedAt ? new Date(placement.employmentVisaStartedAt).toLocaleDateString() : undefined} />
                <InfoItem label="Ticket Date" value={placement.ticketDate} />
                <InfoItem label="Flight Details" value={placement.flightDetails} />
                <InfoItem label="Expected Arrival" value={placement.expectedArrivalDate} />
                <InfoItem label="Arrived" value={placement.arrivedAt ? new Date(placement.arrivedAt).toLocaleDateString() : undefined} />
                <InfoItem label="Deployed" value={placement.deployedAt ? new Date(placement.deployedAt).toLocaleDateString() : undefined} />
                <InfoItem label="Full Payment" value={placement.fullPaymentReceivedAt ? new Date(placement.fullPaymentReceivedAt).toLocaleDateString() : undefined} />
                <InfoItem label="Residence Visa" value={placement.residenceVisaStartedAt ? new Date(placement.residenceVisaStartedAt).toLocaleDateString() : undefined} />
                <InfoItem label="Emirates ID" value={placement.emiratesIdStartedAt ? new Date(placement.emiratesIdStartedAt).toLocaleDateString() : undefined} />
              </CardContent>
            </Card>

            {/* Linked Entities */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-base">Linked Records</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                {placement.contractId ? (
                  <div>
                    <dt className="text-sm text-muted-foreground">Contract</dt>
                    <dd className="mt-0.5">
                      <Link to={`/contracts/${placement.contractId}`} className="text-sm text-primary hover:underline">
                        View Contract
                      </Link>
                    </dd>
                  </div>
                ) : (
                  <InfoItem label="Contract" value="Not linked" />
                )}
                {placement.arrivalId ? (
                  <div>
                    <dt className="text-sm text-muted-foreground">Arrival</dt>
                    <dd className="mt-0.5">
                      <Link to={`/arrivals/${placement.arrivalId}`} className="text-sm text-primary hover:underline">
                        View Arrival
                      </Link>
                    </dd>
                  </div>
                ) : (
                  <InfoItem label="Arrival" value="Not linked" />
                )}
                {placement.employmentVisaApplicationId ? (
                  <div>
                    <dt className="text-sm text-muted-foreground">Employment Visa</dt>
                    <dd className="mt-0.5">
                      <Link to={`/visas/${placement.employmentVisaApplicationId}`} className="text-sm text-primary hover:underline">
                        View Visa Application
                      </Link>
                    </dd>
                  </div>
                ) : (
                  <InfoItem label="Employment Visa" value="Not linked" />
                )}
              </CardContent>
            </Card>
          </div>

          {/* Financial Summary */}
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base">Cost Summary</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                AED {(placement.totalCost || 0).toLocaleString()}
              </div>
              <p className="text-xs text-muted-foreground">
                {placement.costItems?.length || 0} cost items
              </p>
            </CardContent>
          </Card>

          {/* Cost Items */}
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-3">
              <CardTitle className="text-base">Cost Items</CardTitle>
              <PermissionGate permission="placements.manage">
                <Button size="sm" variant="outline" onClick={() => setShowAddCost(true)}>
                  <Plus className="mr-1 h-4 w-4" />
                  Add Cost
                </Button>
              </PermissionGate>
            </CardHeader>
            <CardContent>
              {(!placement.costItems || placement.costItems.length === 0) ? (
                <p className="py-4 text-center text-sm text-muted-foreground">No cost items yet</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b text-left">
                        <th className="pb-2 font-medium">Type</th>
                        <th className="pb-2 font-medium">Description</th>
                        <th className="pb-2 font-medium text-right">Amount</th>
                        <th className="pb-2 font-medium">Status</th>
                        <th className="pb-2 font-medium">Date</th>
                        <th className="pb-2" />
                      </tr>
                    </thead>
                    <tbody>
                      {placement.costItems.map((item) => (
                        <tr key={item.id} className="border-b">
                          <td className="py-2">{item.costType}</td>
                          <td className="py-2">{item.description}</td>
                          <td className="py-2 text-right font-mono">
                            {item.currency} {item.amount.toLocaleString()}
                          </td>
                          <td className="py-2">
                            <Badge variant={item.status === 'Paid' ? 'success' : item.status === 'Cancelled' ? 'destructive' : 'outline'} className="text-xs">
                              {item.status}
                            </Badge>
                          </td>
                          <td className="py-2 text-muted-foreground">
                            {item.costDate || '—'}
                          </td>
                          <td className="py-2">
                            <PermissionGate permission="placements.manage">
                              <Button
                                size="icon"
                                variant="ghost"
                                className="h-7 w-7"
                                onClick={() => deleteCostMutation.mutate({ placementId: id!, itemId: item.id })}
                              >
                                <Trash2 className="h-3.5 w-3.5 text-muted-foreground" />
                              </Button>
                            </PermissionGate>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Status History */}
          {placement.statusHistory && placement.statusHistory.length > 0 && (
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-base">Status History</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {placement.statusHistory.map((h) => (
                    <div key={h.id} className="flex items-start gap-3 border-b pb-3 last:border-0">
                      <div className="mt-0.5 h-2 w-2 rounded-full bg-primary" />
                      <div className="flex-1">
                        <div className="flex items-center gap-2">
                          {h.fromStatus && (
                            <>
                              <PlacementStatusBadge status={h.fromStatus as PlacementStatus} showIcon={false} />
                              <span className="text-xs text-muted-foreground">→</span>
                            </>
                          )}
                          <PlacementStatusBadge status={h.toStatus as PlacementStatus} showIcon={false} />
                        </div>
                        <p className="mt-1 text-xs text-muted-foreground">
                          {new Date(h.changedAt).toLocaleString()}
                          {h.reason && ` — ${h.reason}`}
                        </p>
                        {h.notes && <p className="mt-0.5 text-xs">{h.notes}</p>}
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </div>

      {/* Transition Dialog */}
      <PlacementTransitionDialog
        open={showTransition}
        onOpenChange={setShowTransition}
        currentStatus={currentStatus}
        onTransition={handleTransition}
        isPending={transitionMutation.isPending}
      />

      {/* Add Cost Item Dialog */}
      <Dialog open={showAddCost} onOpenChange={setShowAddCost}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Add Cost Item</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label>Cost Type</Label>
              <Select value={costType} onValueChange={setCostType}>
                <SelectTrigger>
                  <SelectValue placeholder="Select type..." />
                </SelectTrigger>
                <SelectContent>
                  {COST_TYPES.map((t) => (
                    <SelectItem key={t.value} value={t.value}>{t.label}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Description</Label>
              <Input value={costDescription} onChange={(e) => setCostDescription(e.target.value)} placeholder="Cost description" />
            </div>
            <div className="space-y-2">
              <Label>Amount (AED)</Label>
              <Input type="number" value={costAmount} onChange={(e) => setCostAmount(e.target.value)} placeholder="0.00" />
            </div>
            <div className="space-y-2">
              <Label>Notes</Label>
              <textarea
                className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                value={costNotes}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setCostNotes(e.target.value)}
                rows={2}
              />
            </div>
          </div>
          <DialogFooter className="gap-2 sm:space-x-reverse">
            <Button variant="outline" onClick={() => setShowAddCost(false)}>Cancel</Button>
            <Button
              onClick={handleAddCost}
              disabled={!costType || !costDescription || !costAmount || addCostMutation.isPending}
            >
              {addCostMutation.isPending ? 'Adding...' : 'Add Cost'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

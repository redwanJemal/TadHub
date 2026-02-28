import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Badge } from '@/shared/components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import { Skeleton } from '@/shared/components/ui/skeleton';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
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
import { Label } from '@/shared/components/ui/label';
import { Input } from '@/shared/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { cn } from '@/shared/lib/cn';
import { usePermissions } from '@/features/auth/hooks/usePermissions';
import { PermissionGate } from '@/shared/components/PermissionGate';
import {
  useDiscountPrograms,
  useCreateDiscountProgram,
  useUpdateDiscountProgram,
  useDeleteDiscountProgram,
} from '../hooks';
import type { DiscountProgramListDto, DiscountType } from '../types';

const DISCOUNT_TYPES: DiscountType[] = ['Saada', 'Fazaa', 'Custom'];

const TYPE_CLASSES: Record<DiscountType, string> = {
  Saada:  'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
  Fazaa:  'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
  Custom: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300',
};

interface ProgramFormState {
  name: string;
  nameAr: string;
  type: string;
  discountPercentage: string;
  maxDiscountAmount: string;
  isActive: boolean;
  validFrom: string;
  validTo: string;
  description: string;
}

const EMPTY_FORM: ProgramFormState = {
  name: '',
  nameAr: '',
  type: 'Custom',
  discountPercentage: '',
  maxDiscountAmount: '',
  isActive: true,
  validFrom: '',
  validTo: '',
  description: '',
};

export function DiscountProgramsPage() {
  useTranslation('finance');
  const { hasPermission } = usePermissions();

  const [page] = useState(1);
  const pageSize = 50;

  const queryParams = useMemo(() => ({ page, pageSize, sort: '-createdAt' }), [page, pageSize]);
  const { data, isLoading, refetch } = useDiscountPrograms(queryParams);

  const createMutation = useCreateDiscountProgram();
  const updateMutation = useUpdateDiscountProgram();
  const deleteMutation = useDeleteDiscountProgram();

  const [showDialog, setShowDialog] = useState(false);
  const [editTarget, setEditTarget] = useState<DiscountProgramListDto | null>(null);
  const [form, setForm] = useState<ProgramFormState>(EMPTY_FORM);
  const [deleteTarget, setDeleteTarget] = useState<DiscountProgramListDto | null>(null);

  const openCreate = () => {
    setEditTarget(null);
    setForm(EMPTY_FORM);
    setShowDialog(true);
  };

  const openEdit = (program: DiscountProgramListDto) => {
    setEditTarget(program);
    setForm({
      name: program.name,
      nameAr: program.nameAr ?? '',
      type: program.type,
      discountPercentage: String(program.discountPercentage),
      maxDiscountAmount: program.maxDiscountAmount != null ? String(program.maxDiscountAmount) : '',
      isActive: program.isActive,
      validFrom: program.validFrom ? program.validFrom.slice(0, 10) : '',
      validTo: program.validTo ? program.validTo.slice(0, 10) : '',
      description: '',
    });
    setShowDialog(true);
  };

  const handleSave = async () => {
    const payload = {
      name: form.name,
      nameAr: form.nameAr || undefined,
      type: form.type,
      discountPercentage: parseFloat(form.discountPercentage),
      maxDiscountAmount: form.maxDiscountAmount ? parseFloat(form.maxDiscountAmount) : undefined,
      isActive: form.isActive,
      validFrom: form.validFrom || undefined,
      validTo: form.validTo || undefined,
      description: form.description || undefined,
    };

    if (editTarget) {
      await updateMutation.mutateAsync({ id: editTarget.id, data: payload });
    } else {
      await createMutation.mutateAsync(payload);
    }
    setShowDialog(false);
    setEditTarget(null);
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    await deleteMutation.mutateAsync(deleteTarget.id);
    setDeleteTarget(null);
  };

  const canSave =
    form.name.trim() &&
    form.type &&
    form.discountPercentage &&
    parseFloat(form.discountPercentage) >= 0;

  const isPending = createMutation.isPending || updateMutation.isPending;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Discount Programs</h1>
          <p className="text-muted-foreground">Manage Saada, Fazaa, and custom discount programs</p>
        </div>
        <PermissionGate permission="finance.discounts.create">
          <Button onClick={openCreate}>
            <Plus className="me-2 h-4 w-4" />
            New Program
          </Button>
        </PermissionGate>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Discount Programs ({data?.totalCount ?? 0})</CardTitle>
            <Button variant="outline" size="sm" onClick={() => refetch()}>Refresh</Button>
          </div>
        </CardHeader>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Type</TableHead>
                <TableHead className="text-right">Discount %</TableHead>
                <TableHead className="text-right">Max Discount</TableHead>
                <TableHead>Valid From</TableHead>
                <TableHead>Valid To</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="w-[100px]">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {isLoading ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <TableRow key={i}>
                    {Array.from({ length: 8 }).map((_, j) => (
                      <TableCell key={j}><Skeleton className="h-5 w-full" /></TableCell>
                    ))}
                  </TableRow>
                ))
              ) : (data?.items ?? []).length === 0 ? (
                <TableRow>
                  <TableCell colSpan={8} className="text-center py-12 text-muted-foreground">
                    No discount programs found. Create one to get started.
                  </TableCell>
                </TableRow>
              ) : (
                (data?.items ?? []).map((program) => (
                  <TableRow key={program.id}>
                    <TableCell>
                      <div>
                        <p className="font-medium">{program.name}</p>
                        {program.nameAr && (
                          <p className="text-xs text-muted-foreground" dir="rtl">{program.nameAr}</p>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant="outline"
                        className={cn('border-transparent', TYPE_CLASSES[program.type])}
                      >
                        {program.type}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right font-medium tabular-nums">
                      {program.discountPercentage}%
                    </TableCell>
                    <TableCell className="text-right tabular-nums text-muted-foreground">
                      {program.maxDiscountAmount != null
                        ? `${program.maxDiscountAmount.toLocaleString()} AED`
                        : '—'}
                    </TableCell>
                    <TableCell>
                      {program.validFrom ? new Date(program.validFrom).toLocaleDateString() : '—'}
                    </TableCell>
                    <TableCell>
                      {program.validTo ? new Date(program.validTo).toLocaleDateString() : '—'}
                    </TableCell>
                    <TableCell>
                      <Badge variant={program.isActive ? 'success' : 'secondary'}>
                        {program.isActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        {hasPermission('finance.discounts.update') && (
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8"
                            onClick={() => openEdit(program)}
                          >
                            <Pencil className="h-4 w-4" />
                          </Button>
                        )}
                        {hasPermission('finance.discounts.delete') && (
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8 text-destructive hover:text-destructive"
                            onClick={() => setDeleteTarget(program)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Create / Edit Dialog */}
      <Dialog open={showDialog} onOpenChange={setShowDialog}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>{editTarget ? 'Edit Discount Program' : 'New Discount Program'}</DialogTitle>
            <DialogDescription>
              {editTarget ? `Editing "${editTarget.name}"` : 'Create a new discount program'}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>Name (EN) *</Label>
                <Input
                  placeholder="Program name"
                  value={form.name}
                  onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                />
              </div>
              <div className="space-y-2">
                <Label>Name (AR)</Label>
                <Input
                  placeholder="اسم البرنامج"
                  dir="rtl"
                  value={form.nameAr}
                  onChange={(e) => setForm((f) => ({ ...f, nameAr: e.target.value }))}
                />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>Type *</Label>
                <Select value={form.type} onValueChange={(v) => setForm((f) => ({ ...f, type: v }))}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select type" />
                  </SelectTrigger>
                  <SelectContent>
                    {DISCOUNT_TYPES.map((dt) => (
                      <SelectItem key={dt} value={dt}>{dt}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>Discount % *</Label>
                <Input
                  type="number"
                  min="0"
                  max="100"
                  step="0.01"
                  placeholder="e.g. 10"
                  value={form.discountPercentage}
                  onChange={(e) => setForm((f) => ({ ...f, discountPercentage: e.target.value }))}
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label>Max Discount Amount (AED)</Label>
              <Input
                type="number"
                min="0"
                step="0.01"
                placeholder="Optional cap"
                value={form.maxDiscountAmount}
                onChange={(e) => setForm((f) => ({ ...f, maxDiscountAmount: e.target.value }))}
              />
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>Valid From</Label>
                <Input
                  type="date"
                  value={form.validFrom}
                  onChange={(e) => setForm((f) => ({ ...f, validFrom: e.target.value }))}
                />
              </div>
              <div className="space-y-2">
                <Label>Valid To</Label>
                <Input
                  type="date"
                  value={form.validTo}
                  onChange={(e) => setForm((f) => ({ ...f, validTo: e.target.value }))}
                />
              </div>
            </div>

            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="isActive"
                checked={form.isActive}
                onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
                className="h-4 w-4"
              />
              <Label htmlFor="isActive">Active</Label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowDialog(false)}>Cancel</Button>
            <Button onClick={handleSave} disabled={!canSave || isPending}>
              {isPending ? 'Saving...' : editTarget ? 'Save Changes' : 'Create Program'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation */}
      <AlertDialog open={!!deleteTarget} onOpenChange={() => setDeleteTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Discount Program</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete{' '}
              <span className="font-medium">"{deleteTarget?.name}"</span>?
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

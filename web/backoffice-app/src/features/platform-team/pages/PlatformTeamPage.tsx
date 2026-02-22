import { useState } from 'react';
import { Search, MoreHorizontal, Shield, ShieldCheck, UserPlus, Trash2, Eye } from 'lucide-react';
import { usePlatformStaff, useCreatePlatformStaff, useUpdatePlatformStaff, useRemovePlatformStaff } from '../hooks';
import { PlatformStaffDto, PlatformStaffRole } from '../types';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Badge } from '@/shared/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/shared/components/ui/avatar';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Label } from '@/shared/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';

function getInitials(firstName?: string, lastName?: string): string {
  const f = firstName?.[0] || '';
  const l = lastName?.[0] || '';
  return (f + l).toUpperCase() || '?';
}

function formatDate(dateString?: string): string {
  if (!dateString) return 'Never';
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

const ROLE_LABELS: Record<string, string> = {
  'super-admin': 'Super Admin',
  'admin': 'Admin',
  'finance': 'Finance',
  'sales': 'Sales',
  'support': 'Support',
};

function getRoleLabel(role: string): string {
  return ROLE_LABELS[role] || role;
}

export function PlatformTeamPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [removeTarget, setRemoveTarget] = useState<PlatformStaffDto | null>(null);
  const [selectedStaff, setSelectedStaff] = useState<PlatformStaffDto | null>(null);
  const [addDialogOpen, setAddDialogOpen] = useState(false);
  const [newStaffEmail, setNewStaffEmail] = useState('');
  const [newStaffRole, setNewStaffRole] = useState<PlatformStaffRole>('admin');
  const [addError, setAddError] = useState<string | null>(null);

  const { data: staff, isLoading, error } = usePlatformStaff({
    search: searchQuery || undefined,
  });

  const createMutation = useCreatePlatformStaff();
  const updateMutation = useUpdatePlatformStaff();
  const removeMutation = useRemovePlatformStaff();

  const handleRemove = async () => {
    if (!removeTarget) return;
    try {
      await removeMutation.mutateAsync(removeTarget.id);
      setRemoveTarget(null);
    } catch (err) {
      console.error('Failed to remove staff:', err);
    }
  };

  const handleToggleSuperAdmin = async (member: PlatformStaffDto) => {
    const newRole = member.role === 'super-admin' ? 'admin' : 'super-admin';
    try {
      await updateMutation.mutateAsync({
        id: member.id,
        data: { role: newRole },
      });
    } catch (err) {
      console.error('Failed to update staff:', err);
    }
  };

  const handleAddStaff = async () => {
    if (!newStaffEmail.trim()) return;
    setAddError(null);
    try {
      await createMutation.mutateAsync({
        email: newStaffEmail.trim(),
        role: newStaffRole,
      });
      setAddDialogOpen(false);
      setNewStaffEmail('');
      setNewStaffRole('admin');
    } catch (err: any) {
      setAddError(err.message || 'Failed to add staff member');
    }
  };

  const filteredStaff = staff || [];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2">
          <Shield className="h-6 w-6" />
          Platform Team
        </h1>
        <p className="text-muted-foreground">
          Manage users who have access to this admin panel.
        </p>
      </div>

      {/* Stats Card */}
      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-medium text-muted-foreground">Team Members</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-4">
            <div className="text-3xl font-bold">{filteredStaff.length}</div>
            <div className="text-sm text-muted-foreground">
              {filteredStaff.filter(a => a.role === 'super-admin').length} super admins
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Search and Actions */}
      <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
        <div className="relative w-full sm:w-80">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search by name or email..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-10"
          />
        </div>
        <Button onClick={() => setAddDialogOpen(true)}>
          <UserPlus className="h-4 w-4 mr-2" />
          Add Staff
        </Button>
      </div>

      {/* Table */}
      {isLoading ? (
        <div className="space-y-3">
          {[...Array(5)].map((_, i) => (
            <Skeleton key={i} className="h-16 w-full" />
          ))}
        </div>
      ) : error ? (
        <Card>
          <CardContent className="py-10 text-center text-muted-foreground">
            Failed to load platform team members. Please try again.
          </CardContent>
        </Card>
      ) : filteredStaff.length === 0 ? (
        <Card>
          <CardContent className="py-10 text-center text-muted-foreground">
            {searchQuery ? 'No staff match your search.' : 'No platform staff yet. Add your first team member above.'}
          </CardContent>
        </Card>
      ) : (
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>User</TableHead>
                <TableHead>Role</TableHead>
                <TableHead>Last Login</TableHead>
                <TableHead>Added</TableHead>
                <TableHead className="w-[70px]"></TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredStaff.map((member) => (
                <TableRow key={member.id}>
                  <TableCell>
                    <div className="flex items-center gap-3">
                      <Avatar className="h-9 w-9">
                        <AvatarImage src={member.user.avatarUrl} />
                        <AvatarFallback className="bg-primary/10 text-primary">
                          {getInitials(member.user.firstName, member.user.lastName)}
                        </AvatarFallback>
                      </Avatar>
                      <div>
                        <div className="font-medium">
                          {member.user.firstName} {member.user.lastName}
                        </div>
                        <div className="text-sm text-muted-foreground">
                          {member.user.email}
                        </div>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    {member.role === 'super-admin' ? (
                      <Badge className="bg-amber-500 hover:bg-amber-600">
                        <ShieldCheck className="h-3 w-3 mr-1" />
                        Super Admin
                      </Badge>
                    ) : (
                      <Badge variant="secondary">
                        <Shield className="h-3 w-3 mr-1" />
                        {getRoleLabel(member.role)}
                      </Badge>
                    )}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {formatDate(member.user.lastLoginAt)}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {formatDate(member.createdAt)}
                  </TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => setSelectedStaff(member)}>
                          <Eye className="h-4 w-4 mr-2" />
                          View Details
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => handleToggleSuperAdmin(member)}>
                          <ShieldCheck className="h-4 w-4 mr-2" />
                          {member.role === 'super-admin' ? 'Remove Super Admin' : 'Make Super Admin'}
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem
                          className="text-destructive"
                          onClick={() => setRemoveTarget(member)}
                        >
                          <Trash2 className="h-4 w-4 mr-2" />
                          Remove Staff
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      {/* Add Staff Dialog */}
      <Dialog open={addDialogOpen} onOpenChange={setAddDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add Platform Staff</DialogTitle>
            <DialogDescription>
              Add an existing user as a platform staff member. The user must have logged in at least once.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            {addError && (
              <div className="text-sm text-destructive bg-destructive/10 p-3 rounded-md">
                {addError}
              </div>
            )}
            <div className="space-y-2">
              <Label htmlFor="email">Email Address</Label>
              <Input
                id="email"
                type="email"
                placeholder="user@example.com"
                value={newStaffEmail}
                onChange={(e) => setNewStaffEmail(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="role">Role</Label>
              <Select value={newStaffRole} onValueChange={(v) => setNewStaffRole(v as PlatformStaffRole)}>
                <SelectTrigger id="role">
                  <SelectValue placeholder="Select role" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="super-admin">Super Admin</SelectItem>
                  <SelectItem value="admin">Admin</SelectItem>
                  <SelectItem value="finance">Finance</SelectItem>
                  <SelectItem value="sales">Sales</SelectItem>
                  <SelectItem value="support">Support</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setAddDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleAddStaff}
              disabled={!newStaffEmail.trim() || createMutation.isPending}
            >
              {createMutation.isPending ? 'Adding...' : 'Add Staff'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Remove Confirmation Dialog */}
      <AlertDialog open={!!removeTarget} onOpenChange={() => setRemoveTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Remove Staff Access</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to remove staff access from{' '}
              <strong>
                {removeTarget?.user.firstName} {removeTarget?.user.lastName}
              </strong>
              ? They will no longer be able to access this admin panel.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleRemove}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Remove Staff
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* View Details Dialog */}
      <Dialog open={!!selectedStaff} onOpenChange={() => setSelectedStaff(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Staff Details</DialogTitle>
          </DialogHeader>
          {selectedStaff && (
            <div className="space-y-4">
              <div className="flex items-center gap-4">
                <Avatar className="h-16 w-16">
                  <AvatarImage src={selectedStaff.user.avatarUrl} />
                  <AvatarFallback className="bg-primary/10 text-primary text-xl">
                    {getInitials(selectedStaff.user.firstName, selectedStaff.user.lastName)}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <div className="text-lg font-semibold">
                    {selectedStaff.user.firstName} {selectedStaff.user.lastName}
                  </div>
                  <div className="text-muted-foreground">{selectedStaff.user.email}</div>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <div className="font-medium text-muted-foreground">Role</div>
                  <div>{getRoleLabel(selectedStaff.role)}</div>
                </div>
                <div>
                  <div className="font-medium text-muted-foreground">Department</div>
                  <div>{selectedStaff.department || '-'}</div>
                </div>
                <div>
                  <div className="font-medium text-muted-foreground">Status</div>
                  <div>{selectedStaff.user.isActive ? 'Active' : 'Inactive'}</div>
                </div>
                <div>
                  <div className="font-medium text-muted-foreground">Phone</div>
                  <div>{selectedStaff.user.phone || '-'}</div>
                </div>
                <div>
                  <div className="font-medium text-muted-foreground">Last Login</div>
                  <div>{formatDate(selectedStaff.user.lastLoginAt)}</div>
                </div>
                <div>
                  <div className="font-medium text-muted-foreground">Staff Since</div>
                  <div>{formatDate(selectedStaff.createdAt)}</div>
                </div>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}

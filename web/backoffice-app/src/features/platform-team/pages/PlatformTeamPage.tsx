import { useState } from 'react';
import { Search, MoreHorizontal, Shield, ShieldCheck, UserPlus, Trash2, Eye } from 'lucide-react';
import { useAdminUsers, useCreateAdminUser, useUpdateAdminUser, useRemoveAdminUser } from '../hooks';
import { AdminUserDto } from '../types';
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
import { Checkbox } from '@/shared/components/ui/checkbox';
import { Label } from '@/shared/components/ui/label';

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

export function PlatformTeamPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [removeTarget, setRemoveTarget] = useState<AdminUserDto | null>(null);
  const [selectedAdmin, setSelectedAdmin] = useState<AdminUserDto | null>(null);
  const [addDialogOpen, setAddDialogOpen] = useState(false);
  const [newAdminEmail, setNewAdminEmail] = useState('');
  const [newAdminIsSuperAdmin, setNewAdminIsSuperAdmin] = useState(false);
  const [addError, setAddError] = useState<string | null>(null);

  const { data: admins, isLoading, error } = useAdminUsers({
    q: searchQuery || undefined,
  });

  const createMutation = useCreateAdminUser();
  const updateMutation = useUpdateAdminUser();
  const removeMutation = useRemoveAdminUser();

  const handleRemove = async () => {
    if (!removeTarget) return;
    try {
      await removeMutation.mutateAsync(removeTarget.id);
      setRemoveTarget(null);
    } catch (err) {
      console.error('Failed to remove admin:', err);
    }
  };

  const handleToggleSuperAdmin = async (admin: AdminUserDto) => {
    try {
      await updateMutation.mutateAsync({
        id: admin.id,
        data: { isSuperAdmin: !admin.isSuperAdmin },
      });
    } catch (err) {
      console.error('Failed to update admin:', err);
    }
  };

  const handleAddAdmin = async () => {
    if (!newAdminEmail.trim()) return;
    setAddError(null);
    try {
      await createMutation.mutateAsync({
        email: newAdminEmail.trim(),
        isSuperAdmin: newAdminIsSuperAdmin,
      });
      setAddDialogOpen(false);
      setNewAdminEmail('');
      setNewAdminIsSuperAdmin(false);
    } catch (err: any) {
      setAddError(err.message || 'Failed to add admin');
    }
  };

  const filteredAdmins = admins || [];

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
            <div className="text-3xl font-bold">{filteredAdmins.length}</div>
            <div className="text-sm text-muted-foreground">
              {filteredAdmins.filter(a => a.isSuperAdmin).length} super admins
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
          Add Admin
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
      ) : filteredAdmins.length === 0 ? (
        <Card>
          <CardContent className="py-10 text-center text-muted-foreground">
            {searchQuery ? 'No admins match your search.' : 'No platform admins yet.'}
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
              {filteredAdmins.map((admin) => (
                <TableRow key={admin.id}>
                  <TableCell>
                    <div className="flex items-center gap-3">
                      <Avatar className="h-9 w-9">
                        <AvatarImage src={admin.user.avatarUrl} />
                        <AvatarFallback className="bg-primary/10 text-primary">
                          {getInitials(admin.user.firstName, admin.user.lastName)}
                        </AvatarFallback>
                      </Avatar>
                      <div>
                        <div className="font-medium">
                          {admin.user.firstName} {admin.user.lastName}
                        </div>
                        <div className="text-sm text-muted-foreground">
                          {admin.user.email}
                        </div>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    {admin.isSuperAdmin ? (
                      <Badge className="bg-amber-500 hover:bg-amber-600">
                        <ShieldCheck className="h-3 w-3 mr-1" />
                        Super Admin
                      </Badge>
                    ) : (
                      <Badge variant="secondary">
                        <Shield className="h-3 w-3 mr-1" />
                        Admin
                      </Badge>
                    )}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {formatDate(admin.user.lastLoginAt)}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {formatDate(admin.createdAt)}
                  </TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => setSelectedAdmin(admin)}>
                          <Eye className="h-4 w-4 mr-2" />
                          View Details
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => handleToggleSuperAdmin(admin)}>
                          <ShieldCheck className="h-4 w-4 mr-2" />
                          {admin.isSuperAdmin ? 'Remove Super Admin' : 'Make Super Admin'}
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem
                          className="text-destructive"
                          onClick={() => setRemoveTarget(admin)}
                        >
                          <Trash2 className="h-4 w-4 mr-2" />
                          Remove Admin
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

      {/* Add Admin Dialog */}
      <Dialog open={addDialogOpen} onOpenChange={setAddDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add Platform Admin</DialogTitle>
            <DialogDescription>
              Add an existing user as a platform admin. The user must have logged in at least once.
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
                value={newAdminEmail}
                onChange={(e) => setNewAdminEmail(e.target.value)}
              />
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="superAdmin"
                checked={newAdminIsSuperAdmin}
                onCheckedChange={(checked) => setNewAdminIsSuperAdmin(checked === true)}
              />
              <Label htmlFor="superAdmin" className="cursor-pointer">
                Grant super admin privileges
              </Label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setAddDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleAddAdmin}
              disabled={!newAdminEmail.trim() || createMutation.isPending}
            >
              {createMutation.isPending ? 'Adding...' : 'Add Admin'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Remove Confirmation Dialog */}
      <AlertDialog open={!!removeTarget} onOpenChange={() => setRemoveTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Remove Admin Access</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to remove admin access from{' '}
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
              Remove Admin
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* View Details Dialog */}
      <Dialog open={!!selectedAdmin} onOpenChange={() => setSelectedAdmin(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Admin Details</DialogTitle>
          </DialogHeader>
          {selectedAdmin && (
            <div className="space-y-4">
              <div className="flex items-center gap-4">
                <Avatar className="h-16 w-16">
                  <AvatarImage src={selectedAdmin.user.avatarUrl} />
                  <AvatarFallback className="bg-primary/10 text-primary text-xl">
                    {getInitials(selectedAdmin.user.firstName, selectedAdmin.user.lastName)}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <div className="text-lg font-semibold">
                    {selectedAdmin.user.firstName} {selectedAdmin.user.lastName}
                  </div>
                  <div className="text-muted-foreground">{selectedAdmin.user.email}</div>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <div className="font-medium text-muted-foreground">Role</div>
                  <div>{selectedAdmin.isSuperAdmin ? 'Super Admin' : 'Admin'}</div>
                </div>
                <div>
                  <div className="font-medium text-muted-foreground">Status</div>
                  <div>{selectedAdmin.user.isActive ? 'Active' : 'Inactive'}</div>
                </div>
                <div>
                  <div className="font-medium text-muted-foreground">Phone</div>
                  <div>{selectedAdmin.user.phone || '-'}</div>
                </div>
                <div>
                  <div className="font-medium text-muted-foreground">Locale</div>
                  <div>{selectedAdmin.user.locale || 'en'}</div>
                </div>
                <div>
                  <div className="font-medium text-muted-foreground">Last Login</div>
                  <div>{formatDate(selectedAdmin.user.lastLoginAt)}</div>
                </div>
                <div>
                  <div className="font-medium text-muted-foreground">Admin Since</div>
                  <div>{formatDate(selectedAdmin.createdAt)}</div>
                </div>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}

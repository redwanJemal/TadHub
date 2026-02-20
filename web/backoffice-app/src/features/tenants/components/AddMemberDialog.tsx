import { useState } from 'react';
import { useDebounce } from '@/shared/hooks/useDebounce';
import { useSearchUsers, useAddTenantMember } from '../hooks';
import { TenantRole, UserProfileDto } from '../types';
import { Loader2, Search, User, Check } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { Avatar, AvatarFallback, AvatarImage } from '@/shared/components/ui/avatar';
import { cn } from '@/lib/utils';

interface AddMemberDialogProps {
  tenantId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

function getInitials(name: string) {
  return name
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

export function AddMemberDialog({ tenantId, open, onOpenChange }: AddMemberDialogProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedUser, setSelectedUser] = useState<UserProfileDto | null>(null);
  const [selectedRole, setSelectedRole] = useState<TenantRole>('Member');

  const debouncedSearch = useDebounce(searchQuery, 300);
  const { data: searchResults, isLoading: isSearching } = useSearchUsers(debouncedSearch);
  const addMemberMutation = useAddTenantMember();

  const handleAddMember = async () => {
    if (!selectedUser) return;

    try {
      await addMemberMutation.mutateAsync({
        tenantId,
        data: {
          userId: selectedUser.id,
          role: selectedRole,
        },
      });
      // Reset and close
      setSearchQuery('');
      setSelectedUser(null);
      setSelectedRole('Member');
      onOpenChange(false);
    } catch (error) {
      console.error('Failed to add member:', error);
    }
  };

  const handleClose = () => {
    setSearchQuery('');
    setSelectedUser(null);
    setSelectedRole('Member');
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Add Member</DialogTitle>
          <DialogDescription>
            Search for a user and add them to this tenant with a specific role.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          {/* User Search */}
          <div className="space-y-2">
            <label className="text-sm font-medium">Search User</label>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search by email or name..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9"
              />
            </div>

            {/* Search Results */}
            {searchQuery.length >= 2 && (
              <div className="rounded-md border max-h-[200px] overflow-y-auto">
                {isSearching ? (
                  <div className="flex items-center justify-center py-4">
                    <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
                  </div>
                ) : searchResults?.items.length === 0 ? (
                  <div className="flex flex-col items-center py-4 text-center">
                    <User className="h-8 w-8 text-muted-foreground mb-2" />
                    <p className="text-sm text-muted-foreground">No users found</p>
                  </div>
                ) : (
                  <div className="divide-y">
                    {searchResults?.items.map((user) => (
                      <button
                        key={user.id}
                        type="button"
                        onClick={() => setSelectedUser(user)}
                        className={cn(
                          'flex w-full items-center gap-3 p-3 text-left hover:bg-muted/50 transition-colors',
                          selectedUser?.id === user.id && 'bg-muted'
                        )}
                      >
                        <Avatar className="h-9 w-9">
                          <AvatarImage src={user.avatarUrl} />
                          <AvatarFallback>{getInitials(user.fullName)}</AvatarFallback>
                        </Avatar>
                        <div className="flex-1 min-w-0">
                          <div className="font-medium truncate">{user.fullName}</div>
                          <div className="text-sm text-muted-foreground truncate">{user.email}</div>
                        </div>
                        {selectedUser?.id === user.id && (
                          <Check className="h-5 w-5 text-primary" />
                        )}
                      </button>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Selected User Display */}
          {selectedUser && (
            <div className="rounded-lg border bg-muted/30 p-3">
              <div className="flex items-center gap-3">
                <Avatar>
                  <AvatarImage src={selectedUser.avatarUrl} />
                  <AvatarFallback>{getInitials(selectedUser.fullName)}</AvatarFallback>
                </Avatar>
                <div className="flex-1">
                  <div className="font-medium">{selectedUser.fullName}</div>
                  <div className="text-sm text-muted-foreground">{selectedUser.email}</div>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setSelectedUser(null)}
                >
                  Change
                </Button>
              </div>
            </div>
          )}

          {/* Role Selection */}
          <div className="space-y-2">
            <label className="text-sm font-medium">Role</label>
            <Select value={selectedRole} onValueChange={(value) => setSelectedRole(value as TenantRole)}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Member">
                  <div>
                    <div className="font-medium">Member</div>
                    <div className="text-xs text-muted-foreground">Basic access to tenant resources</div>
                  </div>
                </SelectItem>
                <SelectItem value="Admin">
                  <div>
                    <div className="font-medium">Admin</div>
                    <div className="text-xs text-muted-foreground">Can manage members and settings</div>
                  </div>
                </SelectItem>
                <SelectItem value="Owner">
                  <div>
                    <div className="font-medium">Owner</div>
                    <div className="text-xs text-muted-foreground">Full control including billing</div>
                  </div>
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>

        {/* Error Display */}
        {addMemberMutation.error && (
          <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-3">
            <p className="text-sm text-destructive">
              {addMemberMutation.error.message ?? 'Failed to add member'}
            </p>
          </div>
        )}

        <DialogFooter>
          <Button variant="outline" onClick={handleClose}>
            Cancel
          </Button>
          <Button
            onClick={handleAddMember}
            disabled={!selectedUser || addMemberMutation.isPending}
          >
            {addMemberMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Add Member
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

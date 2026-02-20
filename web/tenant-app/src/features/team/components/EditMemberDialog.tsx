import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { Loader2 } from 'lucide-react';
import { apiClient } from '@/shared/api';
import { toast } from 'sonner';

interface Role {
  id: string;
  name: string;
  slug: string;
}

interface TeamMember {
  id: string;
  authId: string;
  email: string;
  displayName: string;
  jobTitle?: string;
  department?: string;
  phoneNumber?: string;
  status: string;
  roleIds: string[];
  roles?: Array<{
    id: string;
    name: string;
    slug: string;
  }>;
}

interface EditMemberDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => void;
  member: TeamMember | null;
  roles: Role[];
}

export function EditMemberDialog({
  open,
  onOpenChange,
  onSuccess,
  member,
  roles,
}: EditMemberDialogProps) {
  const { t } = useTranslation('team');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    displayName: '',
    jobTitle: '',
    department: '',
    phoneNumber: '',
    roleIds: [] as string[],
  });

  // Reset form when dialog opens or member changes
  useEffect(() => {
    if (open && member) {
      setFormData({
        displayName: member.displayName || '',
        jobTitle: member.jobTitle || '',
        department: member.department || '',
        phoneNumber: member.phoneNumber || '',
        roleIds: member.roleIds || [],
      });
      setError(null);
    }
  }, [open, member]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!member) return;

    setError(null);
    setIsLoading(true);

    try {
      await apiClient.put(`/members/${member.id}`, {
        displayName: formData.displayName,
        jobTitle: formData.jobTitle || undefined,
        department: formData.department || undefined,
        phoneNumber: formData.phoneNumber || undefined,
        roleIds: formData.roleIds,
      });

      toast.success(t('notifications.memberUpdated'));
      onSuccess();
      onOpenChange(false);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'An error occurred';
      setError(message);
      toast.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>{t('editDialog.title')}</DialogTitle>
          <DialogDescription>
            {t('editDialog.description', { name: member?.displayName || '' })}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          {error && (
            <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
              {error}
            </div>
          )}

          <div className="space-y-2">
            <label htmlFor="displayName" className="text-sm font-medium">
              {t('fields.displayName')} *
            </label>
            <Input
              id="displayName"
              value={formData.displayName}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, displayName: e.target.value }))
              }
              disabled={isLoading}
              required
            />
          </div>

          <div className="space-y-2">
            <label htmlFor="role" className="text-sm font-medium">
              {t('fields.role')} *
            </label>
            <Select
              value={formData.roleIds[0] || ''}
              onValueChange={(value) =>
                setFormData((prev) => ({ ...prev, roleIds: [value] }))
              }
              disabled={isLoading}
            >
              <SelectTrigger>
                <SelectValue placeholder={t('fields.rolePlaceholder')} />
              </SelectTrigger>
              <SelectContent>
                {roles.map((role) => (
                  <SelectItem key={role.id} value={role.id}>
                    {role.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <label htmlFor="jobTitle" className="text-sm font-medium">
              {t('fields.jobTitle')}
            </label>
            <Input
              id="jobTitle"
              value={formData.jobTitle}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, jobTitle: e.target.value }))
              }
              disabled={isLoading}
            />
          </div>

          <div className="space-y-2">
            <label htmlFor="department" className="text-sm font-medium">
              {t('fields.department')}
            </label>
            <Input
              id="department"
              value={formData.department}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, department: e.target.value }))
              }
              disabled={isLoading}
            />
          </div>

          <div className="space-y-2">
            <label htmlFor="phoneNumber" className="text-sm font-medium">
              {t('fields.phoneNumber')}
            </label>
            <Input
              id="phoneNumber"
              value={formData.phoneNumber}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, phoneNumber: e.target.value }))
              }
              disabled={isLoading}
            />
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isLoading}
            >
              {t('actions.cancel')}
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? (
                <>
                  <Loader2 className="me-2 h-4 w-4 animate-spin" />
                  {t('actions.saving')}
                </>
              ) : (
                t('actions.save')
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

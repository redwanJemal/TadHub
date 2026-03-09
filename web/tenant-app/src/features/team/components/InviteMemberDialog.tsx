import { useState } from 'react';
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
import { Label } from '@/shared/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { useCreateMember, useRoles } from '../hooks';
import { toast } from 'sonner';

interface InviteMemberDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function InviteMemberDialog({ open, onOpenChange }: InviteMemberDialogProps) {
  const { t } = useTranslation('team');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [roleId, setRoleId] = useState<string>('');
  const createMember = useCreateMember();
  const { data: rolesData } = useRoles();
  const roles = rolesData?.items ?? [];

  const canSubmit = email.trim() && password.length >= 8 && firstName.trim() && lastName.trim();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!canSubmit) return;

    try {
      await createMember.mutateAsync({
        email: email.trim(),
        password,
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        roleId: roleId || undefined,
      });

      toast.success(t('invite.memberCreated', 'Member created successfully'));
      setEmail('');
      setPassword('');
      setFirstName('');
      setLastName('');
      setRoleId('');
      onOpenChange(false);
    } catch {
      toast.error(t('invite.createFailed', 'Failed to create member'));
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>{t('invite.createTitle', 'Add Team Member')}</DialogTitle>
            <DialogDescription>{t('invite.createDescription', 'Create a new user account and add them to your team.')}</DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-2 gap-3">
              <div className="grid gap-2">
                <Label htmlFor="firstName">{t('invite.firstName', 'First Name')} *</Label>
                <Input
                  id="firstName"
                  placeholder={t('invite.firstNamePlaceholder', 'John')}
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                  required
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="lastName">{t('invite.lastName', 'Last Name')} *</Label>
                <Input
                  id="lastName"
                  placeholder={t('invite.lastNamePlaceholder', 'Doe')}
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                  required
                />
              </div>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="email">{t('invite.emailLabel')} *</Label>
              <Input
                id="email"
                type="email"
                placeholder={t('invite.emailPlaceholder')}
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="password">{t('invite.password', 'Password')} *</Label>
              <Input
                id="password"
                type="password"
                placeholder={t('invite.passwordPlaceholder', 'Minimum 8 characters')}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                minLength={8}
                required
              />
            </div>

            <div className="grid gap-2">
              <Label>{t('invite.roleLabel')}</Label>
              <Select value={roleId} onValueChange={setRoleId}>
                <SelectTrigger>
                  <SelectValue placeholder={t('invite.rolePlaceholder')} />
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
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              {t('common:cancel')}
            </Button>
            <Button type="submit" disabled={createMember.isPending || !canSubmit}>
              {createMember.isPending ? t('common:loading') : t('invite.createSubmit', 'Create Member')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

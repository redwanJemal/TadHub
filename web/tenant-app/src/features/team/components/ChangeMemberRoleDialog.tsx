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
import { Checkbox } from '@/shared/components/ui/checkbox';
import { Label } from '@/shared/components/ui/label';
import { useRoles, useAssignRole, useRemoveRole } from '../hooks';
import type { TenantMember } from '../types';

interface ChangeMemberRoleDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  member: TenantMember | null;
}

export function ChangeMemberRoleDialog({
  open,
  onOpenChange,
  member,
}: ChangeMemberRoleDialogProps) {
  const { t } = useTranslation('team');
  const { data: rolesData } = useRoles();
  const assignRole = useAssignRole();
  const removeRole = useRemoveRole();
  const [selectedRoleIds, setSelectedRoleIds] = useState<Set<string>>(new Set());
  const [saving, setSaving] = useState(false);

  const roles = rolesData?.items ?? [];
  const currentRoleIds = new Set(member?.roles.map((r) => r.id) ?? []);

  useEffect(() => {
    if (member) {
      setSelectedRoleIds(new Set(member.roles.map((r) => r.id)));
    }
  }, [member]);

  const handleToggleRole = (roleId: string) => {
    setSelectedRoleIds((prev) => {
      const next = new Set(prev);
      if (next.has(roleId)) {
        next.delete(roleId);
      } else {
        next.add(roleId);
      }
      return next;
    });
  };

  const handleSave = async () => {
    if (!member) return;
    setSaving(true);

    try {
      // Roles to add
      const toAdd = [...selectedRoleIds].filter((id) => !currentRoleIds.has(id));
      // Roles to remove
      const toRemove = [...currentRoleIds].filter((id) => !selectedRoleIds.has(id));

      await Promise.all([
        ...toAdd.map((roleId) =>
          assignRole.mutateAsync({ userId: member.userId, roleId })
        ),
        ...toRemove.map((roleId) =>
          removeRole.mutateAsync({ userId: member.userId, roleId })
        ),
      ]);

      onOpenChange(false);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>{t('changeRole.title')}</DialogTitle>
          <DialogDescription>
            {t('changeRole.description', { name: member?.fullName })}
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-3 py-4">
          {roles.map((role) => (
            <div key={role.id} className="flex items-center gap-3">
              <Checkbox
                id={`role-${role.id}`}
                checked={selectedRoleIds.has(role.id)}
                onCheckedChange={() => handleToggleRole(role.id)}
              />
              <Label htmlFor={`role-${role.id}`} className="flex-1 cursor-pointer">
                <span className="font-medium">{role.name}</span>
                {role.description && (
                  <span className="block text-xs text-muted-foreground">
                    {role.description}
                  </span>
                )}
              </Label>
            </div>
          ))}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            {t('common:cancel')}
          </Button>
          <Button onClick={handleSave} disabled={saving}>
            {saving ? t('common:loading') : t('changeRole.save')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

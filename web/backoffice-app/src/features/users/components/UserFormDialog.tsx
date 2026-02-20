import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { PasswordInput } from "@/shared/components/ui/password-input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Checkbox } from "@/shared/components/ui/checkbox";
import { Loader2 } from "lucide-react";
import { apiClient } from "@/shared/api";

interface UserTenant {
  id: string;
  name: string;
  slug: string;
  role: {
    id: string;
    name: string;
    slug: string;
  };
  membershipId?: string;
}

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phone?: string;
  isActive: boolean;
  tenants: UserTenant[];
}

interface Tenant {
  id: string;
  name: string;
  slug: string;
}

interface Role {
  id: string;
  name: string;
  slug: string;
}

interface UserFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => void;
  user?: User | null;
}

export function UserFormDialog({
  open,
  onOpenChange,
  onSuccess,
  user,
}: UserFormDialogProps) {
  const { t } = useTranslation("users");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [tenants, setTenants] = useState<Tenant[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [loadingOptions, setLoadingOptions] = useState(true);

  const [formData, setFormData] = useState({
    email: "",
    password: "",
    firstName: "",
    lastName: "",
    phone: "",
    tenantId: "",
    roleId: "",
    isActive: true,
  });

  const isEditMode = !!user;

  // Load tenants and roles
  useEffect(() => {
    async function loadOptions() {
      try {
        const [tenantsRes, rolesRes] = await Promise.all([
          apiClient.getWithMeta<Tenant[]>("/admin/tenants"),
          apiClient.getWithMeta<Role[]>("/admin/roles"),
        ]);
        setTenants(tenantsRes.data);
        setRoles(rolesRes.data);
      } catch (err) {
        console.error("Failed to load options:", err);
      } finally {
        setLoadingOptions(false);
      }
    }
    if (open) {
      loadOptions();
    }
  }, [open]);

  // Reset form when dialog opens/closes or user changes
  useEffect(() => {
    if (open && user) {
      const primaryTenant = user.tenants[0];
      setFormData({
        email: user.email,
        password: "",
        firstName: user.firstName,
        lastName: user.lastName,
        phone: user.phone || "",
        tenantId: primaryTenant?.id || "",
        roleId: primaryTenant?.role?.id || "",
        isActive: user.isActive,
      });
    } else if (open) {
      setFormData({
        email: "",
        password: "",
        firstName: "",
        lastName: "",
        phone: "",
        tenantId: tenants[0]?.id || "",
        roleId: roles.find((r) => r.slug === "user")?.id || roles[0]?.id || "",
        isActive: true,
      });
    }
    setError(null);
  }, [open, user, tenants, roles]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      if (isEditMode) {
        await apiClient.patch(`/admin/users/${user.id}`, {
          firstName: formData.firstName,
          lastName: formData.lastName,
          phone: formData.phone || undefined,
          isActive: formData.isActive,
          roleId: formData.roleId,
        });
      } else {
        if (!formData.password) {
          setError(t("errors.passwordRequired"));
          setIsLoading(false);
          return;
        }
        await apiClient.post("/admin/users", {
          email: formData.email,
          password: formData.password,
          firstName: formData.firstName,
          lastName: formData.lastName,
          phone: formData.phone || undefined,
          tenantId: formData.tenantId,
          roleId: formData.roleId,
        });
      }

      onSuccess();
      onOpenChange(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : "An error occurred");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>
            {isEditMode ? t("editDialog.title") : t("createDialog.title")}
          </DialogTitle>
          <DialogDescription>
            {isEditMode ? t("editDialog.description") : t("createDialog.description")}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          {error && (
            <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
              {error}
            </div>
          )}

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <label htmlFor="firstName" className="text-sm font-medium">
                {t("fields.firstName")} *
              </label>
              <Input
                id="firstName"
                value={formData.firstName}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, firstName: e.target.value }))
                }
                placeholder={t("fields.firstNamePlaceholder")}
                required
                disabled={isLoading}
              />
            </div>

            <div className="space-y-2">
              <label htmlFor="lastName" className="text-sm font-medium">
                {t("fields.lastName")} *
              </label>
              <Input
                id="lastName"
                value={formData.lastName}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, lastName: e.target.value }))
                }
                placeholder={t("fields.lastNamePlaceholder")}
                required
                disabled={isLoading}
              />
            </div>
          </div>

          <div className="space-y-2">
            <label htmlFor="email" className="text-sm font-medium">
              {t("fields.email")} *
            </label>
            <Input
              id="email"
              type="email"
              value={formData.email}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, email: e.target.value }))
              }
              placeholder={t("fields.emailPlaceholder")}
              required
              disabled={isLoading || isEditMode}
            />
          </div>

          {!isEditMode && (
            <div className="space-y-2">
              <label htmlFor="password" className="text-sm font-medium">
                {t("fields.password")} *
              </label>
              <PasswordInput
                id="password"
                value={formData.password}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, password: e.target.value }))
                }
                placeholder={t("fields.passwordPlaceholder")}
                required={!isEditMode}
                disabled={isLoading}
              />
            </div>
          )}

          <div className="space-y-2">
            <label htmlFor="phone" className="text-sm font-medium">
              {t("fields.phone")}
            </label>
            <Input
              id="phone"
              value={formData.phone}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, phone: e.target.value }))
              }
              placeholder={t("fields.phonePlaceholder")}
              disabled={isLoading}
            />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            {!isEditMode && (
              <div className="space-y-2">
                <label htmlFor="tenant" className="text-sm font-medium">
                  {t("fields.tenant")} *
                </label>
                <Select
                  value={formData.tenantId}
                  onValueChange={(value) =>
                    setFormData((prev) => ({ ...prev, tenantId: value }))
                  }
                  disabled={isLoading || loadingOptions}
                >
                  <SelectTrigger>
                    <SelectValue placeholder={t("fields.tenantPlaceholder")} />
                  </SelectTrigger>
                  <SelectContent>
                    {tenants.map((tenant) => (
                      <SelectItem key={tenant.id} value={tenant.id}>
                        {tenant.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            )}

            <div className="space-y-2">
              <label htmlFor="role" className="text-sm font-medium">
                {t("fields.role")} *
              </label>
              <Select
                value={formData.roleId}
                onValueChange={(value) =>
                  setFormData((prev) => ({ ...prev, roleId: value }))
                }
                disabled={isLoading || loadingOptions}
              >
                <SelectTrigger>
                  <SelectValue placeholder={t("fields.rolePlaceholder")} />
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

          {isEditMode && (
            <div className="flex items-center gap-2">
              <Checkbox
                id="isActive"
                checked={formData.isActive}
                onCheckedChange={(checked) =>
                  setFormData((prev) => ({ ...prev, isActive: !!checked }))
                }
                disabled={isLoading}
              />
              <label htmlFor="isActive" className="text-sm font-medium cursor-pointer">
                {t("fields.isActive")}
              </label>
            </div>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isLoading}
            >
              {t("actions.cancel")}
            </Button>
            <Button type="submit" disabled={isLoading || loadingOptions}>
              {isLoading ? (
                <>
                  <Loader2 className="me-2 h-4 w-4 animate-spin" />
                  {isEditMode ? t("actions.saving") : t("actions.creating")}
                </>
              ) : isEditMode ? (
                t("actions.save")
              ) : (
                t("actions.create")
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

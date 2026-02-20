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
import { Checkbox } from "@/shared/components/ui/checkbox";
import { Badge } from "@/shared/components/ui/badge";
import { Loader2 } from "lucide-react";
import { apiClient } from "@/shared/api";

interface Permission {
  id: string;
  name: string;
  description: string;
  module: string;
  action: string;
}

interface Role {
  id: string;
  name: string;
  slug: string;
  description?: string;
  permissions: Permission[];
  isSystem: boolean;
}

interface RoleFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => void;
  role?: Role | null; // null = create mode, Role = edit mode
}

export function RoleFormDialog({
  open,
  onOpenChange,
  onSuccess,
  role,
}: RoleFormDialogProps) {
  const { t } = useTranslation("roles");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [permissions, setPermissions] = useState<Permission[]>([]);
  const [loadingPermissions, setLoadingPermissions] = useState(true);

  const [formData, setFormData] = useState({
    name: "",
    slug: "",
    description: "",
    permissionIds: [] as string[],
  });

  const isEditMode = !!role;

  // Load permissions
  useEffect(() => {
    async function loadPermissions() {
      try {
        const data = await apiClient.get<Permission[]>("/admin/permissions");
        setPermissions(data);
      } catch (err) {
        console.error("Failed to load permissions:", err);
      } finally {
        setLoadingPermissions(false);
      }
    }
    if (open) {
      loadPermissions();
    }
  }, [open]);

  // Reset form when dialog opens/closes or role changes
  useEffect(() => {
    if (open && role) {
      setFormData({
        name: role.name,
        slug: role.slug,
        description: role.description || "",
        permissionIds: role.permissions.map((p) => p.id),
      });
    } else if (open) {
      setFormData({
        name: "",
        slug: "",
        description: "",
        permissionIds: [],
      });
    }
    setError(null);
  }, [open, role]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      if (isEditMode && role) {
        await apiClient.patch(`/admin/roles/${role.id}`, {
          name: formData.name,
          description: formData.description || undefined,
          permissionIds: formData.permissionIds,
        });
      } else {
        await apiClient.post("/admin/roles", {
          name: formData.name,
          slug: formData.slug,
          description: formData.description || undefined,
          permissionIds: formData.permissionIds,
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

  const generateSlug = (name: string) => {
    return name
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, "-")
      .replace(/^-|-$/g, "");
  };

  const handleNameChange = (name: string) => {
    setFormData((prev) => ({
      ...prev,
      name,
      slug: !isEditMode && prev.slug === generateSlug(prev.name) 
        ? generateSlug(name) 
        : prev.slug,
    }));
  };

  const togglePermission = (permissionId: string) => {
    setFormData((prev) => ({
      ...prev,
      permissionIds: prev.permissionIds.includes(permissionId)
        ? prev.permissionIds.filter((id) => id !== permissionId)
        : [...prev.permissionIds, permissionId],
    }));
  };

  const toggleModule = (module: string) => {
    const modulePermissions = permissions.filter((p) => p.module === module);
    const moduleIds = modulePermissions.map((p) => p.id);
    const allSelected = moduleIds.every((id) => formData.permissionIds.includes(id));

    setFormData((prev) => ({
      ...prev,
      permissionIds: allSelected
        ? prev.permissionIds.filter((id) => !moduleIds.includes(id))
        : [...new Set([...prev.permissionIds, ...moduleIds])],
    }));
  };

  // Group permissions by module
  const permissionsByModule = permissions.reduce<Record<string, Permission[]>>((acc, perm) => {
    const existing = acc[perm.module] ?? [];
    acc[perm.module] = [...existing, perm];
    return acc;
  }, {});

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px] max-h-[85vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            {isEditMode ? t("editDialog.title") : t("createDialog.title")}
          </DialogTitle>
          <DialogDescription>
            {isEditMode ? t("editDialog.description") : t("createDialog.description")}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-6">
          {error && (
            <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
              {error}
            </div>
          )}

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <label htmlFor="name" className="text-sm font-medium">
                {t("fields.name")} *
              </label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => handleNameChange(e.target.value)}
                placeholder={t("fields.namePlaceholder")}
                required
                disabled={isLoading || (isEditMode && role?.isSystem)}
              />
            </div>

            <div className="space-y-2">
              <label htmlFor="slug" className="text-sm font-medium">
                {t("fields.slug")} *
              </label>
              <Input
                id="slug"
                value={formData.slug}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, slug: e.target.value }))
                }
                placeholder={t("fields.slugPlaceholder")}
                required
                disabled={isLoading || isEditMode}
              />
            </div>
          </div>

          <div className="space-y-2">
            <label htmlFor="description" className="text-sm font-medium">
              {t("fields.description")}
            </label>
            <Input
              id="description"
              value={formData.description}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, description: e.target.value }))
              }
              placeholder={t("fields.descriptionPlaceholder")}
              disabled={isLoading}
            />
          </div>

          {/* Permissions Section */}
          <div className="space-y-4">
            <label className="text-sm font-medium">{t("fields.permissions")}</label>
            
            {loadingPermissions ? (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
              </div>
            ) : (
              <div className="space-y-4 rounded-lg border p-4">
                {Object.entries(permissionsByModule).map(([module, perms]) => {
                  const moduleIds = perms.map((p) => p.id);
                  const allSelected = moduleIds.every((id) =>
                    formData.permissionIds.includes(id)
                  );
                  const someSelected =
                    moduleIds.some((id) => formData.permissionIds.includes(id)) &&
                    !allSelected;

                  return (
                    <div key={module} className="space-y-2">
                      <div className="flex items-center gap-2">
                        <Checkbox
                          checked={allSelected}
                          indeterminate={someSelected}
                          onCheckedChange={() => toggleModule(module)}
                          disabled={isLoading || (isEditMode && role?.isSystem)}
                        />
                        <span className="font-medium capitalize">{module}</span>
                        <Badge variant="secondary" className="text-xs">
                          {perms.length}
                        </Badge>
                      </div>
                      <div className="ms-6 grid gap-2 sm:grid-cols-2">
                        {perms.map((perm) => (
                          <label
                            key={perm.id}
                            className="flex items-center gap-2 text-sm cursor-pointer"
                          >
                            <Checkbox
                              checked={formData.permissionIds.includes(perm.id)}
                              onCheckedChange={() => togglePermission(perm.id)}
                              disabled={isLoading || (isEditMode && role?.isSystem)}
                            />
                            <span className="text-muted-foreground">
                              {perm.action}
                            </span>
                          </label>
                        ))}
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isLoading}
            >
              {t("actions.cancel")}
            </Button>
            <Button 
              type="submit" 
              disabled={isLoading || (isEditMode && role?.isSystem)}
            >
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

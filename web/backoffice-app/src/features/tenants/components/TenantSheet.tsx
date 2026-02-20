import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetFooter,
  SheetHeader,
  SheetTitle,
} from "@/shared/components/ui/sheet";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Checkbox } from "@/shared/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Loader2, Building2 } from "lucide-react";
import { apiClient } from "@/shared/api";

interface Plan {
  id: string;
  name: string;
  slug: string;
}

interface Tenant {
  id: string;
  name: string;
  slug: string;
  domain?: string;
  plan?: Plan;
  isActive: boolean;
  settings?: {
    maxUsers?: number;
    allowRegistration?: boolean;
    enableApi?: boolean;
  };
}

interface TenantSheetProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => void;
  tenant?: Tenant | null;
}

export function TenantSheet({
  open,
  onOpenChange,
  onSuccess,
  tenant,
}: TenantSheetProps) {
  const { t } = useTranslation("tenants");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [plans, setPlans] = useState<Plan[]>([]);
  const [loadingPlans, setLoadingPlans] = useState(true);

  const [formData, setFormData] = useState({
    name: "",
    slug: "",
    domain: "",
    planId: "",
    isActive: true,
    maxUsers: 10,
    allowRegistration: true,
    enableApi: false,
  });

  const isEditMode = !!tenant;

  // Load plans
  useEffect(() => {
    async function loadPlans() {
      try {
        const data = await apiClient.get<Plan[]>("/admin/plans");
        setPlans(data);
      } catch (err) {
        console.error("Failed to load plans:", err);
        // Fallback plans if API not available
        setPlans([
          { id: "free", name: "Free", slug: "free" },
          { id: "starter", name: "Starter", slug: "starter" },
          { id: "pro", name: "Pro", slug: "pro" },
          { id: "enterprise", name: "Enterprise", slug: "enterprise" },
        ]);
      } finally {
        setLoadingPlans(false);
      }
    }
    if (open) {
      loadPlans();
    }
  }, [open]);

  // Reset form when dialog opens/closes or tenant changes
  useEffect(() => {
    if (open && tenant) {
      setFormData({
        name: tenant.name,
        slug: tenant.slug,
        domain: tenant.domain || "",
        planId: tenant.plan?.id || "",
        isActive: tenant.isActive,
        maxUsers: tenant.settings?.maxUsers || 10,
        allowRegistration: tenant.settings?.allowRegistration ?? true,
        enableApi: tenant.settings?.enableApi ?? false,
      });
    } else if (open) {
      setFormData({
        name: "",
        slug: "",
        domain: "",
        planId: plans[0]?.id || "",
        isActive: true,
        maxUsers: 10,
        allowRegistration: true,
        enableApi: false,
      });
    }
    setError(null);
  }, [open, tenant, plans]);

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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      const payload = {
        name: formData.name,
        slug: formData.slug,
        domain: formData.domain || undefined,
        planId: formData.planId || undefined,
        isActive: formData.isActive,
        settings: {
          maxUsers: formData.maxUsers,
          allowRegistration: formData.allowRegistration,
          enableApi: formData.enableApi,
        },
      };

      if (isEditMode && tenant) {
        await apiClient.patch(`/admin/tenants/${tenant.id}`, payload);
      } else {
        await apiClient.post("/admin/tenants", payload);
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
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="sm:max-w-lg overflow-y-auto">
        <SheetHeader>
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
              <Building2 className="h-5 w-5 text-primary" />
            </div>
            <div>
              <SheetTitle>
                {isEditMode ? t("sheet.editTitle") : t("sheet.createTitle")}
              </SheetTitle>
              <SheetDescription>
                {isEditMode ? t("sheet.editDescription") : t("sheet.createDescription")}
              </SheetDescription>
            </div>
          </div>
        </SheetHeader>

        <form onSubmit={handleSubmit} className="mt-6 space-y-6">
          {error && (
            <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
              {error}
            </div>
          )}

          {/* Basic Information */}
          <div className="space-y-4">
            <h3 className="text-sm font-medium text-muted-foreground">
              {t("sheet.basicInfo")}
            </h3>

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
                disabled={isLoading}
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
              <p className="text-xs text-muted-foreground">
                {t("fields.slugHint")}
              </p>
            </div>

            <div className="space-y-2">
              <label htmlFor="domain" className="text-sm font-medium">
                {t("fields.domain")}
              </label>
              <Input
                id="domain"
                value={formData.domain}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, domain: e.target.value }))
                }
                placeholder={t("fields.domainPlaceholder")}
                disabled={isLoading}
              />
              <p className="text-xs text-muted-foreground">
                {t("fields.domainHint")}
              </p>
            </div>
          </div>

          {/* Subscription */}
          <div className="space-y-4">
            <h3 className="text-sm font-medium text-muted-foreground">
              {t("sheet.subscription")}
            </h3>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <label htmlFor="plan" className="text-sm font-medium">
                  {t("fields.plan")}
                </label>
                <Select
                  value={formData.planId}
                  onValueChange={(value) =>
                    setFormData((prev) => ({ ...prev, planId: value }))
                  }
                  disabled={isLoading || loadingPlans}
                >
                  <SelectTrigger>
                    <SelectValue placeholder={t("fields.planPlaceholder")} />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="">{t("fields.noPlan")}</SelectItem>
                    {plans.map((plan) => (
                      <SelectItem key={plan.id} value={plan.id}>
                        {plan.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <label htmlFor="status" className="text-sm font-medium">
                  {t("fields.status")}
                </label>
                <Select
                  value={formData.isActive ? "active" : "inactive"}
                  onValueChange={(value) =>
                    setFormData((prev) => ({
                      ...prev,
                      isActive: value === "active",
                    }))
                  }
                  disabled={isLoading}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="active">{t("status.active")}</SelectItem>
                    <SelectItem value="inactive">{t("status.inactive")}</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>

          {/* Settings */}
          <div className="space-y-4">
            <h3 className="text-sm font-medium text-muted-foreground">
              {t("sheet.settings")}
            </h3>

            <div className="space-y-3">
              <label className="flex items-center gap-3 cursor-pointer">
                <Checkbox
                  checked={formData.allowRegistration}
                  onCheckedChange={(checked) =>
                    setFormData((prev) => ({
                      ...prev,
                      allowRegistration: !!checked,
                    }))
                  }
                  disabled={isLoading}
                />
                <div>
                  <p className="text-sm font-medium">{t("settings.allowRegistration")}</p>
                  <p className="text-xs text-muted-foreground">
                    {t("settings.allowRegistrationHint")}
                  </p>
                </div>
              </label>

              <label className="flex items-center gap-3 cursor-pointer">
                <Checkbox
                  checked={formData.enableApi}
                  onCheckedChange={(checked) =>
                    setFormData((prev) => ({
                      ...prev,
                      enableApi: !!checked,
                    }))
                  }
                  disabled={isLoading}
                />
                <div>
                  <p className="text-sm font-medium">{t("settings.enableApi")}</p>
                  <p className="text-xs text-muted-foreground">
                    {t("settings.enableApiHint")}
                  </p>
                </div>
              </label>
            </div>

            <div className="space-y-2">
              <label htmlFor="maxUsers" className="text-sm font-medium">
                {t("settings.maxUsers")}
              </label>
              <Input
                id="maxUsers"
                type="number"
                min={1}
                max={10000}
                value={formData.maxUsers}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    maxUsers: parseInt(e.target.value, 10) || 10,
                  }))
                }
                disabled={isLoading}
              />
            </div>
          </div>

          <SheetFooter className="pt-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isLoading}
            >
              {t("actions.cancel")}
            </Button>
            <Button type="submit" disabled={isLoading}>
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
          </SheetFooter>
        </form>
      </SheetContent>
    </Sheet>
  );
}

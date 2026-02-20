import { useState } from "react";
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
import { Loader2, Building2, User } from "lucide-react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/shared/components/ui/tabs";

interface CreateTenantDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => void;
}

export function CreateTenantDialog({
  open,
  onOpenChange,
  onSuccess,
}: CreateTenantDialogProps) {
  const { t } = useTranslation("tenants");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState("tenant");

  const [formData, setFormData] = useState({
    // Tenant info
    name: "",
    slug: "",
    domain: "",
    timezone: "UTC",
    locale: "en",
    // Owner info
    ownerEmail: "",
    ownerPassword: "",
    ownerFirstName: "",
    ownerLastName: "",
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    // Validate owner fields
    if (!formData.ownerEmail || !formData.ownerPassword || !formData.ownerFirstName || !formData.ownerLastName) {
      setError(t("createDialog.ownerRequired") || "Owner information is required");
      setActiveTab("owner");
      return;
    }

    if (formData.ownerPassword.length < 8) {
      setError(t("createDialog.passwordMinLength") || "Password must be at least 8 characters");
      setActiveTab("owner");
      return;
    }

    setIsLoading(true);

    try {
      const token = localStorage.getItem("forgebase_admin_token");
      const response = await fetch("/api/v1/admin/tenants", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          name: formData.name,
          slug: formData.slug,
          domain: formData.domain || undefined,
          timezone: formData.timezone,
          locale: formData.locale,
          owner: {
            email: formData.ownerEmail,
            password: formData.ownerPassword,
            firstName: formData.ownerFirstName,
            lastName: formData.ownerLastName,
          },
        }),
      });

      const result = await response.json();

      if (!response.ok) {
        throw new Error(result.error?.message || "Failed to create tenant");
      }

      // Reset form
      setFormData({
        name: "",
        slug: "",
        domain: "",
        timezone: "UTC",
        locale: "en",
        ownerEmail: "",
        ownerPassword: "",
        ownerFirstName: "",
        ownerLastName: "",
      });
      setActiveTab("tenant");

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
      slug: prev.slug === generateSlug(prev.name) ? generateSlug(name) : prev.slug,
    }));
  };

  const handleClose = () => {
    setError(null);
    setActiveTab("tenant");
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>{t("createDialog.title")}</DialogTitle>
          <DialogDescription>{t("createDialog.description")}</DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit}>
          {error && (
            <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive mb-4">
              {error}
            </div>
          )}

          <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
            <TabsList className="grid w-full grid-cols-2 mb-4">
              <TabsTrigger value="tenant" className="flex items-center gap-2">
                <Building2 className="h-4 w-4" />
                {t("createDialog.tenantTab") || "Tenant Info"}
              </TabsTrigger>
              <TabsTrigger value="owner" className="flex items-center gap-2">
                <User className="h-4 w-4" />
                {t("createDialog.ownerTab") || "Owner Account"}
              </TabsTrigger>
            </TabsList>

            <TabsContent value="tenant" className="space-y-4">
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
                  disabled={isLoading}
                />
                <p className="text-xs text-muted-foreground">
                  {t("fields.slugHelp")}
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
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <label htmlFor="timezone" className="text-sm font-medium">
                    {t("fields.timezone")}
                  </label>
                  <Input
                    id="timezone"
                    value={formData.timezone}
                    onChange={(e) =>
                      setFormData((prev) => ({ ...prev, timezone: e.target.value }))
                    }
                    placeholder="UTC"
                    disabled={isLoading}
                  />
                </div>
                <div className="space-y-2">
                  <label htmlFor="locale" className="text-sm font-medium">
                    {t("fields.locale")}
                  </label>
                  <Input
                    id="locale"
                    value={formData.locale}
                    onChange={(e) =>
                      setFormData((prev) => ({ ...prev, locale: e.target.value }))
                    }
                    placeholder="en"
                    disabled={isLoading}
                  />
                </div>
              </div>
            </TabsContent>

            <TabsContent value="owner" className="space-y-4">
              <div className="rounded-md bg-muted p-3 text-sm text-muted-foreground mb-4">
                {t("createDialog.ownerHelp") || "The owner will have full admin access to the tenant workspace."}
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <label htmlFor="ownerFirstName" className="text-sm font-medium">
                    {t("fields.firstName") || "First Name"} *
                  </label>
                  <Input
                    id="ownerFirstName"
                    value={formData.ownerFirstName}
                    onChange={(e) =>
                      setFormData((prev) => ({ ...prev, ownerFirstName: e.target.value }))
                    }
                    placeholder="John"
                    required
                    disabled={isLoading}
                  />
                </div>
                <div className="space-y-2">
                  <label htmlFor="ownerLastName" className="text-sm font-medium">
                    {t("fields.lastName") || "Last Name"} *
                  </label>
                  <Input
                    id="ownerLastName"
                    value={formData.ownerLastName}
                    onChange={(e) =>
                      setFormData((prev) => ({ ...prev, ownerLastName: e.target.value }))
                    }
                    placeholder="Doe"
                    required
                    disabled={isLoading}
                  />
                </div>
              </div>

              <div className="space-y-2">
                <label htmlFor="ownerEmail" className="text-sm font-medium">
                  {t("fields.email") || "Email"} *
                </label>
                <Input
                  id="ownerEmail"
                  type="email"
                  value={formData.ownerEmail}
                  onChange={(e) =>
                    setFormData((prev) => ({ ...prev, ownerEmail: e.target.value }))
                  }
                  placeholder="owner@company.com"
                  required
                  disabled={isLoading}
                />
              </div>

              <div className="space-y-2">
                <label htmlFor="ownerPassword" className="text-sm font-medium">
                  {t("fields.password") || "Password"} *
                </label>
                <Input
                  id="ownerPassword"
                  type="password"
                  value={formData.ownerPassword}
                  onChange={(e) =>
                    setFormData((prev) => ({ ...prev, ownerPassword: e.target.value }))
                  }
                  placeholder="••••••••"
                  required
                  minLength={8}
                  disabled={isLoading}
                />
                <p className="text-xs text-muted-foreground">
                  {t("fields.passwordHelp") || "Minimum 8 characters"}
                </p>
              </div>
            </TabsContent>
          </Tabs>

          <DialogFooter className="mt-6">
            <Button
              type="button"
              variant="outline"
              onClick={handleClose}
              disabled={isLoading}
            >
              {t("actions.cancel")}
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? (
                <>
                  <Loader2 className="me-2 h-4 w-4 animate-spin" />
                  {t("actions.creating")}
                </>
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

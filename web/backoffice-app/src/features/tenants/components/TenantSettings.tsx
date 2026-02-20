import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/shared/components/ui/card";
import { Checkbox } from "@/shared/components/ui/checkbox";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Loader2, Save, AlertTriangle, Trash2, Power, PowerOff } from "lucide-react";
import { apiClient } from "@/shared/api";

interface Tenant {
  id: string;
  name: string;
  isActive: boolean;
  settings?: {
    maxUsers?: number;
    allowRegistration?: boolean;
    enableApi?: boolean;
  };
}

interface TenantSettingsProps {
  tenant: Tenant;
  onUpdate: () => void;
}

export function TenantSettings({ tenant, onUpdate }: TenantSettingsProps) {
  const { t } = useTranslation("tenants");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [suspendDialogOpen, setSuspendDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  const [settings, setSettings] = useState({
    maxUsers: tenant.settings?.maxUsers || 10,
    allowRegistration: tenant.settings?.allowRegistration ?? true,
    enableApi: tenant.settings?.enableApi ?? false,
  });

  const handleSaveSettings = async () => {
    setIsLoading(true);
    setError(null);
    setSuccess(null);

    try {
      await apiClient.patch(`/admin/tenants/${tenant.id}`, {
        settings,
      });
      setSuccess(t("settings.saveSuccess"));
      onUpdate();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save settings");
    } finally {
      setIsLoading(false);
    }
  };

  const handleToggleStatus = async () => {
    setIsLoading(true);
    setError(null);

    try {
      if (tenant.isActive) {
        await apiClient.post(`/admin/tenants/${tenant.id}/suspend`);
      } else {
        await apiClient.post(`/admin/tenants/${tenant.id}/activate`);
      }
      onUpdate();
      setSuspendDialogOpen(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update tenant status");
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async () => {
    setIsLoading(true);
    setError(null);

    try {
      await apiClient.delete(`/admin/tenants/${tenant.id}`);
      // Navigate back to tenants list
      window.location.href = "/tenants";
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete tenant");
      setIsLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* General Settings */}
      <Card>
        <CardHeader>
          <CardTitle>{t("settings.general.title")}</CardTitle>
          <CardDescription>{t("settings.general.description")}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {error && (
            <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
              {error}
            </div>
          )}
          {success && (
            <div className="rounded-md bg-green-500/10 p-3 text-sm text-green-600">
              {success}
            </div>
          )}

          <div className="space-y-4">
            <label className="flex items-start gap-3 cursor-pointer">
              <Checkbox
                checked={settings.allowRegistration}
                onCheckedChange={(checked) =>
                  setSettings((prev) => ({ ...prev, allowRegistration: !!checked }))
                }
                disabled={isLoading}
                className="mt-0.5"
              />
              <div>
                <p className="font-medium">{t("settings.allowRegistration")}</p>
                <p className="text-sm text-muted-foreground">
                  {t("settings.allowRegistrationHint")}
                </p>
              </div>
            </label>

            <label className="flex items-start gap-3 cursor-pointer">
              <Checkbox
                checked={settings.enableApi}
                onCheckedChange={(checked) =>
                  setSettings((prev) => ({ ...prev, enableApi: !!checked }))
                }
                disabled={isLoading}
                className="mt-0.5"
              />
              <div>
                <p className="font-medium">{t("settings.enableApi")}</p>
                <p className="text-sm text-muted-foreground">
                  {t("settings.enableApiHint")}
                </p>
              </div>
            </label>
          </div>

          <div className="space-y-2 max-w-xs">
            <label className="text-sm font-medium">{t("settings.maxUsers")}</label>
            <Input
              type="number"
              min={1}
              max={10000}
              value={settings.maxUsers}
              onChange={(e) =>
                setSettings((prev) => ({
                  ...prev,
                  maxUsers: parseInt(e.target.value, 10) || 10,
                }))
              }
              disabled={isLoading}
            />
            <p className="text-xs text-muted-foreground">{t("settings.maxUsersHint")}</p>
          </div>

          <div className="flex justify-end">
            <Button onClick={handleSaveSettings} disabled={isLoading}>
              {isLoading ? (
                <>
                  <Loader2 className="me-2 h-4 w-4 animate-spin" />
                  {t("settings.saving")}
                </>
              ) : (
                <>
                  <Save className="me-2 h-4 w-4" />
                  {t("settings.save")}
                </>
              )}
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Danger Zone */}
      <Card className="border-destructive/50">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-destructive">
            <AlertTriangle className="h-5 w-5" />
            {t("settings.dangerZone.title")}
          </CardTitle>
          <CardDescription>{t("settings.dangerZone.description")}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Suspend/Activate */}
          <div className="flex items-center justify-between p-4 border rounded-lg">
            <div>
              <p className="font-medium">
                {tenant.isActive
                  ? t("settings.dangerZone.suspend.title")
                  : t("settings.dangerZone.activate.title")}
              </p>
              <p className="text-sm text-muted-foreground">
                {tenant.isActive
                  ? t("settings.dangerZone.suspend.description")
                  : t("settings.dangerZone.activate.description")}
              </p>
            </div>
            <Button
              variant={tenant.isActive ? "outline" : "default"}
              onClick={() => setSuspendDialogOpen(true)}
            >
              {tenant.isActive ? (
                <>
                  <PowerOff className="me-2 h-4 w-4" />
                  {t("settings.dangerZone.suspend.button")}
                </>
              ) : (
                <>
                  <Power className="me-2 h-4 w-4" />
                  {t("settings.dangerZone.activate.button")}
                </>
              )}
            </Button>
          </div>

          {/* Delete */}
          <div className="flex items-center justify-between p-4 border border-destructive/50 rounded-lg bg-destructive/5">
            <div>
              <p className="font-medium text-destructive">
                {t("settings.dangerZone.delete.title")}
              </p>
              <p className="text-sm text-muted-foreground">
                {t("settings.dangerZone.delete.description")}
              </p>
            </div>
            <Button variant="destructive" onClick={() => setDeleteDialogOpen(true)}>
              <Trash2 className="me-2 h-4 w-4" />
              {t("settings.dangerZone.delete.button")}
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Suspend/Activate Confirmation */}
      <Dialog open={suspendDialogOpen} onOpenChange={setSuspendDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {tenant.isActive
                ? t("settings.dangerZone.suspend.confirmTitle")
                : t("settings.dangerZone.activate.confirmTitle")}
            </DialogTitle>
            <DialogDescription>
              {tenant.isActive
                ? t("settings.dangerZone.suspend.confirmDescription", { name: tenant.name })
                : t("settings.dangerZone.activate.confirmDescription", { name: tenant.name })}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setSuspendDialogOpen(false)} disabled={isLoading}>
              {t("actions.cancel")}
            </Button>
            <Button
              variant={tenant.isActive ? "destructive" : "default"}
              onClick={handleToggleStatus}
              disabled={isLoading}
            >
              {isLoading ? (
                <Loader2 className="me-2 h-4 w-4 animate-spin" />
              ) : tenant.isActive ? (
                t("settings.dangerZone.suspend.button")
              ) : (
                t("settings.dangerZone.activate.button")
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t("settings.dangerZone.delete.confirmTitle")}</DialogTitle>
            <DialogDescription>
              {t("settings.dangerZone.delete.confirmDescription", { name: tenant.name })}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteDialogOpen(false)} disabled={isLoading}>
              {t("actions.cancel")}
            </Button>
            <Button variant="destructive" onClick={handleDelete} disabled={isLoading}>
              {isLoading ? (
                <Loader2 className="me-2 h-4 w-4 animate-spin" />
              ) : (
                t("settings.dangerZone.delete.button")
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

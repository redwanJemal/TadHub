import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/shared/components/ui/card";
import { Badge } from "@/shared/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Loader2, CreditCard, Calendar, AlertTriangle, CheckCircle } from "lucide-react";
import { apiClient } from "@/shared/api";

interface Plan {
  id: string;
  name: string;
  slug: string;
  price?: number;
  interval?: string;
  features?: string[];
}

interface Tenant {
  id: string;
  name: string;
  plan?: Plan;
  subscription?: {
    id: string;
    status: string;
    currentPeriodEnd?: string;
  };
}

interface TenantSubscriptionProps {
  tenant: Tenant;
  onUpdate: () => void;
}

export function TenantSubscription({ tenant, onUpdate }: TenantSubscriptionProps) {
  const { t } = useTranslation("tenants");
  const [plans, setPlans] = useState<Plan[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [loadingPlans, setLoadingPlans] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [override, setOverride] = useState({
    planId: tenant.plan?.id || "",
    status: tenant.subscription?.status || "active",
    validUntil: tenant.subscription?.currentPeriodEnd?.split("T")[0] || "",
    maxUsersOverride: "",
    notes: "",
  });

  // Load plans
  useEffect(() => {
    async function loadPlans() {
      try {
        const data = await apiClient.get<Plan[]>("/admin/plans");
        setPlans(data);
      } catch {
        // Fallback plans
        setPlans([
          { id: "free", name: "Free", slug: "free", price: 0, interval: "month" },
          { id: "starter", name: "Starter", slug: "starter", price: 19, interval: "month" },
          { id: "pro", name: "Pro", slug: "pro", price: 49, interval: "month" },
          { id: "enterprise", name: "Enterprise", slug: "enterprise", price: 199, interval: "month" },
        ]);
      } finally {
        setLoadingPlans(false);
      }
    }
    loadPlans();
  }, []);

  const handleApplyOverride = async () => {
    setIsLoading(true);
    setError(null);
    setSuccess(null);

    try {
      await apiClient.post(`/admin/tenants/${tenant.id}/subscription`, {
        planId: override.planId || undefined,
        status: override.status,
        validUntil: override.validUntil || undefined,
        maxUsersOverride: override.maxUsersOverride
          ? parseInt(override.maxUsersOverride, 10)
          : undefined,
        notes: override.notes || undefined,
      });

      setSuccess(t("subscription.updateSuccess"));
      onUpdate();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update subscription");
    } finally {
      setIsLoading(false);
    }
  };

  const currentPlan = plans.find((p) => p.id === tenant.plan?.id);

  return (
    <div className="space-y-6">
      {/* Current Subscription */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CreditCard className="h-5 w-5" />
            {t("subscription.current")}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 p-4 bg-muted/50 rounded-lg">
            <div className="flex items-center gap-4">
              <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary/10">
                <CreditCard className="h-6 w-6 text-primary" />
              </div>
              <div>
                <h3 className="text-lg font-semibold">
                  {currentPlan?.name || tenant.plan?.name || t("noPlan")}
                </h3>
                {currentPlan?.price !== undefined && (
                  <p className="text-sm text-muted-foreground">
                    ${currentPlan.price}/{currentPlan.interval || "month"}
                  </p>
                )}
              </div>
            </div>
            <div className="flex items-center gap-3">
              {tenant.subscription?.status && (
                <Badge
                  variant={
                    tenant.subscription.status === "active"
                      ? "success"
                      : tenant.subscription.status === "past_due"
                      ? "destructive"
                      : "secondary"
                  }
                >
                  {tenant.subscription.status}
                </Badge>
              )}
              {tenant.subscription?.currentPeriodEnd && (
                <span className="text-sm text-muted-foreground flex items-center gap-1">
                  <Calendar className="h-4 w-4" />
                  {t("subscription.renewsOn")}{" "}
                  {new Date(tenant.subscription.currentPeriodEnd).toLocaleDateString()}
                </span>
              )}
            </div>
          </div>

          {/* Plan Features */}
          {currentPlan?.features && currentPlan.features.length > 0 && (
            <div className="mt-4">
              <h4 className="text-sm font-medium mb-2">{t("subscription.features")}</h4>
              <ul className="space-y-1">
                {currentPlan.features.map((feature, i) => (
                  <li key={i} className="flex items-center gap-2 text-sm text-muted-foreground">
                    <CheckCircle className="h-4 w-4 text-green-500" />
                    {feature}
                  </li>
                ))}
              </ul>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Manual Override */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-orange-500" />
            {t("subscription.manualOverride")}
          </CardTitle>
          <CardDescription>{t("subscription.manualOverrideHint")}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
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

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <label className="text-sm font-medium">{t("subscription.plan")}</label>
              <Select
                value={override.planId}
                onValueChange={(value) =>
                  setOverride((prev) => ({ ...prev, planId: value }))
                }
                disabled={isLoading || loadingPlans}
              >
                <SelectTrigger>
                  <SelectValue placeholder={t("subscription.selectPlan")} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">{t("noPlan")}</SelectItem>
                  {plans.map((plan) => (
                    <SelectItem key={plan.id} value={plan.id}>
                      {plan.name}
                      {plan.price !== undefined && ` - $${plan.price}/${plan.interval}`}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">{t("subscription.status")}</label>
              <Select
                value={override.status}
                onValueChange={(value) =>
                  setOverride((prev) => ({ ...prev, status: value }))
                }
                disabled={isLoading}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="active">{t("subscription.statuses.active")}</SelectItem>
                  <SelectItem value="trialing">{t("subscription.statuses.trialing")}</SelectItem>
                  <SelectItem value="past_due">{t("subscription.statuses.pastDue")}</SelectItem>
                  <SelectItem value="canceled">{t("subscription.statuses.canceled")}</SelectItem>
                  <SelectItem value="unpaid">{t("subscription.statuses.unpaid")}</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">{t("subscription.validUntil")}</label>
              <Input
                type="date"
                value={override.validUntil}
                onChange={(e) =>
                  setOverride((prev) => ({ ...prev, validUntil: e.target.value }))
                }
                disabled={isLoading}
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">{t("subscription.maxUsersOverride")}</label>
              <Input
                type="number"
                min={0}
                placeholder={t("subscription.maxUsersOverridePlaceholder")}
                value={override.maxUsersOverride}
                onChange={(e) =>
                  setOverride((prev) => ({ ...prev, maxUsersOverride: e.target.value }))
                }
                disabled={isLoading}
              />
            </div>
          </div>

          <div className="space-y-2">
            <label className="text-sm font-medium">{t("subscription.notes")}</label>
            <Input
              placeholder={t("subscription.notesPlaceholder")}
              value={override.notes}
              onChange={(e) =>
                setOverride((prev) => ({ ...prev, notes: e.target.value }))
              }
              disabled={isLoading}
            />
          </div>

          <div className="flex justify-end">
            <Button onClick={handleApplyOverride} disabled={isLoading}>
              {isLoading ? (
                <>
                  <Loader2 className="me-2 h-4 w-4 animate-spin" />
                  {t("subscription.applying")}
                </>
              ) : (
                t("subscription.applyOverride")
              )}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

import { useState, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useParams, useNavigate } from "react-router-dom";
import { Button } from "@/shared/components/ui/button";
import { Badge } from "@/shared/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/shared/components/ui/tabs";

import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  ArrowLeft,
  Building2,
  Edit,
  Users,
  CreditCard,
  Settings,
  Globe,
  Calendar,
  Shield,
} from "lucide-react";
import { TenantSheet } from "./components/TenantSheet";
import { TenantSubscription } from "./components/TenantSubscription";
import { TenantUsers } from "./components/TenantUsers";
import { TenantSettings } from "./components/TenantSettings";
import { apiClient } from "@/shared/api";

interface Plan {
  id: string;
  name: string;
  slug: string;
}

interface TenantDetail {
  id: string;
  name: string;
  slug: string;
  domain?: string;
  plan?: Plan;
  subscription?: {
    id: string;
    status: string;
    currentPeriodEnd?: string;
  };
  usersCount: number;
  isActive: boolean;
  settings?: {
    maxUsers?: number;
    allowRegistration?: boolean;
    enableApi?: boolean;
  };
  createdAt: string;
  updatedAt: string;
}

export function TenantDetailPage() {
  const { t } = useTranslation("tenants");
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [tenant, setTenant] = useState<TenantDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editSheetOpen, setEditSheetOpen] = useState(false);
  const [activeTab, setActiveTab] = useState("overview");

  const fetchTenant = useCallback(async () => {
    if (!id) return;
    setIsLoading(true);
    setError(null);
    try {
      const data = await apiClient.get<TenantDetail>(`/admin/tenants/${id}`);
      setTenant(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load tenant");
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchTenant();
  }, [fetchTenant]);

  const handleBack = () => {
    navigate("/tenants");
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10 rounded-lg" />
          <div className="space-y-2">
            <Skeleton className="h-6 w-48" />
            <Skeleton className="h-4 w-32" />
          </div>
        </div>
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  if (error || !tenant) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={handleBack}>
          <ArrowLeft className="me-2 h-4 w-4" />
          {t("detail.backToList")}
        </Button>
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-muted-foreground">{error || t("detail.notFound")}</p>
            <Button className="mt-4" onClick={handleBack}>
              {t("detail.backToList")}
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Back button */}
      <Button variant="ghost" onClick={handleBack} className="-ms-3">
        <ArrowLeft className="me-2 h-4 w-4" />
        {t("detail.backToList")}
      </Button>

      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="flex items-start gap-4">
          <div className="flex h-14 w-14 items-center justify-center rounded-xl bg-primary/10">
            <Building2 className="h-7 w-7 text-primary" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold">{tenant.name}</h1>
              <Badge variant={tenant.isActive ? "success" : "secondary"}>
                {tenant.isActive ? t("status.active") : t("status.inactive")}
              </Badge>
            </div>
            <div className="flex items-center gap-3 text-sm text-muted-foreground mt-1">
              <span>{tenant.slug}</span>
              {tenant.domain && (
                <>
                  <span>•</span>
                  <span className="flex items-center gap-1">
                    <Globe className="h-3 w-3" />
                    {tenant.domain}
                  </span>
                </>
              )}
            </div>
          </div>
        </div>

        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setEditSheetOpen(true)}>
            <Edit className="me-2 h-4 w-4" />
            {t("actions.edit")}
          </Button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-blue-100 dark:bg-blue-900/20">
                <Users className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{tenant.usersCount}</p>
                <p className="text-xs text-muted-foreground">{t("detail.totalUsers")}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-green-100 dark:bg-green-900/20">
                <CreditCard className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{tenant.plan?.name || t("noPlan")}</p>
                <p className="text-xs text-muted-foreground">{t("detail.currentPlan")}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-purple-100 dark:bg-purple-900/20">
                <Shield className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{tenant.settings?.maxUsers || "∞"}</p>
                <p className="text-xs text-muted-foreground">{t("detail.maxUsers")}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-orange-100 dark:bg-orange-900/20">
                <Calendar className="h-5 w-5 text-orange-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">
                  {new Date(tenant.createdAt).toLocaleDateString()}
                </p>
                <p className="text-xs text-muted-foreground">{t("detail.created")}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="overview">
            <Building2 className="me-2 h-4 w-4" />
            {t("detail.tabs.overview")}
          </TabsTrigger>
          <TabsTrigger value="users">
            <Users className="me-2 h-4 w-4" />
            {t("detail.tabs.users")}
          </TabsTrigger>
          <TabsTrigger value="subscription">
            <CreditCard className="me-2 h-4 w-4" />
            {t("detail.tabs.subscription")}
          </TabsTrigger>
          <TabsTrigger value="settings">
            <Settings className="me-2 h-4 w-4" />
            {t("detail.tabs.settings")}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="mt-6">
          <div className="grid gap-6 lg:grid-cols-2">
            {/* Tenant Info */}
            <Card>
              <CardHeader>
                <CardTitle>{t("detail.info.title")}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">{t("detail.info.name")}</span>
                  <span className="font-medium">{tenant.name}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">{t("detail.info.slug")}</span>
                  <span className="font-mono text-sm">{tenant.slug}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">{t("detail.info.domain")}</span>
                  <span>{tenant.domain || "-"}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">{t("detail.info.status")}</span>
                  <Badge variant={tenant.isActive ? "success" : "secondary"}>
                    {tenant.isActive ? t("status.active") : t("status.inactive")}
                  </Badge>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">{t("detail.info.created")}</span>
                  <span>{new Date(tenant.createdAt).toLocaleString()}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">{t("detail.info.updated")}</span>
                  <span>{new Date(tenant.updatedAt).toLocaleString()}</span>
                </div>
              </CardContent>
            </Card>

            {/* Quick Settings */}
            <Card>
              <CardHeader>
                <CardTitle>{t("detail.quickSettings.title")}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex justify-between items-center">
                  <span className="text-muted-foreground">{t("settings.allowRegistration")}</span>
                  <Badge variant={tenant.settings?.allowRegistration ? "success" : "secondary"}>
                    {tenant.settings?.allowRegistration ? t("enabled") : t("disabled")}
                  </Badge>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-muted-foreground">{t("settings.enableApi")}</span>
                  <Badge variant={tenant.settings?.enableApi ? "success" : "secondary"}>
                    {tenant.settings?.enableApi ? t("enabled") : t("disabled")}
                  </Badge>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-muted-foreground">{t("settings.maxUsers")}</span>
                  <span className="font-medium">{tenant.settings?.maxUsers || "Unlimited"}</span>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="users" className="mt-6">
          <TenantUsers tenantId={tenant.id} />
        </TabsContent>

        <TabsContent value="subscription" className="mt-6">
          <TenantSubscription tenant={tenant} onUpdate={fetchTenant} />
        </TabsContent>

        <TabsContent value="settings" className="mt-6">
          <TenantSettings tenant={tenant} onUpdate={fetchTenant} />
        </TabsContent>
      </Tabs>

      {/* Edit Sheet */}
      <TenantSheet
        open={editSheetOpen}
        onOpenChange={setEditSheetOpen}
        onSuccess={fetchTenant}
        tenant={tenant}
      />
    </div>
  );
}

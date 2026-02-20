import { useState, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { DataTableAdvanced, type Column, type Filter } from "@/shared/components/data-table";
import { Button } from "@/shared/components/ui/button";
import { Badge } from "@/shared/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Plus, Eye, Edit, Trash2, Building2, Loader2 } from "lucide-react";
import { TenantSheet } from "./components/TenantSheet";
import { apiClient } from "@/shared/api";

interface Tenant {
  id: string;
  name: string;
  slug: string;
  domain?: string;
  plan?: {
    id: string;
    name: string;
    slug: string;
  };
  usersCount: number;
  isActive: boolean;
  settings?: {
    maxUsers?: number;
    allowRegistration?: boolean;
    enableApi?: boolean;
  };
  createdAt: string;
}

export function TenantsPage() {
  const { t } = useTranslation("tenants");
  const navigate = useNavigate();

  // State
  const [data, setData] = useState<Tenant[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [page, setPage] = useState(1);
  const [perPage, setPerPage] = useState(20);
  const [total, setTotal] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  // Sheet/Dialog states
  const [sheetOpen, setSheetOpen] = useState(false);
  const [editingTenant, setEditingTenant] = useState<Tenant | null>(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deletingTenant, setDeletingTenant] = useState<Tenant | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  // Fetch data
  const fetchTenants = useCallback(async () => {
    setIsLoading(true);
    try {
      const params: Record<string, string> = {
        page: String(page),
        perPage: String(perPage),
      };
      if (search) params.q = search;
      if (statusFilter) params.status = statusFilter;

      const response = await apiClient.getWithMeta<Tenant[]>("/admin/tenants", params);
      setData(response.data);
      if (response.meta) {
        setTotal(response.meta.total ?? 0);
        setTotalPages(response.meta.totalPages ?? 1);
      }
    } catch (error) {
      console.error("Failed to fetch tenants:", error);
    } finally {
      setIsLoading(false);
    }
  }, [page, perPage, search, statusFilter]);

  useEffect(() => {
    fetchTenants();
  }, [fetchTenants]);

  // Debounced search
  useEffect(() => {
    const timer = setTimeout(() => {
      setPage(1);
    }, 300);
    return () => clearTimeout(timer);
  }, [search]);

  // Handlers
  const handleView = (tenant: Tenant) => {
    navigate(`/tenants/${tenant.id}`);
  };

  const handleEdit = (tenant: Tenant) => {
    setEditingTenant(tenant);
    setSheetOpen(true);
  };

  const handleCreate = () => {
    setEditingTenant(null);
    setSheetOpen(true);
  };

  const handleDeleteClick = (tenant: Tenant) => {
    setDeletingTenant(tenant);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!deletingTenant) return;
    setIsDeleting(true);
    try {
      await apiClient.delete(`/admin/tenants/${deletingTenant.id}`);
      fetchTenants();
      setDeleteDialogOpen(false);
      setDeletingTenant(null);
    } catch (error) {
      console.error("Failed to delete tenant:", error);
    } finally {
      setIsDeleting(false);
    }
  };

  // Columns
  const columns: Column<Tenant>[] = [
    {
      key: "name",
      header: t("columns.name"),
      cell: (row) => (
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <Building2 className="h-4 w-4 text-primary" />
          </div>
          <div>
            <button
              onClick={() => handleView(row)}
              className="font-medium hover:text-primary hover:underline text-left"
            >
              {row.name}
            </button>
            <p className="text-xs text-muted-foreground">{row.slug}</p>
          </div>
        </div>
      ),
    },
    {
      key: "domain",
      header: t("columns.domain"),
      cell: (row) => row.domain || "-",
    },
    {
      key: "plan",
      header: t("columns.plan"),
      cell: (row) => (
        <Badge variant={row.plan ? "default" : "outline"}>
          {row.plan?.name || t("noPlan")}
        </Badge>
      ),
    },
    {
      key: "usersCount",
      header: t("columns.users"),
      cell: (row) => row.usersCount,
      className: "text-center",
    },
    {
      key: "isActive",
      header: t("columns.status"),
      cell: (row) => (
        <Badge variant={row.isActive ? "success" : "secondary"}>
          {row.isActive ? t("status.active") : t("status.inactive")}
        </Badge>
      ),
    },
    {
      key: "createdAt",
      header: t("columns.createdAt"),
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
    },
    {
      key: "actions",
      header: t("columns.actions"),
      cell: (row) => (
        <div className="flex items-center gap-1">
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={() => handleView(row)}
            title={t("actions.view")}
          >
            <Eye className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={() => handleEdit(row)}
            title={t("actions.edit")}
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-destructive"
            onClick={() => handleDeleteClick(row)}
            title={t("actions.delete")}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
      className: "w-[100px]",
    },
  ];

  // Filters
  const filters: Filter[] = [
    {
      key: "status",
      label: t("filters.status"),
      value: statusFilter,
      options: [
        { label: t("status.active"), value: "active" },
        { label: t("status.inactive"), value: "inactive" },
      ],
    },
  ];

  const handleFilterChange = (key: string, value: string | undefined) => {
    if (key === "status") {
      setStatusFilter(value);
      setPage(1);
    }
  };

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold md:text-3xl">{t("title")}</h1>
          <p className="text-muted-foreground">{t("subtitle")}</p>
        </div>
      </div>

      {/* Data table */}
      <DataTableAdvanced
        columns={columns}
        data={data}
        isLoading={isLoading}
        // Search
        searchValue={search}
        onSearchChange={setSearch}
        searchPlaceholder={t("searchPlaceholder")}
        // Filters
        filters={filters}
        onFilterChange={handleFilterChange}
        // Pagination
        page={page}
        totalPages={totalPages}
        total={total}
        perPage={perPage}
        onPageChange={setPage}
        onPerPageChange={(newPerPage) => {
          setPerPage(newPerPage);
          setPage(1);
        }}
        // Selection
        selectable
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        // Actions
        onRefresh={fetchTenants}
        actions={
          <Button onClick={handleCreate}>
            <Plus className="h-4 w-4" />
            <span className="hidden sm:inline ms-2">{t("addTenant")}</span>
          </Button>
        }
      />

      {/* Side Sheet for Add/Edit */}
      <TenantSheet
        open={sheetOpen}
        onOpenChange={setSheetOpen}
        onSuccess={fetchTenants}
        tenant={editingTenant}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t("deleteDialog.title")}</DialogTitle>
            <DialogDescription>
              {t("deleteDialog.description", { name: deletingTenant?.name })}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDeleteDialogOpen(false)}
              disabled={isDeleting}
            >
              {t("actions.cancel")}
            </Button>
            <Button
              variant="destructive"
              onClick={handleDeleteConfirm}
              disabled={isDeleting}
            >
              {isDeleting ? (
                <>
                  <Loader2 className="me-2 h-4 w-4 animate-spin" />
                  {t("actions.deleting")}
                </>
              ) : (
                t("actions.delete")
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

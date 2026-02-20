import { useState, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { DataTableAdvanced, type Column } from "@/shared/components/data-table";
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
import { Plus, Edit, Trash2, Shield, Loader2 } from "lucide-react";
import { RoleFormDialog } from "./components/RoleFormDialog";
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
  permissionsCount: number;
  usersCount: number;
  isSystem: boolean;
  createdAt: string;
}

export function RolesPage() {
  const { t } = useTranslation("roles");

  // State
  const [data, setData] = useState<Role[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [perPage, setPerPage] = useState(20);
  const [total, setTotal] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  // Dialog states
  const [formDialogOpen, setFormDialogOpen] = useState(false);
  const [editingRole, setEditingRole] = useState<Role | null>(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deletingRole, setDeletingRole] = useState<Role | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  // Fetch data
  const fetchRoles = useCallback(async () => {
    setIsLoading(true);
    try {
      const params: Record<string, string> = {
        page: String(page),
        perPage: String(perPage),
      };
      if (search) params.q = search;

      const response = await apiClient.getWithMeta<Role[]>("/admin/roles", params);
      setData(response.data);
      if (response.meta) {
        setTotal(response.meta.total ?? 0);
        setTotalPages(response.meta.totalPages ?? 1);
      }
    } catch (error) {
      console.error("Failed to fetch roles:", error);
    } finally {
      setIsLoading(false);
    }
  }, [page, perPage, search]);

  useEffect(() => {
    fetchRoles();
  }, [fetchRoles]);

  // Debounced search
  useEffect(() => {
    const timer = setTimeout(() => {
      setPage(1);
    }, 300);
    return () => clearTimeout(timer);
  }, [search]);

  // Handlers
  const handleEdit = (role: Role) => {
    setEditingRole(role);
    setFormDialogOpen(true);
  };

  const handleCreate = () => {
    setEditingRole(null);
    setFormDialogOpen(true);
  };

  const handleDeleteClick = (role: Role) => {
    setDeletingRole(role);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!deletingRole) return;
    setIsDeleting(true);
    try {
      await apiClient.delete(`/admin/roles/${deletingRole.id}`);
      fetchRoles();
      setDeleteDialogOpen(false);
      setDeletingRole(null);
    } catch (error) {
      console.error("Failed to delete role:", error);
    } finally {
      setIsDeleting(false);
    }
  };

  // Columns
  const columns: Column<Role>[] = [
    {
      key: "name",
      header: t("columns.name"),
      cell: (row) => (
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <Shield className="h-4 w-4 text-primary" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <p className="font-medium">{row.name}</p>
              {row.isSystem && (
                <Badge variant="outline" className="text-xs">
                  {t("systemRole")}
                </Badge>
              )}
            </div>
            <p className="text-xs text-muted-foreground">
              {row.description || row.slug}
            </p>
          </div>
        </div>
      ),
    },
    {
      key: "permissionsCount",
      header: t("columns.permissions"),
      className: "text-center",
      cell: (row) => (
        <Badge variant="secondary">{row.permissionsCount}</Badge>
      ),
    },
    {
      key: "usersCount",
      header: t("columns.users"),
      className: "text-center",
      cell: (row) => row.usersCount,
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
            onClick={() => handleEdit(row)}
          >
            <Edit className="h-4 w-4" />
          </Button>
          {!row.isSystem && (
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-destructive"
              onClick={() => handleDeleteClick(row)}
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          )}
        </div>
      ),
      className: "w-[80px]",
    },
  ];

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
        onRefresh={fetchRoles}
        actions={
          <Button onClick={handleCreate}>
            <Plus className="h-4 w-4" />
            <span className="hidden sm:inline ms-2">{t("addRole")}</span>
          </Button>
        }
      />

      {/* Create/Edit Dialog */}
      <RoleFormDialog
        open={formDialogOpen}
        onOpenChange={setFormDialogOpen}
        onSuccess={fetchRoles}
        role={editingRole}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t("deleteDialog.title")}</DialogTitle>
            <DialogDescription>
              {t("deleteDialog.description", { name: deletingRole?.name })}
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

import { useState, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { DataTableAdvanced, type Column, type Filter } from "@/shared/components/data-table";
import { Button } from "@/shared/components/ui/button";
import { Badge } from "@/shared/components/ui/badge";
import { Avatar, AvatarFallback } from "@/shared/components/ui/avatar";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Plus, Edit, Trash2, Loader2 } from "lucide-react";
import { UserFormDialog } from "./components/UserFormDialog";
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
  avatar?: string;
  phone?: string;
  isActive: boolean;
  isEmailVerified: boolean;
  lastLoginAt?: string;
  tenants: UserTenant[];
  createdAt: string;
}

export function UsersPage() {
  const { t } = useTranslation("users");

  // State
  const [data, setData] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [page, setPage] = useState(1);
  const [perPage, setPerPage] = useState(20);
  const [total, setTotal] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  // Dialog states
  const [formDialogOpen, setFormDialogOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deletingUser, setDeletingUser] = useState<User | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  // Fetch data
  const fetchUsers = useCallback(async () => {
    setIsLoading(true);
    try {
      const params: Record<string, string> = {
        page: String(page),
        perPage: String(perPage),
      };
      if (search) params.q = search;
      if (statusFilter) params.status = statusFilter;

      const response = await apiClient.getWithMeta<User[]>("/admin/users", params);
      setData(response.data);
      if (response.meta) {
        setTotal(response.meta.total ?? 0);
        setTotalPages(response.meta.totalPages ?? 1);
      }
    } catch (error) {
      console.error("Failed to fetch users:", error);
    } finally {
      setIsLoading(false);
    }
  }, [page, perPage, search, statusFilter]);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  // Debounced search
  useEffect(() => {
    const timer = setTimeout(() => {
      setPage(1);
    }, 300);
    return () => clearTimeout(timer);
  }, [search]);

  // Handlers
  const handleEdit = (user: User) => {
    setEditingUser(user);
    setFormDialogOpen(true);
  };

  const handleCreate = () => {
    setEditingUser(null);
    setFormDialogOpen(true);
  };

  const handleDeleteClick = (user: User) => {
    setDeletingUser(user);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!deletingUser) return;
    setIsDeleting(true);
    try {
      await apiClient.delete(`/admin/users/${deletingUser.id}`);
      fetchUsers();
      setDeleteDialogOpen(false);
      setDeletingUser(null);
    } catch (error) {
      console.error("Failed to delete user:", error);
    } finally {
      setIsDeleting(false);
    }
  };

  const getInitials = (firstName: string, lastName: string) => {
    return `${firstName[0] || ""}${lastName[0] || ""}`.toUpperCase();
  };

  // Columns
  const columns: Column<User>[] = [
    {
      key: "name",
      header: t("columns.name"),
      cell: (row) => (
        <div className="flex items-center gap-3">
          <Avatar className="h-8 w-8">
            <AvatarFallback className="bg-primary/10 text-primary text-xs">
              {getInitials(row.firstName, row.lastName)}
            </AvatarFallback>
          </Avatar>
          <div>
            <p className="font-medium">
              {row.firstName} {row.lastName}
            </p>
            <p className="text-xs text-muted-foreground">{row.email}</p>
          </div>
        </div>
      ),
    },
    {
      key: "tenants",
      header: t("columns.tenant"),
      cell: (row) => (
        <div className="flex flex-wrap gap-1">
          {row.tenants.length > 0 ? (
            row.tenants.slice(0, 2).map((tenant) => (
              <Badge key={tenant.id} variant="outline" className="text-xs">
                {tenant.name}
              </Badge>
            ))
          ) : (
            <span className="text-muted-foreground">-</span>
          )}
          {row.tenants.length > 2 && (
            <Badge variant="secondary" className="text-xs">
              +{row.tenants.length - 2}
            </Badge>
          )}
        </div>
      ),
    },
    {
      key: "role",
      header: t("columns.role"),
      cell: (row) => {
        const primaryTenant = row.tenants[0];
        return primaryTenant ? (
          <Badge
            variant={primaryTenant.role.slug === "super-admin" ? "default" : "secondary"}
          >
            {primaryTenant.role.name}
          </Badge>
        ) : (
          "-"
        );
      },
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
      key: "lastLoginAt",
      header: t("columns.lastLogin"),
      cell: (row) =>
        row.lastLoginAt
          ? new Date(row.lastLoginAt).toLocaleDateString()
          : t("neverLoggedIn"),
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
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-destructive"
            onClick={() => handleDeleteClick(row)}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
      className: "w-[80px]",
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
        onRefresh={fetchUsers}
        actions={
          <Button onClick={handleCreate}>
            <Plus className="h-4 w-4" />
            <span className="hidden sm:inline ms-2">{t("addUser")}</span>
          </Button>
        }
        bulkActions={
          <>
            <Button variant="outline" size="sm">
              {t("bulkActions.activate")}
            </Button>
            <Button variant="outline" size="sm">
              {t("bulkActions.deactivate")}
            </Button>
            <Button variant="destructive" size="sm">
              {t("bulkActions.delete")}
            </Button>
          </>
        }
      />

      {/* Create/Edit Dialog */}
      <UserFormDialog
        open={formDialogOpen}
        onOpenChange={setFormDialogOpen}
        onSuccess={fetchUsers}
        user={editingUser}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t("deleteDialog.title")}</DialogTitle>
            <DialogDescription>
              {t("deleteDialog.description", {
                name: deletingUser
                  ? `${deletingUser.firstName} ${deletingUser.lastName}`
                  : "",
              })}
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

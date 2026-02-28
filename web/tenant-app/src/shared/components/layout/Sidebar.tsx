import { useState } from "react";
import { NavLink, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { cn } from "@/shared/lib/cn";
import { useSidebar } from "./DashboardLayout";
import { useAuth } from "react-oidc-context";
import { usePermissions } from "@/features/auth/hooks/usePermissions";
import {
  LayoutDashboard,
  Users,
  Truck,
  UserSearch,
  HardHat,
  UserRound,
  FileSignature,
  ShieldCheck,
  ClipboardList,
  Settings,
  X,
  LogOut,
  ChevronRight,
  Bell,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";

interface NavChild {
  path: string;
  icon: LucideIcon;
  labelKey: string;
}

interface NavItem {
  path: string;
  icon: LucideIcon;
  labelKey: string;
  exact?: boolean;
  permission: string;
  children?: NavChild[];
}

const navItems: NavItem[] = [
  { path: "/", icon: LayoutDashboard, labelKey: "nav.home", exact: true, permission: "dashboard.view" },
  { path: "/team", icon: Users, labelKey: "nav.team", permission: "members.view" },
  { path: "/suppliers", icon: Truck, labelKey: "nav.suppliers", permission: "suppliers.view" },
  { path: "/candidates", icon: UserSearch, labelKey: "nav.candidates", permission: "candidates.view" },
  { path: "/clients", icon: UserRound, labelKey: "nav.clients", permission: "clients.view" },
  { path: "/workers", icon: HardHat, labelKey: "nav.workers", permission: "workers.view" },
  { path: "/contracts", icon: FileSignature, labelKey: "nav.contracts", permission: "contracts.view" },
  { path: "/compliance", icon: ShieldCheck, labelKey: "nav.compliance", permission: "documents.view" },
  { path: "/audit", icon: ClipboardList, labelKey: "nav.audit", permission: "audit.view" },
  {
    path: "/settings",
    icon: Settings,
    labelKey: "nav.settings",
    permission: "settings.manage",
    children: [
      { path: "/settings/general", icon: Settings, labelKey: "nav.settings_general" },
      { path: "/settings/notifications", icon: Bell, labelKey: "nav.settings_notifications" },
    ],
  },
];

interface SidebarProps {
  onClose?: () => void;
}

export function Sidebar({ onClose }: SidebarProps) {
  const { t } = useTranslation();
  const { collapsed } = useSidebar();
  const auth = useAuth();
  const { hasPermission, isLoaded } = usePermissions();
  const location = useLocation();

  // Track which parent groups are expanded
  const [expandedGroups, setExpandedGroups] = useState<Record<string, boolean>>(() => {
    // Auto-expand if current path is a child
    const initial: Record<string, boolean> = {};
    for (const item of navItems) {
      if (item.children) {
        const isChildActive = item.children.some((child) => location.pathname.startsWith(child.path));
        if (isChildActive) {
          initial[item.path] = true;
        }
      }
    }
    return initial;
  });

  const toggleGroup = (path: string) => {
    setExpandedGroups((prev) => ({ ...prev, [path]: !prev[path] }));
  };

  const visibleNavItems = isLoaded
    ? navItems.filter((item) => hasPermission(item.permission))
    : navItems;

  // Get user info from OIDC profile
  const user = auth.user?.profile;
  const firstName = (user?.given_name as string) || "";
  const lastName = (user?.family_name as string) || "";
  const email = (user?.email as string) || "";

  const userInitials = firstName && lastName
    ? `${firstName[0]}${lastName[0]}`.toUpperCase()
    : email?.[0]?.toUpperCase() || "U";

  const handleLogout = () => {
    auth.signoutRedirect({ post_logout_redirect_uri: window.location.origin });
  };

  const isChildActive = (children: NavChild[]) =>
    children.some((child) => location.pathname.startsWith(child.path));

  return (
    <div className="flex h-full flex-col border-e bg-card">
      {/* Header */}
      <div
        className={cn(
          "flex h-16 items-center border-b shrink-0",
          collapsed ? "justify-center px-3" : "justify-between px-4"
        )}
      >
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary text-primary-foreground font-bold text-lg shrink-0">
            T
          </div>
          {!collapsed && (
            <span className="font-semibold text-lg whitespace-nowrap">
              TadHub
            </span>
          )}
        </div>
        {!collapsed && (
          <button
            className="rounded-lg p-2 hover:bg-muted lg:hidden shrink-0"
            onClick={onClose}
          >
            <X className="h-5 w-5" />
          </button>
        )}
      </div>

      {/* Tenant info */}
      {!collapsed && (
        <div className="border-b px-4 py-3">
          <p className="text-xs text-muted-foreground">Welcome</p>
          <p className="font-medium text-foreground truncate">
            {firstName || "Tenant Portal"}
          </p>
        </div>
      )}

      {/* Navigation */}
      <nav
        className={cn(
          "flex-1 overflow-y-auto py-4",
          collapsed ? "px-3" : "px-3"
        )}
      >
        <div className="space-y-1">
          {visibleNavItems.map((item) => {
            const Icon = item.icon;
            const hasChildren = item.children && item.children.length > 0;
            const isExpanded = expandedGroups[item.path] ?? false;
            const childActive = hasChildren && isChildActive(item.children!);

            // Collapsed sidebar: just show icon (for all items including parents)
            if (collapsed) {
              return (
                <NavLink
                  key={item.path}
                  to={hasChildren ? item.children![0].path : item.path}
                  end={item.exact}
                  onClick={onClose}
                  title={t(item.labelKey)}
                  className={() =>
                    cn(
                      "flex items-center justify-center rounded-lg h-10 w-10 mx-auto text-sm font-medium transition-colors",
                      childActive || (!hasChildren && location.pathname === item.path)
                        ? "bg-primary text-primary-foreground"
                        : "text-muted-foreground hover:bg-muted hover:text-foreground"
                    )
                  }
                >
                  <Icon className="h-5 w-5 shrink-0" />
                </NavLink>
              );
            }

            // Expanded sidebar: parent with children
            if (hasChildren) {
              return (
                <div key={item.path}>
                  {/* Parent toggle button */}
                  <button
                    onClick={() => toggleGroup(item.path)}
                    className={cn(
                      "flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors",
                      childActive
                        ? "text-primary"
                        : "text-muted-foreground hover:bg-muted hover:text-foreground"
                    )}
                  >
                    <Icon className="h-5 w-5 shrink-0" />
                    <span className="flex-1 text-start">{t(item.labelKey)}</span>
                    <ChevronRight
                      className={cn(
                        "h-4 w-4 shrink-0 transition-transform duration-200",
                        isExpanded && "rotate-90"
                      )}
                    />
                  </button>

                  {/* Children */}
                  {isExpanded && (
                    <div className="mt-1 ms-4 space-y-1 border-s ps-3">
                      {item.children!.map((child) => {
                        const ChildIcon = child.icon;
                        return (
                          <NavLink
                            key={child.path}
                            to={child.path}
                            onClick={onClose}
                            className={({ isActive }) =>
                              cn(
                                "flex items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors",
                                isActive
                                  ? "bg-primary text-primary-foreground font-medium"
                                  : "text-muted-foreground hover:bg-muted hover:text-foreground"
                              )
                            }
                          >
                            <ChildIcon className="h-4 w-4 shrink-0" />
                            <span>{t(child.labelKey)}</span>
                          </NavLink>
                        );
                      })}
                    </div>
                  )}
                </div>
              );
            }

            // Regular nav item (no children)
            return (
              <NavLink
                key={item.path}
                to={item.path}
                end={item.exact}
                onClick={onClose}
                className={({ isActive }) =>
                  cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors",
                    isActive
                      ? "bg-primary text-primary-foreground"
                      : "text-muted-foreground hover:bg-muted hover:text-foreground"
                  )
                }
              >
                <Icon className="h-5 w-5 shrink-0" />
                <span>{t(item.labelKey)}</span>
              </NavLink>
            );
          })}
        </div>
      </nav>

      {/* Footer with user info */}
      <div
        className={cn(
          "shrink-0 border-t",
          collapsed ? "p-3" : "p-4"
        )}
      >
        {collapsed ? (
          <button
            onClick={handleLogout}
            title={t("logout")}
            className="flex h-10 w-10 items-center justify-center rounded-lg text-destructive hover:bg-destructive/10 mx-auto"
          >
            <LogOut className="h-5 w-5" />
          </button>
        ) : (
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 items-center justify-center rounded-full bg-primary/10 text-primary text-sm font-medium shrink-0">
              {userInitials}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-foreground truncate">
                {firstName} {lastName}
              </p>
              <p className="text-xs text-muted-foreground truncate">
                {email}
              </p>
            </div>
            <button
              onClick={handleLogout}
              className="shrink-0 rounded-lg p-2 text-muted-foreground hover:bg-muted hover:text-destructive"
            >
              <LogOut className="h-4 w-4" />
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

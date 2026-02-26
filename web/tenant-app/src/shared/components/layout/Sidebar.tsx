import { NavLink } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { cn } from "@/shared/lib/cn";
import { useSidebar } from "./DashboardLayout";
import { useAuth } from "react-oidc-context";
import {
  LayoutDashboard,
  Users,
  Truck,
  UserSearch,
  HardHat,
  UserRound,
  X,
  LogOut,
} from "lucide-react";

const navItems = [
  { path: "/", icon: LayoutDashboard, labelKey: "nav.home", exact: true },
  { path: "/team", icon: Users, labelKey: "nav.team" },
  { path: "/suppliers", icon: Truck, labelKey: "nav.suppliers" },
  { path: "/candidates", icon: UserSearch, labelKey: "nav.candidates" },
  { path: "/clients", icon: UserRound, labelKey: "nav.clients" },
  { path: "/workers", icon: HardHat, labelKey: "nav.workers" },
];

interface SidebarProps {
  onClose?: () => void;
}

export function Sidebar({ onClose }: SidebarProps) {
  const { t } = useTranslation();
  const { collapsed } = useSidebar();
  const auth = useAuth();

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
          {navItems.map((item) => {
            const Icon = item.icon;

            if (collapsed) {
              return (
                <NavLink
                  key={item.path}
                  to={item.path}
                  end={item.exact}
                  onClick={onClose}
                  title={t(item.labelKey)}
                  className={({ isActive }) =>
                    cn(
                      "flex items-center justify-center rounded-lg h-10 w-10 mx-auto text-sm font-medium transition-colors",
                      isActive
                        ? "bg-primary text-primary-foreground"
                        : "text-muted-foreground hover:bg-muted hover:text-foreground"
                    )
                  }
                >
                  <Icon className="h-5 w-5 shrink-0" />
                </NavLink>
              );
            }

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

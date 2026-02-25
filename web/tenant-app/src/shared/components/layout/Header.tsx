import { useTranslation } from "react-i18next";
import { useState, useEffect } from "react";
import { useSidebar } from "./DashboardLayout";
import { useAuth } from "react-oidc-context";
import {
  Menu,
  Moon,
  Sun,
  PanelLeftClose,
  PanelLeft,
  Languages,
  Bell,
  LogOut,
  Settings,
  User,
  KeyRound,
} from "lucide-react";
import { useNavigate } from "react-router-dom";
import { keycloakUrls } from "@/lib/auth-config";
import { TenantSelector } from "@/shared/components/tenant";

interface HeaderProps {
  onMenuClick?: () => void;
}

export function Header({ onMenuClick }: HeaderProps) {
  const { t, i18n } = useTranslation();
  const { collapsed, setCollapsed } = useSidebar();
  const auth = useAuth();
  const navigate = useNavigate();
  const [isDark, setIsDark] = useState(false);
  const [showNotifications, setShowNotifications] = useState(false);
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [currentLang, setCurrentLang] = useState(() => {
    return localStorage.getItem("i18nextLng") || "en";
  });

  // Get user info from OIDC profile
  const user = auth.user?.profile;
  const firstName = (user?.given_name as string) || "";
  const lastName = (user?.family_name as string) || "";
  const email = (user?.email as string) || "";

  useEffect(() => {
    const isDarkMode = document.documentElement.classList.contains("dark");
    setIsDark(isDarkMode);
  }, []);

  useEffect(() => {
    const dir = currentLang === "ar" ? "rtl" : "ltr";
    document.documentElement.dir = dir;
    document.documentElement.lang = currentLang;
    document.body.dir = dir;
  }, [currentLang]);

  // Listen for forbidden events to show toast/notification
  useEffect(() => {
    const handleForbidden = (event: CustomEvent<{ message: string; code: string }>) => {
      // You can show a toast notification here
      console.warn('Permission denied:', event.detail?.message);
    };

    window.addEventListener('auth:forbidden', handleForbidden as EventListener);
    return () => window.removeEventListener('auth:forbidden', handleForbidden as EventListener);
  }, []);

  const handleLanguageToggle = () => {
    const newLang = currentLang === "en" ? "ar" : "en";
    i18n.changeLanguage(newLang);
    localStorage.setItem("i18nextLng", newLang);
    setCurrentLang(newLang);
    const dir = newLang === "ar" ? "rtl" : "ltr";
    document.documentElement.dir = dir;
    document.documentElement.lang = newLang;
    document.body.dir = dir;
  };

  const toggleTheme = () => {
    const newIsDark = !isDark;
    setIsDark(newIsDark);
    document.documentElement.classList.toggle("dark", newIsDark);
    localStorage.setItem("theme", newIsDark ? "dark" : "light");
  };

  const handleLogout = () => {
    auth.signoutRedirect({ post_logout_redirect_uri: window.location.origin + '/login' });
  };

  const handleAccountSettings = () => {
    window.open(keycloakUrls.account, '_blank');
  };

  const userInitials = firstName && lastName
    ? `${firstName[0]}${lastName[0]}`.toUpperCase()
    : email?.[0]?.toUpperCase() || "U";

  return (
    <header className="flex h-16 items-center justify-between border-b bg-card px-4 md:px-6">
      <div className="flex items-center gap-2">
        {/* Mobile menu button */}
        <button
          className="rounded-lg p-2 hover:bg-muted lg:hidden"
          onClick={onMenuClick}
        >
          <Menu className="h-5 w-5" />
        </button>

        {/* Desktop sidebar toggle */}
        <button
          className="hidden rounded-lg p-2 hover:bg-muted lg:flex"
          onClick={() => setCollapsed(!collapsed)}
        >
          {collapsed ? (
            <PanelLeft className="h-5 w-5" />
          ) : (
            <PanelLeftClose className="h-5 w-5" />
          )}
        </button>

        {/* Tenant Selector - only shows if user has multiple tenants */}
        <TenantSelector className="hidden md:block" />
      </div>

      <div className="flex items-center gap-2">
        {/* Notifications */}
        <div className="relative">
          <button
            className="relative rounded-lg p-2 hover:bg-muted"
            onClick={() => setShowNotifications(!showNotifications)}
          >
            <Bell className="h-5 w-5" />
            {/* Notification badge */}
            <span className="absolute end-1 top-1 flex h-2 w-2">
              <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-primary opacity-75"></span>
              <span className="relative inline-flex h-2 w-2 rounded-full bg-primary"></span>
            </span>
          </button>

          {/* Notifications dropdown */}
          {showNotifications && (
            <>
              <div
                className="fixed inset-0 z-40"
                onClick={() => setShowNotifications(false)}
              />
              <div className="absolute end-0 top-full z-50 mt-2 w-80 rounded-lg border bg-card p-4 shadow-lg">
                <h3 className="font-semibold text-foreground mb-3">
                  {t("notifications")}
                </h3>
                <p className="text-sm text-muted-foreground">
                  {t("noResults")}
                </p>
              </div>
            </>
          )}
        </div>

        {/* Theme toggle */}
        <button className="rounded-lg p-2 hover:bg-muted" onClick={toggleTheme}>
          {isDark ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
        </button>

        {/* Language toggle */}
        <button
          className="flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium hover:bg-muted"
          onClick={handleLanguageToggle}
        >
          <Languages className="h-4 w-4" />
          {currentLang === "ar" ? "EN" : "AR"}
        </button>

        {/* User menu */}
        <div className="relative">
          <button
            className="flex items-center gap-2 rounded-lg p-1 hover:bg-muted"
            onClick={() => setShowUserMenu(!showUserMenu)}
          >
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-sm font-medium text-primary-foreground">
              {userInitials}
            </div>
          </button>

          {/* User dropdown */}
          {showUserMenu && (
            <>
              <div
                className="fixed inset-0 z-40"
                onClick={() => setShowUserMenu(false)}
              />
              <div className="absolute end-0 top-full z-50 mt-2 w-56 rounded-lg border bg-card py-2 shadow-lg">
                <div className="px-4 py-2 border-b">
                  <p className="font-medium text-foreground">
                    {firstName} {lastName}
                  </p>
                  <p className="text-sm text-muted-foreground truncate">
                    {email}
                  </p>
                </div>
                <button
                  onClick={() => {
                    setShowUserMenu(false);
                    navigate("/settings");
                  }}
                  className="flex w-full items-center gap-3 px-4 py-2 text-sm hover:bg-muted"
                >
                  <Settings className="h-4 w-4" />
                  {t("settings")}
                </button>
                <button
                  onClick={() => {
                    setShowUserMenu(false);
                    handleAccountSettings();
                  }}
                  className="flex w-full items-center gap-3 px-4 py-2 text-sm hover:bg-muted"
                >
                  <User className="h-4 w-4" />
                  {t("profile")}
                </button>
                <button
                  onClick={() => {
                    setShowUserMenu(false);
                    window.open(keycloakUrls.account + '/account-security/signing-in', '_blank');
                  }}
                  className="flex w-full items-center gap-3 px-4 py-2 text-sm hover:bg-muted"
                >
                  <KeyRound className="h-4 w-4" />
                  {t("changePassword")}
                </button>
                <div className="border-t my-1" />
                <button
                  onClick={handleLogout}
                  className="flex w-full items-center gap-3 px-4 py-2 text-sm text-destructive hover:bg-muted"
                >
                  <LogOut className="h-4 w-4" />
                  {t("logout")}
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </header>
  );
}

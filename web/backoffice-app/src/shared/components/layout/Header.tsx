import { useTranslation } from "react-i18next";
import { Button } from "@/shared/components/ui/button";
import { Menu, Moon, Sun, PanelLeftClose, PanelLeft, Languages } from "lucide-react";
import { useState, useEffect } from "react";
import { useSidebar } from "./DashboardLayout";

interface HeaderProps {
  onMenuClick?: () => void;
}

export function Header({ onMenuClick }: HeaderProps) {
  const { i18n } = useTranslation();
  const { collapsed, setCollapsed } = useSidebar();
  const [isDark, setIsDark] = useState(false);
  const [currentLang, setCurrentLang] = useState(() => {
    return localStorage.getItem("i18nextLng") || "en";
  });

  useEffect(() => {
    const isDarkMode = document.documentElement.classList.contains("dark");
    setIsDark(isDarkMode);
  }, []);

  // Sync direction with current language
  useEffect(() => {
    const dir = currentLang === "ar" ? "rtl" : "ltr";
    document.documentElement.dir = dir;
    document.documentElement.lang = currentLang;
    document.body.dir = dir;
  }, [currentLang]);

  const handleLanguageToggle = () => {
    const newLang = currentLang === "en" ? "ar" : "en";
    
    // Update i18n
    i18n.changeLanguage(newLang);
    
    // Update localStorage
    localStorage.setItem("i18nextLng", newLang);
    
    // Update state to trigger re-render
    setCurrentLang(newLang);
    
    // Force direction update
    const dir = newLang === "ar" ? "rtl" : "ltr";
    document.documentElement.dir = dir;
    document.documentElement.lang = newLang;
    document.body.dir = dir;
  };

  const toggleTheme = () => {
    const newIsDark = !isDark;
    setIsDark(newIsDark);
    document.documentElement.classList.toggle("dark", newIsDark);
  };

  return (
    <header className="flex h-16 items-center justify-between border-b bg-card px-4 md:px-6">
      <div className="flex items-center gap-2">
        {/* Mobile menu button */}
        <Button
          variant="ghost"
          size="icon"
          className="lg:hidden"
          onClick={onMenuClick}
        >
          <Menu className="h-5 w-5" />
        </Button>

        {/* Desktop sidebar toggle */}
        <Button
          variant="ghost"
          size="icon"
          className="hidden lg:flex"
          onClick={() => setCollapsed(!collapsed)}
        >
          {collapsed ? (
            <PanelLeft className="h-5 w-5" />
          ) : (
            <PanelLeftClose className="h-5 w-5" />
          )}
        </Button>
      </div>

      <div className="flex items-center gap-2">
        {/* Theme toggle */}
        <Button variant="ghost" size="icon" onClick={toggleTheme}>
          {isDark ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
        </Button>

        {/* Language toggle */}
        <Button 
          variant="ghost" 
          size="sm" 
          onClick={handleLanguageToggle}
          className="gap-2 font-medium"
          data-testid="lang-toggle"
        >
          <Languages className="h-4 w-4" />
          {currentLang === "ar" ? "EN" : "AR"}
        </Button>
      </div>
    </header>
  );
}

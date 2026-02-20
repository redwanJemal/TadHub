import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import LanguageDetector from "i18next-browser-languagedetector";

// Feature translations - import all
import authEn from "@/features/auth/i18n/en.json";
import authAr from "@/features/auth/i18n/ar.json";
import dashboardEn from "@/features/dashboard/i18n/en.json";
import dashboardAr from "@/features/dashboard/i18n/ar.json";
import tenantsEn from "@/features/tenants/i18n/en.json";
import tenantsAr from "@/features/tenants/i18n/ar.json";
import usersEn from "@/features/users/i18n/en.json";
import usersAr from "@/features/users/i18n/ar.json";
import rolesEn from "@/features/roles/i18n/en.json";
import rolesAr from "@/features/roles/i18n/ar.json";
import auditLogsEn from "@/features/audit-logs/i18n/en.json";
import auditLogsAr from "@/features/audit-logs/i18n/ar.json";
import lookupsEn from "@/features/lookups/i18n/en.json";
import lookupsAr from "@/features/lookups/i18n/ar.json";
import settingsEn from "@/features/settings/i18n/en.json";
import settingsAr from "@/features/settings/i18n/ar.json";

// Shared/common translations
import commonEn from "./common/en.json";
import commonAr from "./common/ar.json";

const resources = {
  en: {
    common: commonEn,
    auth: authEn,
    dashboard: dashboardEn,
    tenants: tenantsEn,
    users: usersEn,
    roles: rolesEn,
    auditLogs: auditLogsEn,
    lookups: lookupsEn,
    settings: settingsEn,
  },
  ar: {
    common: commonAr,
    auth: authAr,
    dashboard: dashboardAr,
    tenants: tenantsAr,
    users: usersAr,
    roles: rolesAr,
    auditLogs: auditLogsAr,
    lookups: lookupsAr,
    settings: settingsAr,
  },
};

// Helper to update document direction
const updateDirection = (lng: string) => {
  const dir = lng === "ar" ? "rtl" : "ltr";
  document.documentElement.setAttribute("dir", dir);
  document.documentElement.setAttribute("lang", lng);
  document.body.setAttribute("dir", dir);
};

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    fallbackLng: "en",
    defaultNS: "common",
    ns: ["common", "auth", "dashboard", "tenants", "users", "roles", "auditLogs", "lookups", "settings"],
    interpolation: {
      escapeValue: false,
    },
    detection: {
      order: ["localStorage", "navigator"],
      caches: ["localStorage"],
    },
  })
  .then(() => {
    // Set initial direction after init
    updateDirection(i18n.language);
  });

// Set document direction on language change
i18n.on("languageChanged", updateDirection);

export default i18n;

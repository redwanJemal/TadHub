import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import LanguageDetector from "i18next-browser-languagedetector";

// Feature translations
import authEn from "@/features/auth/i18n/en.json";
import authAr from "@/features/auth/i18n/ar.json";
import tenantsEn from "@/features/tenants/i18n/en.json";
import tenantsAr from "@/features/tenants/i18n/ar.json";

// Shared/common translations
import commonEn from "./common/en.json";
import commonAr from "./common/ar.json";

const resources = {
  en: {
    common: commonEn,
    auth: authEn,
    tenants: tenantsEn,
  },
  ar: {
    common: commonAr,
    auth: authAr,
    tenants: tenantsAr,
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
    ns: ["common", "auth", "tenants"],
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

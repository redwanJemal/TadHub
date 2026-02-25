import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

// Common translations
import commonEn from './common/en.json';
import commonAr from './common/ar.json';

// Feature translations
import authEn from '../features/auth/i18n/en.json';
import authAr from '../features/auth/i18n/ar.json';
import teamEn from '../features/team/i18n/en.json';
import teamAr from '../features/team/i18n/ar.json';

const resources = {
  en: {
    common: commonEn,
    auth: authEn,
    team: teamEn,
  },
  ar: {
    common: commonAr,
    auth: authAr,
    team: teamAr,
  },
};

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    fallbackLng: 'en',
    defaultNS: 'common',
    ns: ['common', 'auth', 'team'],
    interpolation: {
      escapeValue: false,
    },
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
    },
  });

// Set document direction based on language
i18n.on('languageChanged', (lng) => {
  document.documentElement.dir = lng === 'ar' ? 'rtl' : 'ltr';
  document.documentElement.lang = lng;
});

// Set initial direction
document.documentElement.dir = i18n.language === 'ar' ? 'rtl' : 'ltr';
document.documentElement.lang = i18n.language;

export default i18n;

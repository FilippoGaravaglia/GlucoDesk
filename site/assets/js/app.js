"use strict";

const translations = {
  en: {
    navExperience: "Experience",
    navFeatures: "Features",
    navPrivacy: "Privacy",
    navDownload: "Download",

    heroEyebrow: "Local-first · macOS & Windows",
    heroTitleLineOne: "Your glucose.",
    heroTitleLineTwo: "Calmly in sight.",
    heroDescription:
      "GlucoDesk brings recent glucose context to your desktop with a " +
      "glanceable menu bar presence, calm awareness notifications, " +
      "readable local history and bilingual diary exports.",
    heroPrimaryAction: "Explore the preview",
    heroSecondaryAction: "View source",
    trustLocal: "Local-first",
    trustPlatforms: "macOS & Windows",
    trustOpenSource: "Open source",
    statusHigh: "High",
    statusPrivacy: "Privacy mode",
    statusHidden: "Hidden",
    liveData: "Live data",
    heroFooter: "Designed for calm awareness while you work.",

    experienceKicker: "Glanceable by design",
    experienceTitle: "Awareness that stays out of your way.",
    experienceDescription:
      "GlucoDesk lives quietly on your desktop and surfaces the context " +
      "you need without turning every reading into an interruption.",

    presenceKicker: "Desktop presence",
    presenceTitle: "One icon. Immediate context.",
    presenceDescription:
      "The GlucoDesk G updates as soon as a new reading changes your " +
      "glucose band, giving you immediate visual feedback in the macOS " +
      "menu bar or Windows system tray.",

    stateInRange: "In range",
    stateInRangeDescription: "Calm green feedback",
    stateHigh: "High",
    stateHighDescription: "Visible yellow awareness",
    stateLow: "Low",
    stateLowDescription: "Clear red feedback",
    statePrivacy: "Privacy",
    statePrivacyDescription: "Values safely hidden",

    popoverCaption: "Desktop glucose awareness",
    latestGlucose: "Latest glucose",
    updatedNow: "Updated now",
    openDashboard: "Open dashboard",

    featuresKicker: "A complete desktop loop",
    featuresTitle: "From the latest reading to a clearer story.",
    featuresDescription:
      "GlucoDesk combines live awareness, local history and readable " +
      "exports in one focused desktop experience.",

    dashboardKicker: "Live dashboard",
    dashboardTitle: "Current context without the noise.",
    dashboardDescription:
      "See your latest reading, trend, target range, recent chart, " +
      "statistics and data health in a clean responsive layout.",

    notificationsKicker: "Calm awareness",
    notificationsTitle: "Notifications with boundaries.",
    notificationsDescription:
      "Optional in-app and native notifications use cooldown, snooze, " +
      "dismiss and privacy-conscious wording to reduce alert fatigue.",
    notificationPreview:
      "Glucose is above your configured range.",

    diaryKicker: "Glycemic diary",
    diaryTitle: "History made readable.",
    diaryDescription:
      "Export polished PDF and Excel diaries with daily summaries, " +
      "time blocks, weekly review, patterns and data completeness.",

    patternsKicker: "Local insights",
    patternsTitle: "Understand what changed.",
    patternsDescription:
      "Weekly comparison, recurring time-block patterns and history " +
      "completeness turn local readings into more understandable " +
      "personal context.",

    galleryKicker: "Built as a real desktop product",
    galleryTitle:
      "Every screen follows the same calm visual language.",
    galleryDescription:
      "Clean surfaces, clear hierarchy and consistent feedback keep " +
      "glucose context readable throughout the application.",
    galleryDashboard: "Dashboard",
    galleryDiary: "Diary",
    gallerySettings: "Settings",
    galleryAccount: "Account",

    languageKicker: "English and Italian",
    languageTitle: "One experience, fully localized.",
    languageDescription:
      "Language selection starts at first launch and continues across " +
      "dashboard content, settings, notifications, menus and PDF or " +
      "Excel diary exports.",
    languagePointOne: "Runtime language switching",
    languagePointTwo: "Culture-aware dates and numbers",
    languagePointThree: "Localized export content",

    privacyKicker: "Local-first by design",
    privacyTitle: "Your desktop context stays yours.",
    privacyDescription:
      "GlucoDesk does not require a custom GlucoDesk backend for your " +
      "credentials or glucose history. Settings, cached readings and " +
      "generated insights are handled locally on your computer.",
    privacyLocalTitle: "Local history",
    privacyLocalDescription:
      "Readings and analytics are stored locally.",
    privacyCredentialsTitle: "Protected credentials",
    privacyCredentialsDescription:
      "Platform credential stores are used where supported.",
    privacyScreenTitle: "Screen-share privacy",
    privacyScreenDescription:
      "Hide values instantly while preserving awareness.",
    privacyOpenTitle: "Open source",
    privacyOpenDescription:
      "The implementation is available for inspection.",

    downloadKicker: "v0.3.0-preview",
    downloadTitle: "Ready for your desktop.",
    downloadDescription:
      "Choose the package built for your computer. Every download " +
      "comes from the official GlucoDesk v0.3.0-preview release.",
    downloadStatus: "Downloads available",
    downloadNow: "Download preview",
    downloadFooter:
      "GlucoDesk is currently distributed as an unsigned preview. " +
      "Installation instructions and SHA256 checksums will accompany " +
      "the final download links.",
    releaseNotes: "Read release notes",

    safetyTitle: "Awareness companion, not a medical device",
    safetyDescription:
      "GlucoDesk must not be used for treatment decisions, insulin " +
      "dosing, emergency alerts, diagnosis or as a replacement for " +
      "approved diabetes applications, CGM systems or medical devices.",

    footerDescription:
      "A calm, local-first desktop companion for glucose awareness.",
    footerProduct: "Product",
    footerProject: "Project",
    footerReleases: "Releases"
  },

  it: {
    navExperience: "Esperienza",
    navFeatures: "Funzionalità",
    navPrivacy: "Privacy",
    navDownload: "Download",

    heroEyebrow: "Local-first · macOS e Windows",
    heroTitleLineOne: "La tua glicemia.",
    heroTitleLineTwo: "Sempre sotto controllo.",
    heroDescription:
      "GlucoDesk porta il contesto glicemico recente sul tuo desktop " +
      "con una presenza immediata nella barra di sistema, notifiche " +
      "discrete, storico locale leggibile e diari bilingue.",
    heroPrimaryAction: "Scopri la preview",
    heroSecondaryAction: "Guarda il codice",
    trustLocal: "Local-first",
    trustPlatforms: "macOS e Windows",
    trustOpenSource: "Open source",
    statusHigh: "Alta",
    statusPrivacy: "Modalità privacy",
    statusHidden: "Nascosta",
    liveData: "Dati live",
    heroFooter:
      "Pensato per una consapevolezza discreta mentre lavori.",

    experienceKicker: "Immediato per natura",
    experienceTitle:
      "Consapevolezza che non interrompe il tuo lavoro.",
    experienceDescription:
      "GlucoDesk rimane discretamente sul desktop e mostra il contesto " +
      "necessario senza trasformare ogni lettura in un'interruzione.",

    presenceKicker: "Presenza desktop",
    presenceTitle: "Una sola icona. Contesto immediato.",
    presenceDescription:
      "La G di GlucoDesk si aggiorna non appena una nuova lettura cambia " +
      "fascia, offrendo un feedback visivo immediato nella barra menu " +
      "di macOS o nella system tray di Windows.",

    stateInRange: "Nel range",
    stateInRangeDescription: "Feedback verde e discreto",
    stateHigh: "Alta",
    stateHighDescription: "Segnalazione gialla visibile",
    stateLow: "Bassa",
    stateLowDescription: "Feedback rosso chiaro",
    statePrivacy: "Privacy",
    statePrivacyDescription: "Valori nascosti in sicurezza",

    popoverCaption: "Consapevolezza glicemica desktop",
    latestGlucose: "Ultima glicemia",
    updatedNow: "Aggiornata ora",
    openDashboard: "Apri dashboard",

    featuresKicker: "Un ciclo desktop completo",
    featuresTitle:
      "Dall'ultima lettura a una storia più comprensibile.",
    featuresDescription:
      "GlucoDesk unisce consapevolezza live, storico locale ed " +
      "esportazioni leggibili in un'unica esperienza desktop.",

    dashboardKicker: "Dashboard live",
    dashboardTitle: "Il contesto attuale senza rumore.",
    dashboardDescription:
      "Visualizza ultima lettura, trend, range target, grafico recente, " +
      "statistiche e stato dei dati in un layout pulito e responsive.",

    notificationsKicker: "Consapevolezza discreta",
    notificationsTitle: "Notifiche con limiti intelligenti.",
    notificationsDescription:
      "Le notifiche interne e native opzionali utilizzano cooldown, " +
      "posticipo, chiusura e testi rispettosi della privacy per ridurre " +
      "l'affaticamento da avvisi.",
    notificationPreview:
      "La glicemia è sopra il range configurato.",

    diaryKicker: "Diario glicemico",
    diaryTitle: "Uno storico finalmente leggibile.",
    diaryDescription:
      "Esporta diari PDF ed Excel curati con riepiloghi giornalieri, " +
      "fasce orarie, revisione settimanale, pattern e completezza dati.",

    patternsKicker: "Insight locali",
    patternsTitle: "Comprendi cosa è cambiato.",
    patternsDescription:
      "Confronto settimanale, pattern ricorrenti per fascia oraria e " +
      "completezza dello storico trasformano le letture locali in un " +
      "contesto personale più comprensibile.",

    galleryKicker: "Costruito come un vero prodotto desktop",
    galleryTitle:
      "Ogni schermata segue lo stesso linguaggio visivo.",
    galleryDescription:
      "Superfici pulite, gerarchia chiara e feedback coerenti mantengono " +
      "il contesto glicemico leggibile in tutta l'applicazione.",
    galleryDashboard: "Dashboard",
    galleryDiary: "Diario",
    gallerySettings: "Impostazioni",
    galleryAccount: "Account",

    languageKicker: "Italiano e inglese",
    languageTitle: "Un'unica esperienza, completamente localizzata.",
    languageDescription:
      "La scelta della lingua inizia al primo avvio e continua nella " +
      "dashboard, nelle impostazioni, notifiche, menu e nei diari PDF " +
      "ed Excel.",
    languagePointOne: "Cambio lingua durante l'utilizzo",
    languagePointTwo: "Date e numeri culturalmente corretti",
    languagePointThree: "Contenuti esportati localizzati",

    privacyKicker: "Local-first per scelta",
    privacyTitle: "Il tuo contesto desktop rimane tuo.",
    privacyDescription:
      "GlucoDesk non richiede un backend proprietario per le tue " +
      "credenziali o lo storico glicemico. Impostazioni, letture in " +
      "cache e insight vengono gestiti localmente sul computer.",
    privacyLocalTitle: "Storico locale",
    privacyLocalDescription:
      "Letture e analisi vengono archiviate localmente.",
    privacyCredentialsTitle: "Credenziali protette",
    privacyCredentialsDescription:
      "Vengono usati gli archivi sicuri del sistema operativo.",
    privacyScreenTitle: "Privacy durante la condivisione",
    privacyScreenDescription:
      "Nascondi subito i valori mantenendo la consapevolezza.",
    privacyOpenTitle: "Open source",
    privacyOpenDescription:
      "L'implementazione può essere consultata liberamente.",

    downloadKicker: "v0.3.0-preview",
    downloadTitle: "Pronto per il tuo desktop.",
    downloadDescription:
      "Scegli il pacchetto adatto al tuo computer. Ogni download " +
      "proviene dalla release ufficiale GlucoDesk v0.3.0-preview.",
    downloadStatus: "Download disponibili",
    downloadNow: "Scarica la preview",
    downloadFooter:
      "GlucoDesk è attualmente distribuito come preview non firmata. " +
      "Istruzioni di installazione e checksum SHA256 accompagneranno " +
      "i link definitivi.",
    releaseNotes: "Leggi le note di rilascio",

    safetyTitle:
      "Companion di consapevolezza, non dispositivo medico",
    safetyDescription:
      "GlucoDesk non deve essere usato per decisioni terapeutiche, " +
      "dosaggio dell'insulina, avvisi di emergenza, diagnosi o come " +
      "sostituto di applicazioni, sistemi CGM o dispositivi medici " +
      "approvati.",

    footerDescription:
      "Un companion desktop local-first e discreto per la consapevolezza glicemica.",
    footerProduct: "Prodotto",
    footerProject: "Progetto",
    footerReleases: "Release"
  }
};

const galleryItems = {
  dashboard: {
    src: "assets/images/screenshots/dashboard.png",
    title: {
      en: "Dashboard",
      it: "Dashboard"
    },
    alt: {
      en: "GlucoDesk dashboard screenshot",
      it: "Schermata della dashboard di GlucoDesk"
    }
  },
  diary: {
    src: "assets/images/screenshots/diary.png",
    title: {
      en: "Glycemic diary",
      it: "Diario glicemico"
    },
    alt: {
      en: "GlucoDesk glycemic diary screenshot",
      it: "Schermata del diario glicemico di GlucoDesk"
    }
  },
  settings: {
    src: "assets/images/screenshots/settings.png",
    title: {
      en: "Settings",
      it: "Impostazioni"
    },
    alt: {
      en: "GlucoDesk settings screenshot",
      it: "Schermata delle impostazioni di GlucoDesk"
    }
  },
  account: {
    src: "assets/images/screenshots/account.png",
    title: {
      en: "Account",
      it: "Account"
    },
    alt: {
      en: "GlucoDesk account screenshot",
      it: "Schermata account di GlucoDesk"
    }
  }
};

const stateItems = {
  "in-range": {
    icon: "assets/icons/tray-in-range.png",
    value: "112",
    trend: "→",
    color: "#22c55e",
    label: {
      en: "In range",
      it: "Nel range"
    }
  },
  high: {
    icon: "assets/icons/tray-high.png",
    value: "192",
    trend: "↗",
    color: "#f59e0b",
    label: {
      en: "High",
      it: "Alta"
    }
  },
  low: {
    icon: "assets/icons/tray-low.png",
    value: "64",
    trend: "↘",
    color: "#ef4444",
    label: {
      en: "Low",
      it: "Bassa"
    }
  },
  privacy: {
    icon: "assets/icons/tray-privacy.png",
    value: "•••",
    trend: "—",
    color: "#3b82f6",
    label: {
      en: "Privacy mode",
      it: "Modalità privacy"
    }
  }
};

let currentLanguage = "en";
let currentGalleryItem = "dashboard";
let currentPresenceState = "in-range";

const updateLanguage = (language) => {
  if (!translations[language]) {
    return;
  }

  currentLanguage = language;

  document.documentElement.lang = language;
  document.documentElement.dataset.language = language;

  document.querySelectorAll("[data-i18n]").forEach((element) => {
    const key = element.dataset.i18n;
    const value = translations[language][key];

    if (value) {
      element.textContent = value;
    }
  });

  document.querySelectorAll("[data-language-button]").forEach((button) => {
    const isActive = button.dataset.languageButton === language;

    button.classList.toggle("is-active", isActive);
    button.setAttribute("aria-pressed", String(isActive));
  });

  updateGallery(currentGalleryItem);
  updatePresenceState(currentPresenceState);

  localStorage.setItem("glucodesk-site-language", language);

  document.title =
    language === "it"
      ? "GlucoDesk — La tua glicemia, sempre sotto controllo"
      : "GlucoDesk — A calm desktop companion for glucose awareness";
};

const updateGallery = (itemKey) => {
  const item = galleryItems[itemKey];

  if (!item) {
    return;
  }

  currentGalleryItem = itemKey;

  const image = document.querySelector("[data-gallery-image]");
  const title = document.querySelector("[data-gallery-title]");

  if (image) {
    image.src = item.src;
    image.alt = item.alt[currentLanguage];
  }

  if (title) {
    title.textContent = item.title[currentLanguage];
  }

  document.querySelectorAll("[data-gallery-tab]").forEach((button) => {
    const isActive = button.dataset.galleryTab === itemKey;

    button.classList.toggle("is-active", isActive);
    button.setAttribute("aria-selected", String(isActive));
  });
};

const updatePresenceState = (stateKey) => {
  const state = stateItems[stateKey];

  if (!state) {
    return;
  }

  currentPresenceState = stateKey;

  const demo = document.querySelector("[data-presence-demo]");
  const icon = document.querySelector("[data-state-icon]");
  const value = document.querySelector("[data-state-value]");
  const trend = document.querySelector("[data-state-trend]");
  const label = document.querySelector("[data-state-label]");
  const dot = document.querySelector("[data-state-dot]");

  if (demo) {
    demo.dataset.presenceDemo = stateKey;
  }

  if (icon) {
    icon.src = state.icon;
  }

  if (value) {
    value.textContent = state.value;
  }

  if (trend) {
    trend.textContent = state.trend;
    trend.style.color = state.color;
  }

  if (label) {
    label.textContent = state.label[currentLanguage];
  }

  if (dot) {
    dot.style.backgroundColor = state.color;
  }

  document.querySelectorAll("[data-state]").forEach((button) => {
    button.classList.toggle(
      "is-active",
      button.dataset.state === stateKey
    );
  });
};

const setupRevealAnimations = () => {
  const elements = document.querySelectorAll(".reveal");

  if (
    window.matchMedia("(prefers-reduced-motion: reduce)").matches ||
    !("IntersectionObserver" in window)
  ) {
    elements.forEach((element) => {
      element.classList.add("is-visible");
    });

    return;
  }

  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (!entry.isIntersecting) {
          return;
        }

        entry.target.classList.add("is-visible");
        observer.unobserve(entry.target);
      });
    },
    {
      threshold: 0.13,
      rootMargin: "0px 0px -40px"
    }
  );

  elements.forEach((element) => {
    observer.observe(element);
  });
};

const setupHeader = () => {
  const header = document.querySelector("[data-header]");

  const update = () => {
    header?.classList.toggle("is-scrolled", window.scrollY > 18);
  };

  update();

  window.addEventListener("scroll", update, {
    passive: true
  });
};

const setupMobileNavigation = () => {
  const button = document.querySelector("[data-mobile-menu-button]");
  const navigation = document.querySelector("[data-mobile-nav]");

  if (!button || !navigation) {
    return;
  }

  const close = () => {
    navigation.classList.remove("is-open");
    button.setAttribute("aria-expanded", "false");
    document.body.classList.remove("is-menu-open");
  };

  button.addEventListener("click", () => {
    const willOpen = !navigation.classList.contains("is-open");

    navigation.classList.toggle("is-open", willOpen);
    button.setAttribute("aria-expanded", String(willOpen));
    document.body.classList.toggle("is-menu-open", willOpen);
  });

  navigation.querySelectorAll("a").forEach((link) => {
    link.addEventListener("click", close);
  });

  window.addEventListener("resize", () => {
    if (window.innerWidth > 1100) {
      close();
    }
  });
};

const setupCursorGlow = () => {
  const glow = document.querySelector(".cursor-glow");

  if (
    !glow ||
    window.matchMedia("(pointer: coarse)").matches ||
    window.matchMedia("(prefers-reduced-motion: reduce)").matches
  ) {
    return;
  }

  window.addEventListener(
    "pointermove",
    (event) => {
      glow.style.left = `${event.clientX}px`;
      glow.style.top = `${event.clientY}px`;
    },
    {
      passive: true
    }
  );
};

const setupGallery = () => {
  document.querySelectorAll("[data-gallery-tab]").forEach((button) => {
    button.addEventListener("click", () => {
      updateGallery(button.dataset.galleryTab);
    });
  });
};

const setupPresenceStates = () => {
  document.querySelectorAll("[data-state]").forEach((button) => {
    button.addEventListener("click", () => {
      updatePresenceState(button.dataset.state);
    });
  });
};

const setupLanguage = () => {
  document.querySelectorAll("[data-language-button]").forEach((button) => {
    button.addEventListener("click", () => {
      updateLanguage(button.dataset.languageButton);
    });
  });

  const storedLanguage =
    localStorage.getItem("glucodesk-site-language");

  const browserLanguage =
    navigator.language?.toLowerCase().startsWith("it")
      ? "it"
      : "en";

  updateLanguage(
    storedLanguage === "it" || storedLanguage === "en"
      ? storedLanguage
      : browserLanguage
  );
};

const setupYear = () => {
  document.querySelectorAll("[data-current-year]").forEach((element) => {
    element.textContent = String(new Date().getFullYear());
  });
};

document.addEventListener("DOMContentLoaded", () => {
  setupYear();
  setupLanguage();
  setupGallery();
  setupPresenceStates();
  setupRevealAnimations();
  setupHeader();
  setupMobileNavigation();
  setupCursorGlow();
});

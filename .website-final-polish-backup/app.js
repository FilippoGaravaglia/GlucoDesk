"use strict";

const translations = {
  en: {
    skipToContent: "Skip to content",
    brandHomeLabel: "GlucoDesk home",
    openNavigationLabel: "Open navigation",
    closeNavigationLabel: "Close navigation",
    galleryTabsLabel: "GlucoDesk screens",

    navExperience: "Experience",
    navFeatures: "Features",
    navPrivacy: "Privacy",
    navDownload: "Download",

    heroEyebrow: "Free preview · macOS & Windows",
    heroTitleLineOne: "Your glucose.",
    heroTitleLineTwo: "Always in sight.",
    heroDescription:
      "Keep your glucose visible while you work, without constantly " +
      "checking another device. GlucoDesk brings live readings, local " +
      "history and clear insights directly to your desktop.",
    heroPrimaryAction: "Download GlucoDesk",
    heroSecondaryAction: "View on GitHub",
    trustLocal: "Your data stays local",
    trustPlatforms: "macOS & Windows",
    trustOpenSource: "Free and open source",
    statusHigh: "High",
    statusPrivacy: "Privacy mode",
    statusHidden: "Hidden",
    liveData: "Live data",
    heroFooter: "Less checking. More focus on your day.",

    experienceKicker: "Always within reach",
    experienceTitle: "See what matters without stopping what you are doing.",
    experienceDescription:
      "GlucoDesk stays quietly on your desktop and gives you useful " +
      "glucose context without making every reading feel like an interruption.",

    presenceKicker: "Desktop presence",
    presenceTitle: "One icon. Immediate context.",
    presenceDescription:
      "The GlucoDesk icon changes with your glucose range, so you can " +
      "understand the current situation from the macOS menu bar or " +
      "Windows system tray at a glance.",

    stateInRange: "In range",
    stateInRangeDescription: "Calm green feedback",
    stateHigh: "High",
    stateHighDescription: "Clear yellow feedback",
    stateLow: "Low",
    stateLowDescription: "Clear red feedback",
    statePrivacy: "Privacy",
    statePrivacyDescription: "Values safely hidden",

    popoverCaption: "Glucose on your desktop",
    latestGlucose: "Latest glucose",
    updatedNow: "Updated now",
    openDashboard: "Open dashboard",

    featuresKicker: "Everything in one place",
    featuresTitle: "From the latest reading to a clearer picture.",
    featuresDescription:
      "See what is happening now, review your local history and export " +
      "a clear diary from one focused desktop experience.",

    dashboardKicker: "Live dashboard",
    dashboardTitle: "Your current context, without the noise.",
    dashboardDescription:
      "See your latest reading, trend, target range, recent chart, " +
      "statistics and data completeness in one clear view.",

    notificationsKicker: "Useful, not intrusive",
    notificationsTitle: "Notifications that respect your attention.",
    notificationsDescription:
      "Choose when to be notified. Cooldowns, snooze and privacy-aware " +
      "messages help reduce repeated or unnecessary alerts.",
    notificationPreview:
      "Glucose is above your configured range.",

    diaryKicker: "Glycemic diary",
    diaryTitle: "A history that is easier to understand.",
    diaryDescription:
      "Export clear PDF and Excel diaries with daily summaries, time " +
      "blocks, weekly comparisons, recurring patterns and data completeness.",

    patternsKicker: "Local insights",
    patternsTitle: "See what changed over time.",
    patternsDescription:
      "Weekly comparisons and recurring time-of-day patterns help turn " +
      "your local readings into context that is easier to understand.",

    galleryKicker: "Designed as a real desktop product",
    galleryTitle: "A clear and consistent experience on every screen.",
    galleryDescription:
      "A simple visual hierarchy keeps your glucose information readable " +
      "across the dashboard, diary, settings and account screens.",
    galleryDashboard: "Dashboard",
    galleryDiary: "Diary",
    gallerySettings: "Settings",
    galleryAccount: "Account",

    languageKicker: "English and Italian",
    languageTitle: "Use GlucoDesk in your language.",
    languageDescription:
      "Choose your language at first launch or change it later. The " +
      "dashboard, settings, notifications, menus and diary exports follow it.",
    languagePointOne: "Switch language at any time",
    languagePointTwo: "Dates and numbers shown correctly",
    languagePointThree: "PDF and Excel exports in your language",

    privacyKicker: "Local-first by design",
    privacyTitle: "Your glucose data stays on your computer.",
    privacyDescription:
      "GlucoDesk does not upload your glucose history or credentials to " +
      "a GlucoDesk cloud. Settings, cached readings and insights are " +
      "handled locally on your computer.",
    privacyLocalTitle: "Local history",
    privacyLocalDescription:
      "Your readings and analysis remain on your device.",
    privacyCredentialsTitle: "Protected credentials",
    privacyCredentialsDescription:
      "Secure operating-system storage is used where supported.",
    privacyScreenTitle: "Screen-sharing privacy",
    privacyScreenDescription:
      "Hide glucose values instantly during calls or presentations.",
    privacyOpenTitle: "Open source",
    privacyOpenDescription:
      "The code is public and available for inspection.",
    privacyVisualHidden: "Hidden",
    privacyVisualEnabled: "Privacy mode enabled",
    privacyChipLocal: "Local data",
    privacyChipCloud: "No GlucoDesk cloud",
    privacyChipOpen: "Open source",

    founderKicker: "The story behind GlucoDesk",
    founderTitle: "Built because I needed it.",
    founderDescription:
      "Living with Type 1 diabetes already requires enough attention. " +
      "I created GlucoDesk to make glucose awareness feel simpler and " +
      "less intrusive during everyday work.",
    founderRole: "Developer living with Type 1 diabetes",
    founderQuote: "One less thing to interrupt your day.",

    downloadKicker: "v0.3.0-preview",
    downloadTitle: "Bring GlucoDesk to your desktop.",
    downloadDescription:
      "Choose the package made for your computer. The preview is free, " +
      "open source and available from the official GlucoDesk GitHub release.",
    downloadStatus: "Downloads available",
    downloadNow: "Download preview",
    downloadFooter:
      "GlucoDesk is currently distributed as a free, unsigned preview. " +
      "Follow the release instructions for installation details and verify " +
      "that the package comes from the official GitHub release.",
    releaseNotes: "Installation and release notes",

    safetyTitle: "A companion, not a medical device",
    safetyDescription:
      "GlucoDesk must not be used for treatment decisions, insulin " +
      "dosing, emergency alerts, diagnosis or as a replacement for " +
      "approved diabetes applications, CGM systems or medical devices.",

    footerDescription:
      "Your glucose, always in sight. Free, local-first and open source.",
    footerProduct: "Product",
    footerProject: "Project",
    footerReleases: "Releases"
  },

  it: {
    skipToContent: "Vai al contenuto",
    brandHomeLabel: "Homepage GlucoDesk",
    openNavigationLabel: "Apri navigazione",
    closeNavigationLabel: "Chiudi navigazione",
    galleryTabsLabel: "Schermate di GlucoDesk",

    navExperience: "Esperienza",
    navFeatures: "Funzionalità",
    navPrivacy: "Privacy",
    navDownload: "Download",

    heroEyebrow: "Preview gratuita · macOS e Windows",
    heroTitleLineOne: "La tua glicemia.",
    heroTitleLineTwo: "Sempre in vista.",
    heroDescription:
      "Tieni la glicemia visibile mentre lavori, senza dover controllare " +
      "continuamente un altro dispositivo. GlucoDesk porta letture live, " +
      "storico e informazioni chiare direttamente sul desktop.",
    heroPrimaryAction: "Scarica GlucoDesk",
    heroSecondaryAction: "Guarda su GitHub",
    trustLocal: "I tuoi dati restano locali",
    trustPlatforms: "macOS e Windows",
    trustOpenSource: "Gratis e open source",
    statusHigh: "Alta",
    statusPrivacy: "Modalità privacy",
    statusHidden: "Nascosta",
    liveData: "Dati live",
    heroFooter: "Meno controlli. Più attenzione alla tua giornata.",

    experienceKicker: "Sempre a portata di sguardo",
    experienceTitle: "Vedi ciò che conta senza interrompere quello che fai.",
    experienceDescription:
      "GlucoDesk rimane discretamente sul desktop e ti mostra il contesto " +
      "glicemico utile senza trasformare ogni lettura in un'interruzione.",

    presenceKicker: "Presenza desktop",
    presenceTitle: "Una sola icona. Contesto immediato.",
    presenceDescription:
      "L'icona di GlucoDesk cambia insieme alla tua fascia glicemica, " +
      "così puoi capire la situazione con uno sguardo dalla barra menu " +
      "di macOS o dalla system tray di Windows.",

    stateInRange: "Nel range",
    stateInRangeDescription: "Feedback verde e discreto",
    stateHigh: "Alta",
    stateHighDescription: "Feedback giallo chiaro",
    stateLow: "Bassa",
    stateLowDescription: "Feedback rosso chiaro",
    statePrivacy: "Privacy",
    statePrivacyDescription: "Valori nascosti in sicurezza",

    popoverCaption: "La glicemia sul desktop",
    latestGlucose: "Ultima glicemia",
    updatedNow: "Aggiornata ora",
    openDashboard: "Apri dashboard",

    featuresKicker: "Tutto in un unico posto",
    featuresTitle: "Dall'ultima lettura a un quadro più chiaro.",
    featuresDescription:
      "Controlla cosa sta accadendo, consulta lo storico locale ed esporta " +
      "un diario chiaro da un'unica esperienza desktop.",

    dashboardKicker: "Dashboard live",
    dashboardTitle: "Il contesto attuale, senza rumore.",
    dashboardDescription:
      "Visualizza ultima lettura, trend, range target, grafico recente, " +
      "statistiche e completezza dei dati in una schermata chiara.",

    notificationsKicker: "Utili, non invadenti",
    notificationsTitle: "Notifiche che rispettano la tua attenzione.",
    notificationsDescription:
      "Scegli quando riceverle. Cooldown, posticipo e testi rispettosi " +
      "della privacy aiutano a ridurre avvisi ripetuti o non necessari.",
    notificationPreview:
      "La glicemia è sopra il range configurato.",

    diaryKicker: "Diario glicemico",
    diaryTitle: "Uno storico più semplice da capire.",
    diaryDescription:
      "Esporta diari PDF ed Excel chiari con riepiloghi giornalieri, " +
      "fasce orarie, confronti settimanali, pattern e completezza dati.",

    patternsKicker: "Informazioni locali",
    patternsTitle: "Scopri cosa è cambiato nel tempo.",
    patternsDescription:
      "I confronti settimanali e i pattern ricorrenti per fascia oraria " +
      "trasformano le letture locali in un contesto più comprensibile.",

    galleryKicker: "Progettato come un vero prodotto desktop",
    galleryTitle: "Un'esperienza chiara e coerente in ogni schermata.",
    galleryDescription:
      "Una gerarchia visiva semplice mantiene leggibili le informazioni " +
      "in dashboard, diario, impostazioni e account.",
    galleryDashboard: "Dashboard",
    galleryDiary: "Diario",
    gallerySettings: "Impostazioni",
    galleryAccount: "Account",

    languageKicker: "Italiano e inglese",
    languageTitle: "Usa GlucoDesk nella tua lingua.",
    languageDescription:
      "Scegli la lingua al primo avvio o cambiala in seguito. Dashboard, " +
      "impostazioni, notifiche, menu ed esportazioni seguiranno la scelta.",
    languagePointOne: "Cambia lingua in qualsiasi momento",
    languagePointTwo: "Date e numeri mostrati correttamente",
    languagePointThree: "Esportazioni PDF ed Excel nella tua lingua",

    privacyKicker: "Local-first per scelta",
    privacyTitle: "I tuoi dati glicemici restano sul computer.",
    privacyDescription:
      "GlucoDesk non carica lo storico glicemico o le credenziali su un " +
      "cloud GlucoDesk. Impostazioni, letture in cache e informazioni " +
      "vengono gestite localmente sul tuo computer.",
    privacyLocalTitle: "Storico locale",
    privacyLocalDescription:
      "Letture e analisi rimangono sul tuo dispositivo.",
    privacyCredentialsTitle: "Credenziali protette",
    privacyCredentialsDescription:
      "Dove supportato, viene usato l'archivio sicuro del sistema operativo.",
    privacyScreenTitle: "Privacy durante la condivisione",
    privacyScreenDescription:
      "Nascondi subito i valori durante chiamate o presentazioni.",
    privacyOpenTitle: "Open source",
    privacyOpenDescription:
      "Il codice è pubblico e può essere consultato.",
    privacyVisualHidden: "Nascosta",
    privacyVisualEnabled: "Modalità privacy attiva",
    privacyChipLocal: "Dati locali",
    privacyChipCloud: "Nessun cloud GlucoDesk",
    privacyChipOpen: "Open source",

    founderKicker: "La storia dietro GlucoDesk",
    founderTitle: "Creato perché ne avevo bisogno.",
    founderDescription:
      "Vivere con il diabete tipo 1 richiede già abbastanza attenzione. " +
      "Ho creato GlucoDesk per rendere il controllo della glicemia più " +
      "semplice e meno invasivo durante il lavoro quotidiano.",
    founderRole: "Sviluppatore e persona con diabete tipo 1",
    founderQuote: "Una cosa in meno a interrompere la tua giornata.",

    downloadKicker: "v0.3.0-preview",
    downloadTitle: "Porta GlucoDesk sul tuo desktop.",
    downloadDescription:
      "Scegli il pacchetto adatto al tuo computer. La preview è gratuita, " +
      "open source e disponibile dalla release GitHub ufficiale di GlucoDesk.",
    downloadStatus: "Download disponibili",
    downloadNow: "Scarica la preview",
    downloadFooter:
      "GlucoDesk è attualmente distribuito come preview gratuita e non " +
      "firmata. Segui le istruzioni della release e verifica che il pacchetto " +
      "provenga dalla release GitHub ufficiale.",
    releaseNotes: "Installazione e note di rilascio",

    safetyTitle: "Un companion, non un dispositivo medico",
    safetyDescription:
      "GlucoDesk non deve essere usato per decisioni terapeutiche, " +
      "dosaggio dell'insulina, avvisi di emergenza, diagnosi o come " +
      "sostituto di applicazioni, sistemi CGM o dispositivi medici approvati.",

    footerDescription:
      "La tua glicemia, sempre in vista. Gratis, local-first e open source.",
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

  document
    .querySelectorAll("[data-i18n-aria-label]")
    .forEach((element) => {
      const key = element.dataset.i18nAriaLabel;
      const value = translations[language][key];

      if (value) {
        element.setAttribute("aria-label", value);
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

  const pageMetadata =
    language === "it"
      ? {
          title: "GlucoDesk — La tua glicemia, sempre in vista",
          description:
            "Tieni la glicemia visibile mentre lavori. GlucoDesk porta " +
            "letture live, storico locale, privacy ed esportazioni chiare " +
            "su macOS e Windows.",
          locale: "it_IT"
        }
      : {
          title: "GlucoDesk — Your glucose, always in sight",
          description:
            "Keep your glucose visible while you work. GlucoDesk brings " +
            "live readings, local history, privacy controls and clear " +
            "diary exports to macOS and Windows.",
          locale: "en_US"
        };

  document.title = pageMetadata.title;

  const description = document.querySelector('meta[name="description"]');
  const ogTitle = document.querySelector('meta[property="og:title"]');
  const ogDescription =
    document.querySelector('meta[property="og:description"]');
  const ogLocale = document.querySelector('meta[property="og:locale"]');
  const twitterTitle =
    document.querySelector('meta[name="twitter:title"]');
  const twitterDescription =
    document.querySelector('meta[name="twitter:description"]');

  description?.setAttribute("content", pageMetadata.description);
  ogTitle?.setAttribute("content", pageMetadata.title);
  ogDescription?.setAttribute("content", pageMetadata.description);
  ogLocale?.setAttribute("content", pageMetadata.locale);
  twitterTitle?.setAttribute("content", pageMetadata.title);
  twitterDescription?.setAttribute(
    "content",
    pageMetadata.description
  );
};

const updateGallery = (itemKey, animate = false) => {
  const item = galleryItems[itemKey];

  if (!item) {
    return;
  }

  currentGalleryItem = itemKey;

  const image = document.querySelector("[data-gallery-image]");
  const title = document.querySelector("[data-gallery-title]");
  const windowElement = document.querySelector(".gallery-window");

  const applyItem = () => {
    if (image) {
      image.src = item.src;
      image.alt = item.alt[currentLanguage];
    }

    if (title) {
      title.textContent = item.title[currentLanguage];
    }

    windowElement?.classList.remove("is-switching");
  };

  if (
    animate &&
    windowElement &&
    !window.matchMedia("(prefers-reduced-motion: reduce)").matches
  ) {
    windowElement.classList.add("is-switching");
    window.setTimeout(applyItem, 150);
  } else {
    applyItem();
  }

  document.querySelectorAll("[data-gallery-tab]").forEach((button) => {
    const isActive = button.dataset.galleryTab === itemKey;

    button.classList.toggle("is-active", isActive);
    button.setAttribute("aria-selected", String(isActive));
    button.setAttribute("tabindex", isActive ? "0" : "-1");
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
    const isActive = button.dataset.state === stateKey;

    button.classList.toggle("is-active", isActive);
    button.setAttribute("aria-pressed", String(isActive));
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

  const close = ({ restoreFocus = false } = {}) => {
    navigation.classList.remove("is-open");
    button.setAttribute("aria-expanded", "false");
    button.setAttribute(
      "aria-label",
      translations[currentLanguage].openNavigationLabel
    );
    document.body.classList.remove("is-menu-open");

    if (restoreFocus) {
      button.focus();
    }
  };

  button.addEventListener("click", () => {
    const willOpen = !navigation.classList.contains("is-open");

    navigation.classList.toggle("is-open", willOpen);
    button.setAttribute("aria-expanded", String(willOpen));
    button.setAttribute(
      "aria-label",
      willOpen
        ? translations[currentLanguage].closeNavigationLabel
        : translations[currentLanguage].openNavigationLabel
    );
    document.body.classList.toggle("is-menu-open", willOpen);

    if (willOpen) {
      navigation.querySelector("a")?.focus();
    }
  });

  navigation.querySelectorAll("a").forEach((link) => {
    link.addEventListener("click", () => close());
  });

  document.addEventListener("keydown", (event) => {
    if (
      event.key === "Escape" &&
      navigation.classList.contains("is-open")
    ) {
      close({ restoreFocus: true });
    }
  });

  document.addEventListener("click", (event) => {
    if (
      navigation.classList.contains("is-open") &&
      !navigation.contains(event.target) &&
      !button.contains(event.target)
    ) {
      close();
    }
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
  const tabs = Array.from(
    document.querySelectorAll("[data-gallery-tab]")
  );

  tabs.forEach((button, index) => {
    button.addEventListener("click", () => {
      updateGallery(button.dataset.galleryTab, true);
    });

    button.addEventListener("keydown", (event) => {
      let nextIndex = index;

      if (event.key === "ArrowRight") {
        nextIndex = (index + 1) % tabs.length;
      } else if (event.key === "ArrowLeft") {
        nextIndex = (index - 1 + tabs.length) % tabs.length;
      } else if (event.key === "Home") {
        nextIndex = 0;
      } else if (event.key === "End") {
        nextIndex = tabs.length - 1;
      } else {
        return;
      }

      event.preventDefault();

      const nextTab = tabs[nextIndex];
      nextTab.focus();
      updateGallery(nextTab.dataset.galleryTab, true);
    });
  });
};

const setupPresenceStates = () => {
  document.querySelectorAll("[data-state]").forEach((button) => {
    button.setAttribute(
      "aria-pressed",
      String(button.classList.contains("is-active"))
    );

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

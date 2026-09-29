const translations = {
  en: {
    skipToContent: "Skip to content",
    brandHomeLabel: "GlucoDesk home",
    openNavigationLabel: "Open navigation",
    closeNavigationLabel: "Close navigation",

    navProduct: "Product",
    navCarbGuide: "Carb guide",
    navPrivacy: "Privacy",
    navReviews: "Reviews",
    navUpdates: "Updates",
    navFounder: "Built by",
    navDownload: "Download",
    navSupport: "Support",

    updatesEyebrow: "PRODUCT UPDATES",
    updatesTitle: "Follow the evolution of GlucoDesk.",
    updatesDescription:
      "New releases, meaningful improvements and the features shaping " +
      "the GlucoDesk desktop experience.",

    updatesCurrentLabel: "Current preview",
    updatesYearDescription:
      "The first public chapter of GlucoDesk.",

    release030Marker: "CURRENT",
    releasePreviewBadge: "Preview",
    releaseNewBadge: "New",

    release030Title:
      "First public preview",

    release030Summary:
      "The first complete public GlucoDesk experience: glucose awareness, " +
      "private local history and useful desktop tools in one focused companion.",

    releaseHighlightsTitle:
      "Highlights",

    release030PointOneTitle:
      "A complete desktop companion",

    release030PointOneDescription:
      "Live glucose, recent trends, local history and desktop presence " +
      "come together in one experience.",

    release030PointTwoTitle:
      "In-app Update Center",

    release030PointTwoDescription:
      "GlucoDesk now checks for new versions, shows user-friendly release " +
      "highlights and takes you directly to the correct download for your platform.",

    release030PointThreeTitle:
      "Local-first history and diary",

    release030PointThreeDescription:
      "Build a private local glucose history and export clearer PDF and Excel diaries.",

    release030PointFourTitle:
      "Everyday desktop tools",

    release030PointFourDescription:
      "Awareness notifications, privacy controls, local backup and the visual " +
      "carbohydrate guide expand the daily GlucoDesk experience.",

    releaseViewFull:
      "View full release",

    releaseDownload:
      "Download GlucoDesk",

    updatesNextEyebrow:
      "NEXT RELEASES",

    updatesNextTitle:
      "This timeline will grow with GlucoDesk.",

    updatesNextDescription:
      "Future releases will appear here with a concise, user-friendly summary. " +
      "Full technical details will continue to live on GitHub.",

    updatesGitHubAction:
      "Follow development",

    footerDescription:
      "Your glucose, always in sight.",

    footerIndependent:
      "Independent open-source project"
  },

  it: {
    skipToContent: "Vai al contenuto",
    brandHomeLabel: "Homepage GlucoDesk",
    openNavigationLabel: "Apri navigazione",
    closeNavigationLabel: "Chiudi navigazione",

    navProduct: "Prodotto",
    navCarbGuide: "Guida carboidrati",
    navPrivacy: "Privacy",
    navReviews: "Recensioni",
    navUpdates: "Aggiornamenti",
    navFounder: "Chi l'ha creato",
    navDownload: "Download",
    navSupport: "Sostieni",

    updatesEyebrow:
      "AGGIORNAMENTI DEL PRODOTTO",

    updatesTitle:
      "Segui l'evoluzione di GlucoDesk.",

    updatesDescription:
      "Nuove release, miglioramenti significativi e le funzionalità che " +
      "fanno evolvere l'esperienza desktop di GlucoDesk.",

    updatesCurrentLabel:
      "Preview attuale",

    updatesYearDescription:
      "Il primo capitolo pubblico di GlucoDesk.",

    release030Marker:
      "ATTUALE",

    releasePreviewBadge:
      "Preview",

    releaseNewBadge:
      "Novità",

    release030Title:
      "Prima preview pubblica",

    release030Summary:
      "La prima esperienza pubblica completa di GlucoDesk: consapevolezza " +
      "glicemica, storico locale privato e strumenti desktop in un unico companion.",

    releaseHighlightsTitle:
      "Novità principali",

    release030PointOneTitle:
      "Un companion desktop completo",

    release030PointOneDescription:
      "Glicemia live, andamento recente, storico locale e presenza desktop " +
      "si incontrano in un'unica esperienza.",

    release030PointTwoTitle:
      "Sistema di aggiornamento integrato",

    release030PointTwoDescription:
      "GlucoDesk ora controlla la disponibilità di nuove versioni, mostra " +
      "le principali novità in modo semplice e porta direttamente al download " +
      "corretto per il tuo sistema.",

    release030PointThreeTitle:
      "Storico e diario local-first",

    release030PointThreeDescription:
      "Costruisci uno storico glicemico locale e privato ed esporta diari " +
      "più chiari in PDF ed Excel.",

    release030PointFourTitle:
      "Strumenti per l'uso quotidiano",

    release030PointFourDescription:
      "Notifiche di consapevolezza, controlli per la privacy, backup locale " +
      "e guida visiva ai carboidrati ampliano l'esperienza quotidiana.",

    releaseViewFull:
      "Vedi release completa",

    releaseDownload:
      "Scarica GlucoDesk",

    updatesNextEyebrow:
      "PROSSIME RELEASE",

    updatesNextTitle:
      "La timeline crescerà insieme a GlucoDesk.",

    updatesNextDescription:
      "Le prossime release appariranno qui con un riepilogo breve e pensato " +
      "per gli utenti. I dettagli tecnici completi resteranno disponibili su GitHub.",

    updatesGitHubAction:
      "Segui lo sviluppo",

    footerDescription:
      "La tua glicemia, sempre in vista.",

    footerIndependent:
      "Progetto open source indipendente"
  }
};

let currentLanguage = "en";

const updatePageMetadata = (language) => {
  const metadata =
    language === "it"
      ? {
          title:
            "Aggiornamenti GlucoDesk — Release e novità",
          description:
            "Segui le release, le nuove funzionalità e i miglioramenti di GlucoDesk.",
          locale:
            "it_IT"
        }
      : {
          title:
            "GlucoDesk Updates — Releases and product improvements",
          description:
            "Follow GlucoDesk releases, new features and product improvements.",
          locale:
            "en_US"
        };

  document.title = metadata.title;

  document
    .querySelector('meta[name="description"]')
    ?.setAttribute(
      "content",
      metadata.description
    );

  document
    .querySelector('meta[property="og:title"]')
    ?.setAttribute(
      "content",
      metadata.title
    );

  document
    .querySelector('meta[property="og:description"]')
    ?.setAttribute(
      "content",
      metadata.description
    );

  document
    .querySelector('meta[property="og:locale"]')
    ?.setAttribute(
      "content",
      metadata.locale
    );

  document
    .querySelector('meta[name="twitter:title"]')
    ?.setAttribute(
      "content",
      metadata.title
    );

  document
    .querySelector('meta[name="twitter:description"]')
    ?.setAttribute(
      "content",
      metadata.description
    );
};

const updateLanguage = (language) => {
  if (!translations[language]) {
    return;
  }

  currentLanguage = language;

  document.documentElement.lang = language;

  document
    .querySelectorAll("[data-i18n]")
    .forEach((element) => {
      const key =
        element.dataset.i18n;

      const value =
        translations[language][key];

      if (value !== undefined) {
        element.textContent = value;
      }
    });

  document
    .querySelectorAll("[data-i18n-aria-label]")
    .forEach((element) => {
      const key =
        element.dataset.i18nAriaLabel;

      const value =
        translations[language][key];

      if (value !== undefined) {
        element.setAttribute(
          "aria-label",
          value
        );
      }
    });

  document
    .querySelectorAll("[data-language-button]")
    .forEach((button) => {
      const isActive =
        button.dataset.languageButton === language;

      button.classList.toggle(
        "is-active",
        isActive
      );

      button.setAttribute(
        "aria-pressed",
        String(isActive)
      );
    });

  updatePageMetadata(language);

  localStorage.setItem(
    "glucodesk-site-language",
    language
  );
};

const setupLanguage = () => {
  const storedLanguage =
    localStorage.getItem(
      "glucodesk-site-language"
    );

  const browserLanguage =
    navigator.language
      ?.toLowerCase()
      .startsWith("it")
      ? "it"
      : "en";

  const initialLanguage =
    storedLanguage === "en" ||
    storedLanguage === "it"
      ? storedLanguage
      : browserLanguage;

  updateLanguage(initialLanguage);

  document
    .querySelectorAll("[data-language-button]")
    .forEach((button) => {
      button.addEventListener(
        "click",
        () => {
          updateLanguage(
            button.dataset.languageButton
          );
        }
      );
    });
};

const setupHeader = () => {
  const header =
    document.querySelector("[data-header]");

  if (!header) {
    return;
  }

  const updateHeader = () => {
    header.classList.toggle(
      "is-scrolled",
      window.scrollY > 16
    );
  };

  updateHeader();

  window.addEventListener(
    "scroll",
    updateHeader,
    { passive: true }
  );
};

const setupMobileNavigation = () => {
  const button =
    document.querySelector(
      "[data-mobile-menu-button]"
    );

  const navigation =
    document.querySelector(
      "[data-mobile-nav]"
    );

  if (!button || !navigation) {
    return;
  }

  const closeNavigation = ({
    restoreFocus = false
  } = {}) => {
    navigation.classList.remove(
      "is-open"
    );

    button.setAttribute(
      "aria-expanded",
      "false"
    );

    button.setAttribute(
      "aria-label",
      translations[currentLanguage]
        .openNavigationLabel
    );

    document.body.classList.remove(
      "is-menu-open"
    );

    if (restoreFocus) {
      button.focus();
    }
  };

  button.addEventListener(
    "click",
    () => {
      const shouldOpen =
        !navigation.classList.contains(
          "is-open"
        );

      navigation.classList.toggle(
        "is-open",
        shouldOpen
      );

      button.setAttribute(
        "aria-expanded",
        String(shouldOpen)
      );

      button.setAttribute(
        "aria-label",
        shouldOpen
          ? translations[currentLanguage]
              .closeNavigationLabel
          : translations[currentLanguage]
              .openNavigationLabel
      );

      document.body.classList.toggle(
        "is-menu-open",
        shouldOpen
      );
    }
  );

  navigation
    .querySelectorAll("a")
    .forEach((link) => {
      link.addEventListener(
        "click",
        () => closeNavigation()
      );
    });

  document.addEventListener(
    "keydown",
    (event) => {
      if (
        event.key === "Escape" &&
        navigation.classList.contains(
          "is-open"
        )
      ) {
        closeNavigation({
          restoreFocus: true
        });
      }
    }
  );

  document.addEventListener(
    "click",
    (event) => {
      if (
        navigation.classList.contains(
          "is-open"
        ) &&
        !navigation.contains(event.target) &&
        !button.contains(event.target)
      ) {
        closeNavigation();
      }
    }
  );

  window.addEventListener(
    "resize",
    () => {
      if (window.innerWidth > 1000) {
        closeNavigation();
      }
    }
  );
};

const setupRevealAnimations = () => {
  const elements =
    document.querySelectorAll(".reveal");

  if (
    window.matchMedia(
      "(prefers-reduced-motion: reduce)"
    ).matches ||
    !("IntersectionObserver" in window)
  ) {
    elements.forEach((element) => {
      element.classList.add(
        "is-visible"
      );
    });

    return;
  }

  const observer =
    new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (!entry.isIntersecting) {
            return;
          }

          entry.target.classList.add(
            "is-visible"
          );

          observer.unobserve(
            entry.target
          );
        });
      },
      {
        threshold: 0.1,
        rootMargin:
          "0px 0px -30px"
      }
    );

  elements.forEach((element) => {
    observer.observe(element);
  });
};

const setupCurrentYear = () => {
  document
    .querySelectorAll(
      "[data-current-year]"
    )
    .forEach((element) => {
      element.textContent =
        String(
          new Date().getFullYear()
        );
    });
};

document.addEventListener(
  "DOMContentLoaded",
  () => {
    setupCurrentYear();
    setupLanguage();
    setupHeader();
    setupMobileNavigation();
    setupRevealAnimations();
  }
);

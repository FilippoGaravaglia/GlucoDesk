"use strict";

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
    navFounder: "Built by",
    navDownload: "Download",

    heroBadge: "Free · Open source · macOS & Windows",
    heroTitleOne: "Your glucose.",
    heroTitleTwo: "Always in sight.",
    heroDescription:
      "A private desktop companion for CGM users. Keep the information " +
      "that matters visible while you work, without constantly reaching " +
      "for another device.",
    heroPrimaryAction: "Download GlucoDesk",
    heroSecondaryAction: "View on GitHub",

    trustLocal: "Local-first",
    trustIndependent: "Independent",
    trustFree: "Free",

    heroImageAlt:
      "GlucoDesk dashboard showing current glucose, trend and recent history",
    heroGlucoseStatus: "In range",
    heroGlucoseUpdated: "Updated now",

    valueKicker: "Built for everyday use",
    valueTitle: "Glucose awareness without leaving your work.",
    valueDescription:
      "GlucoDesk brings live context, local history and useful insights " +
      "into one calm desktop experience.",

    valueLiveTitle: "At a glance",
    valueLiveDescription:
      "See your latest reading, trend and glucose context immediately " +
      "from the desktop.",

    valueHistoryTitle: "Understand your history",
    valueHistoryDescription:
      "Review local history, recurring patterns and daily summaries " +
      "without sending data to a GlucoDesk cloud.",

    valuePrivacyTitle: "Stay private",
    valuePrivacyDescription:
      "Hide glucose values when you share your screen and keep your " +
      "information under your control.",

    dashboardKicker: "Live dashboard",
    dashboardTitle: "The context you need. Nothing more.",
    dashboardDescription:
      "Current glucose, trend, target range, recent history, statistics " +
      "and data completeness are presented in one focused view.",
    dashboardPointOne: "Current reading and trend",
    dashboardPointTwo: "Recent glucose history",
    dashboardPointThree: "Local statistics and completeness",
    dashboardImageAlt: "GlucoDesk dashboard",

    presenceKicker: "Desktop presence",
    presenceTitle: "One glance can be enough.",
    presenceDescription:
      "GlucoDesk lives quietly in the macOS menu bar or Windows system " +
      "tray, so your glucose can stay visible without interrupting what " +
      "you are doing.",
    presenceImageAlt:
      "GlucoDesk glucose states in the desktop menu bar",

    carbGuideKicker: "Visual carbohydrate guide",
    carbGuideTitle: "Carb counting, made a little easier.",
    carbGuideDescription:
      "Estimating carbohydrates from everyday portions is not always " +
      "simple. GlucoDesk includes a visual food guide with common foods, " +
      "reference portions and indicative carbohydrate values, available " +
      "locally on your desktop.",
    carbGuideFoodTitle: "Common foods",
    carbGuideFoodDescription:
      "Browse familiar foods in a clear visual catalogue.",
    carbGuidePortionTitle: "Reference portions",
    carbGuidePortionDescription:
      "Compare practical portions instead of relying only on abstract " +
      "nutritional tables.",
    carbGuideLocalTitle: "Local and quick",
    carbGuideLocalDescription:
      "Search the guide directly from GlucoDesk without sending your " +
      "glucose data anywhere.",
    carbGuideNote:
      "Carbohydrate values are indicative references only and are not " +
      "intended for insulin dosing or treatment decisions.",
    carbGuideWindowTitle: "Carbohydrate guide",
    carbGuideImageAlt:
      "GlucoDesk visual carbohydrate guide with food categories and " +
      "reference portions",

    diaryKicker: "History & diary",
    diaryTitle: "Turn readings into useful context.",
    diaryDescription:
      "Review your local history and create clear PDF and Excel diaries " +
      "with summaries, time blocks, recurring patterns and data completeness.",
    diaryPointOne: "Daily summaries",
    diaryPointTwo: "PDF and Excel exports",
    diaryPointThree: "Patterns and period comparisons",
    diaryImageAlt: "GlucoDesk glycemic diary",

    privacyKicker: "Local-first by design",
    privacyTitle: "Your glucose data stays with you.",
    privacyDescription:
      "GlucoDesk does not require a GlucoDesk cloud for your glucose " +
      "history. Cached readings, settings and local insights remain on " +
      "your computer.",

    privacyLocalTitle: "Local history",
    privacyLocalDescription:
      "Readings and analysis remain on your device.",

    privacyCredentialsTitle: "Protected credentials",
    privacyCredentialsDescription:
      "Secure operating-system storage is used where supported.",

    privacyModeTitle: "Privacy mode",
    privacyModeDescription:
      "Hide glucose values instantly during calls or presentations.",

    privacyOpenTitle: "Open source",
    privacyOpenDescription:
      "The code is public and available for inspection.",

    founderKicker: "The story behind GlucoDesk",
    founderTitle: "Built from a real everyday need.",
    founderDescription:
      "Living with type 1 diabetes already requires enough attention. " +
      "I built GlucoDesk to make glucose monitoring simpler, more " +
      "discreet and less intrusive throughout the working day.",
    founderRole: "Software Engineer · Creator of GlucoDesk",
    founderLinkedIn: "LinkedIn profile",
    founderGitHub: "GitHub profile",
    founderLinksLabel: "Author profiles",
    founderImageAlt:
      "Filippo Garavaglia, creator of GlucoDesk",
    reviewsKicker: "User reviews",
    reviewsTitle: "Trusted by people using it every day.",
    reviewsDescription:
      "Real experiences from people using GlucoDesk to keep glucose visible while they work.",
    reviewsAction: "Leave a review",


    openSourceKicker: "Independent and open",
    openSourceTitle: "Built in public. Free to use.",
    openSourceDescription:
      "GlucoDesk is an independent open-source project designed to " +
      "complement official CGM applications, not replace them.",
    openSourceAction: "Explore the project",

    downloadKicker: "v0.3.0-preview",
    downloadTitle: "Bring GlucoDesk to your desktop.",
    downloadDescription:
      "GlucoDesk is currently available as a free preview for Apple " +
      "Silicon Macs and Windows x64.",
    downloadNow: "Download preview",
    macInstallGuideAction: "Installation guide",
    macInstallGuideTitle: "Installing GlucoDesk on macOS",
    macInstallGuideIntro:
      "This preview is not yet signed or notarized by Apple, so macOS may " +
      "require one manual approval on first launch.",
    macInstallStepOne:
      "Extract the downloaded ZIP and open the included DMG.",
    macInstallStepTwo:
      "Drag GlucoDesk.app into the Applications folder.",
    macInstallStepThree:
      "Open GlucoDesk from Applications.",
    macInstallStepFour:
      "If macOS blocks the app, close the warning and open System Settings " +
      "→ Privacy & Security.",
    macInstallStepFive:
      "Scroll to Security, find the GlucoDesk warning and choose Open Anyway.",
    macInstallStepSix:
      "Confirm with your password or Touch ID, then open GlucoDesk again.",
    macInstallGuideOnce:
      "This approval is normally required only the first time.",
    windowsInstallGuideAction: "Installation guide",
    windowsInstallGuideTitle: "Installing GlucoDesk on Windows",
    windowsInstallGuideIntro:
      "This preview is not yet digitally code-signed, so Microsoft Defender " +
      "SmartScreen may show a warning when you run the installer.",
    windowsInstallStepOne:
      "Extract the downloaded ZIP.",
    windowsInstallStepTwo:
      "Run the included GlucoDesk setup.exe file.",
    windowsInstallStepThree:
      "If Windows shows \"Windows protected your PC\", click More info.",
    windowsInstallStepFour:
      "Verify that the file is the GlucoDesk installer, then click Run anyway.",
    windowsInstallStepFive:
      "Follow the installation wizard and launch GlucoDesk from the Start Menu.",
    windowsInstallGuideSafety:
      "Only continue if you downloaded GlucoDesk from the official website " +
      "or GitHub repository.",
    downloadNote:
      "Current builds are unsigned preview releases distributed through " +
      "the official GlucoDesk GitHub repository.",

    safetyLabel: "Important",
    safetyTitle: "A companion, not a medical device.",
    safetyDescription:
      "GlucoDesk is not intended for treatment decisions, insulin dosing, " +
      "emergency alerts, diagnosis or as a replacement for approved " +
      "diabetes applications, CGM systems or medical devices.",

    footerDescription: "Your glucose, always in sight.",
    footerIndependent: "Independent open-source project"
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
    navFounder: "Chi l'ha creato",
    navDownload: "Download",

    heroBadge: "Gratis · Open source · macOS e Windows",
    heroTitleOne: "La tua glicemia.",
    heroTitleTwo: "Sempre in vista.",
    heroDescription:
      "Un companion desktop privato per utenti CGM. Tieni visibili le " +
      "informazioni che contano mentre lavori, senza dover controllare " +
      "continuamente un altro dispositivo.",
    heroPrimaryAction: "Scarica GlucoDesk",
    heroSecondaryAction: "Guarda su GitHub",

    trustLocal: "Local-first",
    trustIndependent: "Indipendente",
    trustFree: "Gratis",

    heroImageAlt:
      "Dashboard GlucoDesk con glicemia attuale, trend e storico recente",
    heroGlucoseStatus: "Nel range",
    heroGlucoseUpdated: "Aggiornata ora",

    valueKicker: "Pensato per l'uso quotidiano",
    valueTitle: "Il tuo contesto glicemico, senza interrompere il lavoro.",
    valueDescription:
      "GlucoDesk riunisce contesto live, storico locale e informazioni " +
      "utili in un'esperienza desktop semplice e discreta.",

    valueLiveTitle: "A colpo d'occhio",
    valueLiveDescription:
      "Visualizza subito ultima lettura, trend e contesto glicemico " +
      "direttamente dal desktop.",

    valueHistoryTitle: "Comprendi il tuo storico",
    valueHistoryDescription:
      "Consulta storico locale, pattern ricorrenti e riepiloghi giornalieri " +
      "senza inviare dati a un cloud GlucoDesk.",

    valuePrivacyTitle: "Mantieni la privacy",
    valuePrivacyDescription:
      "Nascondi i valori glicemici quando condividi lo schermo e mantieni " +
      "le tue informazioni sotto il tuo controllo.",

    dashboardKicker: "Dashboard live",
    dashboardTitle: "Il contesto che ti serve. Niente di più.",
    dashboardDescription:
      "Glicemia attuale, trend, range target, storico recente, statistiche " +
      "e completezza dei dati sono raccolti in un'unica vista.",
    dashboardPointOne: "Lettura attuale e trend",
    dashboardPointTwo: "Storico glicemico recente",
    dashboardPointThree: "Statistiche locali e completezza",
    dashboardImageAlt: "Dashboard di GlucoDesk",

    presenceKicker: "Presenza desktop",
    presenceTitle: "Può bastare uno sguardo.",
    presenceDescription:
      "GlucoDesk rimane discretamente nella barra menu di macOS o nella " +
      "system tray di Windows, così la glicemia resta visibile senza " +
      "interrompere ciò che stai facendo.",
    presenceImageAlt:
      "Stati glicemici GlucoDesk nella barra menu del desktop",

    carbGuideKicker: "Guida visiva ai carboidrati",
    carbGuideTitle: "Contare i carboidrati, un po' più semplicemente.",
    carbGuideDescription:
      "Stimare i carboidrati delle porzioni quotidiane non è sempre " +
      "semplice. GlucoDesk include una guida visiva con alimenti comuni, " +
      "porzioni di riferimento e valori indicativi di carboidrati, " +
      "disponibile direttamente sul desktop.",
    carbGuideFoodTitle: "Alimenti comuni",
    carbGuideFoodDescription:
      "Consulta alimenti familiari in un catalogo chiaro e visuale.",
    carbGuidePortionTitle: "Porzioni di riferimento",
    carbGuidePortionDescription:
      "Confronta porzioni pratiche senza affidarti soltanto a tabelle " +
      "nutrizionali astratte.",
    carbGuideLocalTitle: "Locale e veloce",
    carbGuideLocalDescription:
      "Consulta la guida direttamente da GlucoDesk senza inviare i tuoi " +
      "dati glicemici altrove.",
    carbGuideNote:
      "I valori dei carboidrati sono riferimenti indicativi e non sono " +
      "destinati al dosaggio dell'insulina o a decisioni terapeutiche.",
    carbGuideWindowTitle: "Guida ai carboidrati",
    carbGuideImageAlt:
      "Guida visiva ai carboidrati di GlucoDesk con categorie di alimenti " +
      "e porzioni di riferimento",

    diaryKicker: "Storico e diario",
    diaryTitle: "Trasforma le letture in contesto utile.",
    diaryDescription:
      "Consulta lo storico locale e crea diari PDF ed Excel chiari con " +
      "riepiloghi, fasce orarie, pattern ricorrenti e completezza dei dati.",
    diaryPointOne: "Riepiloghi giornalieri",
    diaryPointTwo: "Esportazioni PDF ed Excel",
    diaryPointThree: "Pattern e confronti tra periodi",
    diaryImageAlt: "Diario glicemico di GlucoDesk",

    privacyKicker: "Local-first per scelta",
    privacyTitle: "I tuoi dati glicemici restano con te.",
    privacyDescription:
      "GlucoDesk non richiede un cloud GlucoDesk per il tuo storico " +
      "glicemico. Letture in cache, impostazioni e informazioni locali " +
      "rimangono sul tuo computer.",

    privacyLocalTitle: "Storico locale",
    privacyLocalDescription:
      "Letture e analisi rimangono sul tuo dispositivo.",

    privacyCredentialsTitle: "Credenziali protette",
    privacyCredentialsDescription:
      "Dove supportato viene usato l'archivio sicuro del sistema operativo.",

    privacyModeTitle: "Modalità privacy",
    privacyModeDescription:
      "Nascondi subito i valori durante chiamate o presentazioni.",

    privacyOpenTitle: "Open source",
    privacyOpenDescription:
      "Il codice è pubblico e disponibile per essere consultato.",

    founderKicker: "La storia dietro GlucoDesk",
    founderTitle: "Nato da un'esigenza quotidiana reale.",
    founderDescription:
      "Vivere con il diabete di tipo 1 richiede già abbastanza attenzione. " +
      "Ho creato GlucoDesk per rendere il controllo della glicemia più " +
      "semplice, discreto e meno invasivo durante la giornata lavorativa.",
    founderRole: "Software Engineer · Creatore di GlucoDesk",
    founderLinkedIn: "Profilo LinkedIn",
    founderGitHub: "Profilo GitHub",
    founderLinksLabel: "Profili dell'autore",
    founderImageAlt:
      "Filippo Garavaglia, creatore di GlucoDesk",
    reviewsKicker: "Recensioni degli utenti",
    reviewsTitle: "Scelto da chi lo usa ogni giorno.",
    reviewsDescription:
      "Esperienze di chi usa GlucoDesk per tenere la glicemia visibile mentre lavora.",
    reviewsAction: "Lascia una recensione",


    openSourceKicker: "Indipendente e aperto",
    openSourceTitle: "Open source. Indipendente. Gratuito.",
    openSourceDescription:
      "GlucoDesk è un progetto open source indipendente pensato per " +
      "affiancare le applicazioni CGM ufficiali, non per sostituirle.",
    openSourceAction: "Esplora il progetto",

    downloadKicker: "v0.3.0-preview",
    downloadTitle: "Porta GlucoDesk sul tuo desktop.",
    downloadDescription:
      "GlucoDesk è attualmente disponibile come preview gratuita per " +
      "Mac Apple Silicon e Windows x64.",
    downloadNow: "Scarica la preview",
    macInstallGuideAction: "Guida installazione",
    macInstallGuideTitle: "Installare GlucoDesk su macOS",
    macInstallGuideIntro:
      "Questa preview non è ancora firmata o notarizzata da Apple, quindi " +
      "macOS potrebbe richiedere un'autorizzazione manuale al primo avvio.",
    macInstallStepOne:
      "Estrai lo ZIP scaricato e apri il file DMG incluso.",
    macInstallStepTwo:
      "Trascina GlucoDesk.app nella cartella Applicazioni.",
    macInstallStepThree:
      "Apri GlucoDesk dalla cartella Applicazioni.",
    macInstallStepFour:
      "Se macOS blocca l'app, chiudi l'avviso e apri Impostazioni di Sistema " +
      "→ Privacy e sicurezza.",
    macInstallStepFive:
      "Scorri fino a Sicurezza, trova l'avviso relativo a GlucoDesk e scegli " +
      "Apri comunque.",
    macInstallStepSix:
      "Conferma con la password del Mac o Touch ID, quindi apri nuovamente " +
      "GlucoDesk.",
    macInstallGuideOnce:
      "Questa autorizzazione è normalmente necessaria solo al primo avvio.",
    windowsInstallGuideAction: "Guida installazione",
    windowsInstallGuideTitle: "Installare GlucoDesk su Windows",
    windowsInstallGuideIntro:
      "Questa preview non è ancora firmata digitalmente, quindi Microsoft " +
      "Defender SmartScreen potrebbe mostrare un avviso quando avvii l'installer.",
    windowsInstallStepOne:
      "Estrai lo ZIP scaricato.",
    windowsInstallStepTwo:
      "Avvia il file setup.exe di GlucoDesk incluso nel pacchetto.",
    windowsInstallStepThree:
      "Se Windows mostra \"Windows ha protetto il PC\", fai clic su " +
      "Ulteriori informazioni.",
    windowsInstallStepFour:
      "Verifica che il file sia l'installer di GlucoDesk, quindi fai clic " +
      "su Esegui comunque.",
    windowsInstallStepFive:
      "Segui la procedura guidata e avvia GlucoDesk dal menu Start.",
    windowsInstallGuideSafety:
      "Continua solo se hai scaricato GlucoDesk dal sito ufficiale o dal " +
      "repository GitHub ufficiale.",
    downloadNote:
      "Le build attuali sono preview non firmate distribuite attraverso " +
      "il repository GitHub ufficiale di GlucoDesk.",

    safetyLabel: "Importante",
    safetyTitle: "Un companion, non un dispositivo medico.",
    safetyDescription:
      "GlucoDesk non è destinato a decisioni terapeutiche, dosaggio " +
      "dell'insulina, avvisi di emergenza, diagnosi o a sostituire " +
      "applicazioni per il diabete, sistemi CGM o dispositivi medici approvati.",

    footerDescription: "La tua glicemia, sempre in vista.",
    footerIndependent: "Progetto open source indipendente"
  }
};

let currentLanguage = "en";

const resetInitialScrollPosition = () => {
  if (window.location.hash) {
    return;
  }

  window.requestAnimationFrame(() => {
    window.requestAnimationFrame(() => {
      window.scrollTo({
        top: 0,
        left: 0,
        behavior: "auto"
      });
    });
  });
};

const updatePageMetadata = (language) => {
  const metadata =
    language === "it"
      ? {
          title: "GlucoDesk — La tua glicemia, sempre in vista",
          description:
            "GlucoDesk è un companion desktop gratuito e local-first " +
            "per utenti CGM, disponibile su macOS e Windows.",
          locale: "it_IT"
        }
      : {
          title: "GlucoDesk — Your glucose, always in sight",
          description:
            "GlucoDesk is a free, local-first desktop companion for CGM " +
            "users, available on macOS and Windows.",
          locale: "en_US"
        };

  document.title = metadata.title;

  document
    .querySelector('meta[name="description"]')
    ?.setAttribute("content", metadata.description);

  document
    .querySelector('meta[property="og:title"]')
    ?.setAttribute("content", metadata.title);

  document
    .querySelector('meta[property="og:description"]')
    ?.setAttribute("content", metadata.description);

  document
    .querySelector('meta[property="og:locale"]')
    ?.setAttribute("content", metadata.locale);

  document
    .querySelector('meta[name="twitter:title"]')
    ?.setAttribute("content", metadata.title);

  document
    .querySelector('meta[name="twitter:description"]')
    ?.setAttribute("content", metadata.description);
};

const updateLanguage = (language) => {
  if (!translations[language]) {
    return;
  }

  currentLanguage = language;

  document.documentElement.lang = language;

  document.querySelectorAll("[data-i18n]").forEach((element) => {
    const key = element.dataset.i18n;
    const value = translations[language][key];

    if (value !== undefined) {
      element.textContent = value;
    }
  });

  document
    .querySelectorAll("[data-i18n-aria-label]")
    .forEach((element) => {
      const key = element.dataset.i18nAriaLabel;
      const value = translations[language][key];

      if (value !== undefined) {
        element.setAttribute("aria-label", value);
      }
    });

  document.querySelectorAll("[data-i18n-alt]").forEach((element) => {
    const key = element.dataset.i18nAlt;
    const value = translations[language][key];

    if (value !== undefined) {
      element.setAttribute("alt", value);
    }
  });

  document.querySelectorAll("[data-language-button]").forEach((button) => {
    const isActive = button.dataset.languageButton === language;

    button.classList.toggle("is-active", isActive);
    button.setAttribute("aria-pressed", String(isActive));
  });

  updatePageMetadata(language);

  localStorage.setItem(
    "glucodesk-site-language",
    language
  );
};

const setupLanguage = () => {
  const storedLanguage =
    localStorage.getItem("glucodesk-site-language");

  const browserLanguage =
    navigator.language?.toLowerCase().startsWith("it")
      ? "it"
      : "en";

  const initialLanguage =
    storedLanguage === "en" || storedLanguage === "it"
      ? storedLanguage
      : browserLanguage;

  updateLanguage(initialLanguage);

  document.querySelectorAll("[data-language-button]").forEach((button) => {
    button.addEventListener("click", () => {
      updateLanguage(button.dataset.languageButton);
    });
  });
};

const setupHeader = () => {
  const header = document.querySelector("[data-header]");

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
    document.querySelector("[data-mobile-menu-button]");

  const navigation =
    document.querySelector("[data-mobile-nav]");

  if (!button || !navigation) {
    return;
  }

  const closeNavigation = ({
    restoreFocus = false
  } = {}) => {
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
    const shouldOpen =
      !navigation.classList.contains("is-open");

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
        ? translations[currentLanguage].closeNavigationLabel
        : translations[currentLanguage].openNavigationLabel
    );

    document.body.classList.toggle(
      "is-menu-open",
      shouldOpen
    );
  });

  navigation.querySelectorAll("a").forEach((link) => {
    link.addEventListener("click", () => {
      closeNavigation();
    });
  });

  document.addEventListener("keydown", (event) => {
    if (
      event.key === "Escape" &&
      navigation.classList.contains("is-open")
    ) {
      closeNavigation({
        restoreFocus: true
      });
    }
  });

  document.addEventListener("click", (event) => {
    if (
      navigation.classList.contains("is-open") &&
      !navigation.contains(event.target) &&
      !button.contains(event.target)
    ) {
      closeNavigation();
    }
  });

  window.addEventListener("resize", () => {
    if (window.innerWidth > 1000) {
      closeNavigation();
    }
  });
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
      threshold: 0.1,
      rootMargin: "0px 0px -30px"
    }
  );

  elements.forEach((element) => {
    observer.observe(element);
  });
};

const setupCurrentYear = () => {
  document.querySelectorAll("[data-current-year]").forEach((element) => {
    element.textContent =
      String(new Date().getFullYear());
  });
};

if ("scrollRestoration" in window.history) {
  window.history.scrollRestoration = "manual";
}

window.addEventListener(
  "pageshow",
  resetInitialScrollPosition
);

window.addEventListener(
  "load",
  resetInitialScrollPosition,
  { once: true }
);

document.addEventListener("DOMContentLoaded", () => {
  setupCurrentYear();
  setupLanguage();
  setupHeader();
  setupMobileNavigation();
  setupRevealAnimations();
});

// ------------------------------------------------------------
// User reviews carousel
// ------------------------------------------------------------

function initializeReviewsCarousel() {
  const carousel = document.querySelector("[data-reviews-carousel]");

  if (!carousel) {
    return;
  }

  const viewport = carousel.querySelector(".reviews-viewport");
  const track = carousel.querySelector("[data-reviews-track]");
  const previousButton = carousel.querySelector("[data-reviews-previous]");
  const nextButton = carousel.querySelector("[data-reviews-next]");
  const controls = carousel.querySelector("[data-reviews-controls]");
  const position = carousel.querySelector("[data-reviews-position]");

  if (
    !viewport ||
    !track ||
    !previousButton ||
    !nextButton ||
    !controls ||
    !position
  ) {
    return;
  }

  const getCards = () =>
    Array.from(track.querySelectorAll(".review-card"));

  const getVisibleCardCount = () => {
    if (window.matchMedia("(max-width: 720px)").matches) {
      return 1;
    }

    if (window.matchMedia("(max-width: 1080px)").matches) {
      return 2;
    }

    return 3;
  };

  const getStep = () => {
    const firstCard = getCards()[0];

    if (!firstCard) {
      return 0;
    }

    const styles = window.getComputedStyle(track);
    const gap =
      Number.parseFloat(styles.columnGap || styles.gap) || 0;

    return firstCard.getBoundingClientRect().width + gap;
  };

  const updateControls = () => {
    const cards = getCards();
    const visibleCount = getVisibleCardCount();
    const shouldShowControls = cards.length > visibleCount;

    controls.hidden = !shouldShowControls;

    if (!shouldShowControls) {
      position.textContent = "";
      return;
    }

    const step = getStep();

    if (step <= 0) {
      return;
    }

    const maxIndex = Math.max(
      0,
      cards.length - visibleCount
    );

    const activeIndex = Math.max(
      0,
      Math.min(
        maxIndex,
        Math.round(viewport.scrollLeft / step)
      )
    );

    previousButton.disabled = activeIndex === 0;
    nextButton.disabled = activeIndex >= maxIndex;

    const firstVisible = activeIndex + 1;
    const lastVisible = Math.min(
      activeIndex + visibleCount,
      cards.length
    );

    position.textContent =
      `${firstVisible}–${lastVisible} / ${cards.length}`;
  };

  const scrollByCard = (direction) => {
    const step = getStep();

    if (step <= 0) {
      return;
    }

    viewport.scrollBy({
      left: direction * step,
      behavior: "smooth"
    });
  };

  previousButton.addEventListener(
    "click",
    () => scrollByCard(-1)
  );

  nextButton.addEventListener(
    "click",
    () => scrollByCard(1)
  );

  viewport.addEventListener(
    "scroll",
    () => requestAnimationFrame(updateControls),
    { passive: true }
  );

  window.addEventListener(
    "resize",
    () => requestAnimationFrame(updateControls)
  );

  updateControls();
}

initializeReviewsCarousel();

using System.Globalization;
using System.Text.RegularExpressions;

namespace GlucoDesk.Infrastructure.Cgm.Diary.Localization;

/// <summary>
/// Provides deterministic request-scoped localization for glycemic diary
/// exports.
/// </summary>
/// <remarks>
/// Export content is localized from exact resource keys or from explicitly
/// supported, fully anchored templates. Arbitrary substring replacement is
/// intentionally forbidden because it can corrupt words and produce mixed
/// language output.
/// </remarks>
public static class GlycemicDiaryExportLocalizer
{
    private const string DefaultLanguageCode = "en";

    private static readonly AsyncLocal<string?> CurrentLanguageCode = new();

    private static readonly CultureInfo EnglishCulture =
        CultureInfo.GetCultureInfo("en-US");

    private static readonly CultureInfo ItalianCulture =
        CultureInfo.GetCultureInfo("it-IT");

    private static readonly IReadOnlyDictionary<string, string> ItalianTexts =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Document and sections.
            ["Glycemic diary"] = "Diario glicemico",
            ["Local-first glucose summary"] = "Riepilogo glicemico local-first",
            ["Overview"] = "Riepilogo",
            ["Glucose story"] = "Storia glicemica",
            ["Weekly review"] = "Revisione settimanale",
            ["Local patterns"] = "Pattern locali",
            ["Patterns"] = "Pattern",
            ["Daily diary"] = "Diario giornaliero",
            ["Time blocks"] = "Fasce orarie",
            ["Data completeness"] = "Completezza dei dati",
            ["Export metadata"] = "Metadati esportazione",
            ["Safety notice"] = "Avviso di sicurezza",

            // Section descriptions.
            ["Summary of the selected glucose history period."] =
                "Riepilogo del periodo selezionato dello storico glicemico.",
            ["Human-readable interpretation of the selected diary period."] =
                "Interpretazione leggibile del periodo selezionato del diario.",
            ["Comparison with the previous equivalent period."] =
                "Confronto con il periodo equivalente precedente.",
            ["Recurring local glucose tendencies detected from diary time blocks."] =
                "Tendenze glicemiche locali ricorrenti rilevate nelle fasce orarie del diario.",
            ["Daily glucose summaries and key time-block values shown in mg/dL."] =
                "Riepiloghi glicemici giornalieri e valori delle principali fasce orarie espressi in mg/dL.",
            ["Daily glucose summaries and key time-block values shown in mmol/L."] =
                "Riepiloghi glicemici giornalieri e valori delle principali fasce orarie espressi in mmol/L.",
            ["Local history is mostly complete, but minor gaps or missing readings may exist."] =
                "Lo storico locale è quasi completo, ma potrebbero essere presenti piccoli intervalli o letture mancanti.",
            ["Local history is incomplete. Some values and detected patterns should be interpreted carefully."] =
                "Lo storico locale è incompleto. Alcuni valori e pattern rilevati devono essere interpretati con cautela.",
            ["Local history is too limited for reliable interpretation."] =
                "Lo storico locale è troppo limitato per un’interpretazione affidabile.",
            ["Days marked as partial may contain missing CGM history and should be interpreted carefully."] =
                "I giorni contrassegnati come parziali possono contenere dati CGM mancanti e devono essere interpretati con cautela.",

            // Metrics and columns.
            ["Period"] = "Periodo",
            ["Period start"] = "Inizio periodo",
            ["Period end"] = "Fine periodo",
            ["Date"] = "Data",
            ["Day"] = "Giorno",
            ["Metric"] = "Metrica",
            ["Previous"] = "Precedente",
            ["Current"] = "Corrente",
            ["Delta"] = "Variazione",
            ["Direction"] = "Direzione",
            ["Severity"] = "Gravità",
            ["Description"] = "Descrizione",
            ["Highlights"] = "Evidenze",
            ["Kind"] = "Tipo",
            ["Title"] = "Titolo",
            ["Time block"] = "Fascia oraria",
            ["Block"] = "Fascia",
            ["Supporting days"] = "Giorni di supporto",
            ["Readings"] = "Letture",
            ["Average"] = "Media",
            ["Average glucose"] = "Glicemia media",
            ["Reading count"] = "Numero di letture",
            ["Pattern count"] = "Numero di pattern",
            ["Daily glucose summaries"] = "Riepiloghi glicemici giornalieri",
            ["Top"] = "Prime",
            ["detected"] = "rilevati",
            ["Avg"] = "Media",
            ["Minimum"] = "Minimo",
            ["Min"] = "Min",
            ["Maximum"] = "Massimo",
            ["Max"] = "Max",
            ["Range"] = "Intervallo",
            ["Time in range"] = "Tempo nel range",
            ["Time in range %"] = "Tempo nel range %",
            ["TIR"] = "TIR",
            ["Data coverage"] = "Copertura dei dati",
            ["Data coverage %"] = "Copertura dei dati %",
            ["Coverage"] = "Copertura",
            ["Coverage %"] = "Copertura %",
            ["Status"] = "Stato",
            ["Gaps"] = "Intervalli mancanti",
            ["Gap count"] = "Numero intervalli mancanti",
            ["Detected gaps"] = "Intervalli mancanti rilevati",
            ["Incomplete days"] = "Giorni incompleti",
            ["Empty days"] = "Giorni senza dati",
            ["Detected patterns"] = "Pattern rilevati",
            ["History reliability"] = "Affidabilità dello storico",
            ["Reliability details"] = "Dettagli sull’affidabilità",
            ["Complete data"] = "Dati completi",
            ["Complete"] = "Completo",
            ["Partial"] = "Parziale",
            ["Incomplete"] = "Incompleto",
            ["No data"] = "Nessun dato",
            ["Has data"] = "Dati presenti",
            ["Yes"] = "Sì",
            ["No"] = "No",
            ["Notes"] = "Note",
            ["Representative"] = "Rappresentativo",
            ["Representative value"] = "Valore rappresentativo",
            ["Representative timestamp"] = "Data e ora rappresentativa",
            ["From"] = "Dalle",
            ["To"] = "Alle",
            ["Start"] = "Inizio",
            ["End"] = "Fine",

            // Standard time blocks.
            ["Breakfast"] = "Colazione",
            ["Lunch"] = "Pranzo",
            ["Dinner"] = "Cena",
            ["Pre-night"] = "Pre-notte",
            ["Bedtime"] = "Pre-notte",
            ["Overall"] = "Complessivo",
            ["overall period"] = "periodo complessivo",

            // Metadata.
            ["Generated at"] = "Generato il",
            ["Unit"] = "Unità",
            ["Target range"] = "Intervallo target",
            ["Source"] = "Origine",
            ["Local glucose history"] = "Storico glicemico locale",
            ["Report type"] = "Tipo di report",
            ["Local-first glycemic diary export"] =
                "Esportazione local-first del diario glicemico",
            ["Current period"] = "Periodo corrente",
            ["Previous period"] = "Periodo precedente",
            ["previous period ends before the current period starts"] =
                "il periodo precedente termina prima dell’inizio del periodo corrente",

            // States and severities.
            ["Reliable"] = "Affidabile",
            ["Caution"] = "Attenzione",
            ["Important"] = "Importante",
            ["Info"] = "Informativo",
            ["Variable"] = "Variabile",
            ["Stable"] = "Stabile",
            ["Excellent"] = "Ottimo",
            ["NoData"] = "Nessun dato",
            ["Unknown"] = "Sconosciuto",
            ["Unchanged"] = "Invariato",
            ["Increased"] = "Aumentato",
            ["Decreased"] = "Diminuito",
            ["NewlyAvailable"] = "Nuovamente disponibile",
            ["NoLongerAvailable"] = "Non più disponibile",
            ["LimitedDataCoverage"] = "Copertura dati limitata",
            ["RecurringLow"] = "Bassi ricorrenti",
            ["RecurringHigh"] = "Alti ricorrenti",
            ["RecurringVariability"] = "Variabilità ricorrente",
            ["StableTimeBlock"] = "Fascia oraria stabile",

            // Static story and review texts.
            ["Variable glucose period"] = "Periodo glicemico variabile",
            ["Mostly stable glucose period"] = "Periodo glicemico prevalentemente stabile",
            ["Strong time in range"] = "Tempo nel range elevato",
            ["No local glucose data"] = "Nessun dato glicemico locale",
            ["Interpret with caution"] = "Interpretare con cautela",
            ["Weekly review unavailable"] =
                "Revisione settimanale non disponibile",
            ["Weekly review: no local readings available"] =
                "Revisione settimanale: nessuna lettura locale disponibile",
            ["Weekly review: comparison limited by missing previous data"] =
                "Revisione settimanale: confronto limitato dai dati precedenti mancanti",
            ["Weekly review: data quality needs attention"] =
                "Revisione settimanale: la qualità dei dati richiede attenzione",
            ["Weekly review: time in range improved"] =
                "Revisione settimanale: il tempo nel range è migliorato",
            ["Weekly review: time in range decreased"] =
                "Revisione settimanale: il tempo nel range è diminuito",
            ["Weekly review: new local patterns detected"] =
                "Revisione settimanale: rilevati nuovi pattern locali",
            ["Weekly review: mostly stable period"] =
                "Revisione settimanale: periodo prevalentemente stabile",
            ["The current period looks broadly similar to the previous one."] =
                "Il periodo corrente appare complessivamente simile al precedente.",
            ["The current period has no local readings, so no meaningful comparison can be generated."] =
                "Il periodo corrente non contiene letture locali, pertanto non è possibile generare un confronto significativo.",
            ["The previous equivalent period has no local readings, so this review cannot produce a true week-over-week comparison. The current period is summarized on its own."] =
                "Il periodo equivalente precedente non contiene letture locali, pertanto non è possibile effettuare un confronto reale tra periodi. Il periodo corrente viene riepilogato singolarmente.",
            ["The weekly comparison could not be generated for this export. The diary data is still available."] =
                "Non è stato possibile generare il confronto settimanale. I dati del diario restano disponibili.",
            ["The weekly comparison could not be generated for this export. The diary data below is still available."] =
                "Non è stato possibile generare il confronto settimanale. I dati del diario riportati di seguito restano disponibili.",
            ["No recurring local patterns detected."] =
                "Nessun pattern locale ricorrente rilevato.",
            ["No recurring local patterns were detected for the selected period."] =
                "Nel periodo selezionato non sono stati rilevati pattern locali ricorrenti.",
            ["None detected"] = "Nessuno rilevato",

            // Footer and safety.
            ["Generated by GlucoDesk"] = "Generato da GlucoDesk",
            ["Page"] = "Pagina",
            ["of"] = "di",
            ["GlucoDesk is not a medical device. This diary is for personal awareness and must not be used for treatment decisions."] =
                "GlucoDesk non è un dispositivo medico. Questo diario è destinato alla consapevolezza personale e non deve essere utilizzato per decisioni terapeutiche.",
            ["GlucoDesk is not a medical device. The exported diary is for personal awareness and must not be used for treatment decisions."] =
                "GlucoDesk non è un dispositivo medico. Il diario esportato è destinato alla consapevolezza personale e non deve essere utilizzato per decisioni terapeutiche."
        };

    /// <summary>
    /// Begins an isolated localization scope for one export.
    /// </summary>
    /// <param name="languageCode">The requested export language code.</param>
    /// <returns>The disposable localization scope.</returns>
    public static IDisposable BeginScope(string? languageCode)
    {
        var previousLanguageCode = CurrentLanguageCode.Value;
        CurrentLanguageCode.Value = NormalizeLanguageCode(languageCode);

        return new LocalizationScope(previousLanguageCode);
    }

    /// <summary>
    /// Gets whether the current export language is Italian.
    /// </summary>
    public static bool IsItalian =>
        string.Equals(
            CurrentLanguageCode.Value,
            "it",
            StringComparison.Ordinal);

    /// <summary>
    /// Gets the culture selected for the current export.
    /// </summary>
    public static CultureInfo Culture =>
        IsItalian
            ? ItalianCulture
            : EnglishCulture;

    /// <summary>
    /// Gets the Excel date format selected for the current export.
    /// </summary>
    public static string ExcelDateFormat =>
        IsItalian
            ? "dd/mm/yyyy"
            : "mm/dd/yyyy";

    /// <summary>
    /// Gets the Excel date-time format selected for the current export.
    /// </summary>
    public static string ExcelDateTimeFormat =>
        IsItalian
            ? "dd/mm/yyyy hh:mm"
            : "mm/dd/yyyy hh:mm";

    /// <summary>
    /// Translates a complete export-facing value.
    /// </summary>
    /// <param name="text">The complete source value.</param>
    /// <returns>The localized value.</returns>
    public static string Translate(string? text)
    {
        if (string.IsNullOrEmpty(text) || !IsItalian)
        {
            return text ?? string.Empty;
        }

        if (ItalianTexts.TryGetValue(text, out var translatedText))
        {
            return LocalizeNumericTokens(translatedText);
        }

        if (TryTranslateUnitHeader(text, out translatedText))
        {
            return LocalizeNumericTokens(translatedText);
        }

        if (TryTranslateGeneratedText(text, out translatedText))
        {
            return LocalizeNumericTokens(translatedText);
        }

        /*
         * Unknown values are never translated through partial replacement.
         * Numeric punctuation may still be adapted to the selected culture,
         * for example 93.87% -> 93,87%.
         */
        return LocalizeNumericTokens(text);
    }

    /// <summary>
    /// Formats a date for the selected export language.
    /// </summary>
    public static string FormatDate(DateTimeOffset timestamp)
    {
        return timestamp.ToString(
            IsItalian ? "dd/MM/yyyy" : "MM/dd/yyyy",
            Culture);
    }

    /// <summary>
    /// Formats a date for the selected export language.
    /// </summary>
    public static string FormatDate(DateOnly date)
    {
        return date.ToString(
            IsItalian ? "dd/MM/yyyy" : "MM/dd/yyyy",
            Culture);
    }

    /// <summary>
    /// Formats a date and time for the selected export language.
    /// </summary>
    public static string FormatDateTime(DateTimeOffset timestamp)
    {
        return timestamp.ToString(
            IsItalian
                ? "dd/MM/yyyy HH:mm zzz"
                : "MM/dd/yyyy HH:mm zzz",
            Culture);
    }

    /// <summary>
    /// Formats a percentage for the selected export language.
    /// </summary>
    public static string FormatPercentage(decimal? value)
    {
        return value.HasValue
            ? $"{value.Value.ToString("0.##", Culture)}%"
            : "—";
    }

    /// <summary>
    /// Formats a decimal number for the selected export language.
    /// </summary>
    public static string FormatNumber(decimal value)
    {
        return value.ToString("0.##", Culture);
    }

    private static bool TryTranslateUnitHeader(
        string text,
        out string translatedText)
    {
        foreach (var unit in new[] { " mg/dL", " mmol/L" })
        {
            if (!text.EndsWith(unit, StringComparison.Ordinal))
            {
                continue;
            }

            var label = text[..^unit.Length];

            if (!ItalianTexts.TryGetValue(label, out var translatedLabel))
            {
                translatedText = string.Empty;
                return false;
            }

            translatedText = translatedLabel + unit;
            return true;
        }

        translatedText = string.Empty;
        return false;
    }

    private static bool TryTranslateGeneratedText(
        string text,
        out string translatedText)
    {
        if (TryTranslatePatternText(text, out translatedText))
        {
            return true;
        }

        if (TryTranslateStoryText(text, out translatedText))
        {
            return true;
        }

        if (TryTranslateReviewText(text, out translatedText))
        {
            return true;
        }

        translatedText = string.Empty;
        return false;
    }

    private static bool TryTranslatePatternText(
        string text,
        out string translatedText)
    {
        var match = Regex.Match(
            text,
            @"^Recurring low tendency around (?<block>.+)$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"Tendenza ricorrente a valori bassi nella fascia {Translate(match.Groups["block"].Value)}";
            return true;
        }

        match = Regex.Match(
            text,
            @"^Recurring high tendency around (?<block>.+)$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"Tendenza ricorrente a valori alti nella fascia {Translate(match.Groups["block"].Value)}";
            return true;
        }

        match = Regex.Match(
            text,
            @"^Recurring variability around (?<block>.+)$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"Variabilità ricorrente nella fascia {Translate(match.Groups["block"].Value)}";
            return true;
        }

        match = Regex.Match(
            text,
            @"^Stable pattern around (?<block>.+)$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"Pattern stabile nella fascia {Translate(match.Groups["block"].Value)}";
            return true;
        }

        match = Regex.Match(
            text,
            @"^(?<days>\d+) days show representative glucose below (?<value>[\d.,]+) mg/dL around (?<block>.+)\.$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"{match.Groups["days"].Value} giorni mostrano un valore glicemico rappresentativo inferiore a " +
                $"{match.Groups["value"].Value} mg/dL nella fascia {Translate(match.Groups["block"].Value)}.";
            return true;
        }

        match = Regex.Match(
            text,
            @"^(?<days>\d+) days show representative glucose above (?<value>[\d.,]+) mg/dL around (?<block>.+)\.$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"{match.Groups["days"].Value} giorni mostrano un valore glicemico rappresentativo superiore a " +
                $"{match.Groups["value"].Value} mg/dL nella fascia {Translate(match.Groups["block"].Value)}.";
            return true;
        }

        match = Regex.Match(
            text,
            @"^(?<days>\d+) days show a glucose spread of at least (?<value>[\d.,]+) mg/dL around (?<block>.+)\.$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"{match.Groups["days"].Value} giorni mostrano un’escursione glicemica di almeno " +
                $"{match.Groups["value"].Value} mg/dL nella fascia {Translate(match.Groups["block"].Value)}.";
            return true;
        }

        match = Regex.Match(
            text,
            @"^(?<days>\d+) days show a relatively stable glucose profile around (?<block>.+)\.$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"{match.Groups["days"].Value} giorni mostrano un profilo glicemico relativamente stabile " +
                $"nella fascia {Translate(match.Groups["block"].Value)}.";
            return true;
        }

        match = Regex.Match(
            text,
            @"^Local history coverage is (?<coverage>.+)\. Detected patterns should be interpreted carefully\.$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"La copertura dello storico locale è {match.Groups["coverage"].Value}. " +
                "I pattern rilevati devono essere interpretati con cautela.";
            return true;
        }

        translatedText = string.Empty;
        return false;
    }

    private static bool TryTranslateStoryText(
        string text,
        out string translatedText)
    {
        var match = Regex.Match(
            text,
            @"^Average glucose was (?<average>.+), time in range was (?<tir>.+), and the observed range was (?<minimum>.+) - (?<maximum>.+)\.$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"La glicemia media è stata {LocalizeNumericTokens(match.Groups["average"].Value)}, " +
                $"il tempo nel range è stato {LocalizeNumericTokens(match.Groups["tir"].Value)} e " +
                $"l’intervallo osservato è stato " +
                $"{LocalizeNumericTokens(match.Groups["minimum"].Value)} - " +
                $"{LocalizeNumericTokens(match.Groups["maximum"].Value)}.";

            return true;
        }

        match = Regex.Match(
            text,
            @"^History reliability: (?<status>[^·]+) · (?<coverage>\d+(?:\.\d+)?%)\. (?<detail>.+)$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"Affidabilità dello storico: " +
                $"{Translate(match.Groups["status"].Value.Trim())} · " +
                $"{LocalizeNumericTokens(match.Groups["coverage"].Value)}. " +
                Translate(match.Groups["detail"].Value.Trim());

            return true;
        }

        translatedText = string.Empty;
        return false;
    }

    private static bool TryTranslateReviewText(
        string text,
        out string translatedText)
    {
        /*
         * Review summaries may contain multiple independently generated
         * sentences. Split only on a completed parenthesized delta, avoiding
         * decimal separators such as 93.62%.
         */
        var sentences = Regex.Split(
            text,
            @"(?<=\)\.)\s+(?=[A-Z])",
            RegexOptions.CultureInvariant);

        if (sentences.Length > 1)
        {
            translatedText = string.Join(
                " ",
                sentences.Select(sentence => Translate(sentence.Trim())));

            return true;
        }

        var match = Regex.Match(
            text,
            @"^Current history reliability: (?<status>[^·]+) · (?<coverage>\d+(?:\.\d+)?%)\. (?<detail>.+)$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"Affidabilità dello storico corrente: " +
                $"{Translate(match.Groups["status"].Value.Trim())} · " +
                $"{LocalizeNumericTokens(match.Groups["coverage"].Value)}. " +
                Translate(match.Groups["detail"].Value.Trim());

            return true;
        }

        match = Regex.Match(
            text,
            @"^(?<metric>.+) increased from (?<previous>.+) to (?<current>.+) \((?<delta>.+)\)\.$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            var grammar = GetItalianMetricGrammar(
                match.Groups["metric"].Value.Trim());

            translatedText =
                $"{grammar.Subject} {grammar.IncreasedVerb} da " +
                $"{LocalizeNumericTokens(match.Groups["previous"].Value)} a " +
                $"{LocalizeNumericTokens(match.Groups["current"].Value)} " +
                $"({LocalizeNumericTokens(match.Groups["delta"].Value)}).";

            return true;
        }

        match = Regex.Match(
            text,
            @"^(?<metric>.+) decreased from (?<previous>.+) to (?<current>.+) \((?<delta>.+)\)\.$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            var grammar = GetItalianMetricGrammar(
                match.Groups["metric"].Value.Trim());

            translatedText =
                $"{grammar.Subject} {grammar.DecreasedVerb} da " +
                $"{LocalizeNumericTokens(match.Groups["previous"].Value)} a " +
                $"{LocalizeNumericTokens(match.Groups["current"].Value)} " +
                $"({LocalizeNumericTokens(match.Groups["delta"].Value)}).";

            return true;
        }

        match = Regex.Match(
            text,
            @"^(?<metric>.+) remained broadly stable \((?<previous>.+) → (?<current>.+)\)\.$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            var grammar = GetItalianMetricGrammar(
                match.Groups["metric"].Value.Trim());

            translatedText =
                $"{grammar.Subject} {grammar.StableVerb} " +
                $"({LocalizeNumericTokens(match.Groups["previous"].Value)} → " +
                $"{LocalizeNumericTokens(match.Groups["current"].Value)}).";

            return true;
        }

        match = Regex.Match(
            text,
            @"^(?<metric>.+) has no comparable previous-period value\. Current value is (?<current>.+)\.$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            var grammar = GetItalianMetricGrammar(
                match.Groups["metric"].Value.Trim());

            translatedText =
                $"{grammar.Subject} non dispone di un valore confrontabile " +
                $"nel periodo precedente. Il valore corrente è " +
                $"{LocalizeNumericTokens(match.Groups["current"].Value)}.";

            return true;
        }

        match = Regex.Match(
            text,
            @"^(?<metric>.+) is not available in the current period\. Previous value was (?<previous>.+)\.$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            var grammar = GetItalianMetricGrammar(
                match.Groups["metric"].Value.Trim());

            translatedText =
                $"{grammar.Subject} non è disponibile nel periodo corrente. " +
                $"Il valore precedente era " +
                $"{LocalizeNumericTokens(match.Groups["previous"].Value)}.";

            return true;
        }

        match = Regex.Match(
            text,
            @"^(?<metric>Average glucose|Time in range|Data coverage|Readings|Detected patterns|Incomplete days|Empty days): (?<previous>.+) → (?<current>.+)\.$",
            RegexOptions.CultureInvariant);

        if (match.Success)
        {
            translatedText =
                $"{Translate(match.Groups["metric"].Value)}: " +
                $"{LocalizeNumericTokens(match.Groups["previous"].Value)} → " +
                $"{LocalizeNumericTokens(match.Groups["current"].Value)}.";

            return true;
        }

        translatedText = string.Empty;
        return false;
    }

    /// <summary>
    /// Formats a weekly-review value for the selected export culture.
    /// </summary>
    /// <param name="value">The generated review value.</param>
    /// <returns>The localized value.</returns>
    public static string FormatReviewValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return IsItalian
            ? LocalizeNumericTokens(value)
            : value;
    }

    /// <summary>
    /// Formats the local-pattern count summary.
    /// </summary>
    public static string FormatPatternSummary(
        int displayedCount,
        int totalCount)
    {
        if (!IsItalian)
        {
            return totalCount > displayedCount
                ? $"Top {displayedCount} of {totalCount}"
                : totalCount == 0
                    ? "None detected"
                    : $"{totalCount} detected";
        }

        return totalCount > displayedCount
            ? $"Prime {displayedCount} di {totalCount}"
            : totalCount == 0
                ? "Nessuno rilevato"
                : totalCount == 1
                    ? "1 rilevato"
                    : $"{totalCount} rilevati";
    }

    /// <summary>
    /// Formats a time-block diary description.
    /// </summary>
    public static string FormatDailyDiaryDescription(
        string unitLabel)
    {
        return IsItalian
            ? $"Riepiloghi glicemici giornalieri e valori delle principali fasce orarie espressi in {unitLabel}."
            : $"Daily glucose summaries and key time-block values shown in {unitLabel}.";
    }

    /// <summary>
    /// Adapts decimal punctuation inside user-facing text without changing
    /// semantic content.
    /// </summary>
    public static string LocalizeNumericTokens(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (!IsItalian)
        {
            return text;
        }

        return Regex.Replace(
            text,
            @"(?<=\d)\.(?=\d)",
            ",",
            RegexOptions.CultureInvariant);
    }

    private static ItalianMetricGrammar GetItalianMetricGrammar(
        string metric)
    {
        return metric switch
        {
            "Average glucose" => new ItalianMetricGrammar(
                "La glicemia media",
                "è aumentata",
                "è diminuita",
                "è rimasta sostanzialmente stabile"),

            "Time in range" => new ItalianMetricGrammar(
                "Il tempo nel range",
                "è aumentato",
                "è diminuito",
                "è rimasto sostanzialmente stabile"),

            "Data coverage" => new ItalianMetricGrammar(
                "La copertura dei dati",
                "è aumentata",
                "è diminuita",
                "è rimasta sostanzialmente stabile"),

            "Readings" => new ItalianMetricGrammar(
                "Le letture",
                "sono aumentate",
                "sono diminuite",
                "sono rimaste sostanzialmente stabili"),

            "Detected patterns" => new ItalianMetricGrammar(
                "I pattern rilevati",
                "sono aumentati",
                "sono diminuiti",
                "sono rimasti sostanzialmente stabili"),

            "Incomplete days" => new ItalianMetricGrammar(
                "I giorni incompleti",
                "sono aumentati",
                "sono diminuiti",
                "sono rimasti sostanzialmente stabili"),

            "Empty days" => new ItalianMetricGrammar(
                "I giorni senza dati",
                "sono aumentati",
                "sono diminuiti",
                "sono rimasti sostanzialmente stabili"),

            _ => new ItalianMetricGrammar(
                Translate(metric),
                "è aumentato",
                "è diminuito",
                "è rimasto sostanzialmente stabile")
        };
    }

    private sealed record ItalianMetricGrammar(
        string Subject,
        string IncreasedVerb,
        string DecreasedVerb,
        string StableVerb);

    private static string NormalizeLanguageCode(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return DefaultLanguageCode;
        }

        return languageCode
            .Trim()
            .StartsWith(
                "it",
                StringComparison.OrdinalIgnoreCase)
            ? "it"
            : DefaultLanguageCode;
    }

    private sealed class LocalizationScope : IDisposable
    {
        private readonly string? _previousLanguageCode;
        private bool _isDisposed;

        public LocalizationScope(string? previousLanguageCode)
        {
            _previousLanguageCode = previousLanguageCode;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            CurrentLanguageCode.Value = _previousLanguageCode;
            _isDisposed = true;
        }
    }
}

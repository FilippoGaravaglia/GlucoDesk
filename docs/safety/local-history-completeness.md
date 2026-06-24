# Local history completeness

GlucoDesk uses local glucose history to generate statistics, summaries, and diary exports.

The **data completeness** value describes how much local data GlucoDesk has for the selected period.

It does not describe whether glucose values were good or bad.

---

## What completeness means

Completeness answers this question:

> Does GlucoDesk have enough local readings to represent this selected period reliably?

A high completeness score means the selected period is well covered by local readings.

A low completeness score means part of the selected period is missing from the local history.

---

## Why completeness can be low

Completeness may be low when:

- GlucoDesk was not running;
- the computer was off;
- sync was interrupted;
- the configured provider was unavailable;
- the selected export range starts before GlucoDesk began collecting local history;
- backfill could not recover older readings;
- provider limits prevented complete historical recovery.

---

## Important interpretation rule

A low completeness score is not a glycemic score.

For example:

```text
Poor · 34.9%
```

does not mean glucose control was poor.

It means GlucoDesk has insufficient local readings for the selected period.

---

## How to read exports safely

When reviewing a PDF or Excel diary export:

1. check the selected period;
2. check export metadata;
3. check data completeness;
4. treat missing-data periods cautiously;
5. compare summaries only when the underlying local history is sufficiently complete.

---

## Recommended product wording

When possible, prefer wording such as:

```text
Local history completeness
Local data coverage
Data availability
```

Avoid wording that could be confused with medical quality, such as:

```text
Good control
Bad control
Poor glucose
```

---

## Safety reminder

GlucoDesk is not a medical device.

Generated exports are informational and depend on locally available data.

Always rely on approved CGM apps, pump systems, and healthcare professionals for medical decisions.

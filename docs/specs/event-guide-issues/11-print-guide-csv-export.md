# Issue 11 — Print Guide PDF + CSV Export

**Phase:** 3 — Exports & Coordination
**Effort:** M
**Depends on:** Issue 01
**Blocks:** Issue 12 (manager dashboard triggers exports)

---

## Summary

Automated exports of approved event data for the print guide and the layout team. Two formats:
1. **PDF** — print-ready formatted guide sorted by day and time, respecting `MaxPrintSlots` limit
2. **CSV** — flat export of all approved events for the layout team / backup

Both are triggered on demand by Manager or Admin from the manager dashboard (Issue 12) or a dedicated export page.

---

## Export Page — `/EventGuide/Export`

Simple page with two download buttons:
- **Download Print Guide (PDF)**
- **Download Approved Events (CSV)**

Access: Manager and Admin only.

---

## PDF Export

### Content
- Header: event name and guide edition dates (from `GuideSettings` / `EventSettings`)
- Events grouped by day, sorted by start time within each day
- If `GuideSettings.MaxPrintSlots` is set: select the top N events by `PriorityRank` (lowest number = highest priority). Events without a rank are included last, in submission order
- Each event entry:
  - Title (bold)
  - Camp name or venue name
  - Time and duration (e.g. "15:00 — 1h 30min")
  - Location (grid address + location note)
  - Category
  - Description

### Implementation
Use an existing PDF library already in the project, or add **QuestPDF** (free for open-source projects). Follow the patterns in the project for adding new NuGet packages (`Directory.Packages.props`). Update the About page with the new dependency.

### Recurring events
Expand recurring events in the PDF the same way the API does — one entry per occurrence date.

---

## CSV Export

### Columns
```
Id, Title, Description, Category, CampName, VenueName, SubmitterName, GridAddress, LocationNote, Date, StartTime, DurationMinutes, IsRecurring, PriorityRank, Status, SubmittedAt, ApprovedAt
```

- Includes **all approved events** (no slot limit)
- One row per event (recurring events: one row per occurrence)
- Dates/times in event timezone (from `EventSettings`)
- UTF-8 with BOM for Excel compatibility

---

## Acceptance Criteria

- [ ] `/EventGuide/Export` page accessible to Manager and Admin only
- [ ] PDF download contains approved events sorted by day and time
- [ ] PDF respects `MaxPrintSlots` limit by priority rank when set
- [ ] Recurring events appear once per occurrence date in the PDF
- [ ] CSV download contains all approved events with the specified columns
- [ ] CSV is UTF-8 with BOM, opens correctly in Excel
- [ ] Both downloads trigger a file download (correct `Content-Disposition` header)
- [ ] About page updated with any new PDF library dependency
- [ ] `dotnet build` passes with no errors

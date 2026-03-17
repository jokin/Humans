# Issue 12 — Manager Dashboard

**Phase:** 3 — Exports & Coordination
**Effort:** M
**Depends on:** Issue 01, Issue 05 (moderation actions exist)
**Blocks:** —

---

## Summary

A dashboard for Managers and Admins giving a high-level view of the event guide status: submission counts by status, coverage by day, category breakdown, and quick access to the print/CSV exports.

---

## Dashboard Page — `/EventGuide/Dashboard`

Access: Manager and Admin only.

---

## Dashboard Sections

### Overview Cards

| Card | Value |
|------|-------|
| Total submissions | Count of all `GuideEvent` records |
| Pending review | Count with `Status = Pending` |
| Approved | Count with `Status = Approved` |
| Rejected | Count with `Status = Rejected` |
| Resubmit requested | Count with `Status = ResubmitRequested` |
| Withdrawn | Count with `Status = Withdrawn` |

### Coverage by Day

Table showing approved event count per event day:

| Day | Approved Events |
|-----|----------------|
| Day 1 (Wed 5 Aug) | 12 |
| Day 2 (Thu 6 Aug) | 18 |
| … | … |

Days derived from `GuideSettings` linked `EventSettings` date range. Recurring event occurrences are counted per day they appear.

### Coverage by Category

Table showing approved event count per active `EventCategory`:

| Category | Submitted | Approved | Pending | Rejected |
|----------|-----------|----------|---------|----------|
| Workshop | 24 | 18 | 4 | 2 |
| Music | 15 | 12 | 3 | 0 |
| … | … | … | … | … |

### Top Submitting Camps

Table of camps ordered by total submission count (descending):

| Camp | Submitted | Approved | Pending |
|------|-----------|----------|---------|
| Camp Sunrise | 8 | 6 | 2 |
| … | … | … | … |

### Export Section

Two prominent buttons linking to the export page (Issue 11):
- **Generate Print Guide (PDF)**
- **Download Event Data (CSV)**

### Moderation Queue Link

Prominent link to `/EventGuide/Moderate` showing the current pending count as a badge.

---

## Nav Link

Add "Event Guide Dashboard" to the Manager/Admin nav section.

---

## Implementation Notes

- At this scale (~500 users, < 500 events), load all events into memory and compute stats in-process — no complex DB aggregation needed
- Use `IMemoryCache` with a short TTL (e.g. 5 minutes) for the dashboard stats to avoid repeated queries on page refresh
- The recurring event day-coverage calculation should expand recurrences the same way the API does

---

## Acceptance Criteria

- [ ] `/EventGuide/Dashboard` accessible only to Manager and Admin
- [ ] Overview cards show accurate counts for each status
- [ ] Coverage by day table shows approved event count per event day (expanding recurring events)
- [ ] Coverage by category table shows counts per category across all statuses
- [ ] Top submitting camps table ordered by total submission count
- [ ] Export buttons link to the export actions from Issue 11
- [ ] Pending count badge on the moderation queue link updates live on page load
- [ ] "Event Guide Dashboard" link in Manager/Admin nav
- [ ] `dotnet build` passes with no errors

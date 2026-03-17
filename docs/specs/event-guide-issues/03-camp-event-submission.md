# Issue 03 — Camp Event Submission (Team Page)

**Phase:** 1 — Core Content Management
**Effort:** M
**Depends on:** Issue 01
**Blocks:** Issue 05 (events must exist to moderate), Issue 07 (duplicate detection on moderation queue)

---

## Summary

Camp organisers (team Leads) can submit events for their camp directly from their team page in Humans. This adds an "Events" tab to the team page listing the camp's submissions, and a submission form for creating and editing events. Submissions go into `Pending` status and await moderation.

---

## GuideCamp Auto-Creation

When a Lead submits their first camp event and no `GuideCamp` exists for their team, automatically create one with:
- `TeamId` = their team's ID
- `CampName` = team name (editable later via a camp profile edit form, out of scope for this issue)
- `IsPublished` = false

This happens transparently — no extra step required from the organiser.

---

## Team Page — Events Tab

Add an **Events** tab to the team detail page (`/Teams/{id}`) visible to Leads and above.

### Tab contents
- Summary: submitted count, approved count, pending count
- Table of all events submitted by this camp:
  - Title, category, date/time, status badge, priority rank
  - Edit action (if status is `Draft`, `Rejected`, or `ResubmitRequested`)
  - Withdraw action (if status is `Pending` or `Draft`)
- "Submit New Event" button (visible when submission window is open per `GuideSettings.SubmissionOpenAt / SubmissionCloseAt`)
- If submission window is closed, show a read-only message with the window dates

### Access
- Team Lead and above for that team, plus Admin and Manager

---

## Event Submission Form — `/Teams/{teamId}/Events/New`

### Fields
| Field | Constraints |
|-------|-------------|
| Title | Required, ≤ 80 chars |
| Description | Required, ≤ 300 chars |
| Category | Required — dropdown of active `EventCategory` records |
| Date | Required — date picker, restricted to event dates from `GuideSettings` |
| Start time | Required — time picker |
| Duration | Required — dropdown or numeric (15 min increments, up to 8 hours) |
| Location note | Optional, ≤ 120 chars — free-text detail ("near the fire pit") |
| Is recurring | Checkbox |
| Recurrence days | Shown when recurring = true — multi-select of event days (day offsets) |
| Priority rank | Integer, 1 = highest priority for print guide selection |

### On submit
- Sets `GuideCampId` from the team's `GuideCamp` (auto-created if needed)
- Sets `SubmitterUserId` to the current user
- Sets `Status = Pending`, `SubmittedAt = now`
- Redirects to team Events tab with success message

### Edit form — `/Teams/{teamId}/Events/{eventId}/Edit`
- Same fields, pre-populated
- Only available when `Status` is `Draft`, `Rejected`, or `ResubmitRequested`
- On resubmit: sets `Status = Pending`, `SubmittedAt = now`

### Withdraw
- POST action, soft-deletes the submission (sets `Status = Withdrawn` — add to enum)
- Only available when `Status` is `Draft` or `Pending`
- Confirm dialog before action

---

## GuideEventStatus Update

Add `Withdrawn` to `GuideEventStatus` enum (extend Issue 01 if not yet merged, or add here).

---

## Acceptance Criteria

- [ ] Events tab appears on team page for Leads, MetaLeads, Managers, and Admins
- [ ] "Submit New Event" button only visible when submission window is open
- [ ] Submission form creates a `GuideEvent` with `Status = Pending` and auto-creates `GuideCamp` if needed
- [ ] All field validations enforced server-side
- [ ] Edit form pre-populates all fields; resubmitting a rejected event resets status to `Pending`
- [ ] Withdraw sets status to `Withdrawn` and is confirmed before executing
- [ ] Team Events tab shows live status for all submissions
- [ ] Access restricted to Lead+ for the specific team, plus Admin/Manager
- [ ] `dotnet build` passes with no errors

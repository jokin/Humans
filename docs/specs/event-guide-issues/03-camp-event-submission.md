# Issue 03 — Camp Event Submission

**Phase:** 1 — Core Content Management
**Effort:** M
**Depends on:** Issue 01
**Blocks:** Issue 05 (events must exist to moderate), Issue 07 (duplicate detection on moderation queue)

---

## Summary

Camp leads can submit events for their camp from the camp detail page. This adds an "Events" button to the camp's Actions section, an events listing page, and a submission form for creating and editing events. Submissions go into `Pending` status and await moderation.

---

## Camp Lookup

Events are linked to the existing `Camp` entity via `GuideEvent.CampId`. Authorization is via `CampLead` — any active lead (Primary or CoLead) for a camp can submit and manage events. Admin and CampAdmin roles also have full access.

---

## Camp Events Page — `/Camps/{slug}/Events`

### Page contents

- Summary: submitted count, approved count, pending count
- Table of all events submitted for this camp:
  - Title, category, date/time, status badge, priority rank
  - Edit action (any status except `Withdrawn`)
  - Withdraw action (if status is `Pending` or `Draft`)
- "Submit New Event" button (visible when submission window is open per `GuideSettings.SubmissionOpenAt / SubmissionCloseAt`)
- If submission window is closed, show a read-only message with the window dates

### Access

- Camp Lead (Primary or CoLead) for that camp, plus Admin and CampAdmin

---

## Event Submission Form — `/Camps/{slug}/Events/New`

### Fields

| Field | Constraints |
|-------|-------------|
| Title | Required, ≤ 80 chars |
| Description | Required, ≤ 300 chars |
| Category | Required — dropdown of active `EventCategory` records |
| Date | Required — dropdown restricted to event dates from `EventSettings` |
| Start time | Required — time picker |
| Duration | Required — dropdown (15 min increments, up to 8 hours) |
| Location note | Optional, ≤ 120 chars — free-text detail ("near the fire pit") |
| Is recurring | Checkbox |
| Recurrence days | Shown when recurring = true — comma-separated day offsets |
| Priority rank | Integer, 1 = highest priority for print guide selection |

### On submit

- Sets `CampId` from the camp
- Sets `SubmitterUserId` to the current user
- Sets `Status = Pending`, `SubmittedAt = now`
- Redirects to camp Events page with success message

### Edit form — `/Camps/{slug}/Events/{eventId}/Edit`

- Same fields, pre-populated
- Available for any status except `Withdrawn`
- Saving an edit on an `Approved` or `Pending` event resets `Status = Pending`, `SubmittedAt = now`, sending it back through moderation

### Withdraw

- POST action, sets `Status = Withdrawn`
- Only available when `Status` is `Draft` or `Pending`
- Confirm dialog before action

---

## Nav Links

- Events button added to camp detail page Actions section (visible to leads + admins)
- Dual routing: both `/Camps/{slug}/Events` and `/Barrios/{slug}/Events` work

---

## Acceptance Criteria

- [ ] Events page accessible at `/Camps/{slug}/Events` for leads and admins
- [ ] "Submit New Event" button only visible when submission window is open
- [ ] Submission form creates a `GuideEvent` with `Status = Pending` linked to the camp
- [ ] All field validations enforced server-side
- [ ] Edit form pre-populates all fields; resubmitting a rejected event resets status to `Pending`
- [ ] Withdraw sets status to `Withdrawn` and is confirmed before executing
- [ ] Events page shows live status for all submissions
- [ ] Access restricted to camp leads (Primary/CoLead) plus Admin/CampAdmin
- [ ] `dotnet build` passes with no errors

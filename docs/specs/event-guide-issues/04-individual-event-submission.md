# Issue 04 — Individual Event Submission

**Phase:** 1 — Core Content Management
**Effort:** M
**Depends on:** Issue 01, Issue 02 (venues must exist)
**Blocks:** Issue 05 (events must exist to moderate)

---

## Summary

Any registered human — not just camp leads — can submit an event hosted at an admin-curated communal venue (e.g. "Main Stage", "The Middle of Elsewhere"). The submission goes through the same moderation queue as camp events. This implements the participatory culture: no special role required to propose an event.

---

## Entry Point

Add a **"Submit an Event"** link in the user's personal nav area (profile dropdown or dashboard) and on the `/EventGuide` public-facing page (when it exists). The link is visible to all authenticated users.

If the submission window is closed (per `GuideSettings`), redirect to an informational page showing when it opens.

---

## Submission Form — `/EventGuide/Submit`

### Fields
| Field | Constraints |
|-------|-------------|
| Title | Required, ≤ 80 chars |
| Description | Required, ≤ 300 chars |
| Category | Required — dropdown of active `EventCategory` records |
| Venue | Required — dropdown of active `GuideSharedVenue` records |
| Location note | Optional, ≤ 120 chars — free-text detail within the venue |
| Date | Required — date picker, restricted to event dates from `GuideSettings` |
| Start time | Required — time picker |
| Duration | Required — dropdown or numeric (15 min increments, up to 8 hours) |
| Is recurring | Checkbox |
| Recurrence days | Shown when recurring = true — multi-select of event days |

No priority rank field for individual events — priority ranking is for camp events competing for print guide slots.

### On submit
- Sets `GuideSharedVenueId` from the selected venue
- Sets `GuideCampId = null`
- Sets `SubmitterUserId` to the current user
- Sets `Status = Pending`, `SubmittedAt = now`
- Redirects to "My Event Submissions" page with success message

---

## My Event Submissions — `/EventGuide/MySubmissions`

Page showing the current user's individual event submissions.

### Contents
- Table: title, venue, date/time, category, status badge
- Edit action (if `Draft`, `Rejected`, or `ResubmitRequested`)
- Withdraw action (if `Draft` or `Pending`)
- "Submit New Event" link

### Access
- Any authenticated user (their own submissions only)

---

## Edit Form — `/EventGuide/Submit/{eventId}/Edit`

Same fields as the creation form, pre-populated. Available when status is `Draft`, `Rejected`, or `ResubmitRequested`. Resubmitting resets status to `Pending`.

---

## Withdraw

POST action on `/EventGuide/Submit/{eventId}/Withdraw`. Sets `Status = Withdrawn`. Confirm dialog required.

---

## Attribution in the Guide

Individual events are attributed to the submitter's display name (profile name) rather than a camp. The API response (Issue 08) should include a `submitterName` field for non-camp events. No camp name is shown.

---

## Acceptance Criteria

- [ ] Any authenticated user can access `/EventGuide/Submit` when the submission window is open
- [ ] Form shows only active `GuideSharedVenue` records in the venue dropdown
- [ ] Submission creates a `GuideEvent` with `GuideSharedVenueId` set and `GuideCampId = null`
- [ ] "My Event Submissions" page shows only the current user's individual submissions
- [ ] Edit and withdraw work as specified
- [ ] Unauthenticated users are redirected to login
- [ ] "Submit an Event" link is reachable from the user's nav area
- [ ] `dotnet build` passes with no errors

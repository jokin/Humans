# Event Guide Management in Humans — Project Definition

**Elsewhere 2026 | Draft v1.0 | March 2026**

---

## 1. The Problem

The Elsewhere event guide is a standalone PWA that lives entirely outside of Humans. This creates the same fragmentation problem as VIM did for volunteers: camp organisers submit events through one system, moderation happens in an ad-hoc process, and attendees carry yet another URL they have to find on-site with no connectivity.

Specific pain points:

- **No connection to Humans.** Camp organisers are already in Humans as teams, but event submission is a completely separate flow with no identity linkage.
- **Moderation has no tooling.** Content review happens informally; there is no structured queue, duplicate detection, or rejection workflow with email notification.
- **Print guide is manual.** The print-ready PDF is assembled by hand after extracting data from the guide — no automated export exists.
- **Personalisation is device-local only.** Favourites and personal schedules live in localStorage with no account backing. Switching devices loses everything.
- **Data lives in a separate system.** Any update to an event requires touching a different codebase, with no audit trail and no link to who submitted it.

The goal is to make Humans the authoritative backend for event guide data — submission, moderation, and publication — while keeping a lightweight, offline-capable PWA as the attendee-facing frontend, now served from Humans' API.

---

## 2. What We're Building

An event guide management module embedded in Humans, covering:

- **Event submission** for camp organisers (linked to their Humans team) and for any individual human hosting an event at a communal or shared venue
- **Communal venue registry** — admin-managed list of shared spaces (e.g. "The Middle of Elsewhere", "Main Stage") that any human can use as an event location without belonging to a camp
- **Moderation queue** for volunteer moderators to approve, reject, or request edits
- **Duplicate detection** flagging same-camp, same-slot submissions automatically
- **Email notifications** for submission received, rejection with reason, resubmission confirmation
- **Category-based filtering** including opt-out of categories (Adult, Spiritual, etc.)
- **Recurring event support** — one submission, multiple displayed dates
- **Print guide export** generated automatically from approved events (PDF, sorted by day and time)
- **Published API** serving the existing (or rebuilt) PWA frontend from Humans data
- **Account-backed personalisation** — favourites and personal schedule tied to a Humans account, not just a device

This is an MVP scoped for Elsewhere 2026.

---

## 3. What We're NOT Building (Right Now)

- A replacement PWA frontend — the existing guide stays; Humans becomes its data source
- An interactive site map — location data is text-based (grid address / clock position); a visual map is Phase 2
- A kiosk mode — read-only access without login remains on the PWA side, not in Humans
- Multi-event / multi-year support — scoped to Elsewhere 2026 only
- Cantina integration or dietary headcount from guide data
- Real-time update push to cached PWA (update mechanism is a Phase 2 concern)

---

## 4. Who Uses This and How

### Camp Organiser (submitter)

From their **team page in Humans**, a camp organiser can:

- Submit events for their camp (title, description, category, date/time, duration, location, recurrence)
- Rank events by priority (for print guide selection when space is limited)
- Receive email notification on rejection with reason, then edit and resubmit
- See the current status of each of their submissions (Pending / Approved / Rejected / Resubmit Requested)

### Individual Human (submitter)

Any registered human — not necessarily a camp lead — can submit an event hosted at a **communal or shared venue**. From their **profile page or a dedicated "Submit an Event" link**, they can:

- Submit an event choosing a shared venue from the admin-managed communal venue list (e.g. "The Middle of Elsewhere")
- Optionally provide a more specific free-text location note within the venue (e.g. "near the fire pit")
- Receive the same email notifications as camp organisers (submission received, rejection with reason, approved)
- See the status of their personal submissions

Individual submissions go through the same moderation queue as camp events. They are displayed in the guide attributed to the submitter's name (or a chosen display name) rather than a camp.

### Volunteer Moderator

A dedicated **moderation queue** at `/EventGuide/Moderate` showing:

- All pending submissions in chronological order of receipt
- Duplicate flags (same camp + overlapping time slot)
- Approve / Reject / Request Edit actions per submission
- On rejection: a required reason field that triggers the notification email to the organiser

### Manager / Admin

Global overview of the event guide:

- Total submissions by status
- Approved event count and coverage by day
- Trigger print guide PDF export (full approved list, sorted by day and time)
- Manage event categories (add, rename, hide from public)
- Set guide open date (when submission form becomes available to camps)

### Attendee (via the PWA)

Read-only access to the published event guide through the PWA frontend (unchanged UX):

- Browse all approved events with filtering by day, time of day, and category
- Category preference opt-out (e.g. hide Adult, Spiritual) — preference stored per device or, if logged in, per account
- Keyword search across title and description
- Event detail with camp name, grid address, time, duration, full description
- Favourite events and view a personal schedule — stored to Humans account if logged in, localStorage if not
- Camp directory with all hosted events

---

## 5. Core Data Model

This reuses what Humans already has (profiles, teams, roles, email outbox) and adds the following:

### GuideSettings

Single record per event edition storing: submission open date, submission close date, guide publish date, event start/end dates, timezone, max print guide slots.

### EventCategory

Lookup table: name, slug, is_sensitive (Adult, Spiritual, etc. trigger the opt-out UI), display order, is_active.

### GuideCamp

Links a Humans `Team` to a camp identity in the guide context: camp name (may differ from team name), description, grid address, is_published. One camp per team for 2026; many-to-one is a Phase 2 concern.

### GuideSharedVenue

Admin-managed list of communal or infrastructure spaces that exist independently of any camp — e.g. "The Middle of Elsewhere", "Main Stage", "Open Playa", "Community Centre". Fields: name, description, grid address or location description, is_active, display_order. These are the valid location choices for individual (non-camp) event submissions.

### GuideEvent

A single event submission. Fields: `GuideCampId` (nullable), `GuideSharedVenueId` (nullable), `SubmitterUserId`, location_note (optional free-text detail within the venue, e.g. "near the fire pit"), title (≤ 80 chars), description (≤ 300 chars), `CategoryId`, start datetime, duration minutes, is_recurring, recurrence_days (comma-separated day offsets from event start), priority_rank (submitter-assigned), status (`Draft → Pending → Approved / Rejected / ResubmitRequested`), admin_notes (internal), submitted_at, last_updated_at.

Exactly one of `GuideCampId` or `GuideSharedVenueId` must be set — a camp event is anchored to a camp, an individual event is anchored to a shared venue. `SubmitterUserId` is always set and is the authoritative link to the submitter regardless of event type.

Duration and end datetime are computed. Recurring events generate display rows at read time, not separate DB rows.

### ModerationAction

Append-only log of every moderation decision: `GuideEventId`, actor `UserId`, action (Approved / Rejected / ResubmitRequested), reason (required for non-approval), timestamp. Preserves audit trail without mutating GuideEvent directly.

### UserGuidePreference

Stores per-account guide preferences: excluded category slugs (JSON array). One row per user, upserted on change. Device-local preferences (no account) remain in localStorage on the PWA side.

### UserEventFavourite

Links a `UserId` to a `GuideEventId`. One row per favourite. Deleted on unfavourite. Used to build the account-backed personal schedule.

### Email triggers

Reuses the existing `EmailOutboxMessage` + `ProcessEmailOutboxJob` infrastructure. New triggers: submission received, moderation rejection (with reason), resubmit requested (with reason), submission approved.

---

## 6. Key Design Decisions

**Humans is the backend, the PWA is the frontend.** The attendee-facing experience stays as a lightweight PWA — the change is that it now calls a Humans API (`/api/guide/...`) for its data instead of a static JSON file. This separates content management from content delivery cleanly.

**Camp = Team.** The existing Humans team hierarchy maps directly to camps. No new org entity is needed. GuideCamp is a thin projection that adds guide-specific fields (grid address, published flag) on top of a Team.

**Two submission paths, one moderation queue.** Camp events are submitted by a team Lead and anchored to a GuideCamp. Individual events are submitted by any registered human and anchored to a GuideSharedVenue. Both flow through the same moderation queue with the same approve / reject / resubmit workflow. There is no fast-track or bypass for either type.

**Any human can submit an individual event.** No special role is required — every registered human in Humans can submit an event at a communal venue. This reflects the participatory culture of the event. Moderation is the quality gate, not access control.

**Communal venues are admin-curated.** The list of shared spaces (GuideSharedVenue) is managed by Admins, not submitted by attendees. This prevents free-text location sprawl ("the middle", "centre of elsewhere", "el centro") and ensures all individual events point to a clean, consistent location name in the guide.

**Moderation is mandatory.** No event goes public without at least one Approved moderation action. Auto-approval is not offered, even for trusted camps. This protects content quality and safety (especially for sensitive categories).

**Duplicate detection is advisory, not blocking.** A flag appears on the moderation queue UI when a submission matches an existing approved or pending event from the same camp in the same time slot. The moderator decides; the system does not reject automatically.

**Recurring events are one row.** A recurring event is stored once with a recurrence pattern. The display layer expands it into one card per occurrence. This avoids data duplication and keeps edits atomic.

**Print guide export is automated.** Priority rank (set by the organiser) combined with the approved status determines selection order. The PDF is generated on demand from the approved event set — no manual layout step for coordinator CSV work.

**Category opt-out, not hard filter.** Sensitive categories (Adult, Spiritual, etc.) are visible by default but have a persistent opt-out. The opt-out is stored per account in `UserGuidePreference`; for non-logged-in attendees it stays in localStorage. The goal is to avoid hiding content from those who want it while giving others a clear, persistent way to avoid it.

**Soft delete over hard delete.** Deactivating an approved event hides it from the published guide but preserves the ModerationAction history. Hard deletion is blocked if a ModerationAction exists for the event.

---

## 7. Phased Delivery for Elsewhere 2026

### Phase 1 — Core Content Management (must ship before submission opens)

1. GuideSettings, EventCategory, GuideCamp, GuideSharedVenue entities and admin UI
2. GuideEvent submission form on team page (Lead role) and individual submission form (any human), status display
3. Moderation queue at `/EventGuide/Moderate` with approve / reject / resubmit-request actions
4. ModerationAction audit log
5. Email notifications: submission received, rejection, resubmit requested, approved
6. Duplicate detection flag on moderation queue

### Phase 2 — Published Guide & Personalisation (must ship before guide opens to attendees)

7. Published events API (`/api/guide/events`, `/api/guide/camps`) consumed by PWA
8. Category filter with opt-out (sensitive categories)
9. Recurring event display expansion in API responses
10. UserGuidePreference (account-backed category opt-out)
11. UserEventFavourite (account-backed personal schedule)
12. Personal schedule view within Humans (for logged-in attendees visiting Humans directly)

### Phase 3 — Exports & Coordination (target: 4 weeks before event)

13. Print guide PDF export (Manager / Admin triggered)
14. Priority rank UI on submission form (drag-to-reorder or number field)
15. Approved event CSV export (for layout team / backup)
16. Manager dashboard: submission stats, coverage by day, category breakdown

### Phase 4 — Enhancements (post-2026 or if time allows)

17. Visual location map (Leaflet or static schematic with camp pins)
18. PWA update push mechanism (e.g. versioned cache-busting on publish)
19. Multi-camp-per-team support
20. Kiosk mode (read-only, no login, installable on shared device)
21. Schedule export to PDF from personal schedule

---

## 8. Migration from the Existing Guide

The existing PWA event data is not large and does not need to be migrated into Humans for 2026:

- Start fresh: all camps resubmit their events through the new Humans form
- The existing PWA frontend can be pointed at the new Humans API with a config change (base URL swap)
- Historical event data from previous years does not need to be imported; the guide is per-edition
- Camp → Team matching is already done in Humans; GuideCamp records are created by Leads when they first submit

---

## 9. Open Questions

1. **Submission authentication** — Does the submission form require a Humans account (Lead role), or should camps be able to submit via a magic-link/token for camps not yet in Humans?
2. **Guide open date** — When does the event guide go live to attendees for Elsewhere 2026?
3. **Print guide slot limit** — Is there a fixed number of events that fit in the printed programme? Who decides the cutoff?
4. **Moderator role** — Is moderation done by an existing Humans role (e.g. Admin, MetaLead) or should a new `GuideModeration` role be introduced?
5. **PWA frontend ownership** — Who maintains the PWA? Does it stay as a separate repo pointed at the Humans API, or is it brought into the Humans repo?
6. **Category list** — What is the canonical list of event categories for Elsewhere 2026? Who owns adding/removing them?
7. **Sensitive category list** — Which categories require the opt-out UI (Adult confirmed; what else)?
8. **Account-optional attendee access** — Should attendees be able to log in to Humans via the PWA to sync their favourites, or is this out of scope for the guide experience?
9. **Individual event attribution in the guide** — How should individual (non-camp) events be displayed to attendees? Options: submitter's real name, a chosen display name, or anonymously as "Independent" / the venue name. Does this need to be configurable per submission?
10. **Communal venue seeding** — Who defines the initial list of GuideSharedVenue records for Elsewhere 2026, and by when? Are venues fixed per edition or can new ones be added after submission opens?

---

## 10. Success Criteria for 2026

- Camp organisers and individual humans can submit events through Humans with no external form or spreadsheet
- Moderation queue is fully processed before guide publication — zero unreviewed submissions go live
- The event guide PWA serves data from Humans with no manual JSON export or file upload step
- Print guide PDF is generated in one click from the admin panel — no layout-team spreadsheet required
- Attendees can opt out of sensitive categories and have that preference persist across sessions
- Account-backed favourites survive switching devices

---

## 11. Relationship to Volunteer Management

The event guide and shift management modules share the same team and role infrastructure in Humans. A camp organiser who is also a shift lead uses the same team page as their entry point for both. These two modules are independent at the data level but share:

- Team / Department hierarchy
- Role-based permissions (Lead, MetaLead, Manager, Admin)
- Email outbox infrastructure
- EventSettings (shared event date ranges, timezone)

No cross-module data dependency exists at Phase 1; coordination between the two is handled at the human level (e.g. a camp lead who is also a shift lead).

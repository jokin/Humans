# Issue 10 — Account-Backed Favourites & Personal Schedule

**Phase:** 2 — Published Guide & Personalisation
**Effort:** M
**Depends on:** Issue 08 (published events API)

---

## Summary

Authenticated users can favourite events and view a personal schedule. Favourites are stored in `UserEventFavourite` (server-side), surviving device switches. The PWA uses these API endpoints in preference to localStorage when the user is logged in.

---

## API Endpoints

### `GET /api/guide/favourites`

Returns the current user's favourited events (full event objects, same shape as `/api/guide/events`).

**Requires authentication.**

Response: array of event objects, ordered by `startAt` ascending.

### `POST /api/guide/favourites/{eventId}`

Favourites an event.

**Requires authentication.**

- Creates a `UserEventFavourite` record for the current user + event ID
- Returns 200 if saved, 409 if already favourited, 404 if event not found or not approved

### `DELETE /api/guide/favourites/{eventId}`

Removes a favourite.

**Requires authentication.**

- Deletes the `UserEventFavourite` record
- Returns 200 if removed, 404 if not favourited

---

## Personal Schedule Page — `/EventGuide/Schedule`

An in-Humans page for logged-in attendees to view their personal schedule without using the PWA.

### Contents
- All favourited events sorted by day and start time
- Grouped by day (Day 1, Day 2, …)
- Each event shows: title, camp/venue, time, duration, category, location
- Unfavourite button on each event
- Warning indicator when two favourited events overlap in time (conflict detection)

### Conflict Detection
Two favourited events conflict if their time windows overlap. Show a ⚠ warning on both events in the schedule view. No automatic action — informational only.

### Access
- Any authenticated user (their own schedule only)

---

## Nav Link

Add "My Schedule" link to the user nav area (profile dropdown), visible only when authenticated.

---

## Acceptance Criteria

- [ ] `GET /api/guide/favourites` returns the user's favourited events sorted by start time
- [ ] `POST /api/guide/favourites/{eventId}` adds a favourite; returns 409 if already exists; 404 if event not approved
- [ ] `DELETE /api/guide/favourites/{eventId}` removes a favourite
- [ ] All three endpoints require authentication
- [ ] `/EventGuide/Schedule` shows the user's favourites grouped by day
- [ ] Time conflicts between favourited events are indicated with a warning
- [ ] Unfavouriting from the schedule page removes the event immediately (post-redirect-get)
- [ ] "My Schedule" link visible in authenticated user nav
- [ ] `dotnet build` passes with no errors

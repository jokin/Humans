# Issue 09 — Category Opt-Out (UserGuidePreference)

**Phase:** 2 — Published Guide & Personalisation
**Effort:** S
**Depends on:** Issue 08 (published events API)

---

## Summary

Authenticated attendees can opt out of seeing events in sensitive categories (Adult, Spiritual, etc.). The preference is stored per account in `UserGuidePreference` and applied in the API response. Device-local opt-out for unauthenticated users remains in localStorage on the PWA side (out of scope here).

---

## API Endpoint

### `GET /api/guide/preferences`

Returns the current user's guide preferences.

**Requires authentication.**

```json
{
  "excludedCategorySlugs": ["adult", "spiritual"]
}
```

Returns `{ "excludedCategorySlugs": [] }` if no preference record exists yet.

### `PUT /api/guide/preferences`

Upserts the current user's guide preferences.

**Requires authentication.**

Request body:
```json
{
  "excludedCategorySlugs": ["adult"]
}
```

- Validates that each slug exists in the active `EventCategory` list
- Creates or updates `UserGuidePreference` for the current user
- Returns 200 with the saved preference on success

---

## Events API Integration

`GET /api/guide/events` (Issue 08) should respect the authenticated user's opt-out when called with a valid session cookie:

- If the user is authenticated and has a `UserGuidePreference` with excluded slugs, filter out events in those categories from the response
- If unauthenticated, return all approved events regardless (PWA handles localStorage-based filtering client-side)

This filtering is applied server-side, not as a query parameter, so the PWA doesn't need to know the preference storage mechanism.

---

## In-App Preference UI (Optional / Stretch)

If time allows, a simple preference page at `/EventGuide/Preferences` where logged-in users can tick/untick categories to exclude. Links to this page from the user profile nav.

This is a stretch goal — the API endpoints are the required deliverable.

---

## Acceptance Criteria

- [ ] `GET /api/guide/preferences` returns the user's excluded categories (empty list if none set)
- [ ] `PUT /api/guide/preferences` saves the preference, validates slugs, and returns the saved state
- [ ] Both preference endpoints require authentication (401 for unauthenticated calls)
- [ ] `GET /api/guide/events` filters out excluded-category events for authenticated users with preferences set
- [ ] Unauthenticated calls to `/api/guide/events` are unaffected by any preference logic
- [ ] `dotnet build` passes with no errors

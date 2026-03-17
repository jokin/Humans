# Issue 08 — Published Events API

**Phase:** 2 — Published Guide & Personalisation
**Effort:** M
**Depends on:** Issue 01
**Blocks:** Issue 09 (category opt-out), Issue 10 (favourites & schedule)

---

## Summary

Public read-only API endpoints serving approved event guide data to the existing PWA frontend. The PWA replaces its static JSON source with calls to these endpoints. No authentication required for read-only access.

---

## Endpoints

### `GET /api/guide/events`

Returns all approved `GuideEvent` records, expanded for recurrence.

**Query parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `day` | `int?` | Filter by day offset (0 = first event day) |
| `categorySlug` | `string?` | Filter by category slug |
| `campId` | `int?` | Filter by `GuideCampId` |
| `q` | `string?` | Keyword search across title and description |

**Response:**
```json
[
  {
    "id": 42,
    "title": "Morning yoga",
    "description": "...",
    "category": { "id": 3, "name": "Workshop", "slug": "workshop", "isSensitive": false },
    "startAt": "2026-08-06T08:00:00Z",
    "durationMinutes": 60,
    "dayOffset": 1,
    "isRecurring": true,
    "recurrenceDays": [0, 2, 4],
    "camp": { "id": 7, "name": "Camp Sunrise", "gridAddress": "4:30 & Esplanade" },
    "venue": null,
    "submitterName": null,
    "locationNote": "near the east entrance",
    "priorityRank": 1
  }
]
```

- For camp events: `camp` is populated, `venue` and `submitterName` are null
- For individual events: `venue` is populated, `camp` is null, `submitterName` is the submitter's display name
- Recurring events are **expanded**: one response entry per occurrence date. Each entry has a unique `occurrenceDate` field; `id` refers to the parent `GuideEvent.Id`
- Results ordered by `startAt` ascending

### `GET /api/guide/events/{id}`

Returns a single approved event by ID (the parent `GuideEvent.Id`). Returns 404 if not approved or not found.

### `GET /api/guide/camps`

Returns all `GuideCamp` records with `IsPublished = true`.

```json
[
  {
    "id": 7,
    "name": "Camp Sunrise",
    "description": "...",
    "gridAddress": "4:30 & Esplanade"
  }
]
```

### `GET /api/guide/camps/{id}`

Returns a single published camp and all its approved events.

### `GET /api/guide/categories`

Returns all active `EventCategory` records.

```json
[
  { "id": 3, "name": "Workshop", "slug": "workshop", "isSensitive": false, "displayOrder": 2 }
]
```

---

## Recurrence Expansion

Recurring events (`IsRecurring = true`) are stored once with a `RecurrenceDays` field (comma-separated day offsets from event start). The API expands these at read time into multiple entries — one per day offset — with distinct `startAt` values. No extra DB rows are created.

---

## Caching

These endpoints can be cached aggressively (e.g. 60-second response cache or `IMemoryCache`) since the guide data changes only when a moderator takes action. Cache invalidation on any `ModerationAction` creation is sufficient.

---

## CORS

Enable CORS for the `/api/guide/*` routes to allow the PWA (served from a different origin) to call these endpoints.

---

## No Authentication Required

These are public read endpoints. No `[Authorize]` attribute. The PWA does not require a Humans account for read access.

---

## Acceptance Criteria

- [ ] `GET /api/guide/events` returns all approved events, with optional filters working correctly
- [ ] Recurring events are expanded into one entry per occurrence in the response
- [ ] `GET /api/guide/events/{id}` returns 404 for non-approved or missing events
- [ ] `GET /api/guide/camps` returns published camps only
- [ ] `GET /api/guide/camps/{id}` returns camp detail with all its approved events
- [ ] `GET /api/guide/categories` returns active categories
- [ ] All endpoints return 200 with empty array (not 404) when no results match
- [ ] CORS enabled for `/api/guide/*` routes
- [ ] No authentication required for any endpoint
- [ ] `dotnet build` passes with no errors

# Issue 02 — Admin UI: GuideSettings, Categories & Venues

**Phase:** 1 — Core Content Management
**Effort:** M
**Depends on:** Issue 01
**Blocks:** Issue 04 (venues must exist before individual submission)

---

## Summary

Admin UI to configure the event guide before submission opens: guide dates/settings, event category management, and communal venue management. All three sections are admin-only.

---

## GuideSettings — `/Admin/GuideSettings`

Single-record settings form for the event guide edition.

### Fields
- Submission open date/time
- Submission close date/time
- Guide publish date/time
- Max print guide slots (integer)
- FK link to `EventSettings` (existing record — selected from dropdown of active event editions)

### Behaviour
- If no `GuideSettings` record exists, show a "Create Guide Settings" form
- If one exists, show an edit form (no delete)
- All datetimes displayed in the event's timezone (from linked `EventSettings`)

### Access
- Admin only (`[Authorize(Roles = RoleNames.Admin)]`)

---

## Event Categories — `/Admin/GuideCategories`

CRUD list of `EventCategory` records.

### List view
- Table: name, slug, sensitive flag, active flag, display order
- Inline reorder (up/down arrows or drag) updating `DisplayOrder`
- Edit and deactivate actions per row
- "Add Category" button

### Create / Edit form
- Name (required, ≤ 60 chars)
- Slug (auto-generated from name, editable, unique, URL-safe)
- Is Sensitive checkbox (Adult, Spiritual, etc.)
- Is Active checkbox
- Display Order (numeric)

### Constraints
- Cannot delete a category that has associated `GuideEvent` records — show error
- Deactivating hides it from submission forms but does not affect existing events

### Access
- Admin only

---

## Communal Venues — `/Admin/GuideVenues`

CRUD list of `GuideSharedVenue` records.

### List view
- Table: name, location description, active flag, display order
- Edit and deactivate actions per row
- "Add Venue" button

### Create / Edit form
- Name (required, ≤ 120 chars)
- Description (optional, ≤ 500 chars)
- Location description — grid address or text (optional, ≤ 120 chars)
- Is Active checkbox
- Display Order (numeric)

### Constraints
- Cannot delete a venue that has associated `GuideEvent` records — show error
- Deactivating hides it from individual submission forms

### Access
- Admin only

---

## Nav Links

- Add "Event Guide" section to the Admin nav with links to:
  - Guide Settings (`/Admin/GuideSettings`)
  - Categories (`/Admin/GuideCategories`)
  - Venues (`/Admin/GuideVenues`)

---

## Acceptance Criteria

- [ ] `/Admin/GuideSettings` — create and edit guide settings, linked to an EventSettings record
- [ ] `/Admin/GuideCategories` — list, create, edit, reorder, and deactivate event categories
- [ ] `/Admin/GuideVenues` — list, create, edit, reorder, and deactivate communal venues
- [ ] Attempting to delete a category or venue with associated events returns a user-visible error, not an exception
- [ ] All three pages are linked from the Admin nav section
- [ ] Only Admin role can access all three pages (403 for others)
- [ ] `dotnet build` passes with no errors

# Issue 01 — Domain Model, Enums, EF Config & Migration

**Phase:** 1 — Core Content Management
**Effort:** L
**Depends on:** —
**Blocks:** All other event guide issues

---

## Summary

Add the complete event guide domain model to Humans: entities, enums, EF Core configurations, and a single migration covering all new tables. This is the foundational issue; no other event guide work can proceed without it.

---

## New Entities

All entities follow the patterns established by shift management (feature 25). NodaTime for all temporal fields. Enums stored as strings.

### `GuideSettings`
Single record per event edition.

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `int` | PK |
| `EventSettingsId` | `int` | FK → `EventSettings.Id` (shared date/timezone context) |
| `SubmissionOpenAt` | `Instant` | When camps can start submitting |
| `SubmissionCloseAt` | `Instant` | When submission form closes |
| `GuidePublishAt` | `Instant` | When guide goes live to attendees |
| `MaxPrintSlots` | `int` | Max events in the printed programme |

### `EventCategory`
Lookup table for event categories.

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `int` | PK |
| `Name` | `string(60)` | Display name |
| `Slug` | `string(60)` | URL-safe identifier, unique |
| `IsSensitive` | `bool` | Triggers opt-out UI (Adult, Spiritual, etc.) |
| `DisplayOrder` | `int` | Sort order |
| `IsActive` | `bool` | Soft disable without deletion |

### `GuideCamp`
Links a Humans `Team` to a camp identity in the guide.

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `int` | PK |
| `TeamId` | `int` | FK → `Teams.Id`, unique |
| `CampName` | `string(120)` | May differ from team name |
| `Description` | `string(500)` | nullable |
| `GridAddress` | `string(60)` | nullable |
| `IsPublished` | `bool` | Controls visibility in guide |

### `GuideSharedVenue`
Admin-managed communal spaces (e.g. "Main Stage", "The Middle of Elsewhere").

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `int` | PK |
| `Name` | `string(120)` | |
| `Description` | `string(500)` | nullable |
| `LocationDescription` | `string(120)` | Grid address or text description |
| `IsActive` | `bool` | |
| `DisplayOrder` | `int` | |

### `GuideEvent`
A single event submission.

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `int` | PK |
| `GuideCampId` | `int?` | FK → `GuideCamps.Id`, nullable |
| `GuideSharedVenueId` | `int?` | FK → `GuideSharedVenues.Id`, nullable |
| `SubmitterUserId` | `int` | FK → `Users.Id` |
| `CategoryId` | `int` | FK → `EventCategories.Id` |
| `Title` | `string(80)` | |
| `Description` | `string(300)` | |
| `LocationNote` | `string(120)` | nullable, free-text detail within venue |
| `StartAt` | `Instant` | |
| `DurationMinutes` | `int` | |
| `IsRecurring` | `bool` | |
| `RecurrenceDays` | `string?` | Comma-separated day offsets from event start |
| `PriorityRank` | `int` | Submitter-assigned, used for print selection |
| `Status` | `GuideEventStatus` | Stored as string |
| `AdminNotes` | `string?` | Internal moderator notes |
| `SubmittedAt` | `Instant` | |
| `LastUpdatedAt` | `Instant` | |

Constraint: exactly one of `GuideCampId` / `GuideSharedVenueId` must be non-null (enforced in service layer).

### `ModerationAction`
Append-only audit log of moderation decisions.

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `int` | PK |
| `GuideEventId` | `int` | FK → `GuideEvents.Id` |
| `ActorUserId` | `int` | FK → `Users.Id` |
| `Action` | `ModerationActionType` | Stored as string |
| `Reason` | `string(500)` | Required for non-approval actions |
| `CreatedAt` | `Instant` | |

No UPDATE or DELETE on this table from application code.

### `UserGuidePreference`
Per-account guide preferences.

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `int` | PK |
| `UserId` | `int` | FK → `Users.Id`, unique |
| `ExcludedCategorySlugs` | `string` | JSON array of slugs |
| `UpdatedAt` | `Instant` | |

### `UserEventFavourite`
Links a user to a favourited event.

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `int` | PK |
| `UserId` | `int` | FK → `Users.Id` |
| `GuideEventId` | `int` | FK → `GuideEvents.Id` |
| `CreatedAt` | `Instant` | |

Unique constraint on `(UserId, GuideEventId)`.

---

## New Enums

**`GuideEventStatus`** (stored as string):
- `Draft` — saved but not submitted
- `Pending` — submitted, awaiting moderation
- `Approved` — published to guide
- `Rejected` — rejected, submitter notified
- `ResubmitRequested` — moderator requested edits

**`ModerationActionType`** (stored as string):
- `Approved`
- `Rejected`
- `ResubmitRequested`

---

## New Role Constant

Add `GuideModerator` to `RoleNames` constants. No new claims transformation needed — picked up by existing `RoleAssignmentClaimsTransformation`.

---

## EF Configuration

One configuration class per entity in `src/Humans.Infrastructure/Data/Configurations/`:
- Lowercase snake_case table names (`guide_settings`, `event_categories`, `guide_camps`, `guide_shared_venues`, `guide_events`, `moderation_actions`, `user_guide_preferences`, `user_event_favourites`)
- All string columns with explicit `HasMaxLength`
- All enum columns with `.HasConversion<string>()`
- Delete behaviors: `Restrict` on `GuideEvent → GuideCamp` and `GuideEvent → GuideSharedVenue` (don't cascade delete events when a venue/camp is removed)
- Index on `GuideEvent.Status`, `GuideEvent.GuideCampId`, `GuideEvent.SubmitterUserId`

---

## Migration

Single migration covering all 8 new tables. Name: `AddEventGuide`.

---

## Acceptance Criteria

- [ ] All 8 entities exist in `src/Humans.Domain/Entities/`
- [ ] All enums exist in `src/Humans.Domain/Enums/`
- [ ] `GuideModerator` added to `RoleNames`
- [ ] EF configurations exist for all entities
- [ ] `HumansDbContext` has DbSet properties for all new entities
- [ ] Migration `AddEventGuide` applies cleanly on a fresh database (`dotnet ef database update`)
- [ ] `dotnet build` passes with no errors

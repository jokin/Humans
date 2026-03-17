# Event Guide — Implementation Plan

**Feature:** 26-events | **Spec:** `docs/specs/event-guide-proposal-v1.md`

---

## Implementation Strategy

Vertical slices grouped by phase. Each issue is independently deployable and testable. The domain model and migration ship as a single foundational issue; all subsequent issues build on it.

### Dependency Graph

```
Issue 01 (Domain + Migration)
├── Issue 02 (Admin Settings UI)
├── Issue 03 (Camp Event Submission)
│   └── Issue 07 (Duplicate Detection) ─┐
├── Issue 04 (Individual Event Submission)│
│                                        ▼
├── Issue 05 (Moderation Queue) ◄────────┘
│   └── Issue 06 (Email Notifications)
│
├── Issue 08 (Published Events API)
│   ├── Issue 09 (Category Opt-Out)
│   └── Issue 10 (Favourites & Schedule)
│
└── Issue 11 (Print Guide & CSV Export)
    └── Issue 12 (Manager Dashboard)
```

### Phase 1 — Core Content Management (Issues 01–07)

Must ship before submission opens to camps.

| # | Issue | Depends On | Effort |
|---|-------|-----------|--------|
| 01 | Domain model, enums, EF config, migration | — | L |
| 02 | Admin UI: GuideSettings, categories, venues | 01 | M |
| 03 | Camp event submission (team page) | 01 | M |
| 04 | Individual event submission | 01, 02 (venues exist) | M |
| 05 | Moderation queue | 01 | L |
| 06 | Email notifications | 05 | S |
| 07 | Duplicate detection on moderation queue | 05 | S |

### Phase 2 — Published Guide & Personalisation (Issues 08–10)

Must ship before guide opens to attendees.

| # | Issue | Depends On | Effort |
|---|-------|-----------|--------|
| 08 | Public API (`/api/guide/...`) | 01 | M |
| 09 | Category opt-out (UserGuidePreference) | 08 | S |
| 10 | Account-backed favourites & personal schedule | 08 | M |

### Phase 3 — Exports & Coordination (Issues 11–12)

Target: 4 weeks before event.

| # | Issue | Depends On | Effort |
|---|-------|-----------|--------|
| 11 | Print guide PDF + CSV export | 01 | M |
| 12 | Manager dashboard | 01, 05 | M |

---

## Patterns to Follow

All implementations should follow the patterns established by shift management (feature 25):

- **Domain entities** in `src/Humans.Domain/Entities/` — NodaTime for all temporal fields, domain methods for state transitions
- **Enums** in `src/Humans.Domain/Enums/` — stored as strings via `.HasConversion<string>()`
- **EF configurations** in `src/Humans.Infrastructure/Data/Configurations/` — lowercase_snake table names, explicit delete behavior, max lengths on all strings
- **Service interfaces** in `src/Humans.Application/Interfaces/` — return result records with `Ok()`/`Fail()` factory methods
- **Service implementations** in `src/Humans.Infrastructure/Services/` — inject DbContext, IClock, ILogger; early validation with fail-fast returns
- **Controllers** in `src/Humans.Web/Controllers/` — inject services via constructor, TempData for post-redirect-get, role checks via `User.IsInRole()` + service-level auth helpers
- **Views** in `src/Humans.Web/Views/{Controller}/` — view models in `src/Humans.Web/Models/`
- **Emails** via existing `IEmailService` / `EmailOutboxMessage` + `ProcessEmailOutboxJob`
- **Auth** via `RoleAssignment` claims + controller-level `[Authorize]` + service-level permission checks

## Key Technical Decisions

1. **Single migration** for all new tables (GuideSettings through UserEventFavourite) — keeps the schema change atomic
2. **GuideEvent.Status** stored as string enum, state transitions enforced by domain methods with `IClock` parameter
3. **ModerationAction** is append-only — no update/delete; query latest action for current status display
4. **GuideCamp auto-creation** — when a Lead first submits a camp event and no GuideCamp exists for their team, create one automatically with team name as default camp name
5. **EventSettings reuse** — GuideSettings references the existing `EventSettings.Id` for shared date/timezone context (FK relationship)
6. **New role: `GuideModerator`** — added to `RoleNames` constants; checked in moderation queue controller. Admin also has full moderation access.
7. **No new claims transformation needed** — `GuideModerator` role stored in `RoleAssignment`, picked up by existing `RoleAssignmentClaimsTransformation`

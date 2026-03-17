# Issue 05 — Moderation Queue

**Phase:** 1 — Core Content Management
**Effort:** L
**Depends on:** Issue 01
**Blocks:** Issue 06 (email notifications), Issue 07 (duplicate detection), Issue 12 (manager dashboard)

---

## Summary

A dedicated moderation queue at `/EventGuide/Moderate` where volunteer moderators and admins review all pending event submissions. Moderators can approve, reject, or request edits. All decisions are recorded as `ModerationAction` entries (append-only).

---

## Access

- `GuideModerator` role and `Admin` role
- All other roles receive 403

---

## Queue Page — `/EventGuide/Moderate`

### Layout
Tabbed or filtered view:

| Tab / Filter | Contents |
|--------------|----------|
| **Pending** | All events with `Status = Pending`, ordered by `SubmittedAt` ascending (oldest first) |
| **Approved** | All events with `Status = Approved` |
| **Rejected** | All events with `Status = Rejected` |
| **Resubmit Requested** | All events with `Status = ResubmitRequested` |

Default tab: **Pending**.

### Event row
Each row shows:
- Title
- Submitter name (or camp name for camp events)
- Category
- Date/time and duration
- Submitted at
- Duplicate flag (⚠) if applicable (Issue 07 adds this — placeholder column for now)
- Action buttons: **Approve**, **Reject**, **Request Edit**

Clicking the title opens the event detail modal/panel.

---

## Event Detail Panel

Expandable panel or modal showing full event details:
- Title, description
- Category, date/time, duration, location
- Recurrence info (if recurring)
- Priority rank
- Submitter name and link to their profile
- Camp name and link to team page (if camp event)
- Submission history: list of previous `ModerationAction` entries (actor, action, reason, timestamp)

---

## Moderation Actions

### Approve
- Sets `GuideEvent.Status = Approved`
- Creates `ModerationAction` with `Action = Approved`, `ActorUserId = current user`, `CreatedAt = now`
- No reason required
- Shows success toast; removes event from Pending tab

### Reject
- Modal/inline form requiring a **reason** (required, ≤ 500 chars)
- Sets `GuideEvent.Status = Rejected`
- Creates `ModerationAction` with `Action = Rejected`, reason, actor, timestamp
- Triggers rejection email (Issue 06 — stub the call, email sending comes later)

### Request Edit
- Modal/inline form requiring a **reason** (required, ≤ 500 chars)
- Sets `GuideEvent.Status = ResubmitRequested`
- Creates `ModerationAction` with `Action = ResubmitRequested`, reason, actor, timestamp
- Triggers "resubmit requested" email (Issue 06 — stub the call)

---

## Service Interface

```csharp
// src/Humans.Application/Interfaces/IGuideEventModerationService.cs
public interface IGuideEventModerationService
{
    Task<ModerationResult> ApproveAsync(int eventId, int actorUserId);
    Task<ModerationResult> RejectAsync(int eventId, int actorUserId, string reason);
    Task<ModerationResult> RequestResubmitAsync(int eventId, int actorUserId, string reason);
    Task<IReadOnlyList<GuideEventSummary>> GetPendingAsync();
    Task<IReadOnlyList<GuideEventSummary>> GetByStatusAsync(GuideEventStatus status);
}

public record ModerationResult(bool Success, string? Error)
{
    public static ModerationResult Ok() => new(true, null);
    public static ModerationResult Fail(string error) => new(false, error);
}
```

---

## Nav Link

Add "Moderate Events" to the moderator/admin nav area. Link only visible to `GuideModerator` and `Admin` roles.

---

## Acceptance Criteria

- [ ] `/EventGuide/Moderate` accessible only to `GuideModerator` and `Admin`
- [ ] Pending tab lists all `Pending` events ordered oldest first
- [ ] Approve creates a `ModerationAction(Approved)` and updates event status
- [ ] Reject requires a reason, creates `ModerationAction(Rejected)`, updates event status
- [ ] Request Edit requires a reason, creates `ModerationAction(ResubmitRequested)`, updates event status
- [ ] Event detail panel shows full event info and full moderation history
- [ ] Once an action is taken, the event moves out of the Pending tab immediately (post-redirect-get)
- [ ] "Moderate Events" link visible to `GuideModerator` and `Admin` roles in nav
- [ ] `dotnet build` passes with no errors

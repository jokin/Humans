# Issue 07 — Duplicate Detection on Moderation Queue

**Phase:** 1 — Core Content Management
**Effort:** S
**Depends on:** Issue 05 (moderation queue UI)

---

## Summary

The moderation queue automatically flags submissions that appear to be duplicates of existing pending or approved events from the same camp in the same time slot. Detection is advisory — the moderator decides; the system does not auto-reject.

---

## Detection Logic

A `GuideEvent` is flagged as a **potential duplicate** if:
- It belongs to the same `GuideCamp` (same `GuideCampId`), AND
- Its time window overlaps with another `GuideEvent` that has `Status = Pending` or `Status = Approved`

Time overlap: `event A` and `event B` overlap if `A.StartAt < B.StartAt + B.Duration` AND `B.StartAt < A.StartAt + A.Duration`.

Individual events (no camp, `GuideCampId = null`) are **not** checked for duplicates — there is no shared identity to deduplicate against.

---

## UI Integration

In the moderation queue (Issue 05), add a **⚠ Duplicate** badge to any flagged event row. Clicking the badge expands a panel showing the conflicting event(s):
- Title, camp, date/time, status of each conflicting event
- Link to each conflicting event in the queue

No automatic action is taken. The moderator may approve both, reject one, or request an edit.

---

## Service Method

Add to `IGuideEventModerationService` (or a dedicated `IGuideEventDuplicateService`):

```csharp
Task<IReadOnlyList<GuideEventSummary>> GetDuplicateCandidatesAsync(int guideEventId);
```

Returns events that overlap in time with the same camp. Returns empty list if not a camp event or no overlaps found.

---

## Performance Note

At the scale of this system (~500 users, likely < 500 submitted events per edition), a simple in-memory check over all pending/approved events is acceptable. No need for a DB-level overlap query.

---

## Acceptance Criteria

- [ ] Camp events that overlap in time with another pending/approved event from the same camp are flagged with a ⚠ badge in the moderation queue
- [ ] Clicking the badge shows the conflicting event(s) with title, time, and status
- [ ] Individual events (no camp) are never flagged
- [ ] Events with no time overlap are not flagged
- [ ] Flagging is advisory only — moderator can still approve or reject independently
- [ ] `dotnet build` passes with no errors

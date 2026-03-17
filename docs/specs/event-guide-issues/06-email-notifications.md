# Issue 06 — Email Notifications

**Phase:** 1 — Core Content Management
**Effort:** S
**Depends on:** Issue 05 (moderation queue triggers notifications)

---

## Summary

Email notifications sent to event submitters at key points in the moderation workflow. Reuses the existing `IEmailService` / `EmailOutboxMessage` + `ProcessEmailOutboxJob` infrastructure.

---

## Notification Triggers

| Trigger | Recipient | Subject |
|---------|-----------|---------|
| Event submitted | Submitter | "Your event submission has been received" |
| Event approved | Submitter | "Your event has been approved" |
| Event rejected | Submitter | "Your event submission was not approved" |
| Resubmit requested | Submitter | "Changes requested for your event submission" |

---

## Email Content

### Submission received
> Your event **{title}** has been received and is now in the moderation queue. You will be notified once it has been reviewed.
>
> View your submissions: {link to MySubmissions or team Events tab}

### Approved
> Your event **{title}** has been approved and will appear in the event guide.
>
> View the event guide: {link to guide}

### Rejected
> Your event **{title}** was not approved for the event guide.
>
> **Reason:** {reason from ModerationAction}
>
> You can edit and resubmit your event here: {edit link}

### Resubmit requested
> The moderation team has requested changes to your event **{title}** before it can be approved.
>
> **Feedback:** {reason from ModerationAction}
>
> Please update and resubmit here: {edit link}

---

## Implementation Notes

- Queue all emails via `EmailOutboxMessage` — do not send synchronously
- The edit link for camp events points to `/Teams/{teamId}/Events/{eventId}/Edit`
- The edit link for individual events points to `/EventGuide/Submit/{eventId}/Edit`
- Use the existing email template/layout for consistent styling
- The submission-received email is triggered in the submission service (Issues 03 and 04)
- Approved/Rejected/ResubmitRequested emails are triggered in `IGuideEventModerationService` implementations (Issue 05)

---

## Acceptance Criteria

- [ ] Submitting a camp event queues a "submission received" email to the submitter
- [ ] Submitting an individual event queues a "submission received" email
- [ ] Approving an event queues an "approved" email to the submitter
- [ ] Rejecting an event queues a "rejected" email with the moderator's reason
- [ ] Requesting edits queues a "resubmit requested" email with the moderator's feedback
- [ ] All emails include the correct edit/view link for the event type
- [ ] Emails are queued via `EmailOutboxMessage` (not sent synchronously)
- [ ] `dotnet build` passes with no errors

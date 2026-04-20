# Teams

## What this section is for

Teams are how humans organize around a shared purpose. A team is either a **department** (top-level, like Build or Kitchen) or a **sub-team** that lives under exactly one department. Each team can have coordinators, named role slots, and optionally a `@nobodies.team` Google Group plus a linked Shared Drive folder.

A few teams are **system teams** (Volunteers, Coordinators, Board, Asociados, Colaboradores) — the app manages those automatically, so you cannot join or leave them by hand. Some teams are also **hidden**: privacy-sensitive groupings visible only to admins.

## Key pages at a glance

- **Teams directory** (`/Teams`) — landing page; your teams, other departments, system teams. Anonymous visitors see only teams with a public page.
- **Team detail** (`/Teams/{slug}`) — description, members, coordinators, sub-teams, join/leave.
- **My teams** (`/Teams/My`) — your teams with Leave / Manage buttons.
- **Birthdays** (`/Teams/Birthdays`) — teammate birthday calendar (month + day).
- **Roster** (`/Teams/Roster`) — cross-team view of named role slots.
- **Members admin** (`/Teams/{slug}/Members`) — members, join requests, role assignments.
- **Edit team page** (`/Teams/{slug}/EditPage`) — markdown and calls-to-action for a department's public page.
- **Roles** (`/Teams/{slug}/Roles`) — define named role slots.
- **Summary**, **Create**, **Edit**, **Sync** — admin pages at `/Teams/Summary`, `/Teams/Create`, `/Teams/{id}/Edit`, `/Teams/Sync`.

## As a Volunteer

### See the teams you're in

Go to `/Teams/My`. Each card shows your role (Member or Coordinator) and a Leave button for user-created teams. System teams appear here too but have no Leave button.

### Browse and discover teams

Open `/Teams`. "My Teams" sits at the top, "Other Teams" below with pagination. Each card shows name, short description, member count, and whether it requires approval. Click through to the team detail page for the full description, coordinators, and (for departments) the public page content.

![TODO: screenshot — Teams directory showing "My Teams" at top and "Other Teams" cards below with member counts and approval badges]

### Join a team

On the team detail page, click **Join**. Open teams add you immediately and grant Google Group / Drive access on the next sync. Teams that require approval create a pending join request (with an optional message); you can withdraw it any time. You cannot have two pending requests to the same team.

### Leave a team

From `/Teams/My` or the team detail page, click **Leave**. Your membership is soft-removed and Google access is revoked on the next sync. You cannot leave system teams.

### See teammates

On a team detail page you can see other active humans on the team. Basic profile info follows the normal [Profiles](Profiles.md) rules.

## As a Coordinator

(assumes Volunteer knowledge)

A **Coordinator** is a human assigned to a department's management role, with full authority over the department and every sub-team under it. **Sub-team managers** have the same tools scoped to a single sub-team — no access to the parent department, sibling sub-teams, or Google resources. See [Glossary](Glossary.md#coordinator).

### Manage members and join requests

Open `/Teams/{slug}/Members`. Approve or reject pending join requests (with optional review notes), add existing humans directly, or remove members. Removing a member also removes their role assignments on that team; all changes are audit-logged.

### Edit the team's public page

For departments, go to `/Teams/{slug}/EditPage`. Write the body in markdown, toggle public visibility, and configure up to three call-to-action buttons (text + URL + Primary or Secondary style; only one may be Primary). Sub-teams and system teams cannot have public pages.

### Manage role slots

At `/Teams/{slug}/Roles` you define named roles (e.g., "Social Media Lead"), set slot counts and priority, and mark one role per team as the management role. Assigning a human to a role auto-adds them to the team.

### Manage Google Group and Drive (departments only)

Department coordinators manage the linked Google Group membership and Shared Drive folder permissions. Sub-team managers cannot — Google resources live at the department level and roll up automatically. See [GoogleIntegration](GoogleIntegration.md).

## As a Board member / Admin

(assumes Coordinator knowledge)

### Create a team

`/Teams/Create` (TeamsAdmin, Board, Admin). Set name, description, approval mode, optional parent department, optional Google Group prefix, and the hidden flag. The slug is auto-generated and can be overridden on edit.

### Edit team settings

`/Teams/{id}/Edit` lets you change name, slug, approval mode, parent, Google Group prefix, directory promotion (for sub-teams), and `IsHidden`. Making a department into a sub-team re-scopes its coordinators to managers and syncs them out of the Coordinators system team.

### Delete a team

**Board** and **Admin** can delete (deactivate) user-created teams. System teams cannot be deleted.

### Hidden teams

Toggle `IsHidden` on create or edit. Hidden teams do not appear in the directory, profile cards, birthday lists, or "My Teams" for non-admins; campaigns can still target them by ID.

### System team sync

Admins view sync status at `/Teams/Sync` and (Admin only) run immediate syncs. The hourly `SystemTeamSyncJob` keeps Volunteers, Coordinators, and Board membership aligned with role assignments and consent status.

## Related sections

- [Profiles](Profiles.md) — team membership feeds profile cards and visibility.
- [Shifts](Shifts.md) — rotas are owned by a department or sub-team; coordinator/manager status drives shift management.
- [Camps](Camps.md) — camps are team-owned; team membership feeds camp planning.
- [GoogleIntegration](GoogleIntegration.md) — how team membership maps to Google Groups and Shared Drive folders.
- [Governance](Governance.md) — Board and tier roles feed the Board and Colaborador/Asociado system teams.
- [Glossary](Glossary.md#coordinator) — coordinator, manager, department, sub-team, system team definitions.

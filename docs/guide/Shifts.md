# Shifts

## What this section is for

Shifts is where the org schedules work slots and where humans sign up for them. The system covers the full arc of an event: Set-up (build), Event, and Strike. Set-up and Strike use all-day shifts you can book as a date range; Event shifts are time-slotted.

Shifts are team-owned. Every shift belongs to a **rota** (a named container), and every rota belongs to a department or sub-team. That ownership drives visibility and management — department coordinators run shifts for their department, and a rota can be hidden from humans until a coordinator is ready to open it.

The section also captures the data that makes scheduling work: your shift preferences (skills, work style, languages) and per-event profile info.

![TODO: screenshot — `/Shifts` browse page showing filters and a mix of Event and Build rotas]

## Key pages at a glance

| Page | Path | What it's for |
|---|---|---|
| Browse shifts | `/Shifts` | Find and sign up for shifts across all departments |
| My shifts | `/Shifts/Mine` | See your upcoming, pending, and past signups; bail if needed |
| Shift preferences wizard | `/Profile/ShiftInfo` | Tell coordinators about your skills, work style, and languages |
| Team shift admin | `/Teams/{slug}/Shifts` | Coordinators: manage rotas and shifts for a department |
| Event settings | `/Shifts/Settings` | Admin: configure event dates, timezone, early-entry capacity, and the global browsing toggle |

Your dashboard also surfaces shift info — upcoming signups, or a guided discovery card with urgent understaffed shifts if you have none.

## As a Volunteer

**Set your preferences first.** At `/Profile/ShiftInfo`, walk through the three-step wizard: skills (Bartending, Cooking, Sound, First Aid, Driving, etc.), work style (Early Bird / Night Owl / All Day / No Preference, plus toggles like Sober Shift, Work In Shade, No Heights), and languages. Change any of it later. Coordinators use this to match you with shifts that fit.

**Browse shifts at `/Shifts`.** Filter by department, date range, and period (Set-up / Event / Strike). Tag filters live above the list — click a tag like "Heavy lifting" or "Meeting new people" to narrow the view. Open the preferences panel to save tag preferences so matching shifts are highlighted with a star. Consecutive all-day Set-up and Strike shifts in the same rota are compressed into date ranges; click to expand individual days.

**Sign up.** For Event shifts, pick time slots — the system flags overlaps. For Set-up and Strike, pick a date range and the system creates signups for every day in that block. If the rota's policy is **Public**, you're auto-confirmed. If **RequireApproval**, your signup goes in as **Pending** and a coordinator will approve or refuse it (email either way).

**See your shifts at `/Shifts/Mine`.** Signups are grouped into upcoming, pending, and past. To cancel, use **Bail** — on a single shift or a whole range you booked together. Once early entry closes, non-privileged humans can't bail a Set-up shift without a coordinator's help.

![TODO: screenshot — `/Shifts/Mine` showing upcoming, pending, and past sections]

## As a Coordinator

If you're a department coordinator or sub-team manager, `/Teams/{slug}/Shifts` is your home. Sub-team managers only manage their own sub-team's rotas; department coordinators see everything in the department.

**Create a rota.** Give it a name, period (Build, Event, or Strike), priority (Normal / Important / Essential — this feeds urgency scoring on the homepage), signup policy (Public for auto-confirm, RequireApproval for review), and any practical info humans should read first. Rotas start visible by default; toggle **IsVisibleToVolunteers** off to stage them. Tag rotas with shared labels (Heavy lifting, Working in the shade, etc.) — reuse existing tags where you can.

**Create shifts.** Build and Strike rotas get all-day shifts — use the bulk creator to generate one per day offset. Event rotas get time-slotted shifts — the generator produces the Cartesian product of day offsets and time slots. Add individual shifts by hand with day offset, start time, duration, and min/max volunteers. Mark a shift **AdminOnly** to hide it from regular humans.

**Manage signups.** The admin page shows a "Signed Up" column with names (Event rotas) or avatars (Build/Strike rotas). Pending signups have their own table — approve, refuse (with an optional reason), or handle a date-range block at once. **Voluntell** to enroll a specific human directly (auto-confirmed; emails them). **Remove** unassigns a confirmed signup. After a shift ends you can **mark no-show**, recorded on the human's profile for coordinators.

**Move a rota** if it lands under the wrong department — shifts and signups come along, and the move is audit-logged. Deleting a rota or shift is blocked if there are confirmed signups; bail or remove them first.

## As a Board member / Admin

Admins get the site-wide shift view and can approve, refuse, bail, or voluntell across any department. **NoInfoAdmin** has the same signup powers but can't create or edit rotas and shifts. **VolunteerCoordinator** has full coordinator capabilities across every department.

Admin-only controls:

- **Event settings** at `/Shifts/Settings` — gate opening date, build/event/strike offsets, timezone, early-entry capacity, barrios allocation, early-entry close instant, and the **shift browsing toggle** (when closed, regular humans only see shifts they already have signups for; privileged roles can always browse).
- **Medical data** on volunteer event profiles is visible only to Admin and NoInfoAdmin. Coordinators and VolunteerCoordinator see skills and dietary info but not medical.
- **Early-entry freeze** — after early-entry close, non-privileged humans can't sign up for or bail from Set-up shifts; admins can still adjust.

Only one active Event Settings exists at a time. Changes ripple through every shift date on the site.

![TODO: screenshot — `/Shifts/Settings` showing gate opening date, build/event/strike offsets, early-entry capacity, and the shift browsing toggle]

## Related sections

- [Teams](Teams.md) — rotas belong to departments and sub-teams; coordinator status on a team unlocks shift management.
- [Profiles](Profiles.md) — your volunteer event profile (skills, dietary, languages) feeds shift matching; no-show history shows on your profile to coordinators.
- [Camps](Camps.md) — camp timing and early-entry windows share the event settings configured here.

# Camps

## What this section is for

Camps (also called "[barrios](Glossary.md#barrio)") are self-organizing themed communities that register each year to participate in the event. Each camp has a unique URL slug, one or more leads, optional images, and a per-year **[Camp Season](Glossary.md#camp-season)** capturing that year's name, description, vibes, space needs, and placement details.

Camp Admins control which year is shown publicly and which seasons are open for new registrations or opt-ins. The directory is public; registering or leading a camp requires an account.

![TODO: screenshot — Camps directory page showing camp list and filters]

## Key pages at a glance

- **Camps directory** (`/Camps`) — public listing of all camps for the current year.
- **Camp detail** (`/Camps/{slug}`) — public detail page for a single camp with description, images, and current season.
- **Camp detail for a specific year** (`/Camps/{slug}/Season/{year}`) — public detail for a camp's past season.
- **Register a new camp** (`/Camps/Register`) — authenticated humans register a new camp when a season is open.
- **Edit a camp** (`/Camps/{slug}/Edit`) — camp leads, Camp Admin, and Admin edit camp identity and season data.
- **Camps admin dashboard** (`/Camps/Admin`) — Camp Admin and Admin review pending seasons, manage registration windows, and export data.

A public JSON API at `/api/camps/{year}` and `/api/camps/{year}/placement` is available for integrating listings into other sites.

## As a [Volunteer](Glossary.md#volunteer)

Most of what the directory offers is open to anyone, signed in or not:

- **Browse the directory** at [/Camps](/Camps). Cards show each camp's name, short blurb, image, vibes, and status badges. Filter by vibe, [sound zone](Glossary.md#sound-zone), kids-friendliness, and whether the camp is accepting humans.
- **View a camp's detail page** at `/Camps/{slug}` for the long description, images, links, current season info, and leads by display name. Previous names appear unless the camp has chosen to hide them.
- **Contact a camp** via the "Contact this camp" button on the detail page. The camp's email is never exposed publicly — the button opens a facilitated form. Signing in is required so the camp knows who reached out.
- **Register a new camp** at [/Camps/Register](/Camps/Register) when a season is open. You'll fill in the camp's identity (name, contact info, times at the event, Swiss camp flag) and season-specific details (blurb, languages, vibes, kids policy, sound zone, performance space, space requirement). On submit you become the [Primary Lead](Glossary.md#primary-lead) and the season is created in **Pending** status, waiting on Camp Admin approval.
- **View previous seasons** at `/Camps/{slug}/Season/{year}` to see how a camp described itself in earlier years.

There is no "join a camp" action in the app — camp rosters and who is physically camping where are managed by each camp directly.

## As a [Coordinator](Glossary.md#coordinator) (Camp Coordinator)

If you are a **Camp Lead** (Primary or Co-Lead), you can manage your specific camp. You cannot edit camps you don't lead.

- **Edit your camp** at `/Camps/{slug}/Edit`. Update contact info, links, the current season's data (blurb, vibes, kids policy, space and sound needs, performance info), and camp-level fields like times at the event and the Swiss camp flag. Toggle **Hide historical names** to suppress the "Also known as" section on the public page.
- **Manage names.** If a Camp Admin has set a name lock date for the year, name changes are blocked after that date. Any rename is automatically recorded as a historical name.
- **Manage co-leads** from the Edit page: add a co-lead, remove a lead, or transfer the Primary role.
- **Upload, delete, and reorder images** from the Edit page. Images appear on the directory card and detail page in the order you set.
- **Opt into a new season** when Camp Admins open one. This creates a fresh Pending season carrying your camp's identity forward, but requires you to review and update the season-specific fields before approval.
- **Withdraw a season** if plans change, or **mark a season Full** when you're no longer accepting humans. Reactivating Withdrawn or Full back to Active is a Camp Admin action.

## As a Board member / Admin (Camp Admin)

**Camp Admin** is the domain admin for this section; **[Admin](Glossary.md#admin)** is a superset that can also delete a camp outright. Admin tools live under [/Camps/Admin](/Camps/Admin).

- **Review season registrations.** Pending seasons are listed on the dashboard. **Approve** moves a season to Active; **Reject** requires notes explaining why and records your user id and timestamp. Withdrawn and rejected seasons are visible to Camp Admin for history.
- **Reactivate seasons** that were marked Full or Withdrawn, moving them back to Active.
- **Open and close registration seasons** for any year. Opening a year adds it to the list accepting new registrations and opt-ins; closing removes it.
- **Set the public year** that controls which year is shown on `/Camps` and on the JSON API.
- **Set name lock dates** per year, after which camp name changes are no longer allowed for that year's season.
- **Edit any camp** (all the lead-level Edit actions above, on any camp).
- **Export camps as CSV** from the dashboard. The export covers every camp for the current public year and includes name, slug, status, contact info, leads, and placement-relevant season data. The file is named `barrios-{year}.csv`.
- **Delete a camp** (Admin only — not Camp Admin). This permanently removes the camp and all of its seasons, leads, images, and historical names. Confirmation is required.

## Related sections

- [Profiles](Profiles.md) — camp leads are linked to human accounts; a valid profile is required to be a lead.
- [Glossary](Glossary.md) — definitions for "barrio", "season", "Primary Lead", "sound zone", and other camp terms.

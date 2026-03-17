**Desert Event Guide**

Functional Features & User Stories Document

*Analysis & Documentation · March 2026*

+-----------------------------------+-----------------------------------+
| **Type of Application**           | **Context**                       |
|                                   |                                   |
| Progressive Web App (PWA) / SPA   | Desert community event guide      |
|                                   | (PWA)                             |
+===================================+===================================+
| **Offline Capability**            | **URL**                           |
|                                   |                                   |
| Yes --- works without internet    | \[redacted\]                      |
| on-site                           |                                   |
+-----------------------------------+-----------------------------------+

# **1. Overview**

This Desert Event Guide is a digital, offline-capable event guide
designed for large-scale temporary community gatherings in desert or
remote settings. It serves as the digital counterpart to a traditional
printed programme --- listing all scheduled events, theme camps, and
locations during the event.

The application is built as a Single Page Application (SPA) /
Progressive Web App (PWA), allowing attendees to browse events, search
by keyword, mark favourites, build personal schedules, and navigate
locations --- all without requiring an active internet connection once
loaded on-device.

# **2. Observed Site Structure & Navigation**

Based on analysis of the publicly accessible application routes and the
known conventions of digital event guide platforms of this type, the
application exposes the following primary sections:

  ------------------------------------------------------------------------
  **Route**          **Purpose**                    **Notes**
  ------------------ ------------------------------ ----------------------
  **/**              Landing / home screen          *Entry point, likely
                                                    event selector or
                                                    splash*

  **/main**          Main event listing             *Primary events
                                                    browser view*

  **/Locations**     Locations / map view           *Camp and art
                                                    installation
                                                    placement*

  **/#favourites**   Saved / favourited events      *Inferred from digital
                                                    guide conventions*

  **/#schedule**     Personal schedule              *Inferred from digital
                                                    guide conventions*

**/#search**       Search / filter                *Inferred from known
                                                    feature set*
  ------------------------------------------------------------------------

# **3. Feature Catalogue**

## **3.1 Event Browsing & Discovery**

  ------------------------------------------------------------------------
  **Feature**           **Description**                   **Priority**
  --------------------- --------------------------------- ----------------
  **Event Listing**     Full paginated list of all        **Must Have**
                        approved events with title, camp,
                        time, location and category.

  **Category Filter**   Filter events by type: workshop,  **Must Have**
                        performance, ritual, adult,
                        music, participatory, etc.

  **Day / Time Filter** Filter the event list by event    **Must Have**
                        day and time of day (morning,
                        afternoon, evening, night).

  **Keyword Search**    Free-text search across event     **Must Have**
                        titles and descriptions.

  **Event Detail View** Expanded view showing full        **Must Have**
                        description, camp name, exact
                        grid location, and recurrence.

**Recurring Events**  Events that repeat on multiple    **Should Have**
                        days are shown as a single entry  
                        with all dates listed.
  ------------------------------------------------------------------------

## **3.2 Locations View**

  ------------------------------------------------------------------------
  **Feature**           **Description**                   **Priority**
  --------------------- --------------------------------- ----------------
  **Camp Directory**    Browsable list of all theme camps **Must Have**
                        with name, description, and grid  
                        address.

  **Grid Address        Clock-based or grid-equivalent    **Must Have**
  Display**             addressing (e.g. \'3:00 &
                        Esplanade\') shown per camp and
                        per event.

  **Art Installations** Listing of standalone art pieces  **Should Have**
                        with placement and artist info.

  **Location Map**      Visual or schematic map of the    **Should Have**
                        event site with camp and art
                        placement pins.

**Camp → Events       From a camp\'s detail page, user  **Must Have**
  Link**                can see all events that camp is
                        hosting.
  ------------------------------------------------------------------------

## **3.3 Personalisation**

  ------------------------------------------------------------------------
  **Feature**           **Description**                   **Priority**
  --------------------- --------------------------------- ----------------
  **Favourite Events**  Tap/click a star or heart icon to **Must Have**
                        mark an event as a favourite for  
                        quick retrieval.

  **Personal Schedule** Auto-generated personal schedule  **Must Have**
                        view showing only favourited
                        events, sorted by day/time.

  **Schedule Export**   Export or print personal schedule **Should Have**
                        as PDF or print-friendly format
                        for offline reference.

**Conflict            Visual warning when two           **Nice to Have**
  Detection**           favourited events overlap in
                        time.
  ------------------------------------------------------------------------

## **3.4 Offline & PWA Capabilities**

  ------------------------------------------------------------------------
  **Feature**           **Description**                   **Priority**
  --------------------- --------------------------------- ----------------
  **Offline Mode**      Full app functionality available  **Must Have**
                        without internet once the PWA has
                        been loaded and cached.

  **Service Worker      All event, location, and asset    **Must Have**
  Cache**               data pre-cached for on-playa /
                        on-site use.

  **Install to Home     Browser prompt allows user to     **Should Have**
  Screen**              install the PWA on iOS or Android
                        home screen.

**Kiosk Mode**        Walk-up kiosk version --- full    **Nice to Have**
                        read-only guide accessible at a
                        shared on-site device without
                        login.
  ------------------------------------------------------------------------

## **3.5 Content Submission (Admin / Organiser Side)**

  ------------------------------------------------------------------------
  **Feature**           **Description**                   **Priority**
  --------------------- --------------------------------- ----------------
  **Event Submission    [As an atendee I want to hide     **Must Have**
  Form**                ceratin categories of events. To  
                        avoid browsing to things I wont
                        go like Spiritual. Party or
                        Adult]{.mark}

  **Duplicate           System flags or removes duplicate **Must Have**
  Detection**           submissions from the same camp
                        for the same slot.

  **Moderation Queue**  Volunteer moderators review all   **Must Have**
                        submissions for content
                        compliance before publication.

  **Edit & Resubmit**   Submitters receive email          **Must Have**
                        notification on rejection and can
                        edit and resubmit.

  **Priority Ordering** Submitters rank their events;     **Should Have**
                        system uses ordering to select
                        events for the printed guide.

**Print Guide         System generates a print-ready    **Must Have**
  Export**              PDF guide from approved events
                        for physical distribution.
  ------------------------------------------------------------------------

# **4. User Stories**

## **4.1 Attendee --- Event Discovery**

  --------------------------------------------------------------------------
  **ID**      **User Story**                  **Acceptance Criteria**
  ----------- ------------------------------- ------------------------------
  **US-01**   As an attendee, I want to       *Events are grouped or
              browse all events by day so     filterable by day. Default
              that I can plan what to attend  view shows current event day.*
              each day of the event.

  **US-02**   As an attendee, I want to       *Category chips/tags are
              filter events by category       visible. Selecting one filters
              (workshop, music, adult, etc.)  the list immediately.*
              so I can find things relevant
              to my interests.

  **US-03**   As an attendee, I want to       *Search input returns matching
              search for events by keyword so results across title and
              that I can find a specific      description in \<1 second.*
              workshop I heard about.

  **US-04**   As an attendee, I want to see   *Event detail shows camp name,
              the full details of an event    grid address, time, duration,
              including exact location so I   and full description.*
              can actually find it on site.

  **US-05**   As an attendee, I want to see   *Camp page shows all
              all events hosted by a specific associated events in
              camp so I can plan to visit     chronological order.*
              that camp.

              [As an atendee I want to hide   
              ceratin categories of events.   
              To avoid browsing to things I   
              wont go like Spiritual. Party   
              or Adult]{.mark}                
  --------------------------------------------------------------------------

## **4.2 Attendee --- Personalisation & Planning**

  --------------------------------------------------------------------------
  **ID**      **User Story**                  **Acceptance Criteria**
  ----------- ------------------------------- ------------------------------
  **US-06**   As an attendee, I want to       *Favourite icon on each event.
              favourite events so that I can  Favourites page shows all
              quickly retrieve the ones I     saved events.*
              don\'t want to miss.

  **US-07**   As an attendee, I want to see   *Schedule tab shows favourites
              my favourited events in a       sorted chronologically by day
              personal schedule view sorted   and start time.*
              by time so I can follow my plan
              on the day.

  **US-08**   As an attendee, I want to       *Export button generates a
              export my personal schedule as  clean, print-ready PDF with
              a PDF so I can share it with    user\'s favourites.*
              campmates or print it.

**US-09**   As an attendee, I want to be    *Conflict indicator appears on
              warned if two of my favourite   the schedule when events share
              events overlap so I know I need overlapping times.*
              to choose between them.
  --------------------------------------------------------------------------

## **4.3 Attendee --- Offline & On-site Use**

  --------------------------------------------------------------------------
  **ID**      **User Story**                  **Acceptance Criteria**
  ----------- ------------------------------- ------------------------------
  **US-10**   As an attendee at a remote      *All data accessible offline.
              event with no connectivity, I   No spinner or network errors
              want the guide to work without  when offline.*
              internet so I\'m not stranded
              without information.

  **US-11**   As an attendee using a mobile   *PWA install prompt appears.
              phone, I want to install the    App launches in standalone
              guide to my home screen so I    mode from home screen.*
              can open it quickly without
              searching for the URL.

  **US-12**   As an attendee who doesn\'t     *Kiosk device shows full guide
              have their phone, I want to use in read-only mode without
              a shared kiosk to look up       requiring login.*
              events and locations.

              As an atendee I want to hide    
              ceratin categories of events.   
              To avoid browsing to things I   
              wont go like Spiritual. Party   
              or Adult                        
  --------------------------------------------------------------------------

## **4.4** **Camp Organiser --- Event Submission**

  --------------------------------------------------------------------------
  **ID**      **User Story**                  **Acceptance Criteria**
  ----------- ------------------------------- ------------------------------
  **US-13**   As a camp organiser, I want to  *Form accepts title (≤40
              submit my camp\'s events via a  chars), description (≤80
              form so they appear in the      chars), date, time, duration,
              digital and print guide.        location, category.*

  **US-14**   As a camp organiser, I want to  *Drag-to-reorder or numbered
              rank my events in priority      priority field on submission
              order so that the most          form.*
              important ones are selected for
              the limited-space print guide.  

  **US-15**   As a camp organiser, I want to  *Rejection email sent within
              be notified if my submission is 24h with reason. Edit and
              rejected so I can fix and       resubmit link included.*
              resubmit it.

**US-16**   As a camp organiser, I want to  *\'Repeating event\' option on
              submit recurring events once    form. Single entry displays
              rather than creating duplicate  all recurrence dates.*
              entries for each day.
  --------------------------------------------------------------------------

## **4.5 Volunteer Moderator --- Content Review**

  --------------------------------------------------------------------------
  **ID**      **User Story**                  **Acceptance Criteria**
  ----------- ------------------------------- ------------------------------
  **US-17**   As a moderator, I want to       *Moderation UI lists pending
              review all submitted events in  events with approve / reject /
              a queue so I can approve or     request-edit actions.*
              reject them for publication.

  **US-18**   As a moderator, I want the      *Duplicate detection
              system to flag potential        highlights submissions with
              duplicates automatically so I   same camp + same time slot.*
              can resolve them quickly.

**US-19**   As a moderator, I want to       *Print export generates
              export the approved event list  paginated, formatted PDF
              for the print guide so I can    sorted by day and time.*
              hand it off for layout.
  --------------------------------------------------------------------------

# **5. Non-Functional Requirements**

### **Performance**

- Initial load time under 3 seconds on a 3G mobile connection.

- Full offline cache established after first load.

- Search results returned in under 500ms.

### **Accessibility**

- Minimum contrast ratio 4.5:1 for body text (WCAG AA).

- All interactive elements keyboard-navigable.

- Screen reader compatible event listings.

### **Usability**

- Usable in bright sunlight on a phone screen --- high contrast colour
    > scheme.

- All core functions reachable in ≤3 taps from the home screen.

- No account / login required for read-only access.

### **Data & Privacy**

- Favourites and personal schedule stored locally in browser
    > (localStorage / IndexedDB).

- No PII collected from attendees for read-only usage.

- Submission form collects only camp name, contact email, and event
    > details.

# **6. Open Questions & Gaps**

The following items could not be fully determined from external analysis
and should be clarified with the site owners:

- Authentication: Does the submission form require a login or is it
    > open to all participants?

- API / Data source: Is event data pulled from a backend API or
    > statically built at deploy time?

- Map implementation: Is the /Locations view an interactive map (e.g.
    > Leaflet / Mapbox) or a static schematic image?

- Multi-event support: Does the guide serve a single event or multiple
    > regional events from one domain?

- Update mechanism: How are last-minute event additions or changes
    > pushed to users who have already cached the PWA offline?

- Print guide integration: Is the PDF print guide generated by this
    > same application or by a separate system?

*Document prepared by functional analysis of the Desert Event Guide
application · March 2026*

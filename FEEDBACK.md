# Flowie - Product Requirements Document (PRD)

> Based on user feedback session. Organized by priority, each with a plan and prioritization.

---

## Priority Legend

| Priority | Label | Meaning |
|----------|-------|---------|
| P0 | Critical | Core UX issues, blocking daily use |
| P1 | High | High-value features users actively miss |
| P2 | Medium | Nice-to-have improvements that add real value |
| P3 | Low | Future considerations, exploratory |

---

## Table of Contents

### P0 - Critical
1. [Session - Prevent Auto-Logout on Inactivity](#1-session---prevent-auto-logout-on-inactivity)
2. [Tasks - Make Employee Assignment Optional](#2-tasks---make-employee-assignment-optional)
3. [Tasks - Make Deadline Optional (Keep Visible)](#3-tasks---make-deadline-optional-keep-visible)

### P1 - High
4. [Projects - "Algemeen" (General) Tab](#4-projects---algemeen-general-tab)
5. [Projects - Company Label in "Alles" Tab](#5-projects---company-label-in-alles-tab)
6. [Tasks - Show Creation Date](#6-tasks---show-creation-date)
7. [Tasks - "Wachten Op" (Waiting On) Status](#7-tasks---wachten-op-waiting-on-status)
8. [Tasks - Drag & Drop Reordering](#8-tasks---drag--drop-reordering)
9. [Integration - Calendar Sync (Deadlines)](#9-integration---calendar-sync-deadlines)

### P2 - Medium
10. [Tasks - Rich Description (List/Bullet Support)](#10-tasks---rich-description-listbullet-support)
11. [Tasks - Priority Levels](#11-tasks---priority-levels)

### P3 - Low
12. [Dashboard - Content & Widgets](#12-dashboard---content--widgets)
13. [Dashboard - Private/Personal Tab (Role-based Visibility)](#13-dashboard---privatepersonal-tab-role-based-visibility)
14. [Dashboard - Theme Color Customization](#14-dashboard---theme-color-customization)
15. [Projects - Hide "Medewerker" Field (Defer to Inkoper Role)](#15-projects---hide-medewerker-field-defer-to-inkoper-role)
16. [Tasks - Reconsider Task/Subtask Hierarchy](#16-tasks---reconsider-tasksubtask-hierarchy)
17. [Tasks - Profile Photo on Assignee](#17-tasks---profile-photo-on-assignee)
18. [Completed Tasks - Keep & Search](#18-completed-tasks---keep--search)
19. [Attachments - File Upload (Magic Map)](#19-attachments---file-upload-magic-map)
20. [Attachments - Image Upload](#20-attachments---image-upload)
21. [Integration - Whise Koppeling](#21-integration---whise-koppeling)
22. [Integration - Email Linking](#22-integration---email-linking)
23. [Notifications - Pop-up Alerts](#23-notifications---pop-up-alerts)

---

# P0 - Critical

---


## 1. Session - Prevent Auto-Logout on Inactivity

- [x] Implemented
- [x] User Tested

**Priority: P0 - Critical**
**Area:** Auth, Session
**Feedback:** "Als je te lang inactief wordt sluit de browser vanzelf af, niet makkelijk want dat staat de hele dag open bij mij."

### Problem
The application logs users out or becomes unresponsive after a period of inactivity. Users keep the app open all day and expect it to remain active.

### Requirements
- Application should stay active/logged in for extended periods
- Silent token refresh in the background
- No forced logout during business hours unless explicitly logging out
- Token should last for a full work day (8 hours)

### Plan
1. Review current JWT expiry and refresh token settings
2. Implement automatic silent token refresh before expiry (e.g., refresh 5 minutes before JWT expires)
3. Extend refresh token lifetime (e.g., 30 days instead of 7)
4. Add a periodic keep-alive ping from the frontend
5. Handle token refresh failures gracefully (show re-login prompt instead of hard logout)

### Acceptance Criteria
- [ ] App stays logged in for at least a full workday without interaction
- [ ] Token refreshes automatically in the background
- [ ] No abrupt session termination during normal use

---


## 2. Tasks - Make Employee Assignment Optional

- [x] Implemented
- [x] User Tested

**Priority: P0 - Critical**
**Area:** Tasks
**Feedback:** "Medewerker verplicht kan, maar hoeft niet perse."

### Problem
Employee assignment is currently required (`EmployeeId` is non-nullable). Users want to create tasks without immediately assigning someone.

### Requirements
- Make `EmployeeId` nullable on tasks
- Update validation to allow null
- Update UI to show "Niet toegewezen" (Unassigned) when empty

### Plan
1. Make `EmployeeId` nullable in the `Task` entity
2. Update FluentValidation rules (remove Required on EmployeeId)
3. Database migration
4. Update frontend form to allow empty employee selection
5. Update task display to handle unassigned state

### Acceptance Criteria
- [x] Tasks can be created without selecting an employee
- [x] Unassigned tasks display clearly in the UI
- [x] Existing assigned tasks are unaffected

---


## 3. Tasks - Make Deadline Optional (Keep Visible)

- [x] Implemented
- [x] User Tested

**Priority: P0 - Critical**
**Area:** Tasks
**Feedback:** "Deadline ook niet verplicht, moet wel blijven staan."

### Problem
Deadline (`DueDate`) may be required in forms. User wants it optional but still visible when set.

### Requirements
- `DueDate` should be optional (nullable) when creating/editing tasks
- When a deadline is set, it should be prominently displayed
- When no deadline is set, show "Geen deadline" or leave blank

### Plan
1. Verify `DueDate` is already nullable in the model (it's `DateOnly` - check if nullable)
2. Update validation to not require DueDate
3. Update frontend form to allow empty date
4. Update task card display to handle missing deadline gracefully

### Acceptance Criteria
- [x] Tasks can be created without a deadline
- [x] Deadline is visible when set
- [x] No validation error when deadline is omitted

---

# P1 - High

---


## 4. Projects - "Algemeen" (General) Tab

- [x] Implemented
- [x] User Tested

**Priority: P1 - High**
**Area:** Projects
**Feedback:** "Kan er nog een TABBLAD algemeen komen?"

### Problem
Projects are currently split by company (Immoseed / Novara). There's no place for tasks/projects that don't belong to either company.

### Requirements
- Add a third company/category option: "Algemeen" (General)
- Add corresponding tab in the project filter bar
- "Alles" tab still shows everything including Algemeen

### Plan
1. Add `General` (or `Algemeen`) value to the `Company` enum in the backend
2. Update frontend filter tabs: Alles | Immoseed | Novara | Algemeen
3. Update project creation form to allow selecting Algemeen

### Acceptance Criteria
- [x] New "Algemeen" tab appears in project list
- [x] Projects can be created under "Algemeen"
- [x] "Alles" tab shows all three categories

---


## 5. Projects - Company Label in "Alles" Tab

- [ ] Implemented
- [ ] User Tested

**Priority: P1 - High**
**Area:** Projects
**Feedback:** "In het tabblad 'alles' toch alles kan zien samen en dat er nog eens bijstaat NOVARA OF IMMOSEED."

### Problem
When viewing all projects together, there's no visual indicator of which company each project belongs to.

### Requirements
- Show a company badge/label on each project card in the "Alles" view
- Badge should display "Immoseed", "Novara", or "Algemeen"

### Plan
1. Add a company badge component (colored chip/tag)
2. Display it on the project card/row in the "Alles" tab
3. Use distinct colors per company for quick scanning

### Acceptance Criteria
- [ ] Each project in "Alles" view shows its company name as a visible label
- [ ] Labels are color-coded per company

---


## 6. Tasks - Show Creation Date

- [x] Implemented
- [x] User Tested

**Priority: P1 - High**
**Area:** Tasks
**Feedback:** "Datum van aanmaak tonen, handig om te zien hoelang we bezig zijn met een project van ingave tot voltooid zetten."

### Problem
No creation date is shown on tasks. Users want to see how long a task has been open to track turnaround time.

### Requirements
- Display `CreatedAt` date on each task
- Optionally show duration (e.g., "5 dagen geleden" or "Open sinds 12 jan 2026")
- Useful for tracking from creation to completion

### Plan
1. Ensure `CreatedAt` is already part of the Task entity (it inherits `AuditableEntity`)
2. Include `CreatedAt` in task API responses (DTOs)
3. Display creation date on task cards and detail views
4. Calculate and show elapsed time

### Acceptance Criteria
- [x] Creation date is visible on every task
- [x] Users can see at a glance how old a task is

---


## 7. Tasks - "Wachten Op" (Waiting On) Status

- [x] Implemented
- [x] User Tested

**Priority: P1 - High**
**Area:** Tasks
**Feedback:** "Handig zou zijn ook een status WACHTEN OP. BV als ik de aanvraag voor een attest heb gedaan, maar ik moet wachten tot die aangeleverd is."

### Problem
Current statuses are: Pending, Ongoing, Done. There's no way to indicate "action taken, but waiting on external party."

### Requirements
- Add a 4th status: `WaitingOn` / `WachtenOp`
- Visually distinct from other statuses (e.g., orange/yellow badge)
- Optional: allow adding a note about what/who you're waiting on

### Plan
1. Add `WaitingOn` (3) to the `TaskStatus` enum
2. Database migration (if enum is stored as int, just add the value)
3. Update backend status change logic
4. Add "Wachten Op" button/option in the frontend status selector
5. Style with a distinct color (e.g., orange)
6. Optional: add a `WaitingOnNote` field for context

### Acceptance Criteria
- [x] "Wachten Op" status is available in the task status options
- [x] Tasks with this status are visually distinct
- [x] Status flow works: Pending -> Ongoing -> Wachten Op -> Done (flexible)

---


## 8. Tasks - Drag & Drop Reordering

- [x] Implemented
- [x] User Tested

**Priority: P1 - High**
**Area:** Tasks
**Feedback:** "Mogelijkheid om de volgorde van de taken te verslepen." (mentioned twice)

### Problem
Tasks and subtasks are displayed in a fixed order. Users want to manually reorder them by dragging.

### Requirements
- Drag & drop to reorder tasks within a project
- Drag & drop to reorder subtasks within a parent task
- Persist the custom order

### Plan
1. Add a `SortOrder` (int) field to the `Task` entity
2. Database migration
3. API endpoint: `PATCH /api/tasks/reorder` accepting an ordered list of task IDs
4. Frontend: integrate Angular CDK DragDrop module
5. On drop, update sort order via API call
6. Default ordering by `SortOrder` ascending

### Acceptance Criteria
- [x] Users can drag tasks to reorder them
- [x] Order persists after page refresh
- [x] Subtasks can also be reordered within their parent

---


## 9. Integration - Calendar Sync (Deadlines)

- [ ] Implemented
- [ ] User Tested

**Priority: P1 - High**
**Area:** Integrations
**Feedback:** "Koppeling in je agenda met de deadlines."

### Problem
Task deadlines don't appear in users' calendars. They must manually track them.

### Requirements
- Export/sync task deadlines to external calendar (Outlook/Google)
- Auto-update when deadlines change

### Plan
1. iCal feed URL per user (subscribe in any calendar app)
2. Generate `.ics` feed endpoint: `GET /api/calendar/{userId}/feed.ics`

### Acceptance Criteria
- [ ] Users can subscribe to a calendar feed showing their task deadlines
- [ ] Calendar updates when tasks/deadlines change

---

# P2 - Medium

---


## 10. Tasks - Rich Description (List/Bullet Support)

- [ ] Implemented
- [ ] User Tested

**Priority: P2 - Medium**
**Area:** Tasks
**Feedback:** "Mogelijkheid om de omschrijving in lijstvorm te doen zodat het onder elkaar komt ipv naast elkaar."

### Problem
Task descriptions are plain text. When users enter multiple items, they appear inline rather than as a structured list.

### Requirements
- Support basic formatting in task descriptions (at minimum: bullet lists)
- Render description with line breaks and list formatting

### Plan
1. Add a simple markdown or rich-text editor (e.g., textarea with markdown preview, or a lightweight WYSIWYG)

### Acceptance Criteria
- [ ] Line breaks in descriptions are preserved and displayed
- [ ] Lists (bullet points) render vertically, not inline

---


## 11. Tasks - Priority Levels

- [ ] Implemented
- [ ] User Tested

**Priority: P2 - Medium**
**Area:** Tasks
**Feedback:** "Misschien level van prioriteit kunnen zetten."

### Problem
All tasks have equal visual weight. No way to flag urgent or high-priority items.

### Requirements
- Add priority levels to tasks (e.g., Low, Medium, High, Urgent)
- Visual indicator (color, icon) on task cards
- Optional: sort/filter by priority

### Plan
1. Create `TaskPriority` enum: `Low` (0), `Medium` (1), `High` (2), `Urgent` (3)
2. Add `Priority` field to `Task` entity (default: Medium)
3. Database migration
4. Update task creation/edit forms
5. Display priority badge on task cards
6. Allow filtering by priority

### Acceptance Criteria
- [ ] Tasks can be assigned a priority level
- [ ] Priority is visually indicated on task cards
- [ ] Default priority is Medium

---

# P3 - Low

---


## 12. Dashboard - Content & Widgets

- [ ] Implemented
- [ ] User Tested

**Priority: P3 - Low**
**Area:** Dashboard
**Feedback:** "Wat komt hier nog?" - Dashboard is currently empty.

### Problem
The dashboard shows only a title. Users have no at-a-glance overview of their work.

### Requirements
- Show summary widgets: open tasks assigned to me, overdue tasks, recently completed tasks
- Show project progress overview (tasks done / total per project)
- Quick-access links to active projects
- Optional: recent activity feed

### Plan
1. Design widget component system (reusable card components)
2. Create API endpoint `GET /api/dashboard` returning aggregated data for the current user
3. Build frontend widgets: My Tasks, Overdue, Project Progress, Recent Activity
4. Wire up to dashboard page

### Acceptance Criteria
- [ ] Dashboard shows at least 3 meaningful widgets on load
- [ ] Data is scoped to the logged-in user
- [ ] Responsive layout (mobile + desktop)

---


## 13. Dashboard - Private/Personal Tab (Role-based Visibility)

- [ ] Implemented
- [ ] User Tested

**Priority: P3 - Low**
**Area:** Dashboard, Auth/Roles
**Feedback:** "Is er een mogelijkheid dat er een tabblad komt die alleen ik kan zien, dus niet Peter."

### Problem
No concept of private views. All users see the same dashboard. User wants a personal tab (e.g., for tracking hours) invisible to other users.

### Requirements
- Personal tab on dashboard visible only to the tab owner
- Content could include: personal notes, hour tracking, private to-do list
- Other users cannot see or access another user's personal tab

### Plan
1. Add a `PersonalNote` or `UserWidget` entity linked to `UserId`
2. API endpoints scoped by authenticated user only
3. Frontend: add a "Persoonlijk" tab on the dashboard
4. Ensure API authorization prevents cross-user access

### Dependencies
- Requires a basic role/permission check or at minimum user-scoped data

### Acceptance Criteria
- [ ] Personal tab is only visible to the owning user
- [ ] User can add/edit personal content (notes, hours)
- [ ] No other user can view or access this data

---


## 14. Dashboard - Theme Color Customization

- [ ] Implemented
- [ ] User Tested

**Priority: P3 - Low**
**Area:** Dashboard, UI/UX
**Feedback:** "Kan het blauw in een andere kleur?"

### Problem
The app uses a fixed blue theme. User wants customization.

### Requirements
- Allow users to pick a primary theme color (or choose from presets)
- Persist preference per user

### Plan
1. Add `ThemeColor` field to User/Employee model
2. Create a settings page or theme picker in the navbar
3. Apply chosen color via CSS custom properties / Tailwind config
4. Store preference in backend, load on login

### Acceptance Criteria
- [ ] User can select from at least 4-5 color presets
- [ ] Selection persists across sessions
- [ ] All primary UI elements reflect the chosen color

---


## 15. Projects - Hide "Medewerker" Field (Defer to Inkoper Role)

- [ ] Implemented
- [ ] User Tested

**Priority: P3 - Low**
**Area:** Projects
**Feedback:** "Medewerker moet niet verplicht zijn - kan in een later stadium wel gebruikt worden onder INKOPER."

### Problem
The "Medewerker" (employee/coworker) field on projects/tasks should not be required.

### Requirements
- Make sure the employee field is not required so the user can save a task/sub task without entering an employee. (Optional field)

### Plan


### Acceptance Criteria


---


## 16. Tasks - Reconsider Task/Subtask Hierarchy

- [ ] Implemented
- [ ] User Tested

**Priority: P3 - Low**
**Area:** Tasks
**Feedback:** "Twijfel of er echt een onderverdeling nodig is tussen taak/subtaak, misschien minder overzicht."

### Problem
The task/subtask two-level hierarchy may be causing confusion instead of providing clarity.

### Requirements
- Evaluate if subtasks add value or reduce overview
- Consider a flat task list with tags/labels as an alternative
- If keeping subtasks, make them more visually distinct and collapsible

### Plan
1. **Option A:** Keep subtasks but improve UX - better indentation, collapse/expand, clear visual hierarchy
2. **Option B:** Remove subtask concept entirely, use a flat list with grouping/labels
3. Discuss with user which approach they prefer

### Acceptance Criteria
- [ ] User confirms chosen approach
- [ ] Task view is perceived as more clear and organized

---


## 17. Tasks - Profile Photo on Assignee

- [ ] Implemented
- [ ] User Tested

**Priority: P3 - Low**
**Area:** Tasks, User Profile
**Feedback:** "Leuke profielfoto dat je aan de foto kan zien aan wie de subtaak toegewezen is."

### Problem
Task assignees are shown by name/initials only. A profile photo would make it more visual and quick to scan.

### Requirements
- Allow users to upload a profile photo
- Display profile photo (or initials fallback) on task cards next to the assignee

### Plan
1. Add `ProfilePhotoUrl` to `Employee` entity
2. Create image upload endpoint (`POST /api/employees/{id}/photo`)
3. Store images (Azure Blob Storage or local file system)
4. Display avatar on task cards, falling back to initials if no photo
5. Add photo upload option in user profile/settings

### Acceptance Criteria
- [ ] Users can upload a profile photo
- [ ] Photo appears on assigned tasks
- [ ] Initials shown as fallback when no photo is uploaded

---


## 18. Completed Tasks - Keep & Search

- [ ] Implemented
- [ ] User Tested

**Priority: P3 - Low**
**Area:** Tasks, Projects
**Feedback:** "Sowieso de voltooide taken/projecten laten staan. Makkelijk om dingen soms terug te zoeken + motiverend."

### Problem
Completed tasks/projects may be hidden or hard to find. Users want them searchable and visible for reference and motivation.

### Requirements
- Completed tasks should remain visible (not auto-hidden)
- Provide a filter to show/hide completed tasks
- Search functionality across all tasks (including completed)
- Completed projects should also remain accessible

### Plan
1. Add a toggle/filter: "Toon voltooide taken" (Show completed tasks)
2. Default: show completed tasks in a collapsed/separate section
3. Add search/filter bar on the task list
4. Ensure soft-deleted items are excluded, but completed items are always queryable

### Acceptance Criteria
- [ ] Completed tasks are not removed from view
- [ ] Users can filter to show/hide completed tasks
- [ ] Search works across all task statuses

---


## 19. Attachments - File Upload (Magic Map)

- [ ] Implemented
- [ ] User Tested

**Priority: P3 - Low**
**Area:** Tasks, Files
**Feedback:** "Bijlages koppelen (via de Magic Map)."

### Problem
No way to attach files to tasks. Users reference a "Magic Map" (likely a shared drive/folder system).

### Requirements
- Upload files and attach them to tasks
- View/download attached files from task detail view
- Optional: link to files in a shared network drive (Magic Map)

### Plan
1. Create `TaskAttachment` entity: `Id`, `TaskId`, `FileName`, `FilePath`, `FileSize`, `UploadedAt`
2. File upload endpoint: `POST /api/tasks/{id}/attachments`
3. File storage (Azure Blob Storage or configured file path)
4. Frontend: file upload component on task detail
5. Display attachment list with download links
6. Future: Magic Map integration (shared drive browser)

### Acceptance Criteria
- [ ] Files can be uploaded to a task
- [ ] Attachments are listed on the task detail view
- [ ] Files can be downloaded

---


## 20. Attachments - Image Upload

- [ ] Implemented
- [ ] User Tested

**Priority: P3 - Low**
**Area:** Tasks, Files
**Feedback:** "Afbeelding toevoegen."

### Problem
Similar to file attachments, but specifically for images with preview support.

### Requirements
- Upload images to tasks
- Show image thumbnails/previews in the task detail
- Support common formats: JPG, PNG, WEBP

### Plan
1. Extend the attachment system (Feature #19) to support image previews
2. Generate thumbnails on upload
3. Display inline image previews on task detail
4. Lightbox/modal for full-size viewing

### Dependencies
- Feature #19 (File Upload) should be built first

### Acceptance Criteria
- [ ] Images can be uploaded to tasks
- [ ] Thumbnails are displayed inline
- [ ] Full-size preview on click

---


## 21. Integration - Whise Koppeling

- [ ] Implemented
- [ ] User Tested

**Priority: P3 - Low**
**Area:** Integrations
**Feedback:** "Eventuele koppeling aan Whise zelf."

### Problem
Whise is the real estate CRM platform used by the team. No integration exists.

### Requirements
- Research Whise API capabilities
- Define which data should sync (contacts, properties, dossiers)
- Link tasks/projects to Whise entities

### Plan
1. Research Whise API documentation and authentication
2. Define integration scope with stakeholders
3. Build Whise API client service
4. Create linking mechanism (e.g., WhiseReferenceId on Projects/Tasks)
5. Sync relevant data

### Acceptance Criteria
- [ ] Scope defined and approved
- [ ] Basic Whise API connection established
- [ ] At least one entity type linked between Flowie and Whise

---


## 22. Integration - Email Linking

- [ ] Implemented
- [ ] User Tested

**Priority: P3 - Low**
**Area:** Integrations
**Feedback:** "Eventueel mails koppelen."

### Problem
No way to link emails to tasks. Relevant communication lives outside the tool.

### Requirements
- Link emails to tasks for context
- At minimum: paste an email reference or forward to a task-specific address

### Plan
1. **Option A (Simple):** Allow pasting email content/link as a task comment or note
2. **Option B (Advanced):** Email forwarding integration (task-specific email address)
3. **Option C (Full):** Microsoft Graph API integration for Outlook
4. Start with Option A

### Acceptance Criteria
- [ ] Users can associate email content with a task
- [ ] Linked emails are viewable from the task detail

---


## 23. Notifications - Pop-up Alerts

- [ ] Implemented
- [ ] User Tested

**Priority: P3 - Low**
**Area:** Notifications
**Feedback:** "Pop-ups op je scherm bij een taak / mail met Progress."

### Requirements
- In-app notifications for task updates (assigned to me, status changes, approaching deadlines)
- Browser push notifications (with user permission)
- Optional: email notifications for critical updates

### Plan
1. Create notification system backend (`Notification` entity, SignalR hub)
2. In-app notification bell with dropdown
3. Browser push notification support (Web Push API)
4. Notification preferences per user
5. Trigger notifications on: task assignment, status change, deadline approaching

### Acceptance Criteria
- [ ] Users receive in-app notifications for relevant task events
- [ ] Browser push notifications work when enabled
- [ ] Users can configure notification preferences

---

## Implementation Roadmap (Prioritized)

### Phase 1 - Quick Wins & Critical Fixes
> These are blocking daily usage or are small changes with big impact.

| # | Feature | Priority | Effort |
|---|---------|----------|--------|
| 1 | Prevent auto-logout on inactivity | P0 | Small |
| 2 | Make employee assignment optional | P0 | Small |
| 3 | Make deadline optional | P0 | Small |
| 5 | Company label in "Alles" tab | P1 | Small |
| 6 | Show creation date on tasks | P1 | Small |

### Phase 2 - Core Feature Enhancements
> These significantly improve the daily workflow.

| # | Feature | Priority | Effort |
|---|---------|----------|--------|
| 4 | "Algemeen" project tab | P1 | Small |
| 7 | "Wachten Op" status | P1 | Small |
| 8 | Drag & drop task reordering | P1 | Medium |
| 9 | Calendar sync (deadlines) | P1 | Medium |

### Phase 3 - UX Improvements
> Polish and usability improvements.

| # | Feature | Priority | Effort |
|---|---------|----------|--------|
| 10 | Rich description (lists/bullets) | P2 | Small |
| 11 | Priority levels | P2 | Medium |
| 16 | Reconsider task/subtask hierarchy | P3 | Medium |

### Phase 4 - Attachments & Integrations
> Bigger features requiring more architecture.

| # | Feature | Priority | Effort |
|---|---------|----------|--------|
| 19 | File attachments | P3 | Medium |
| 20 | Image upload with previews | P3 | Medium |
| 12 | Dashboard content & widgets | P3 | Medium |

### Phase 5 - Advanced Features & Integrations
> Long-term features requiring external APIs or significant architecture.

| # | Feature | Priority | Effort |
|---|---------|----------|--------|
| 13 | Private/personal dashboard tab | P3 | Medium |
| 14 | Theme color customization | P3 | Small |
| 15 | Hide medewerker field (for now) | P3 | Small |
| 17 | Profile photos on assignees | P3 | Medium |
| 18 | Keep & search completed tasks | P3 | Medium |
| 21 | Whise integration | P3 | Large |
| 22 | Email linking | P3 | Large |
| 23 | Pop-up notifications | P3 | Large |



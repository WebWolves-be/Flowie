# Native iOS feel for Flowie mobile

**Date:** 2026-07-30
**Status:** Approved, ready for implementation planning
**Scope:** Frontend only. No backend changes, no new screens.

## Context

Every Flowie mobile user is on iOS, running the app as an installed PWA. The
mobile layout fundamentals are already solid — 44px touch targets, 16px form
fields, safe-area insets, `h-dvh`, separate phone templates — and are recorded
as non-negotiable rules in `frontend/flowie-app/CLAUDE.md`.

What is missing is the layer above those fundamentals: the app is correct on a
phone but does not yet *behave* like something native. Writes block on the
network, one screen replaces another with no transition, the detail sheet
advertises a drag gesture it does not implement, icons disappear offline, and
the only way back out of a project scrolls off the screen.

This spec covers that layer.

### Why not a native app

Rejected deliberately. Flowie is a private, internal tool for two companies.
Native distribution would require either Apple Business Manager custom apps
(org verification plus ongoing administration) or TestFlight, where builds
expire every 90 days and every colleague reinstalls four times a year — in
exchange for rewriting a working Angular app and maintaining a second codebase.

Capacitor was considered as a middle path. It would wrap the existing build
with real haptics and native push and no rewrite, but it still ships a webview,
so every gesture below would still have to be written by hand. It adds
distribution overhead without removing any work. Revisit only when a device
capability is needed that the web genuinely cannot reach.

### Platform constraints

- iOS Safari does not expose the Vibration API, so **haptics are unavailable**
  to a PWA. No design below depends on them.
- There is no install prompt on iOS; installation is Share → Add to Home Screen.
- In `display: standalone` there is no browser chrome and no edge-swipe back
  gesture, so in-app back navigation must be self-sufficient.

## Goals

1. Writes feel instant.
2. Getting back out of any screen is always possible and always visible.
3. Gestures the UI advertises actually work.
4. The app works offline as well as its service worker promises.
5. No regression to the mobile layout rules already enforced by
   `e2e/mobile-layout.spec.ts`.

## Non-goals

Dark mode, Dynamic Type, an iOS 26 floating tab bar, and the empty Dashboard
screen are all out of scope. They are real gaps but are separate pieces of work;
the visual refresh in particular is best done once, after this structural pass
has settled, rather than retrofitted into templates that are about to change.

## Design

### 1. Optimistic writes in `TaskFacade`

**Problem.** `onTaskStatusChanged` in
`features/tasks/components/tasks-page/tasks-page.ts` issues
`updateTaskStatus`, then on success refetches `getTasks` *and* `getProjects`,
then shows a toast. Three sequential round-trips elapse before the tapped row
visibly changes. This is the single largest contributor to the app feeling slow
on a mobile network.

**Change.** Status updates become optimistic, and the logic moves into
`TaskFacade` so there is one implementation rather than two:

1. Capture the current value, then apply the new status to the `#tasks` signal
   immediately. The row changes on tap.
2. Fire the request in the background.
3. On success, silently refetch `getTasks` and `getProjects` to reconcile
   derived values — section `completedTaskCount`, project progress, and parent
   task status derived from subtasks. The user never waits on this.
4. On error, revert the signal to the captured value and show an error toast.

Reconciling derived counts through a background refetch, rather than
recomputing them client-side, is deliberate: it is self-correcting, and it keeps
count derivation in exactly one place (the backend).

**Consequences.**

- The success toast for status changes is removed. The row visibly changing is
  the feedback; a toast covering the header on every tap is noise. Error toasts
  remain. Toasts for other operations (create, delete, reorder) are unchanged.
- `onTaskStatusChanged` and `onSubtaskStatusChanged` are currently near-identical
  duplicates in `tasks-page.ts`. Both collapse onto the single facade path.

**Interface.** `TaskFacade.updateTaskStatus` continues to return
`Observable<void>` per the facade convention in
`frontend/flowie-app/CLAUDE.md`. Callers no longer perform their own refetch.

### 2. Pinned back bar in `project-detail`

**Problem.** Below `lg` the project header — including the
"Terug naar projecten" button — is rendered *inside* the scroll pane via
`ngTemplateOutlet` (`project-detail.component.html`). Scroll down a long
project and the only route back leaves the screen. Because a standalone PWA has
no browser back button and no edge-swipe, the user is stranded.

**Change.** Below `lg`, a 44px sticky bar at the top of the project pane
containing:

- a back chevron (the existing `backToList` output),
- the project title, truncated to one line,
- the existing kebab (`button[title="Acties"]`), moved here from the scrolling
  header.

Everything else — description, code badge, company pill, task filter — keeps
scrolling exactly as it does today.

**Why 44px and not the whole header.** `frontend/flowie-app/CLAUDE.md` records
that pinning the full header and filter costs ~170px, which is half a landscape
phone, and that this was unacceptable. That decision stands. 44px is the honest
price of always being able to leave the screen, and matches the iOS convention
of a large title collapsing to a compact bar.

### 3. `appSheetDrag` directive

**Problem.** `task-detail-sheet.component.html` renders the standard iOS grab
bar, which advertises a drag-to-dismiss gesture the sheet does not implement.

**Change.** A standalone directive in `core/directives/` built on Pointer
Events:

- Drag initiates from the grab bar and header region **only**. It must never
  initiate from the scrollable body, or scrolling inside the sheet breaks.
- Track `dy`; translate the panel downward. Upward movement gets rubber-band
  resistance rather than moving freely.
- On release, dismiss if `dy` exceeds 25% of sheet height *or* velocity exceeds
  ~0.5px/ms; otherwise spring back to rest.
- Honour `prefers-reduced-motion` by snapping rather than animating.
- The sheet is `@if`-rendered, so dismissal needs an exit animation before the
  node is removed.

The directive owns gesture mechanics only. It emits a dismiss event; the sheet
component decides what dismissal means.

### 4. Route transitions

**Change.** `provideRouter(routes, withViewTransitions())` in `src/main.ts`,
plus CSS assigning `view-transition-name` to the list and detail panes so
`/taken` ↔ `/taken/project/:id` slides horizontally instead of cross-fading.

Navigation is already genuinely route-driven, so no restructuring is needed.

**Suppressed** at `lg` and up, where both panes are visible simultaneously and
nothing should transition, and under `prefers-reduced-motion`.

**Known caveat.** Both routes resolve to the same `TasksPage` component, and the
visible pane is chosen by the `compactView` signal via `@if`. The View
Transitions API snapshots before/after DOM state, so this works, but the
`view-transition-name`s must be applied to the two `@if` blocks rather than to
the component host.

### 5. Self-hosted icons

**Problem.** `src/index.html` loads Font Awesome from `cdnjs.cloudflare.com`.
`ngsw-config.json` does already precache it through an `icon-font` asset group,
so this is narrower than "icons vanish offline" — but the app's entire icon set
(status glyphs, chevrons, kebab menus) depends on a third party being reachable
at service-worker install time, and on the very first load the stylesheet is a
render-blocking cross-origin request. If cdnjs is slow, blocked, or unreachable
when the worker installs, the precache silently fails and the icons are gone.

**Change.** Install `@fortawesome/fontawesome-free`, serve its CSS and webfonts
locally through `angular.json`, and delete the now-redundant `icon-font` asset
group from `ngsw-config.json` — locally served CSS and fonts are already
covered by the existing `app` and `assets` groups. Remove the CDN `<link>`.

Every existing `<i class="fas fa-*">` keeps working unchanged — no template
churn.

### 6. Keyboard hints

**Problem.** The codebase contains no `enterkeyhint`, `inputmode`,
`autocapitalize`, `autocorrect`, or `autocomplete` attribute anywhere. Every
field gets a generic keyboard with a "return" key, and titles do not
auto-capitalise.

**Change.** Across the auth pages and the save dialogs:

| Field | Attributes |
|---|---|
| Intermediate text fields | `enterkeyhint="next"` |
| Final field before submit | `enterkeyhint="done"` |
| Titles, descriptions | `autocapitalize="sentences"` |
| Project code (`#code`) | `autocapitalize="characters"`, `autocomplete="off"` |
| Login email | `inputmode="email"`, `autocomplete="username"`, `autocapitalize="none"` |
| Login password | `autocomplete="current-password"` |
| Register password | `autocomplete="new-password"` |

## Testing

E2E tests are mandatory per the root `CLAUDE.md` and ship with the feature, not
after it.

**New — `e2e/native-feel.spec.ts`:**

- *Optimistic status change.* Delay the status `PATCH` with `page.route`, tap
  the status control, and assert the row reflects the new status **before** the
  response resolves.
- *Optimistic revert.* Fail the `PATCH` and assert the row returns to its
  previous status and an error toast appears.
- *Sheet drag dismissal.* Synthesised pointer events from the grab bar past the
  threshold close the sheet; a short drag springs back and leaves it open.
- *Back bar reachability.* Open a project with enough sections to scroll,
  scroll to the bottom, and assert the back control is still within the
  viewport. **This test fails against the current code** — it is the regression
  test for the stranding bug.

**Extended — `e2e/mobile-layout.spec.ts`:** the pinned bar at both 375×812 and
812×375, asserting no new horizontal scroll and no safe-area regression.

**Backend:** unchanged, but `dotnet test Flowie.sln` still runs as part of
`/test-all`.

The full `npm run e2e` suite (desktop, mobile, mobile-small, mobile-landscape)
must be green before anything is committed.

## Documentation

`frontend/flowie-app/CLAUDE.md` gains entries under "Mobile / PWA layout rules"
for: the pinned 44px back bar and why it is exempt from the no-pinned-chrome
rule; the sheet-drag initiation constraint; and the fact that status writes are
optimistic and must not be followed by a blocking refetch.

## Release

Version bump: **minor**, 1.0.0 → 1.1.0, per "Versioning a release" in the root
`CLAUDE.md`.

## Deferred

Deliberately not built now, to be judged after a week of real use:

- **Swipe actions on task rows.** Real value, but high effort, and it conflicts
  with the existing long-press drag-to-reorder on the same element. If built,
  disambiguate the way iOS does: horizontal movement past ~10px claims the
  gesture as a swipe, a stationary hold past 500ms claims it as a reorder,
  vertical movement always yields to the scroll container, and the first to fire
  cancels the other.
- **Edge-swipe back.** Once a pinned back bar exists this is garnish rather than
  the only escape route.
- **Pull-to-refresh.** Expected to matter much less once writes are optimistic.

Also out of scope and tracked separately: the empty Dashboard screen (needs a
new cross-project task endpoint — `GetTasksQuery` is per-project today), dark
mode, Dynamic Type, the iOS 26 floating tab bar, mobile toast placement over
the header, and the `orientation`/`theme_color` inconsistencies in
`public/manifest.webmanifest` versus `src/index.html`.

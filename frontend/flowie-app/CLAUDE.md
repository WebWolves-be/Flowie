# Flowie Frontend (Angular 20)

> **AUTO-UPDATE**: When you modify code patterns, update this file to reflect current conventions.
> **NEVER write inline comments**. Make sure code is self explanatory.

## Signals & State

- **Signals-first**: Use `signal()` for mutable state, `computed()` for derived values
- **Readonly exposure**: `#state = signal([]); state = this.#state.asReadonly()`
- **Error state**: `errorMessage = signal<string | null>(null)`
- **Component I/O**: Use `input()`, `input.required<T>()`, `output<T>()` (not decorators)
- **Cleanup**: Use `takeUntilDestroyed(this.#destroy)` with `DestroyRef` for subscriptions

```typescript
// Component example
export class MyComponent {
  #destroy = inject(DestroyRef);
  project = input.required<Project>();
  taskChanged = output<{ id: number; status: TaskStatus }>();

  #tasks = signal<Task[]>([]);
  tasks = this.#tasks.asReadonly();

  taskCount = computed(() => this.tasks().length);
}
```

## Forms

- **Typed FormGroup**: Define control types explicitly
- **NonNullable**: Use `{ nonNullable: true }` for required fields
- **Validation**: Apply `Validators.required` only; rely on backend for detailed validation
- **Property names**: Match backend DTOs exactly (camelCase)
- **Dynamic validators**: Use `setValidators()` / `clearValidators()` when needed
- **Whitespace check**: Manually validate `.trim()` for text inputs
- **Submission**: Check `form.invalid`, call `markAllAsTouched()`, disable submit button
- **Error display**: Use `extractErrorMessage()` utility, show errors at form top (not inline)

```typescript
form = new FormGroup({
  title: new FormControl("", { nonNullable: true, validators: [Validators.required] }),
  taskTypeId: new FormControl<number | null>(null)
});

onSubmit()
{
  if (this.form.invalid) {
    this.form.markAllAsTouched();
    return;
  }
  const trimmed = this.form.value.title!.trim();
  if (!trimmed) {
    this.form.controls.title.setErrors({ required: true });
    return;
  }
  // submit...
}
```

## Facades

- **Injectable**: `@Injectable({ providedIn: "root" })`
- **State pattern**: Private signals + public readonly exposure
- **Observable returns**: Create/update/delete MUST return `Observable<void>` for error handling
- **Loading states**: Track with signals (`#isLoading = signal(false)`)
- **HTTP params**: Use `HttpParams` for query strings

```typescript
#items = signal<Item[]>([]);
items = this.#items.asReadonly();

createItem(request
:
CreateRequest
):
Observable < void > {
  return this.#http.post<void>(`${this.#apiUrl}/api/items`, request);
}

getItems(filter ? : string)
:
void {
  let params = new HttpParams();
  if(filter) params = params.set("filter", filter);

  this.#http.get<Response>(`${this.#apiUrl}/api/items`, { params })
    .subscribe(res => this.#items.set(res.items));
}
```

## Dialogs

- **Data typing**: Create interfaces for dialog data and results
- **DialogRef**: Use `DialogRef<ResultType>` for typed results
- **Success flow**: Execute → refresh data → show notification → close
- **Error handling**: Use `catchError()` + `extractErrorMessage()`

```typescript
interface MyDialogData {
  mode: "create" | "edit";
  item?: Item;
}

export class MyDialogComponent {
  #ref = inject(DialogRef);
  #data = inject<MyDialogData>(DIALOG_DATA);
  #facade = inject(ItemFacade);
  #notifications = inject(NotificationService);

  errorMessage = signal<string | null>(null);

  onSubmit() {
    this.#facade.createItem(request)
      .pipe(catchError((error: HttpErrorResponse) => {
        this.errorMessage.set(extractErrorMessage(error));
        return EMPTY;
      }))
      .subscribe(() => {
        this.#facade.getItems();
        this.#notifications.showSuccess("Item succesvol aangemaakt");
        this.#ref.close();
      });
  }
}
```

## Error Handling

- **Utility**: Always use `extractErrorMessage(error: HttpErrorResponse)`
- **Pattern**: `catchError() → set error signal → return EMPTY`
- **Reset**: Set `errorMessage.set(null)` before new requests
- **UI**: Error box at top with red styling + SVG icon

## Styling (Tailwind)

- **Dialog**: `w-screen max-w-full h-dvh md:h-auto md:w-[32rem] md:max-w-[calc(100vw-2rem)] md:max-h-[90dvh] min-w-0 bg-white rounded-none md:rounded-xl shadow-xl` + `role="dialog" aria-modal="true"`. The header carries `pt-safe-t`, the footer `safe-area-bottom`.
- **Primary button**: `inline-flex items-center justify-center px-4 min-h-touch text-sm rounded-lg bg-teal-600 text-white hover:bg-teal-500 disabled:opacity-50`
- **Secondary button**: `border border-gray-300 text-gray-700 hover:bg-gray-50`
- **Input base**: `w-full rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2`
- **Input valid**: `border border-gray-300 focus:ring-teal-500 focus:border-teal-500`
- **Input invalid**: `border-2 border-red-500 focus:ring-red-500 focus:border-red-500`
- **Error box**: `bg-red-50 border border-red-200 text-red-800`
- **Icons**: Use Font Awesome (`<i class="fas fa-X"></i>`), served locally from
  the `@fortawesome/fontawesome-free` package via `angular.json` — never from a
  CDN, or the icon set depends on a third party being reachable when the service
  worker installs. Drag handles use inline SVG (no FA equivalent).

## Mobile / PWA layout rules (non-negotiable)

The app is installed as a PWA on phones. These rules exist because breaking them
produced horizontal scrolling and content hidden under the notch:

- **Never use a bare `flex-1`.** Add `min-w-0` to every flex child that holds
  text, or it cannot shrink below its content and pushes the page sideways. Pair
  with `truncate` or `break-words` on the text itself, and `flex-shrink-0` +
  `whitespace-nowrap` on adjacent badges and icon buttons.
- **Never use `overflow-y-auto` alone** — it computes `overflow-x` to `auto`,
  silently adding a second scroll axis. Use the `.scroll-pane` class from
  `styles.scss` (both axes named, `overscroll-behavior: contain`).
- **Form fields must render at ≥16px on touch viewports**, otherwise iOS zooms
  the page on focus and never zooms back, leaving it pannable. Enforced globally
  in `styles.scss` (with `!important`, since a Tailwind `text-sm` class outranks
  a bare element selector).
- **Interactive controls are ≥44px** (`min-h-touch` / `min-w-touch`, = 2.75rem).
  An icon button needs `flex items-center justify-center min-w-touch min-h-touch`;
  padding alone leaves a 20px-wide target. These two are **plain CSS in
  `styles.scss` under `@media (max-width: 1023px)`**, not Tailwind theme values —
  a mouse is precise enough for the compact desktop controls, so keep each
  element's own `py-*` alongside them and desktop is left exactly as it was.
- **Use `h-dvh` / `min-h-dvh`, not `h-screen`** — 100vh is wrong on iOS when the
  keyboard or the browser toolbars are on screen.
- **Fixed app chrome carries safe-area padding** (`pt-safe-t` on the mobile
  header, `pb-safe-b` on the bottom nav, `pt-header-safe` / `pb-nav-safe` on the
  shell). `index.html` needs `viewport-fit=cover` for `env(safe-area-inset-*)` to
  be non-zero at all.
- **Hover is not available on touch.** Anything revealed by `group-hover` must be
  visible by default below `lg`, or replaced by a touch gesture. Drag handles
  took the second route: a permanently visible grip costs ~28px of every row for
  an occasional action, so below `lg` the row is dragged directly after a hold
  (`cdkDragStartDelay`) and no grip is rendered at all.
- **Every control on one row is the same box.** A filled button and an outlined
  one side by side must agree on height, width and top edge — so the filled one
  carries `border border-transparent` (a border changes flex-basis maths and
  otherwise makes it 2px narrower), and neither insets its visible edge inside
  its tap target. An earlier `.touch-pill` helper drew a compact border inside a
  44px box; it read as two different buttons and has been removed.
- **A 44px tap target must not resize the glyph inside it.** Quill sizes its
  toolbar icons to the button's content box, so growing the button without
  adding padding doubled the weight of B and I against the rest of the form.
- **`pt-safe-t` overwrites `py-*`, it does not add to it.** A header that needs
  its own padding *plus* the notch inset uses `pt-4-safe-t`
  (`calc(1rem + env(safe-area-inset-top))`). Written as `py-4 pt-safe-t` the top
  padding collapses to zero on every device without a notch, which is what left
  dialog titles flush against their own header band.
- **`input[type="date"]` opens its picker from a tap anywhere in the field**
  (`onOpenDatePicker` → `showPicker()`), and the native indicator is widened
  below `lg`. Its default target is ~14px in the corner of the field.
- **Phone and desktop get separate templates, not one responsive template.**
  Shared logic lives in an abstract `@Directive()` base and each surface renders
  what suits it. Tasks are the reference implementation:
  `task-item/task-item-base.ts` holds every input, output, signal and status
  rule; `app-task-item` is the desktop card, `app-task-item-mobile` a one-row
  summary (tappable status, title, due date, assignee, subtask progress), and
  `app-task-detail-sheet` a bottom sheet holding everything the row leaves out.
  Add behaviour to the base, never to one template.
- **A bottom sheet's host element needs its own box.** With only `fixed`
  children the host measures 0×0 and counts as hidden. Put
  `position: fixed; inset: 0; z-index: …` on `:host` and position the backdrop
  and panel `absolute` inside it. The z-index must beat the bottom nav's `z-50`,
  which is rendered later in the document.
- **Below `lg` the project header and filter scroll with the list**, rendered
  inside the `.scroll-pane` via `ngTemplateOutlet`; only at `lg` and up are they
  pinned chrome. Pinned they cost ~170px — half a landscape phone. Section
  headers are `sticky top-0` so context survives the scroll.
- **A 44px back bar is pinned above the project detail below `lg`**, and is the
  one exception to the rule above that the header scrolls. A standalone PWA has
  no browser back button and no edge-swipe, so when the header scrolled away the
  user had no way out of a project at all. It carries the project title in a
  `<span>`, never an `<h2>` — `e2e/mobile-layout.spec.ts` asserts the large
  `<h2>` title leaves the viewport, and a pinned duplicate would defeat it.
- **A sheet's drag-to-dismiss belongs on its handle, not its panel.** Applied to
  the whole panel it takes the gesture from the scrolling body, so a downward
  swipe over the description closes the sheet instead of scrolling it. The
  mechanics live in `core/directives/sheet-drag.directive.ts`, which emits
  `dismissed` and knows nothing about tasks; the dismiss/spring-back decision is
  the exported `shouldDismiss` and is unit-tested rather than driven through a
  browser.
- **Status writes are optimistic and must not be awaited.** `TaskFacade`
  applies the new status to its signal before the request leaves, and restores
  the previous value if it fails. Callers refetch afterwards only to reconcile
  the counts a row cannot derive itself, and show no success toast — the row
  changing is the feedback, and a toast on every tap covers the mobile header.
- Custom utilities live in `tailwind.config.js`: `min-h-touch`, `min-w-touch`,
  `h-dvh`, `min-h-dvh`, `pt-safe-t`, `pt-4-safe-t`, `pb-safe-b`,
  `pt-header-safe`, `pb-nav-safe`.

`e2e/mobile-layout.spec.ts` enforces all of the above at 375×812 and 812×375.

## Best Practices

- **Standalone components**: Always `standalone: true` with explicit imports
- **Control flow**: Use `@if`, `@for`, `@switch` (not `*ngIf`, `*ngFor`, `*ngSwitch`)
- **Dependency injection**: Use `inject()` function with `#privateFields`
- **Notifications**: Dutch messages via `NotificationService`
- **Type safety**: Create interfaces for all data structures
- **Labels**: Match backend error message terminology

---


## Self-Validation: Frontend E2E Testing

> **Primary suite:** `npm run e2e` (Playwright, `e2e/` folder, config in
> `playwright.config.ts`). Projects: `desktop` (1280×720), `mobile` (Pixel 7,
> 412px), `mobile-small` (375×812 — narrowest supported phone) and
> `mobile-landscape` (812×375). Requires both dev servers running.
> `npm run e2e:mobile` for the Pixel 7 project only.
> For exploratory mobile checks use the `/test-mobile` skill.
> Playwright CLI / Chrome DevTools MCP remain for ad-hoc debugging.

### Unit tests (Karma/Jasmine)

`npm test -- --watch=false --browsers=ChromeHeadless` runs `*.spec.ts` files
under `src/`. Use these for logic that e2e cannot reach — anything depending on
the service worker, which `ng serve` disables. Injectable seams exist for that
purpose: `AppReloader` wraps `location.reload()` so a test can assert a reload
without navigating. E2E remains the default; unit tests are the exception.

## PWA versioning

- `package.json`'s `version` is the only source. See "Versioning a release" in
  the root `CLAUDE.md` for when and how to bump it.
- `src/build-info.ts` is **generated** from it by
  `scripts/generate-build-info.mjs`, which `npm run build` runs via `prebuild`.
  Because it only ever contains the version, a build is byte-identical unless the
  version changed — no dirty working tree afterwards, and no `git checkout` dance.
  It stays committed because CI starts the app with `npx ng serve`, which skips
  npm lifecycle hooks; gitignored, the CI build would not compile.
- Never hand-edit `src/build-info.ts` — the next build overwrites it.
- Settings → Versie shows that number and nothing else. Build date, commit hash
  and branch were dropped: they described the build machine rather than the
  release, and the branch is a detached `HEAD` in CI anyway.
- `PwaUpdateService` (`core/services/`) prompts via `NotificationService.showAction`
  when `SwUpdate` reports `VERSION_READY`, polls every 30 minutes, and handles
  `unrecoverable`. `AppComponent`'s constructor calls `initialize()`.
- `NotificationService.showAction()` creates a **sticky** toast (no duration) with
  a button. Use it for anything the user must act on; the `show*` helpers all
  auto-dismiss.

## E2E Tests Are Mandatory

Every new feature or behavior change ships with e2e specs in `e2e/`, and the
suite must be green before the work counts as done — see the root `CLAUDE.md`
for the full rule. Bug fixes get a regression test in `e2e/regressions.spec.ts`
that fails before the fix. Reuse `e2e/helpers.ts` rather than writing new
locators, and never skip or loosen an assertion to get green.

### Running the suite

1. Ensure backend (`http://localhost:5229/health`) and frontend
   (`https://localhost:4200`) are running.
2. `cd frontend/flowie-app`
3. `npm run e2e` (full suite) or `npm run e2e:mobile` / `npm run e2e:desktop`
4. On failure: `npx playwright show-report`, inspect the trace, fix, re-run.

The config sets `ignoreHTTPSErrors: true`, so the self-signed dev certificate
needs no workaround inside the suite.

### Key UI facts for selectors

- UI language is Dutch: `Nieuw project`, `Annuleren`, `Aanmaken`, `Bewerken`,
  `Verwijderen`, `Toevoegen`, `Sectie toevoegen`; statuses `Beginnen` /
  `Klaar` / `Wachten op` / `Heropenen` / `Openzetten`.
- All dialogs: `[role="dialog"][aria-modal="true"]` with an `<h2>` heading;
  delete-confirm button is `Verwijderen` (use `.last()` inside the dialog).
- Desktop-only action buttons are `hidden lg:flex`; **below `lg` (< 1024px)** the
  same actions live behind kebab buttons `button[title="Acties"]` — branch
  specs on Playwright's `isMobile` fixture.
- The desktop add-task button inside a section is just `Taak`; the mobile
  kebab menu item is `Taak toevoegen`.
- Task dialog inputs: `#title`, `.ql-editor` (Quill), `#taskTypeId`,
  `#dueDate`, `#employeeId`. Project dialog: `#title`, `#code` (maxlength 5),
  `#description`, `#company`. Section dialog: `#title` + Quill.
- Mobile back button in project detail: `Terug naar projecten`.
- Logout lives in two places: the desktop sidebar (`lg` and up) and the mobile
  header's account menu (`button[title="Account"]` → `button[title="Uitloggen"]`).
  Both match `button[title="Uitloggen"]`, so **scope to `app-mobile-header`** on
  phones — the sidebar keeps a `display:none` copy in the DOM.
- Sections *and* tasks are both `.cdk-drag` elements, and a section contains its
  tasks' `<h3>`s. To target a task row use `.cdk-drag:has(> app-task-item)`
  (`> app-task-item-mobile` below `lg`), otherwise the locator resolves to the
  enclosing section.
- A task is `app-task-item` on desktop and `app-task-item-mobile` below `lg`;
  its title is an `<h3>` there and a `<span>` here. Use `taskCard()`, which
  matches either. Task actions live in the expanded desktop card or in
  `app-task-detail-sheet` on mobile — reach them via `openTaskDetail()` +
  `taskActions()` rather than locating buttons on the row.
- The detail sheet is itself `role="dialog" aria-modal="true"` and stays open
  underneath a CDK dialog opened from it, so the `dialog()` helper is scoped to
  `.cdk-overlay-container`.
- **Drag handles only exist at `lg` and up.** Below that no `[cdkdraghandle]` is
  rendered and the row itself is the drag target, with `cdkDragStartDelay` making
  the reorder start on a long press. Use the `dragOnto` helper — pass
  `{ holdMs: 500 }` for touch — instead of hand-rolling mouse moves.

### The `lg` breakpoint is the mobile/desktop line

The mobile header, bottom nav, kebab menus and single-pane project view apply
below `lg` (1024px); the sidebar and two-pane layout apply at `lg` and up.
`md` is deliberately *not* the switch: a phone in landscape is 812px wide but
only 375px tall, and using `md` left that range with no navigation at all and
175px of horizontal scroll. `BreakpointService.isCompact` is the TS counterpart.

### Shared helpers

Import from `e2e/helpers.ts` (`createProject`, `openProject`, `createSection`,
`openCreateTaskDialog`, `projectAction`, `confirmDelete`, `deleteProject`,
`uniqueName`, `dialog`) instead of duplicating locators. Test data must use
`uniqueName()` (the `E2E-` prefix) so the cleanup teardown can remove leftovers.

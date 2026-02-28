# Flowie Angular Guidelines

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

- **Dialog**: `w-[25rem] bg-white rounded-xl shadow-xl p-6` + `role="dialog" aria-modal="true"`
- **Primary button**: `px-4 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-500 disabled:opacity-50`
- **Secondary button**: `border border-gray-300 text-gray-700 hover:bg-gray-50`
- **Input base**: `w-full rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2`
- **Input valid**: `border border-gray-300 focus:ring-blue-500 focus:border-blue-500`
- **Input invalid**: `border-2 border-red-500 focus:ring-red-500 focus:border-red-500`
- **Error box**: `bg-red-50 border border-red-200 text-red-800`
- **Icons**: Use inline SVG (not icon fonts)

## Best Practices

- **Standalone components**: Always `standalone: true` with explicit imports
- **Control flow**: Use `@if`, `@for`, `@switch` (not `*ngIf`, `*ngFor`, `*ngSwitch`)
- **Dependency injection**: Use `inject()` function with `#privateFields`
- **Notifications**: Dutch messages via `NotificationService`
- **Type safety**: Create interfaces for all data structures
- **Labels**: Match backend error message terminology

---

## Self-Validation: Frontend E2E Testing

After modifying UI, forms, or user flows, **always verify in the browser**.
Two tools are available: **Playwright CLI** (skill) and **Chrome DevTools MCP**.

### Option A: Playwright CLI (full E2E flows)

Use for navigating pages, filling forms, clicking buttons, and verifying outcomes.

#### SSL Certificate Workaround

The frontend uses a self-signed SSL cert. The `--config` flag does NOT bypass this.
You **must** use `run-code` to create a new browser context with `ignoreHTTPSErrors: true`:

```bash
playwright-cli open about:blank

playwright-cli run-code "async page => {
  const browser = page.context().browser();
  const ctx = await browser.newContext({ ignoreHTTPSErrors: true, viewport: { width: 1280, height: 720 } });
  const p = await ctx.newPage();
  await p.goto('https://localhost:4200/login', { waitUntil: 'networkidle' });
  return p.url();
}"
```

#### Shell Escaping for Password

The password `iK845)%U$UYdn25` contains `%` and `$`. Use JS hex escapes in `run-code`:

```
\x25 = %
\x24 = $
Result: 'iK845)\x25U\x24UYdn25'
```

#### Login Flow

```bash
playwright-cli run-code "async page => {
  const browser = page.context().browser();
  const ctx = await browser.newContext({ ignoreHTTPSErrors: true, viewport: { width: 1280, height: 720 } });
  const p = await ctx.newPage();
  await p.goto('https://localhost:4200/login', { waitUntil: 'networkidle' });
  await p.fill('#email', 'claude.code@testing.be');
  await p.fill('#password', 'iK845)\x25U\x24UYdn25');
  await p.click('button[type=submit]');
  await p.waitForURL(url => !url.toString().includes('/login'), { timeout: 15000 });
  return 'Logged in, URL: ' + p.url();
}"
```

#### Interacting After Login

The new context is separate from the CLI's tracked page. Continue using `run-code` and
find the page via the browser's contexts:

```bash
playwright-cli run-code "async page => {
  const browser = page.context().browser();
  for (const ctx of browser.contexts()) {
    for (const p of ctx.pages()) {
      if (p.url().includes('localhost:4200') && !p.url().includes('about:blank')) {
        // Interact with p here
        const bodyText = await p.locator('body').innerText();
        return bodyText.substring(0, 1000);
      }
    }
  }
  return 'Page not found';
}"
```

#### Cleanup

```bash
playwright-cli close
```

### Option B: Chrome DevTools MCP (inspect, debug, check errors)

Use for checking console errors, inspecting DOM state, taking screenshots, and verifying
network requests. The MCP server connects to Chrome via `chrome-devtools-mcp`.

#### Check for Console Errors After a Change

```
mcp__chrome-devtools__list_console_messages  (types: ["error", "warn"])
```

If errors are found, get details:

```
mcp__chrome-devtools__get_console_message  (msgid: <id>)
```

#### Take a Snapshot of the Page (DOM inspection)

```
mcp__chrome-devtools__take_snapshot
```

Returns the accessibility tree with element UIDs for interaction.

#### Take a Screenshot

```
mcp__chrome-devtools__take_screenshot
```

#### Interact with Elements

```
mcp__chrome-devtools__click       (uid: "<element-uid>")
mcp__chrome-devtools__fill        (uid: "<element-uid>", value: "text")
mcp__chrome-devtools__fill_form   (elements: [{uid, value}, ...])
```

#### Check Network Requests (verify API calls)

```
mcp__chrome-devtools__list_network_requests  (resourceTypes: ["fetch", "xhr"])
mcp__chrome-devtools__get_network_request    (reqid: <id>)
```

#### Navigate

```
mcp__chrome-devtools__navigate_page  (type: "url", url: "https://localhost:4200/taken")
```

#### Typical Validation Flow with Chrome DevTools MCP

1. Navigate to the page you changed
2. `take_snapshot` to verify the DOM looks correct
3. `list_console_messages` with `types: ["error"]` to catch JS errors
4. `list_network_requests` with `resourceTypes: ["fetch"]` to verify API calls succeeded
5. If something looks wrong, `take_screenshot` for visual verification

### Key Selectors

| Page      | Element             | Selector / ID                                    |
|-----------|---------------------|--------------------------------------------------|
| Login     | Email input         | `#email`                                         |
| Login     | Password input      | `#password`                                      |
| Login     | Submit button       | `button[type="submit"]`                          |
| Tasks     | Add task button     | `button:has-text("Taak Toevoegen")`              |
| Task form | Title               | `#title`                                         |
| Task form | Type dropdown       | `#taskTypeId`                                    |
| Task form | Description         | `.ql-editor` (Quill rich text editor)            |
| Task form | Deadline            | `#dueDate`                                       |
| Task form | Employee dropdown   | `#employeeId`                                    |
| Task form | Submit (create)     | `button[type="submit"]:has-text("Aanmaken")`     |
| Task form | Cancel              | `button:has-text("Annuleren")`                   |

### Key Routes

| Route                  | Description            |
|------------------------|------------------------|
| `/login`               | Login page             |
| `/dashboard`           | Dashboard (after login)|
| `/taken`               | Tasks overview         |
| `/taken/project/:id`   | Tasks for a project    |
| `/instellingen`        | Settings               |

### What to Verify

1. **UI change** → `take_snapshot` or `take_screenshot`, confirm visual result
2. **Form change** → Fill and submit, verify success notification and data in list
3. **New component** → Navigate to it, check no console errors
4. **Styling change** → `take_screenshot` to visually confirm
5. **Any change** → Always check `list_console_messages` for errors after interacting

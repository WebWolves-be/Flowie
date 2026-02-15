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

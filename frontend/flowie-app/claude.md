# Technical Guidelines for Flowie Angular Application

## Error Handling

### HTTP Error Handling

- **Always use the `extractErrorMessage` utility** (`src/app/core/utils/error-message.util.ts`) for processing HTTP
  errors
- **Validation errors (400)**: Extract and display custom error messages from the `errors` array
- **Other errors**: Fall back to `error.title` or a generic error message
- **Type safety**: Cast errors to `HttpErrorResponse` in catch blocks

### Error Display

- **Use signals for error messages**: `errorMessage = signal<string | null>(null)`
- **Reset error state**: Set `errorMessage.set(null)` before making new requests
- **Template pattern**: Use `@if (errorMessage())` blocks to conditionally display errors
- **Error UI styling**:
    - Background: `bg-red-50`
    - Border: `border border-red-200`
    - Text color: `text-red-800`
    - Include an error icon (SVG) for visual clarity

### RxJS Error Handling Pattern

```typescript
.
pipe(
  catchError((error: HttpErrorResponse) => {
    this.errorMessage.set(extractErrorMessage(error));
    return EMPTY;
  })
)
```

## Form Handling

### Form Setup

- **Use Reactive Forms**: Import `ReactiveFormsModule`
- **FormGroup with FormControl**: Create typed form groups with explicit type definitions
- **Typed FormGroup pattern**: Define the form structure with typed controls for better type safety
  ```typescript
  taskForm!: FormGroup<{
    title: FormControl<string>;
    description: FormControl<string>;
    taskTypeId: FormControl<number | null>;
    dueDate: FormControl<string>;
    employeeId: FormControl<number | null>;
  }>;
  ```
- **NonNullable controls**: Use `{ nonNullable: true }` for required fields
  ```typescript
  new FormControl("", { nonNullable: true, validators: [Validators.required] })
  ```
- **Validators**: Apply validators at control creation (e.g., `Validators.required`). Do NOT use any other validations.
  We rely on the backend for this.
- **Backend property alignment**: Form control names MUST match backend DTO property names (camelCase)
  - Backend `TaskTypeId` → frontend `taskTypeId`
  - Backend `EmployeeId` → frontend `employeeId`
  - Backend `DueDate` → frontend `dueDate`

### Form Validation

- **Check validity before submit**: `if (this.form.invalid) return;`
- **Mark all as touched**: When form is invalid on submit, call `this.form.markAllAsTouched()` to show validation errors
- **Visual feedback**: Add conditional classes based on `control.invalid && control.touched`
    - Invalid: `border-2 border-red-500 focus:ring-red-500 focus:border-red-500`
    - Valid: `border border-gray-300 focus:ring-blue-500 focus:border-blue-500`
- **Disable submit button**: `[disabled]="form.invalid"`
- **Required field indicator**: Add `<span class="text-red-600">*</span>` to labels
- **Whitespace validation**: When backend doesn't handle it, manually validate trimmed values aren't empty
- **No client-side error hints**: Do NOT display inline validation error messages (e.g., "Title must be between 3 and 200 characters"). Rely on backend validation errors displayed via the error message signal at the top of the form.

## Component Architecture

### Dependency Injection

- **Use inject() function**: Prefer functional injection over constructor injection
- **Private fields with # prefix**: `#facade = inject(TaskTypeFacade)`
- **Services to inject**:
    - `DialogRef` for dialog control
    - `DIALOG_DATA` for dialog inputs
    - Facade for business logic
    - `NotificationService` for user feedback

### Dialog Components

- **DialogRef control**: Close dialogs with `this.#ref.close()`
- **Cancel action**: Provide `onCancel()` method that closes without action
- **Success flow**:
    1. Execute action via facade
    2. Refresh data (`this.#facade.getTaskTypes()`)
    3. Show success notification
    4. Close dialog

### Notifications

- **Success messages**: Use `this.#notificationService.showSuccess()` after successful operations
- **Language**: Use Dutch for user-facing messages
- **Examples**:
    - "Taak type succesvol aangemaakt"
    - "Taak type succesvol verwijderd"

## Styling Guidelines

### Dialog Structure

- **Container**: `w-[25rem] bg-white rounded-xl shadow-xl p-6`
- **Role attributes**: Add `role="dialog" aria-modal="true"` for accessibility
- **Title**: `text-lg font-semibold text-gray-900 mb-4`

### Buttons

- **Primary button**: `px-4 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-500`
- **Disabled state**: `disabled:opacity-50 disabled:cursor-not-allowed`
- **Secondary/Cancel button**: `px-4 py-2 text-sm rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50`
- **Button container**: `flex justify-end gap-3`

### Form Inputs

- **Input base**: `w-full rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2`
- **Labels**: `block text-sm font-medium text-gray-700 mb-2`
- **Label text alignment**: Label text should match the terminology used in backend validation error messages for consistency
  - Example: If backend says "Vervaldatum moet in de toekomst zijn", use "Vervaldatum" not "Deadline"
- **Spacing**: `mb-6` for form groups, `mb-4` for error messages

### Icons

- **Prefer inline SVG**: Use inline SVG icons instead of icon fonts (Font Awesome, etc.)
- **Example error icon**:
  ```html
  <svg class="inline w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 20 20">
    <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/>
  </svg>
  ```
- **Example info icon**:
  ```html
  <svg class="inline w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 20 20">
    <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"/>
  </svg>
  ```

## Type Safety

### Models

- **Create interfaces**: Define clear interfaces for all data structures
- **Validation error model**: Use `ValidationError` interface for API error responses
- **Dialog data models**: Create typed interfaces for dialog inputs (e.g., `DeleteTaskTypeDialogData`)

### Non-null Assertions

- **Use sparingly**: Only use `!` when you've validated the value (e.g., after form validation)
- **Example**: `this.form.value.name!` after checking `this.form.invalid`

## Best Practices

### Standalone Components

- **Always standalone**: Use `standalone: true` in component metadata
- **Explicit imports**: Import only what's needed in the `imports` array
- **Control flow syntax**: Use modern Angular control flow (`@if`, `@for`, `@switch`) instead of structural directives (`*ngIf`, `*ngFor`, `*ngSwitch`)
  - Avoids need for `CommonModule` import in most cases
  - Example: `@for (item of items; track item.id) { ... }` instead of `*ngFor="let item of items"`

### Readonly Properties

- **Public readonly**: Expose data properties as readonly when appropriate
- **Example**: `readonly taskType = this.#data.taskType`

### Signal Usage

- **Reactive state**: Use signals for component state that triggers UI updates
- **Error messages**: Always use signals for error state
- **Access pattern**: Use function call syntax in templates: `{{ errorMessage() }}`

### Facade Pattern

- **Business logic separation**: Always call backend services through facades
- **Consistent naming**: Use descriptive facade methods (e.g., `createTaskType`, `deleteTaskType`, `getTaskTypes`)
- **Observable return**: Create/update/delete methods MUST return Observables to allow dialogs to handle errors
  - Example: `createProject(request): Observable<void>` not `void`
  - This enables proper error handling with `catchError` in components
  - Read/fetch methods can use internal subscriptions for state management

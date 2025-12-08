# Frontend Authentication Implementation Plan

## Overview
Implement JWT authentication for the Angular frontend, connecting to the existing backend auth endpoints.

**User choices:**
- Login page only (no registration)
- Store tokens in sessionStorage
- Auto-refresh tokens before expiry

---

## New Files to Create

### 1. Auth Models
**`src/app/core/models/auth.model.ts`**
```typescript
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface LogoutRequest {
  refreshToken: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  tokenType: string;
}

export interface AuthUser {
  userId: string;
  employeeId: number;
  email: string;
  name: string;
}
```

### 2. Auth Facade
**`src/app/core/facades/auth.facade.ts`**

Key responsibilities:
- Store/retrieve tokens from sessionStorage (`access_token`, `refresh_token`, `expires_at`)
- Manage auth state with signals (`#isAuthenticated`, `#currentUser`)
- Parse JWT to extract user claims (user_id, employee_id, email, name)
- Schedule proactive token refresh 5 minutes before expiry
- Provide `login()`, `logout()`, `refreshToken()` methods returning `Observable<void>`

### 3. Auth Interceptor
**`src/app/core/interceptors/auth.interceptor.ts`**

- Attach `Authorization: Bearer <token>` header to all requests except `/auth/login` and `/auth/refresh`
- Handle 401 responses by clearing auth state and redirecting to login

### 4. Auth Guard
**`src/app/core/guards/auth.guard.ts`**

- Protect routes: redirect to `/login` if not authenticated

### 5. Guest Guard
**`src/app/core/guards/guest.guard.ts`**

- Prevent authenticated users from accessing login page: redirect to `/dashboard`

### 6. Login Page Component
**`src/app/features/auth/components/login-page/login-page.ts`**
**`src/app/features/auth/components/login-page/login-page.html`**
**`src/app/features/auth/components/login-page/login-page.scss`**

- Form with email + password fields
- Error message display for invalid credentials
- Submit button with loading state
- Dutch labels: "E-mail", "Wachtwoord", "Inloggen"
- Centered card layout on gray background
- Add a loader for the login api call

---

## Files to Modify

### 7. Update Routes
**`src/main.ts`**

Changes:
- Add `/login` route with `guestGuard`
- Add `authGuard` to dashboard, taken, taken/project/:id, instellingen routes
- Register `authInterceptor` before `errorInterceptor` in providers

### 8. Update App Component
**`src/app/app.component.ts`**

Changes:
- Inject `AuthFacade`
- Expose `isAuthenticated`, `currentUser`, `userInitials` signals
- Add `onLogout()` method

### 9. Update App Template
**`src/app/app.component.html`**

Changes:
- Wrap sidebar in `@if (isAuthenticated())`
- Replace hardcoded "AV" with `{{ userInitials() }}`
- Replace hardcoded "Amalia Van Dosselaer" with `{{ currentUser()?.name }}`
- Add logout button with icon next to user info
- Show only `<router-outlet>` when not authenticated (for login page)

---

## Implementation Order

| Step | Task |
|------|------|
| 1 | Create `auth.model.ts` |
| 2 | Create `auth.facade.ts` |
| 3 | Create `auth.interceptor.ts` |
| 4 | Create `auth.guard.ts` |
| 5 | Create `guest.guard.ts` |
| 6 | Create login page component (ts, html, scss) |
| 7 | Update `main.ts` with routes and interceptor |
| 8 | Update `app.component.ts` |
| 9 | Update `app.component.html` |

---

## Backend Endpoints Reference

| Endpoint | Method | Body | Response |
|----------|--------|------|----------|
| `/auth/login` | POST | `{ email, password }` | `TokenResponse` |
| `/auth/refresh` | POST | `{ refreshToken }` | `TokenResponse` |
| `/auth/logout` | POST | `{ refreshToken }` | 200 OK |

**TokenResponse:** `{ accessToken, refreshToken, expiresAt, tokenType: "Bearer" }`

---

## Error Messages (Dutch)

- Invalid credentials: "Ongeldige e-mail of wachtwoord"
- Session expired: "Uw sessie is verlopen. Log opnieuw in."
- Logout success: "U bent uitgelogd"

---

## Key Patterns to Follow

- **Facade pattern:** Private signals with readonly exposure (see `employee.facade.ts`)
- **Interceptor pattern:** Functional interceptor with `inject()` (see `error.interceptor.ts`)
- **Form handling:** `FormGroup` with `nonNullable`, `markAllAsTouched()`, `catchError` + EMPTY
- **Styling:** Tailwind classes as per CLAUDE.md guidelines

---

## Token Auto-Refresh Strategy

The `AuthFacade` implements proactive token refresh:

1. **On login/restore from storage:** Calculate time until token expiry minus 5 minutes
2. **Schedule refresh:** Use `setTimeout` to call refresh endpoint before expiry
3. **On successful refresh:** Store new tokens, reschedule next refresh
4. **On refresh failure:** Clear tokens, redirect to login
5. **Cleanup:** Clear timeout on logout

```typescript
#scheduleTokenRefresh(expiresAt: Date): void {
  if (this.#refreshTimerId) {
    clearTimeout(this.#refreshTimerId);
  }

  const now = new Date();
  const refreshBuffer = 5 * 60 * 1000; // 5 minutes
  const timeUntilRefresh = expiresAt.getTime() - now.getTime() - refreshBuffer;

  if (timeUntilRefresh <= 0) {
    this.refreshToken().subscribe();
    return;
  }

  this.#refreshTimerId = setTimeout(() => {
    this.refreshToken().subscribe();
  }, timeUntilRefresh);
}
```

---

## Edge Cases to Handle

1. **Token expired during offline:** Check validity on app init, try refresh, redirect to login if fails
2. **Concurrent 401 responses:** Only redirect once (check if already on login page)
3. **Refresh token expired:** Backend returns 401 - clear tokens and redirect
4. **Multiple tabs:** Each tab has its own sessionStorage scope (independent sessions)
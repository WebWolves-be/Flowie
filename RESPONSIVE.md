# Mobile-Responsive Flowie App Implementation Plan

## Context

The Flowie Angular app is currently **100% desktop-focused** with zero responsive design. It needs to be made mobile-responsive to prepare for Capacitor mobile app conversion. The user wants a mobile-first approach with native mobile patterns (bottom tab navigation) while maintaining the current desktop experience.

**Current Issues:**
- Fixed sidebar (224px) and project panel (320px) widths
- Dialogs sized at 400-512px overflow mobile screens
- Two-column tasks layout breaks on mobile
- No responsive breakpoints anywhere in the codebase
- Forms use `grid-cols-2` without mobile adjustments

**User Requirements:**
- Mobile-first breakpoints: 320px+ (mobile), 768px+ (tablet), 1024px+ (desktop)
- Bottom tab navigation on mobile (native pattern)
- Single-view tasks page on mobile (list → detail with back button)
- All dialogs responsive
- Prepare for Capacitor integration

---

## Implementation Strategy

### Mobile-First Approach

Use Tailwind responsive prefixes:
- Default (no prefix) = Mobile (320px+)
- `md:` = Tablet (768px+)
- `lg:` = Desktop (1024px+)

### Navigation Pattern

- **Mobile:** Bottom tab bar with Dashboard/Tasks/Settings icons, sidebar hidden
- **Desktop:** Sidebar visible, bottom tab hidden

### Tasks Page Flow

- **Mobile:** Show project list first, tap project → show detail view with back button
- **Desktop:** Two-column layout (project list + detail) as current

---

## Critical Files to Modify

### New Files (3)

1. **`src/app/core/services/breakpoint.service.ts`**
   - Provides reactive signals: `isMobile()`, `isTablet()`, `isDesktop()`
   - Uses Angular CDK `BreakpointObserver`

2. **`src/app/core/components/bottom-nav/bottom-nav.component.ts`**
   - Mobile-only bottom navigation with icons
   - Fixed bottom, hidden on `md:` and above

3. **`src/app/core/components/bottom-nav/bottom-nav.component.html`**
   - Three nav items: Dashboard, Taken, Instellingen
   - Active state with `routerLinkActive`

### Root Layout (2 files)

4. **`src/app/app.component.html`**
   - Sidebar: `hidden lg:flex` (hidden mobile/tablet, visible desktop)
   - Add `pb-16 md:pb-0` to root div (space for bottom nav)
   - Add `<app-bottom-nav>` component

5. **`src/app/app.component.ts`**
   - Import `BottomNavComponent`

### Tasks Feature (8 files)

6. **`src/app/features/tasks/components/tasks-page/tasks-page.ts`**
   - Add `mobileView` signal: `'list' | 'detail'`
   - Inject `BreakpointService`
   - Add `showProjectList()`, `showProjectDetail()` computed signals
   - Modify `onProjectSelected()` to switch view on mobile
   - Add `onBackToProjectList()` method

7. **`src/app/features/tasks/components/tasks-page/tasks-page.html`**
   - Project sidebar: `w-full md:w-80` (full width mobile, fixed tablet+)
   - Use `@if (showProjectList())` and `@if (showProjectDetail())`
   - Detail panel: `w-full md:flex-1`
   - Company filters: `flex-wrap gap-2` (wrap on mobile)

8. **`src/app/features/tasks/components/project-detail/project-detail.component.ts`**
   - Add `isMobile = input<boolean>(false)`
   - Add `backToList = output<void>()`

9. **`src/app/features/tasks/components/project-detail/project-detail.component.html`**
   - Add back button: `@if (isMobile()) { ... }`
   - Header: `flex-col md:flex-row` (stack mobile, row desktop)
   - Action buttons: `flex-col md:flex-row` (stack mobile)
   - Padding: `p-4 md:p-6`

10. **`src/app/features/tasks/components/task-item/task-item.component.html`**
    - Metadata row: `flex-col md:flex-row` (stack mobile)
    - Separators: `hidden md:inline` (hide pipes on mobile)
    - Action buttons: `flex-wrap` (wrap mobile)

11. **`src/app/features/tasks/components/save-task-dialog/save-task-dialog.component.html`**
    - Width: `w-full max-w-[32rem]`
    - Padding: `p-4 md:p-6`
    - Max height: `max-h-[90vh] overflow-y-auto`
    - Form grid: `grid-cols-1 md:grid-cols-2`
    - Buttons: `flex-col-reverse md:flex-row` with `w-full md:w-auto`

12. **`src/app/features/tasks/components/save-project-dialog/save-project-dialog.component.html`**
    - Same responsive pattern as save-task-dialog

13. **`src/app/features/tasks/components/save-section-dialog/save-section-dialog.component.html`**
    - Same responsive pattern

### Other Dialog Components (2 files)

14. **`src/app/features/tasks/components/delete-task-dialog/delete-task-dialog.component.html`**
    - Responsive width and buttons

15. **`src/app/features/tasks/components/delete-section-dialog/delete-section-dialog.component.html`**
    - Responsive width and buttons

### Auth Pages (2 files)

16. **`src/app/features/auth/components/login-page/login-page.component.html`**
    - Outer container: add `p-4`
    - Card: `w-full max-w-[25rem]`

17. **`src/app/features/auth/components/register-page/register-page.component.html`**
    - Same pattern as login page

### Settings (1 file)

18. **`src/app/features/settings/components/settings-page/settings-page.html`**
    - Title: `text-lg md:text-xl`
    - Tabs: `overflow-x-auto` with `flex-shrink-0` on buttons

### Global Styles (1 file)

19. **`src/styles.scss`**
    - Add `.safe-area-bottom { padding-bottom: env(safe-area-inset-bottom); }`
    - Add `body { overscroll-behavior-y: none; }`
    - Add `html, body { overflow-x: hidden; }`
    - Add `.dialog-panel` responsive padding

---

## Implementation Steps (In Order)

### Phase 1: Foundation (Day 1)

1. ✅ Create `BreakpointService`
   - Install `@angular/cdk` if not present
   - Implement with `BreakpointObserver`
   - Export signals: `isMobile`, `isTablet`, `isDesktop`

2. ✅ Create `BottomNavComponent`
   - Template with three nav items
   - Styling: `fixed bottom-0 left-0 right-0 md:hidden`
   - Active state with blue highlighting

3. ✅ Update global styles
   - Safe area insets for iOS
   - Prevent horizontal scroll
   - Dialog responsive padding

### Phase 2: Root Layout (Day 1)

4. ✅ Update `app.component.html`
   - Sidebar: change to `hidden lg:flex`
   - Add bottom padding: `pb-16 md:pb-0`
   - Add `<app-bottom-nav>`

5. ✅ Update `app.component.ts`
   - Import and declare `BottomNavComponent`

**Test Checkpoint:** Verify sidebar hides on mobile, bottom nav appears, no regressions on desktop.

### Phase 3: Dialogs & Forms (Day 2)

6. ✅ Update all dialog templates
   - save-task-dialog
   - save-project-dialog
   - save-section-dialog
   - delete-task-dialog
   - delete-section-dialog
   - Pattern: `w-full max-w-[...]`, `p-4 md:p-6`, responsive buttons and grids

**Test Checkpoint:** Open all dialogs on mobile viewport, verify no overflow, buttons accessible.

### Phase 4: Auth & Settings (Day 2)

7. ✅ Update auth pages
   - login-page
   - register-page

8. ✅ Update settings page
   - Responsive tabs

**Test Checkpoint:** Navigate to login, register, settings on mobile viewport.

### Phase 5: Tasks Page (Day 3 - Most Complex)

9. ✅ Update `tasks-page.ts` state management
   - Inject `BreakpointService`
   - Add `mobileView` signal
   - Add computed helpers
   - Modify selection methods

10. ✅ Update `tasks-page.html`
    - Responsive widths
    - Conditional rendering with `@if`

11. ✅ Update `project-detail` component
    - Add inputs/outputs
    - Add back button to template
    - Responsive layout

12. ✅ Update `task-item` component
    - Wrap metadata on mobile
    - Hide separators on mobile

**Test Checkpoint:**
- Mobile: Tap project → see detail, tap back → see list
- Tablet: Two columns visible
- Desktop: No regressions

### Phase 6: Testing & Polish (Day 4)

13. ✅ Manual testing
    - Test all flows on 375px (iPhone), 768px (tablet), 1280px+ (desktop)
    - Check keyboard interactions
    - Verify no horizontal scroll

14. ✅ E2E test (optional but recommended)
    - Create `.playwright/tests/responsive-layout.spec.ts`
    - Test navigation visibility
    - Test tasks page view toggle

---

## Tailwind Class Patterns Reference

### Width
```
w-56       → hidden lg:flex lg:w-56   (Sidebar)
w-80       → w-full md:w-80          (Project list)
w-[25rem]  → w-full max-w-[25rem]   (Dialogs)
w-[32rem]  → w-full max-w-[32rem]   (Task dialog)
```

### Layout Direction
```
flex-row     → flex-col md:flex-row
grid-cols-2  → grid-cols-1 md:grid-cols-2
```

### Spacing
```
p-6       → p-4 md:p-6
gap-4     → gap-2 md:gap-4
```

### Typography
```
text-2xl  → text-xl md:text-2xl
```

### Visibility
```
Always visible desktop  → hidden lg:flex
Mobile only            → block md:hidden
```

---

## Verification (Post-Implementation)

### Manual Checklist

**Mobile (375px):**
- [ ] Bottom nav visible, sidebar hidden
- [ ] Tasks page shows project list
- [ ] Tap project → detail view with back button
- [ ] Back button → returns to project list
- [ ] All dialogs full-width with padding
- [ ] Forms stack vertically
- [ ] No horizontal scroll on any page
- [ ] Login/register pages don't overflow

**Tablet (768px):**
- [ ] Bottom nav hidden
- [ ] Sidebar still hidden
- [ ] Tasks page shows two columns
- [ ] Dialogs centered with max-width

**Desktop (1024px+):**
- [ ] Sidebar visible
- [ ] Bottom nav hidden
- [ ] All layouts work as before (no regressions)

### Testing Commands

```bash
# Frontend dev server (if not running)
cd frontend/flowie-app && npm start

# Manual testing via Chrome DevTools
# - Open https://localhost:4200
# - Toggle device toolbar (Ctrl+Shift+M)
# - Test at 375px, 768px, 1280px

# E2E tests (optional)
cd .playwright
playwright-cli open https://localhost:4200 --browser=chromium
```

---

## Success Criteria

✅ **No horizontal scrolling** at 320px width
✅ **All interactive elements** have 44px min touch target (mobile)
✅ **Navigation accessible** on all screen sizes
✅ **Dialogs usable** without zooming on mobile
✅ **Tasks page navigable** with intuitive mobile flow
✅ **Desktop experience preserved** (zero regressions)
✅ **Ready for Capacitor** (uses mobile-native patterns)

---

## Notes

- **Safe area insets:** Added for iOS notch/home indicator support
- **Bottom nav z-index:** Ensure it's above other content (`z-50`)
- **Drag-and-drop:** CDK drag-drop should work on touch, but test thoroughly
- **Rich text editor:** Quill editor in task dialog may need custom CSS for mobile
- **Real device testing:** Always test on real iOS/Android before finalizing

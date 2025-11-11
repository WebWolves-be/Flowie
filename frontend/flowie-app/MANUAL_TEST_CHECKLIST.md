# Manual E2E Testing Checklist

This checklist can be used to manually verify that the API integration is working correctly.

## Pre-Test Setup

- [ ] Backend is running on `http://localhost:5000`
- [ ] Backend database is seeded with test data
- [ ] Frontend is running on `http://localhost:4200`
- [ ] Browser DevTools is open (F12) with Network tab visible
- [ ] Console tab is visible to check for errors

## Test Session Information

**Date:** ___________  
**Tester:** ___________  
**Browser:** ___________  
**Backend Commit:** ___________  
**Frontend Commit:** ___________

---

## Dashboard Tests

### ✓ Test 1: Dashboard Loads

- [ ] Navigate to `http://localhost:4200`
- [ ] Redirects to `/dashboard`
- [ ] Dashboard page loads without errors
- [ ] Network request to `/api/projects` is visible
- [ ] Response status is 200
- [ ] No console errors

**Notes:** ___________

### ✓ Test 2: Dashboard Metrics Display

- [ ] "Totaal aantal taken" metric displays a number
- [ ] "Afgeronde taken" metric displays with trend
- [ ] "Gemiddelde voortgang" metric displays percentage
- [ ] "Actieve projecten" metric displays with trend
- [ ] "Afgeronde projecten" metric displays
- [ ] "Nog niet gestart" metric displays
- [ ] All metrics show data from backend (not all zeros)

**Notes:** ___________

---

## ProjectModel Tests

### ✓ Test 3: Load All Projects

- [ ] Navigate to `/taken` (Tasks page)
- [ ] Network request to `/api/projects` is visible
- [ ] Projects load and display in cards
- [ ] Each project shows: title, task count, progress bar
- [ ] Company logos/names are displayed correctly

**Notes:** ___________

### ✓ Test 4: Filter Projects by Company

- [ ] Click on "Immoseed" filter button
- [ ] Network request to `/api/projects?company=0` is visible
- [ ] Only Immoseed projects are displayed
- [ ] Click on "Novara Real Estate" filter
- [ ] Network request to `/api/projects?company=1` is visible
- [ ] Only Novara projects are displayed
- [ ] Click "All" to show all projects again

**Notes:** ___________

### ✓ Test 5: Create New ProjectModel

- [ ] Click "New ProjectModel" button (or similar)
- [ ] Fill in project form:
    - Title: "Test ProjectModel E2E"
    - Description: "This is a test"
    - Company: Select "Immoseed"
- [ ] Click Save/Submit
- [ ] Network request to `POST /api/projects` is visible
- [ ] Request body contains the form data
- [ ] Response status is 200/201/204
- [ ] ProjectModel list refreshes automatically
- [ ] New project appears in the list

**Notes:** ___________

### ✓ Test 6: Update Existing ProjectModel

- [ ] Click on a project to edit it
- [ ] Change the title to "Updated Test ProjectModel"
- [ ] Click Save
- [ ] Network request to `PUT /api/projects` is visible
- [ ] Response status is 200/204
- [ ] ProjectModel title updates in the list

**Notes:** ___________

---

## Task Tests

### ✓ Test 7: Load Tasks for a ProjectModel

- [ ] Click on a project card to view its tasks
- [ ] Network request to `/api/tasks?projectId=X&onlyShowMyTasks=false` is visible
- [ ] Tasks load and display in list
- [ ] Each task shows: title, type, status, assignee, due date
- [ ] Task progress indicators display correctly

**Notes:** ___________

### ✓ Test 8: Filter "My Tasks"

- [ ] Toggle "Show only my tasks" checkbox/button
- [ ] Network request to `/api/tasks?projectId=X&onlyShowMyTasks=true` is visible
- [ ] Only tasks assigned to current user are shown
- [ ] Toggle off to show all tasks again

**Notes:** ___________

### ✓ Test 9: Create New Task

- [ ] With a project selected, click "New Task" button
- [ ] Fill in task form:
    - Title: "Test Task E2E"
    - Type: Select a task type
    - Due Date: Select a future date
    - Assignee: Select an employee
    - Description: "This is a test task"
- [ ] Click Save
- [ ] Network request to `POST /api/tasks` is visible
- [ ] Request body contains task data with correct IDs
- [ ] Response status is 200/201/204
- [ ] Task list refreshes
- [ ] New task appears in the list with correct details

**Notes:** ___________

### ✓ Test 10: Update Task

- [ ] Click on a task to edit it
- [ ] Change title to "Updated Test Task"
- [ ] Change status to "Ongoing" or "Done"
- [ ] Click Save
- [ ] Network request to `PUT /api/tasks` is visible
- [ ] Response status is 200/204
- [ ] Task updates in the list

**Notes:** ___________

### ✓ Test 11: Update Task Status

- [ ] Click on task status dropdown/button
- [ ] Change status (e.g., from Pending to Ongoing)
- [ ] Network request to `PATCH /api/tasks/{id}/status` is visible
- [ ] Response status is 200/204
- [ ] Task status updates immediately in UI
- [ ] Progress indicators update if applicable

**Notes:** ___________

### ✓ Test 12: Delete Task

- [ ] Click delete button on a task (if available)
- [ ] Confirm deletion
- [ ] Network request to `DELETE /api/tasks/{id}` is visible
- [ ] Response status is 200/204
- [ ] Task disappears from list

**Notes:** ___________

---

## Task Type Tests

### ✓ Test 13: Load Task Types

- [ ] Navigate to `/instellingen` (Settings page)
- [ ] Network request to `/api/task-types` is visible
- [ ] Response status is 200
- [ ] Task types are listed
- [ ] All expected task types are shown (Compromis, Ontwerp, etc.)

**Notes:** ___________

### ✓ Test 14: Create Task Type

- [ ] In settings, enter new task type name: "E2E Test Type"
- [ ] Click Add/Create button
- [ ] Network request to `POST /api/task-types` is visible
- [ ] Request body: `{"name": "E2E Test Type"}`
- [ ] Response status is 200/201/204
- [ ] Task types list refreshes
- [ ] New type appears in the list

**Notes:** ___________

### ✓ Test 15: Delete Task Type

- [ ] Click delete button next to a task type
- [ ] Confirm deletion (if prompted)
- [ ] Network request to `DELETE /api/task-types/{id}` is visible
- [ ] Response status is 200/204
- [ ] Task type disappears from list

**Notes:** ___________

---

## Error Handling Tests

### ✓ Test 16: Backend Unavailable

- [ ] Stop the backend server
- [ ] Refresh the frontend page
- [ ] Attempt to load projects
- [ ] Loading spinner/indicator shows
- [ ] Error is logged to console
- [ ] App doesn't crash
- [ ] User sees empty state or error message (if implemented)

**Notes:** ___________

### ✓ Test 17: Invalid Data Submission

- [ ] Try to create a project with empty title
- [ ] Submit the form
- [ ] Backend returns 400 error
- [ ] Error is logged to console
- [ ] Form validation prevents submission (if implemented)

**Notes:** ___________

### ✓ Test 18: CORS Configuration

- [ ] Verify all API requests succeed from `http://localhost:4200`
- [ ] Check response headers include CORS headers
- [ ] No CORS errors in console

**Notes:** ___________

---

## Performance Tests

### ✓ Test 19: Multiple Rapid Requests

- [ ] Quickly switch between company filters 5 times
- [ ] All requests complete successfully
- [ ] Latest filter selection's data is displayed
- [ ] No stale data shown
- [ ] UI remains responsive

**Notes:** ___________

### ✓ Test 20: Large ProjectModel with Many Tasks

- [ ] Select a project with 10+ tasks
- [ ] Observe load time (should be < 2 seconds)
- [ ] All tasks render correctly
- [ ] UI remains responsive when scrolling
- [ ] No performance warnings in console

**Notes:** ___________

---

## EmployeeModel Data Tests

### ✓ Test 21: EmployeeModel List (Fallback to Mock)

- [ ] Navigate to create task form
- [ ] EmployeeModel dropdown shows employee list
- [ ] Check Network tab - request to `/api/employees` might fail (404)
- [ ] Despite 404, employees still display (fallback mock data)
- [ ] Can select an employee from the list
- [ ] No console errors prevent task creation

**Notes:** ___________  
**Expected:** This test may show a 404 for employees endpoint - this is expected behavior.

---

## Browser Compatibility Tests

### ✓ Test 22: Chrome/Edge

- [ ] All tests pass in Chrome/Edge
- [ ] No browser-specific errors

**Notes:** ___________

### ✓ Test 23: Firefox

- [ ] All tests pass in Firefox
- [ ] No browser-specific errors

**Notes:** ___________

### ✓ Test 24: Safari (if available)

- [ ] All tests pass in Safari
- [ ] No browser-specific errors

**Notes:** ___________

---

## API Request Validation

For each test, verify in Network tab:

### Request Headers

- [ ] `Content-Type: application/json` for POST/PUT requests
- [ ] Requests originate from `http://localhost:4200`

### Response Headers

- [ ] `Content-Type: application/json`
- [ ] CORS headers present: `Access-Control-Allow-Origin`

### Request Payload Validation

- [ ] POST/PUT requests have valid JSON bodies
- [ ] All required fields are present
- [ ] Data types match backend expectations (numbers, strings, enums)

### Response Validation

- [ ] Responses are valid JSON
- [ ] Response structure matches expected DTOs
- [ ] Arrays contain objects with correct properties
- [ ] Enum values are correct

---

## Test Results Summary

**Total Tests:** 24  
**Tests Passed:** _____ / 24  
**Tests Failed:** _____  
**Tests Skipped:** _____

**Overall Result:** ☐ PASS ☐ FAIL ☐ PARTIAL

### Critical Issues Found:

1. ___________
2. ___________
3. ___________

### Minor Issues Found:

1. ___________
2. ___________
3. ___________

### Recommendations:

1. ___________
2. ___________
3. ___________

---

## Sign-Off

**Tester Signature:** ___________  
**Date:** ___________

**Approved By:** ___________  
**Date:** ___________

---

## Notes for Future Testing

1. Consider adding automated e2e tests with Playwright or Cypress
2. Add visual regression testing
3. Implement more robust error messages in UI
4. Add loading indicators/skeletons
5. Implement employee endpoint in backend
6. Add integration tests in CI/CD pipeline

## References

- See `E2E_TESTING.md` for detailed testing procedures
- See `API_INTEGRATION.md` for API documentation
- See `IMPLEMENTATION_SUMMARY.md` for implementation details

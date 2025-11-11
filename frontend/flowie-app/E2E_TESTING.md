# End-to-End (E2E) Testing Guide

This document describes how to perform end-to-end testing of the API integration between the Angular frontend and .NET
backend.

## Prerequisites

Before running e2e tests, ensure:

1. **Backend is running**: The .NET API must be running on `http://localhost:5000`
2. **Database is seeded**: The database should contain test data
3. **Frontend dependencies are installed**: Run `npm install` in `frontend/flowie-app`

## Starting the Backend

```bash
cd backend/Flowie.Api
dotnet run --urls "http://localhost:5000"
```

Wait for the message: `Now listening on: http://localhost:5000`

## Starting the Frontend

In a separate terminal:

```bash
cd frontend/flowie-app
npm start
```

The application will be available at `http://localhost:4200`

## Manual E2E Test Cases

### Test Case 1: Load Projects

**Steps:**

1. Navigate to `http://localhost:4200`
2. You should be redirected to the dashboard
3. Open browser DevTools > Network tab
4. Check for a GET request to `http://localhost:5000/api/projects`
5. Verify the response contains project data
6. Verify projects are displayed in the UI

**Expected Result:**

- ✅ API call successful (status 200)
- ✅ Projects load and display in dashboard
- ✅ No console errors

### Test Case 2: Filter Projects by Company

**Steps:**

1. Navigate to `http://localhost:4200/taken`
2. Open Network tab
3. Click on company filter (Immoseed or Novara Real Estate)
4. Check for GET request to `http://localhost:5000/api/projects?company=X`
5. Verify filtered projects display

**Expected Result:**

- ✅ API call includes company parameter
- ✅ Only projects for selected company are shown
- ✅ ProjectModel count updates correctly

### Test Case 3: Load Tasks for a ProjectModel

**Steps:**

1. Navigate to tasks page
2. Click on a project card
3. Check Network tab for GET request to `http://localhost:5000/api/tasks?projectId=X&onlyShowMyTasks=false`
4. Verify tasks for the project are displayed

**Expected Result:**

- ✅ API call successful with correct projectId
- ✅ Tasks load and display
- ✅ Task details (title, status, assignee, due date) are visible

### Test Case 4: Create a New ProjectModel

**Steps:**

1. Navigate to tasks page
2. Click "New ProjectModel" button
3. Fill in project form:
    - Title: "E2E Test ProjectModel"
    - Description: "Testing API integration"
    - Company: "Immoseed"
4. Click Save
5. Check Network tab for POST request to `http://localhost:5000/api/projects`
6. Verify request body contains project data
7. Verify new project appears in the list

**Expected Result:**

- ✅ POST request successful (status 200 or 201)
- ✅ ProjectModel list refreshes automatically
- ✅ New project appears in the list

### Test Case 5: Create a New Task

**Steps:**

1. Navigate to tasks page
2. Select a project
3. Click "New Task" button
4. Fill in task form:
    - Title: "E2E Test Task"
    - Type: Select a task type
    - Due Date: Select a date
    - Assignee: Select an employee
    - Description: "Testing task creation"
5. Click Save
6. Check Network tab for POST request to `http://localhost:5000/api/tasks`
7. Verify new task appears in the task list

**Expected Result:**

- ✅ POST request successful
- ✅ Task list refreshes automatically
- ✅ New task appears with correct details

### Test Case 6: Update Task Status

**Steps:**

1. Navigate to tasks page
2. Select a project with tasks
3. Click on a task to change its status
4. Check Network tab for PATCH request to `http://localhost:5000/api/tasks/{id}/status`
5. Verify task status updates in UI

**Expected Result:**

- ✅ PATCH request successful
- ✅ Task status updates immediately
- ✅ Progress indicators update if applicable

### Test Case 7: Load Task Types

**Steps:**

1. Navigate to settings page (`http://localhost:4200/instellingen`)
2. Check Network tab for GET request to `http://localhost:5000/api/task-types`
3. Verify task types are listed

**Expected Result:**

- ✅ API call successful
- ✅ Task types display in settings
- ✅ All task types from backend are shown

### Test Case 8: Create Task Type

**Steps:**

1. In settings page, enter a new task type name: "E2E Test Type"
2. Click Add button
3. Check Network tab for POST request to `http://localhost:5000/api/task-types`
4. Verify task type list refreshes
5. Verify new type appears in the list

**Expected Result:**

- ✅ POST request successful
- ✅ List refreshes automatically
- ✅ New task type appears

### Test Case 9: Delete Task Type

**Steps:**

1. In settings page, click delete on a task type
2. Check Network tab for DELETE request to `http://localhost:5000/api/task-types/{id}`
3. Verify task type is removed from list

**Expected Result:**

- ✅ DELETE request successful
- ✅ Task type disappears from list
- ✅ No errors in console

### Test Case 10: Dashboard Metrics

**Steps:**

1. Navigate to dashboard
2. Wait for data to load
3. Verify metrics display:
    - Total tasks
    - Completed tasks
    - Overall progress
    - Active projects
    - Completed projects
4. Verify metrics match data from backend

**Expected Result:**

- ✅ All metrics display correctly
- ✅ Metrics calculated from real API data
- ✅ Progress bars and trends show

## Automated E2E Test Script

You can run this bash script to test the API endpoints directly:

```bash
#!/bin/bash

# E2E API Test Script
API_URL="http://localhost:5000"

echo "Testing Flowie API Integration..."
echo "================================="

# Test 1: Get Projects
echo -e "\n1. Testing GET /api/projects"
curl -s -o /dev/null -w "Status: %{http_code}\n" "$API_URL/api/projects"

# Test 2: Get Projects with Company Filter
echo -e "\n2. Testing GET /api/projects?company=0"
curl -s -o /dev/null -w "Status: %{http_code}\n" "$API_URL/api/projects?company=0"

# Test 3: Get Tasks
echo -e "\n3. Testing GET /api/tasks?projectId=1&onlyShowMyTasks=false"
curl -s -o /dev/null -w "Status: %{http_code}\n" "$API_URL/api/tasks?projectId=1&onlyShowMyTasks=false"

# Test 4: Get Task Types
echo -e "\n4. Testing GET /api/task-types"
curl -s -o /dev/null -w "Status: %{http_code}\n" "$API_URL/api/task-types"

# Test 5: Create ProjectModel
echo -e "\n5. Testing POST /api/projects"
curl -s -o /dev/null -w "Status: %{http_code}\n" \
  -X POST "$API_URL/api/projects" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "E2E Test ProjectModel",
    "description": "Testing API integration",
    "company": 0
  }'

# Test 6: Create Task Type
echo -e "\n6. Testing POST /api/task-types"
curl -s -o /dev/null -w "Status: %{http_code}\n" \
  -X POST "$API_URL/api/task-types" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "E2E Test Type"
  }'

echo -e "\n================================="
echo "E2E Tests Complete!"
echo "All status codes should be 200, 201, or 204"
```

Save this as `test-api.sh` and run:

```bash
chmod +x test-api.sh
./test-api.sh
```

## Error Handling Tests

### Test Case 11: Backend Down

**Steps:**

1. Stop the backend
2. Refresh the frontend
3. Try to load projects

**Expected Result:**

- ✅ Loading state activates
- ✅ Error logged to console
- ✅ Empty state or error message shown
- ✅ App doesn't crash

### Test Case 12: Invalid Data

**Steps:**

1. Try to create a project with missing required fields
2. Check validation

**Expected Result:**

- ✅ Backend returns 400 error
- ✅ Error handled gracefully
- ✅ User-friendly error message (future enhancement)

### Test Case 13: CORS Issues

**Steps:**

1. Check if requests from `http://localhost:4200` work
2. Verify CORS headers in backend

**Expected Result:**

- ✅ Requests succeed from frontend origin
- ✅ No CORS errors in console

## Performance Tests

### Test Case 14: Multiple Rapid Requests

**Steps:**

1. Quickly switch between company filters multiple times
2. Check if all requests complete
3. Verify no race conditions

**Expected Result:**

- ✅ All requests complete successfully
- ✅ Latest request's data is displayed
- ✅ No stale data shown

### Test Case 15: Large Dataset

**Steps:**

1. Load a project with many tasks
2. Measure load time
3. Check UI responsiveness

**Expected Result:**

- ✅ Data loads in reasonable time (<2s)
- ✅ UI remains responsive
- ✅ No performance degradation

## Browser Compatibility Tests

Test in the following browsers:

- [ ] Chrome/Edge (latest)
- [ ] Firefox (latest)
- [ ] Safari (latest)

## Test Results Template

Use this template to document test results:

```
Test Date: YYYY-MM-DD
Tester: [Name]
Environment: Development/Staging/Production
Backend Version: [commit hash]
Frontend Version: [commit hash]

| Test Case | Status | Notes |
|-----------|--------|-------|
| TC1: Load Projects | ✅ PASS | |
| TC2: Filter Projects | ✅ PASS | |
| TC3: Load Tasks | ✅ PASS | |
| TC4: Create ProjectModel | ✅ PASS | |
| TC5: Create Task | ✅ PASS | |
| TC6: Update Task Status | ✅ PASS | |
| TC7: Load Task Types | ✅ PASS | |
| TC8: Create Task Type | ✅ PASS | |
| TC9: Delete Task Type | ✅ PASS | |
| TC10: Dashboard Metrics | ✅ PASS | |
| TC11: Backend Down | ✅ PASS | |
| TC12: Invalid Data | ✅ PASS | |
| TC13: CORS | ✅ PASS | |
| TC14: Rapid Requests | ✅ PASS | |
| TC15: Large Dataset | ✅ PASS | |

Overall Result: PASS/FAIL
```

## Known Limitations

1. **No EmployeeModel Endpoint**: The `/api/employees` endpoint doesn't exist yet, so employee data falls back to mock
   data. This is expected behavior.

2. **No E2E Testing Framework**: Currently, tests are manual. Consider adding:
    - Playwright for automated browser testing
    - Cypress for component and integration testing
    - Jest for API endpoint testing

## Future Enhancements

1. **Automated E2E Tests**: Set up Playwright or Cypress
2. **CI/CD Integration**: Run e2e tests in GitHub Actions
3. **Visual Regression Testing**: Compare UI screenshots
4. **Load Testing**: Test API performance under load
5. **Security Testing**: Test for common vulnerabilities

## Troubleshooting

### Backend Won't Start

**Problem**: Database connection issues

**Solution**:

- Ensure LocalDB is running
- Check connection string in `appsettings.Development.json`
- Run migrations: `dotnet ef database update`

### CORS Errors

**Problem**: Requests blocked by CORS

**Solution**:

- Check backend CORS configuration in `Program.cs`
- Verify frontend URL is in allowed origins
- Clear browser cache

### No Data Loading

**Problem**: Empty lists in frontend

**Solution**:

- Check Network tab for failed requests
- Verify backend is running on correct port
- Check environment.ts has correct API URL
- Seed database with test data

### TypeScript Errors

**Problem**: Build fails with type errors

**Solution**:

- Run `npm install` to ensure dependencies are installed
- Check that DTO interfaces match backend responses
- Clear `node_modules` and reinstall if needed

## Contact

For questions or issues with e2e testing, refer to:

- `API_INTEGRATION.md` for API documentation
- `IMPLEMENTATION_SUMMARY.md` for implementation details

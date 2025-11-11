# API Integration Documentation

This document describes the API integration between the Angular frontend and the .NET backend.

## Overview

The frontend now uses Angular's `HttpClient` to make API calls to the backend instead of using mock data. All API
communication is handled through dedicated service classes.

## Architecture

```
Components
    ↓
Facade Services (TaskFacade, TaskTypeFacade)
    ↓
API Services (ProjectApiService, TaskApiService, TaskTypeApiService)
    ↓
Backend API (.NET Minimal API)
```

## Environment Configuration

API URLs are configured in environment files:

- **Development**: `src/environments/environment.ts` - Points to `http://localhost:5000`
- **Production**: `src/environments/environment.prod.ts` - Points to `https://api.flowie.com`

Update these URLs as needed for your environment.

## API Services

### ProjectApiService (`src/app/core/services/project-api.service.ts`)

Handles all project-related API calls:

- `getProjects(company?: Company)` - GET `/api/projects` - Fetches projects with optional company filter
- `getProjectById(id: number)` - GET `/api/projects/{id}` - Fetches a single project
- `createProject(request)` - POST `/api/projects` - Creates a new project
- `updateProject(request)` - PUT `/api/projects` - Updates an existing project

### TaskApiService (`src/app/core/services/task-api.service.ts`)

Handles all task-related API calls:

- `getTasks(projectId, onlyShowMyTasks)` - GET `/api/tasks` - Fetches tasks for a project
- `getTaskById(id)` - GET `/api/tasks/{id}` - Fetches a single task
- `createTask(request)` - POST `/api/tasks` - Creates a new task
- `updateTask(request)` - PUT `/api/tasks` - Updates an existing task
- `updateTaskStatus(taskId, request)` - PATCH `/api/tasks/{id}/status` - Updates task status
- `deleteTask(id)` - DELETE `/api/tasks/{id}` - Deletes a task

### TaskTypeApiService (`src/app/core/services/task-type-api.service.ts`)

Handles all task type API calls:

- `getTaskTypes()` - GET `/api/task-types` - Fetches all task types
- `createTaskType(request)` - POST `/api/task-types` - Creates a new task type
- `deleteTaskType(id)` - DELETE `/api/task-types/{id}` - Deletes a task type

### EmployeeApiService (`src/app/core/services/employee-api.service.ts`)

**Note**: This service is a placeholder. The backend does not currently have an employees endpoint.
EmployeeModel data is currently handled through:

- Task assignees (embedded in task data)
- Auth endpoints (when registering/authenticating users)

The `TaskFacade.getEmployees()` method falls back to mock data when the API call fails.

## Facade Services

### TaskFacade

Updated to use API services for:

- Loading projects (with company filtering)
- Loading tasks (with "my tasks" filtering)
- Creating and updating projects
- Creating and updating tasks
- Loading employees (with fallback to mock data)

### TaskTypeFacade

Updated to use API services for:

- Loading task types
- Creating task types
- Deleting task types

### DashboardFacade

No changes needed - it depends on `TaskFacade` which now uses real API calls.

## Data Mapping

The API DTOs from the backend are mapped to frontend models in the facade services:

- `ProjectDto` → `ProjectModel` model
- `TaskModel` → `Task` model
- `TaskTypeDto` → `TaskType` model

The progress field for projects is calculated on the frontend:

```typescript
progress: dto.taskCount > 0
  ? Math.round((dto.completedTaskCount / dto.taskCount) * 100)
  : 0
```

## Error Handling

All API calls include error handlers that:

1. Log errors to the console
2. Set appropriate loading states
3. Clear/reset data on error
4. For employees, fall back to mock data if the API fails

Example:

```typescript
this.projectApi.getProjects(company).subscribe({
  next: (response) => {
    // Handle success
  },
  error: (error) => {
    console.error('Error loading projects:', error);
    this.#projects.set([]);
    this.#isLoadingProjects.set(false);
  }
});
```

## Testing the Integration

### Prerequisites

1. Ensure the .NET backend is running on `http://localhost:5000`
2. The database should be seeded with initial data

### Running the Frontend

```bash
cd frontend/flowie-app
npm install
npm start
```

The app will be available at `http://localhost:4200`

### Testing Checklist

- [ ] Projects load from the backend
- [ ] Company filter works (Immoseed, Novara Real Estate, All)
- [ ] Tasks load when clicking on a project
- [ ] "My Tasks" filter works
- [ ] Creating a new project works
- [ ] Creating a new task works
- [ ] Task types load in settings
- [ ] Adding a new task type works
- [ ] Deleting a task type works

## Known Issues & Future Work

### Missing Backend Endpoints

1. **Employees Endpoint**: There's no dedicated endpoint to fetch all employees. Currently:
    - EmployeeModel data comes from task assignees
    - The frontend falls back to mock data
    - **Recommendation**: Create `/api/employees` endpoint in the backend

### Potential Improvements

1. **Error Handling**: Add user-friendly error messages (toasts/notifications)
2. **Loading States**: Add loading spinners/skeletons
3. **Optimistic Updates**: Update UI immediately before API confirmation
4. **Caching**: Implement caching strategy for frequently accessed data
5. **Interceptors**: Add HTTP interceptor for:
    - Authentication token handling
    - Global error handling
    - Request/response logging
6. **Retry Logic**: Add retry logic for failed requests
7. **Response Types**: Ensure backend response types match exactly (currently some assumptions)

## Backend API Reference

The backend uses minimal API pattern with the following structure:

```
/api/projects
  GET    /              - Get all projects (query: ?company=)
  GET    /{id}          - Get project by ID
  POST   /              - Create project
  PUT    /              - Update project

/api/tasks
  GET    /              - Get tasks (query: ?projectId=&onlyShowMyTasks=)
  GET    /{id}          - Get task by ID
  POST   /              - Create task
  PUT    /              - Update task
  PATCH  /{id}/status   - Update task status
  DELETE /{id}          - Delete task

/api/task-types
  GET    /              - Get all task types
  POST   /              - Create task type
  DELETE /{id}          - Delete task type
```

## CORS Configuration

The backend is configured to accept requests from:

- `http://localhost:4200`
- `https://localhost:4200`

If you're running on a different port or domain, update the CORS configuration in the backend's `Program.cs`.

## Troubleshooting

### CORS Errors

If you see CORS errors in the browser console:

1. Check that the backend is running
2. Verify the backend CORS configuration includes your frontend URL
3. Check that the frontend environment file has the correct backend URL

### 404 Errors

If API calls return 404:

1. Verify the backend is running
2. Check the API URL in environment files
3. Verify the endpoint exists in the backend

### Data Not Loading

If data doesn't load:

1. Open browser DevTools Network tab
2. Check if API requests are being made
3. Check request/response for errors
4. Verify backend database has data

### EmployeeModel Data Not Loading

This is expected - the backend doesn't have an employees endpoint yet. The frontend falls back to mock data for
employees.

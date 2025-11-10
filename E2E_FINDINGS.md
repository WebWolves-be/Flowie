# E2E Testing Findings - Refactoring Investigation

## Investigation Date
2025-11-10

## Summary
After investigating the backend API structure to prepare for e2e testing, I discovered a critical issue with the previous refactoring attempt. The backend DTOs and frontend models are **NOT 1-to-1 duplicates** - there are significant structural differences that require mapping.

## Backend API Structure (Actual C# DTOs)

### Projects Endpoint
```csharp
// Backend: Features/Projects/GetProjects/GetProjectsQueryResult.cs
record ProjectDto(
    int ProjectId,      // Not "id"
    string Title,
    Company Company,
    int TaskCount,
    int CompletedTaskCount  // No "progress" field
);
```

### Tasks Endpoint
```csharp
// Backend: Features/Tasks/GetTasks/GetTasksQueryResult.cs
record TaskDto(
    int TaskId,         // Not "id"
    int ProjectId,
    int? ParentTaskId,
    string Title,
    string? Description,
    int TypeId,
    string TypeName,
    DateOnly? DueDate,
    TaskStatus Status,
    int? EmployeeId,    // Separate fields, not nested
    string? EmployeeName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? CompletedAt,
    int SubtaskCount,
    int CompletedSubtaskCount,
    IEnumerable<SubtaskDto> Subtasks
);

record SubtaskDto(
    int TaskId,         // Not "id"
    int? ParentTaskId,
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskStatus Status,
    int? EmployeeId,    // Separate fields
    string? EmployeeName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? CompletedAt
);
```

## Frontend Model Structure (Required by UI)

### Project Model
```typescript
interface Project {
    id: number;                    // <-- Different name
    title: string;
    description?: string | null;   // <-- Not in backend list response
    taskCount: number;
    completedTaskCount: number;
    progress: number;              // <-- Calculated client-side
    company: Company;
}
```

### Task Model
```typescript
interface Task {
    id: number;                    // <-- Different name
    projectId: number;
    parentTaskId?: number | null;
    title: string;
    description?: string | null;
    typeId?: number;
    typeName?: string;
    status?: TaskStatus;
    statusName?: string;
    dueDate?: string | null;
    progress?: number;
    assignee: {                    // <-- Nested structure
        id?: number | null;
        name: string;
    };
    createdAt?: string;
    updatedAt?: string | null;
    completedAt?: string | null;
    subtaskCount?: number;
    completedSubtaskCount?: number;
    subtasks?: Subtask[];
}
```

## Key Differences Requiring Mapping

### 1. Field Name Differences
- **Backend**: `ProjectId`, `TaskId` (PascalCase)
- **Frontend**: `id` (camelCase and different name)

**Impact**: Direct assignment would fail. Need mapping: `id: dto.projectId`

### 2. Structural Differences
- **Backend**: `int? EmployeeId`, `string? EmployeeName` (separate fields)
- **Frontend**: `assignee: { id?: number | null; name: string }` (nested object)

**Impact**: Cannot directly assign. Need transformation:
```typescript
assignee: {
  id: dto.employeeId,
  name: dto.employeeName
}
```

### 3. Calculated Fields
- **Frontend**: Requires `progress` field
- **Backend**: Only provides `taskCount` and `completedTaskCount`

**Impact**: Must calculate:
```typescript
progress: dto.taskCount > 0 
  ? Math.round((dto.completedTaskCount / dto.taskCount) * 100) 
  : 0
```

### 4. Missing Fields
- **Frontend**: Needs `description` in project lists
- **Backend**: Doesn't return it in list endpoint (only in GetById)

**Impact**: Set to `undefined` in list view

## JSON Serialization Consideration

.NET typically serializes to camelCase by default in minimal APIs, so:
- `ProjectId` → `projectId` in JSON
- `TaskId` → `taskId` in JSON
- `EmployeeId` → `employeeId` in JSON

However, the frontend models use different names:
- `projectId` → needs to become `id`
- `taskId` → needs to become `id`
- Separate employee fields → need to become nested `assignee` object

## Why the Original Code Was Correct

The original facade code with manual mapping was **necessary and correct**:

```typescript
// task.facade.ts - Original (CORRECT)
this.projectApi.getProjects(company).subscribe({
  next: (response) => {
    const projects: Project[] = response.projects.map((dto) => ({
      id: dto.projectId,          // Name transformation
      title: dto.title,
      description: undefined,      // Not in backend response
      taskCount: dto.taskCount,
      completedTaskCount: dto.completedTaskCount,
      progress: dto.taskCount > 0  // Calculated field
        ? Math.round((dto.completedTaskCount / dto.taskCount) * 100)
        : 0,
      company: dto.company,
    }));
    this.#projects.set(projects);
  }
});
```

## Actions Taken

### 1. Reverted Changes
- ✅ Restored all model files (project.model.ts, task.model.ts, employee.model.ts, subtask.model.ts)
- ✅ Restored all mapping code in facades
- ✅ Restored all component imports to use models (not DTOs)
- ✅ Verified build passes

### 2. Build Verification
```bash
cd frontend/flowie-app
npm install
npm run build
# Result: ✅ SUCCESS - Build completes without errors
```

## Solutions for Future Refactoring

If we want to eliminate duplication, there are two approaches:

### Option A: Update Backend to Match Frontend
Modify backend DTOs to use frontend's structure:
- Rename `ProjectId` → `Id`
- Rename `TaskId` → `Id`  
- Nest employee fields: `Assignee: { Id, Name }`
- Add `Progress` calculation in backend
- Include `Description` in project lists

**Pros**: Frontend simpler, no mapping needed
**Cons**: Backend changes required, API contract changes

### Option B: Accept the Mapping Layer
Keep current structure with mapping in facades.

**Pros**: Clear separation of concerns, backend and frontend can evolve independently
**Cons**: Small amount of "duplicate" code (but it's transformation, not duplication)

## Recommendation

**Keep the current approach with mapping**. The mapping code is not duplication - it's a necessary transformation layer that:
1. Converts backend naming conventions to frontend conventions
2. Transforms flat structures to nested structures
3. Calculates derived fields
4. Handles missing fields

This is a standard pattern in API integration and provides flexibility for backend and frontend to evolve independently.

## E2E Testing Status

### Build Status
✅ **PASS** - Frontend builds successfully

### Manual E2E Testing Required
To fully validate, need to:
1. Start backend: `cd backend/Flowie.Api && dotnet run`
2. Start frontend: `cd frontend/flowie-app && npm start`
3. Follow checklist in `MANUAL_TEST_CHECKLIST.md`
4. Verify all API calls work correctly
5. Verify data displays correctly in UI

### Automated E2E Testing Script
Use `test-api-integration.sh` to test backend endpoints:
```bash
cd frontend/flowie-app
chmod +x test-api-integration.sh
./test-api-integration.sh
```

## Conclusion

The refactoring attempt to "eliminate duplicate models" was based on the incorrect assumption that backend DTOs and frontend models were identical. Investigation revealed significant structural differences that require the mapping layer. The original code was correct and has been restored.

For true e2e validation, the backend and frontend need to be run together and tested according to the procedures in `E2E_TESTING.md` and `MANUAL_TEST_CHECKLIST.md`.

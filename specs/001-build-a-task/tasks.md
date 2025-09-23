# Tasks: Task-based Workflow for Real Estate Projects (Backend)

**Input**: Design documents from `/specs/001-build-a-task/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)
```
1. Load plan.md and design docs
2. Generate tasks by category with dependencies and parallelization
3. Ensure tests precede implementation (TDD) and coverage goals in CI
4. Return SUCCESS with tasks ready for execution
```

## Phase 3.1: Setup
- [x] T001 Enable nullable + analyzers + warnings-as-errors (backend/Flowie/Flowie.csproj)
- [x] T002 Add packages: MediatR, MediatR.Extensions.Microsoft.DependencyInjection, FluentValidation, EFCore (SqlServer + Sqlite), Swashbuckle.AspNetCore
- [x] T003 [P] Create test project backend/Flowie.Tests (xUnit + FluentAssertions); setup coverage (≥80%)

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
Contract tests (from contracts/openapi.yaml):
- [x] T004 [P] POST /api/projects → tests/contract/Projects_PostTests.cs
- [x] T005 [P] GET /api/projects?company= → tests/contract/Projects_GetTests.cs
- [x] T006 [P] POST /api/projects/{id}/tasks → tests/contract/Tasks_PostTests.cs
- [x] T007 [P] POST /api/tasks/{id}/status → tests/contract/Tasks_Status_PostTests.cs
- [x] T008 [P] GET /api/tasks/{id} → tests/contract/Tasks_GetTests.cs
- [x] T009 [P] PATCH /api/projects/{id} → tests/contract/Projects_PatchTests.cs
- [x] T010 [P] PATCH /api/tasks/{id} → tests/contract/Tasks_PatchTests.cs
- [x] T011 [P] GET /api/projects/{id} → tests/contract/Projects_GetByIdTests.cs
- [x] T012 [P] GET /api/projects/{id}/tasks → tests/contract/Projects_Tasks_GetTests.cs
- [x] T013 [P] POST /api/tasks/{id}/subtasks → tests/contract/Subtasks_PostTests.cs

Integration tests (from user stories and rules):
- [x] T014 [P] Subtasks auto-complete parent when all children Done → tests/integration/Subtasks_AutoCompleteTests.cs
- [x] T015 [P] Task status flow Pending→Ongoing→Done audit trail → tests/integration/Task_StatusFlow_AuditTests.cs
- [x] T016 [P] Project filtering by company → tests/integration/Projects_FilterByCompanyTests.cs

## Phase 3.3: Core Implementation
Models and Db:
- [x] T017 Create EF entities and DbContext per data-model.md (backend/Flowie/Shared/Domain/Entities/*.cs; Db in Infrastructure/Database)
- [x] T018 EF Core migrations; configure Sqlite for dev, SqlServer for prod

Vertical Slices + Handlers (Minimal API + MediatR):
- [ ] T019 [P] Projects slice: CreateProject (command+handler), GetById (query+handler), List (query+handler)
- [ ] T020 [P] Tasks slice: CreateTask, GetById, UpdateTask, ArchiveTask
- [ ] T021 [P] Tasks slice: ChangeTaskStatus, Subtasks Create/List
- [ ] T022 [P] TaskTypes slice: Create, Update, Deactivate, List

Validation, Mapping, and Endpoint Wiring:
- [ ] T023 Validators (FluentValidation) for all commands/queries
- [ ] T024 Map Minimal API endpoints to MediatR (Program.cs or endpoint registration files)
- [ ] T025 Seed initial TaskTypes

## Phase 3.4: Integration
- [ ] T026 Add structured logging and correlation IDs (Middleware)
- [ ] T027 Add OpenAPI/Swagger UI and bind to openapi.yaml shapes
- [ ] T028 Ensure pagination and N+1 protections for listing endpoints

## Phase 3.5: Polish
- [ ] T029 [P] Unit tests for validators and handlers (backend/Flowie.Tests/unit/*)
- [ ] T030 Performance smoke tests (typical endpoints < 300ms locally)
- [ ] T031 [P] Update quickstart steps and verify CI coverage ≥ 80%
- [ ] T032 Remove dead code; analyzer warnings = 0; ensure nullable warnings resolved

## Dependencies
- Setup (T001–T003) before tests and implementation
- Contract/Integration tests (T004–T016) before core implementation (T017–T025)
- Entities/Db (T017–T018) before handlers (T019–T022)
- Validators (T023) before endpoint mapping (T024)
- Core features before integration polish (T026–T032)

## Parallel Execution Examples
```
# Launch contract tests in parallel after setup:
Task: "T004"
Task: "T005"
Task: "T006"
Task: "T007"
Task: "T008"

# Implement vertical slices in parallel (different files):
Task: "T019"
Task: "T020"
Task: "T021"
Task: "T022"
```

---

*Based on Constitution v1.0.0 - See `/memory/constitution.md`*

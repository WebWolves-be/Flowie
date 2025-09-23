# Data Model (Phase 1)

## Entities

### Project
- id (GUID)
- title (string, required, 3..200)
- description (string, optional, 0..4000)
- company (enum: Immoseed | NovaraRealEstate)
- createdAt (datetime)
- updatedAt (datetime)
- archivedAt (datetime, nullable)

### Task
- id (GUID)
- projectId (GUID, FK → Project)
- parentTaskId (GUID?, FK → Task)
- title (string, required, 3..200)
- description (string, optional, 0..4000)
- typeId (GUID, FK → TaskType)
- deadline (date, optional)
- status (enum: Pending | Ongoing | Done)
- assigneeId (GUID?, FK → Employee)
- createdAt (datetime)
- updatedAt (datetime)
- completedAt (datetime, nullable)

Constraints:
- Parent Done auto-rule: when all direct children Done → mark parent Done + completedAt

### TaskType
- id (GUID)
- name (string unique, 2..100)
- active (bool)
- createdAt (datetime)
- updatedAt (datetime)

### Employee
- id (GUID)
- name (string 2..150)
- email (string email)
- active (bool)

### AuditEntry
- id (GUID)
- entityType (string)
- entityId (GUID)
- action (string)
- actorId (GUID)
- timestamp (datetime)
- details (json)

## Relationships
- Project 1—* Task
- Task 1—* Task (self for subtasks)
- TaskType 1—* Task
- Employee 1—* Task (assignment)

## State Machine: Task
- Pending → Ongoing → Done
- Parent auto-Done when all children Done
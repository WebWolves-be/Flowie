````markdown
# Feature Specification: Task-based Workflow for Real Estate Projects

**Feature Branch**: `001-build-a-task`  
**Created**: 2025-09-21  
**Status**: Draft  
**Input**: User description: "Build a task based workflow application for a real estate company so they can track what to do per project. A project has a title and a description. It can also be designated to the company Immoseed or Novara Real estate.

Each project can have multiple tasks. A task hes a title, description, a type, a deadline date and it can be linked to an employee who needs to execute the task.

A task has 3 statusses : Pending, Ongoing, Done

Some tasks can also have subtasks. When all subtasks are done the main task is considered as done aswell.

People can add/edit projects and tasks. Also the task type is a managed list."

## Execution Flow (main)
```
1. Parse user description from Input
	→ If empty: ERROR "No feature description provided"
2. Extract key concepts from description
	→ Identify: actors, actions, data, constraints
3. For each unclear aspect:
	→ Mark with [NEEDS CLARIFICATION: specific question]
4. Fill User Scenarios & Testing section
	→ If no clear user flow: ERROR "Cannot determine user scenarios"
5. Generate Functional Requirements
	→ Each requirement must be testable
	→ Mark ambiguous requirements
6. Identify Key Entities (if data involved)
7. Run Review Checklist
	→ If any [NEEDS CLARIFICATION]: WARN "Spec has uncertainties"
	→ If implementation details found: ERROR "Remove tech details"
8. Return: SUCCESS (spec ready for planning)
```

---

## ⚡ Quick Guidelines
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement (no tech stack, APIs, code structure)
- 👥 Written for business stakeholders, not developers

### Section Requirements
- **Mandatory sections**: Must be completed for every feature
- **Optional sections**: Include only when relevant to the feature
- When a section doesn't apply, remove it entirely (don't leave as "N/A")

### For AI Generation
When creating this spec from a user prompt:
1. **Mark all ambiguities**: Use [NEEDS CLARIFICATION: specific question] for any assumption you'd need to make
2. **Don't guess**: If the prompt doesn't specify something (e.g., "login system" without auth method), mark it
3. **Think like a tester**: Every vague requirement should fail the "testable and unambiguous" checklist item
4. **Common underspecified areas**:
	- User types and permissions
	- Data retention/deletion policies  
	- Performance targets and scale
	- Error handling behaviors
	- Integration requirements
	- Security/compliance needs

---

## User Scenarios & Testing *(mandatory)*

### Primary User Story
As an operations manager at a real estate company, I want to create a project and define tasks with
types, assignees, deadlines, and subtasks, so that our team can consistently track progress and
complete work on time for each client.

### Acceptance Scenarios
1. Project CRUD
	- Given I am on the projects page
	- When I create a project with a title, description, and company designation (Immoseed or Novara
	  Real Estate)
	- Then the project appears in the list filtered by the designated company

2. Task CRUD with assignment
	- Given a project exists
	- When I add a task with title, description, type, deadline, and assign an employee
	- Then the task appears under that project with status Pending and the assignee visible

3. Task status flow
	- Given a Pending task
	- When the assignee starts work and sets status to Ongoing, then later marks Done
	- Then the task shows Ongoing then Done and the project’s progress updates accordingly

4. Subtasks behavior
	- Given a task with two subtasks both Pending
	- When both subtasks are marked Done
	- Then the parent task is automatically set to Done

5. Managed task types
	- Given an admin manages task types in a dedicated list
	- When a new task type is added or one is renamed
	- Then users can select it when creating/editing tasks and existing tasks keep their type mapping

6. Accessibility
	- Given keyboard-only navigation
	- When I navigate through project and task forms and lists
	- Then focus order is logical and visible, and labels/ARIA roles convey semantics (WCAG 2.1 AA)

7. Performance
	- Given typical mobile network conditions
	- When loading the projects list and a project detail with tasks
	- Then p75 Web Vitals meet LCP ≤ 2.5s, INP ≤ 200ms, CLS ≤ 0.1

### Edge Cases
- Creating a project without a title → show validation error and prevent save
- Setting a task deadline in the past → warn and prevent save unless justified by role policy
- Deleting a task that has subtasks → require confirmation and cascade behavior is explicit
- Reassigning a task when the new assignee is already overloaded → show warning (informational)
- Two users editing the same task concurrently → last-write-wins with conflict warning

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: System MUST allow users to create, edit, and archive projects with title, description,
  and company designation (Immoseed or Novara Real Estate).
- **FR-002**: System MUST allow users to create, edit, and archive tasks under a project with title,
  description, type, deadline, and assignee.
- **FR-003**: System MUST enforce task statuses: Pending, Ongoing, Done.
- **FR-004**: System MUST support hierarchical subtasks under a task.
- **FR-005**: System MUST automatically set a parent task to Done when all of its subtasks are Done.
- **FR-006**: System MUST provide a managed list of task types (create, rename, deactivate) and
  allow selecting a type for tasks.
- **FR-007**: System MUST allow listing and filtering projects by company (Immoseed or Novara Real
  Estate).
- **FR-008**: System MUST allow assigning and reassigning tasks to employees.
- **FR-009**: System MUST validate required fields and show actionable error messages.
- **FR-010**: System MUST record status changes and deadlines in an audit trail.
- **FR-011**: System MUST NOT authenticate users yet. It is a prototype for now.

### Key Entities *(include if feature involves data)*
- **Project**: id, title, description, company (enum: Immoseed | Novara Real Estate), createdAt,
  updatedAt, archivedAt?
- **Task**: id, projectId, parentTaskId?, title, description, typeId, deadline (date), status
  (Pending | Ongoing | Done), assigneeId, createdAt, updatedAt, completedAt?
- **TaskType**: id, name, active (bool), createdAt, updatedAt
- **Employee**: id, name, email, active (bool)
- **AuditEntry**: id, entityType, entityId, action, actorId, timestamp, details

---

## Review & Acceptance Checklist
*GATE: Automated checks run during main() execution*

### Content Quality
- [ ] No implementation details (languages, frameworks, APIs)
- [ ] Focused on user value and business needs
- [ ] Written for non-technical stakeholders
- [ ] All mandatory sections completed

### Requirement Completeness
- [ ] No [NEEDS CLARIFICATION] markers remain
- [ ] Requirements are testable and unambiguous  
- [ ] Success criteria are measurable
- [ ] Scope is clearly bounded
- [ ] Dependencies and assumptions identified
// Constitution alignment
- [ ] UX uses shared design system components; accessibility meets WCAG 2.1 AA
- [ ] Performance targets stated for affected flows

---

*Based on Constitution v1.0.0 - See `/memory/constitution.md`*

---

## Execution Status
*Updated by main() during processing*

- [ ] User description parsed
- [ ] Key concepts extracted
- [ ] Ambiguities marked
- [ ] User scenarios defined
- [ ] Requirements generated
- [ ] Entities identified
- [ ] Review checklist passed

---

````

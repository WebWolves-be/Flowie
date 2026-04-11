# Plan: Collapsible Done Tasks

> Source PRD: [GitHub Issue #22](https://github.com/WebWolves-be/Flowie/issues/22)

## Architectural decisions

- **Frontend-only** — no backend changes
- **Component**: `TaskItemComponent` owns collapse/expand state via a local `signal<boolean>`
- **Done detection**: `task.status === TaskStatus.Done`
- **Click target**: entire header row (status icon + title) toggles expand/collapse for done tasks
- **No persistence**: done tasks always start collapsed on page load
- **No sorting change**: done tasks keep their drag-drop `displayOrder`

---

## Phase 1: Collapse/expand done tasks

**User stories**: 1, 2, 3, 4, 5, 9

### What to build

Done tasks render collapsed by default — only the status icon and title visible. Clicking the header row expands the task to show description, metadata, subtasks, and action buttons. Clicking again collapses it. Non-done tasks always render fully expanded and are not toggleable. Behavior is consistent across all sections.

### Acceptance criteria

- [x] Done tasks show only status icon + title on page load
- [x] Description, metadata row, subtasks, and action buttons are hidden when collapsed
- [x] Clicking header row on a collapsed done task expands it fully
- [x] Clicking header row on an expanded done task collapses it
- [x] Non-done tasks are always fully expanded, not toggleable
- [x] Collapse behavior works identically in every section
- [x] Drag-drop reordering still works for done tasks

---

## Phase 2: Chevron indicator + animation

**User stories**: 6, 7, 8

### What to build

Add a chevron icon on done tasks indicating expandability (matching the existing section collapse pattern). Add a smooth CSS transition for the expand/collapse action. Ensure the "Openzetten" (reopen) button is accessible when a done task is expanded.

### Acceptance criteria

- [x] Collapsed done tasks show a chevron icon indicating they can be expanded
- [x] Chevron rotates when task is expanded (consistent with section collapse pattern)
- [x] Expand/collapse has a smooth visual transition (CSS transition on chevron rotation)
- [x] "Openzetten" button is visible and functional when a done task is expanded

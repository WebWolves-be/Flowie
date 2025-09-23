<!--
Sync Impact Report
Version change: N/A → 1.0.0
Modified principles: (initial adoption)
Added sections: Non-Functional Standards; Development Workflow & Quality Gates
Removed sections: none
Templates requiring updates:
	✅ .specify/templates/plan-template.md
	✅ .specify/templates/spec-template.md
	✅ .specify/templates/tasks-template.md
	✅ .specify/memory/constitution.md (this file)
Follow-up TODOs: none
-->

# Flowie Constitution

## Core Principles

### I. Code Quality & Maintainability (NON-NEGOTIABLE)
- Backend (C#/.NET 8): enable nullable reference types, use analyzers (e.g., Roslyn/StyleCop or	equivalent) and prohibit unused/dead code. 
- Architecture: respect clear layering and module boundaries. Cross-layer dependencies MUST be via interfaces or well-defined contracts. No circular dependencies. The structure of the project is feature focused.

### II. Test-First Engineering (TDD Discipline)
- New or changed behavior MUST have failing tests prior to implementation (Red→Green→Refactor).
- Test pyramid: prioritize unit tests; 
- Tests MUST be deterministic and isolated. Use test data builders/factories; avoid network calls and real third-party dependencies in unit tests.
- Contract changes MUST update corresponding tests and versioned schemas.
Rationale: Test-first ensures clarity of intent, prevents regressions, and enables safe refactoring.

**Version**: 1.0.0 | **Ratified**: 2025-09-21 | **Last Amended**: 2025-09-21
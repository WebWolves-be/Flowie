---
name: migrate
description: Create and apply EF Core database migrations for the Flowie backend.
---

# Database Migration Skill

Create and manage EF Core migrations for the Flowie backend.

## Usage
```
/migrate list                      # List all migrations
/migrate name:AddUserRole         # Create new migration
/migrate name:AddUserRole apply   # Create and apply immediately
```

## Workflow
1. Validate migration name (PascalCase, descriptive)
2. Navigate to the repo's `backend/` directory
3. Execute: `dotnet ef migrations add {name} --project Flowie.Api`
4. If 'apply' flag: `dotnet ef database update --project Flowie.Api`
5. Display created files and confirmation

## Implementation

When invoked:
- Parse arguments: `name:MigrationName` and optional `apply` flag
- Validate migration name is PascalCase and descriptive (min 5 chars)
- Navigate to the `backend/` directory at the repo root
- For `list`: run `dotnet ef migrations list --project Flowie.Api`
- For new migration: run `dotnet ef migrations add {name} --project Flowie.Api`
- If `apply` flag present: run `dotnet ef database update --project Flowie.Api`
- Display created migration files and success confirmation

## Notes
- Migrations auto-apply on backend startup (`db.Database.Migrate()` in `Program.cs`), so the 'apply' flag can usually be skipped in local dev
- Migration names should describe the change (e.g., AddUserRole, UpdateTaskSchema)
- The `--project Flowie.Api` flag is required since the command is run from `backend/`, one level above the actual project folder

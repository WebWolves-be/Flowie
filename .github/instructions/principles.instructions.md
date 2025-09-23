---
applyTo: '**'
---

# Backend

- NEVER generate inline commments or documentation (e.g., XML comments). Use meaningful names and clean code instead.
- All feature code should be have tests with XUnit and FakeItEasy if needed.

## Coding

- Follow the SOLID principles.
- Use dependency injection for all dependencies.
- Keep methods small and focused on a single task.
- Use async/await for all I/O bound operations. Never use ConfigureAwait(false).
- When using record use the constructor syntax (e.g., `public record Project(int Id, string Name);`).
- Use Primary Constructors for classes when possible (e.g., `public class ProjectService(IProjectRepository repository) { ... }`).
- Make custom exceptions when needed (e.g., `ProjectNotFoundException`).

## Minimal Api

- Endpoints should be close to the feature they serve (e.g., `Features/Projects/CreateProject/CreateProjectEndpoint.cs`).
- All endpoints should be registrated together in a single file per feature (e.g., `Features/Projects/ProjectsEndpoints.cs`).
- No validation logic in the endpoint, use MediatR pipeline behaviors for that.
- No exception handling in the endpoint, use middleware for that or MediatR.

## MediatR

- Use response objects (e.g., `Features/Projects/GetProjects/ProjectResponse.cs`) for all commands and queries and return this object in the api endpoint.

### Commands

- Commands should be named with a verb phrase (e.g., `CreateProjectCommand`).
- CommandHandlers should be named with the command name followed by `Handler` (e.g., `CreateProjectCommandHandler`).
- CommandValidators should be named with the command name followed by `Validator` (e.g., `CreateProjectCommandValidator`).

### Queries

- Queries should be named with a noun phrase (e.g., `GetProjectByIdQuery`).
- QueryHandlers should be named with the query name followed by `Handler` (e.g., `GetProjectByIdQueryHandler`).

## Database & Entities

- All entities should have an integer Id as primary key.
- All entities should implement IAuditableEntity which has CreatedAt, UpdatedAt properties
- All configurations should be in separate files (e.g., `ProjectEntityConfiguration.cs`).
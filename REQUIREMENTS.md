# Todo Application Requirements (Enterprise Minimal)

## 1. Purpose and Scope
This document defines the core, production-critical requirements for a Todo application. It is intentionally minimal and opinionated to enable fast delivery while maintaining enterprise-grade reliability and maintainability. Nice-to-have features are explicitly excluded.

## 2. Architecture Overview and System Boundaries
- The system is a single-tenant, web-based Todo application.
- Backend is a REST API built with ASP.NET Core 10.
- Frontend is a React 19 single-page application built with Vite 6.
- Data is stored in SQL Server and accessed via EF Core 10.
- The system is deployed as containerized services for backend, frontend, and SQL Server.

In scope:
- CRUD lifecycle for Todos.
- Authentication and authorization for protected endpoints.
- Minimal observability: structured logs with CorrelationId.
- Automated tests and CI pipeline.

Out of scope:
- Real-time updates, background jobs, multi-tenancy, advanced monitoring, API versioning, and other excluded items.

## 3. Clean Architecture Layers and Dependencies
### 3.1 Layer Responsibilities
- Domain: Entities, value objects, domain rules. No external dependencies.
- Application: CQRS commands/queries, handlers, validators, interfaces. No infrastructure implementations.
- Infrastructure: EF Core DbContext, repositories, authentication infrastructure, external services.
- API: Controllers, middleware, DI configuration, HTTP boundary.
- Shared.Contracts: DTOs and request/response models shared between backend and frontend.

### 3.2 Dependency Rules
- Domain has no dependencies.
- Application depends only on Domain and Shared.Contracts.
- Infrastructure depends on Application and Domain.
- API depends on Application, Infrastructure, and Shared.Contracts.
- Frontend consumes Shared.Contracts equivalents via generated or mirrored DTOs.

## 4. CQRS with MediatR
### 4.1 Command and Query Rules
- Commands mutate state and return void or an identifier.
- Queries read state and return DTOs.
- One handler per command/query.
- Commands/queries are immutable records.

### 4.2 Minimal Example
```csharp
public record CreateTodoCommand(string Title, string? Description, DateOnly? DueDate) : IRequest<Guid>;

public record GetTodoByIdQuery(Guid Id) : IRequest<TodoItemDto>;
```

## 5. Domain Entities, DTOs, and Mapping
### 5.1 Todo Entity
- Fields: Id, Title, Description, Status, DueDate, CreatedAt, UpdatedAt.
- Status values: Pending, Completed.
- CreatedAt and UpdatedAt are set server-side and updated on every write.

### 5.2 DTOs
- TodoItemDto mirrors entity for read models.
- CreateTodoRequest and UpdateTodoRequest for API inputs.

### 5.3 Mapping
- AutoMapper is used in Application layer for entity-to-DTO projections.
- All query handlers use ProjectTo with AsNoTracking.

## 6. Validation
### 6.1 Backend (FluentValidation)
- All commands and queries have validators.
- Validation failures return RFC 7807 Problem Details.

Rules:
- Title: required, 3-200 characters.
- Description: optional, max 1000 characters.
- DueDate: optional, must be today or future.

### 6.2 Frontend (Zod)
- Frontend forms mirror backend validation.
- API responses are validated with Zod schemas.

## 7. Error Handling (RFC 7807)
- All errors returned as Problem Details.
- Validation errors return 400 with field-level errors.
- Not found returns 404.
- Unhandled errors return 500 with a generic message.

## 8. Authentication and Authorization
- JWT Bearer authentication for all protected endpoints.
- Authorization policies for actions such as Create/Update/Delete.
- Unauthenticated requests receive 401.
- Unauthorized requests receive 403.

## 9. Logging and CorrelationId
- Each request has a CorrelationId header: X-Correlation-ID.
- If missing, the API generates it.
- Logs include CorrelationId and authenticated UserId.
- Logging is structured using Serilog.

## 10. Todo CRUD Requirements
### 10.1 Functional Requirements
- Create Todo with Title, optional Description, optional DueDate.
- Read Todo by Id.
- Update Todo fields: Title, Description, Status, DueDate.
- Delete Todo by Id.
- List Todos with optional Status filter.

### 10.2 Non-Functional Requirements
- All operations are async.
- Read queries use AsNoTracking.
- Commands enforce validation and update UpdatedAt.

## 11. REST API Endpoints
Base route: /api/todoitems

- POST /api/todoitems
- GET /api/todoitems/{id}
- PUT /api/todoitems/{id}
- DELETE /api/todoitems/{id}
- GET /api/todoitems?status=Pending|Completed

## 12. Database Schema and EF Core Configuration
### 12.1 Table: TodoItems
Columns:
- Id (uniqueidentifier, PK)
- Title (nvarchar(200), not null)
- Description (nvarchar(1000), null)
- Status (int, not null)
- DueDate (date, null)
- CreatedAt (datetime2, not null)
- UpdatedAt (datetime2, not null)

Indexes:
- IX_TodoItems_Status
- IX_TodoItems_CreatedAt

### 12.2 EF Core Configuration
- Fluent API configuration for all constraints and indexes.
- DateOnly is mapped to SQL Server date.

## 13. Migrations Strategy
- Development: EF Core migrations generated per change.
- Production: migrations applied during deployment with explicit approval.
- No manual edits to applied migration files.

## 14. Frontend Routing and Layouts
- React Router v7 with createBrowserRouter.
- Main layout with header, content, and footer.
- Routes:
  - / (protected Todos list)
  - /login
  - * (not found)

## 15. Frontend API Layer
- Axios instance with base URL from VITE_API_BASE_URL.
- Request interceptor adds Authorization header and CorrelationId.
- Response interceptor handles 401 and 403 globally.

## 16. State Management
- TanStack Query v5 for server state.
- Optional Zustand for client-only state (auth, UI preferences).
- No mixing of server and client state responsibilities.

## 17. UI Components
- shadcn/ui for all core components.
- Required components: Button, Card, Dialog, Form, Input, Textarea, Toast.
- UI is responsive for desktop and mobile.

## 18. Testing Strategy
### 18.1 Backend
- Unit tests for handlers and validators (xUnit + Moq).
- Integration tests for API endpoints (WebApplicationFactory).

### 18.2 Frontend
- Component tests with Vitest + React Testing Library.
- Hook tests for TanStack Query hooks.

## 19. Docker Setup
- Backend: multi-stage Dockerfile (sdk build, aspnet runtime).
- Frontend: build with Node, serve with nginx.
- SQL Server container for local dev.

## 20. CI Pipeline Outline
- Backend: restore, build, test.
- Frontend: install, lint, test, build.
- Pipeline must fail on test failures.

## 21. Excluded Features (Explicit)
- AI/ML features
- Advanced accessibility deep dives (basic a11y ok)
- Internationalization (i18n)
- API versioning strategy
- Background jobs (Hangfire)
- Real-time updates (SignalR)
- Kubernetes/Helm
- Advanced E2E or contract testing
- Performance monitoring / Web Vitals
- Multi-tenancy
- ADRs
- SLO/SLA monitoring
- Dev containers
- React Server Components
- Idempotency middleware
- Advanced resilience or circuit breakers beyond basics

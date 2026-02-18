# Implementation Tickets (Smart Todo App)

Comprehensive tracking document for all 25 implementation tickets. Each ticket is sequenced with clear dependencies and acceptance criteria. GitHub issues are linked below for collaborative tracking.

**Ticket Numbering Convention:**
- Tickets are numbered sequentially in this document (#1-#25)
- GitHub Issues have separate numbering (starting from #1 but may have gaps due to other issues)
- Each ticket heading shows both: `Ticket #X (GH #Y)` to avoid confusion
- Example: Ticket #12 in this document corresponds to GitHub Issue #15

---

## Phase 0: Project Documentation

### Ticket #1 (GH #1) - Add Copilot instructions and requirements docs
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#1](https://github.com/akarthickarun/smart-todo-app/issues/1)  
**Dependencies:** None  
**Estimated:** 0.5 day

**Description:**
Establish baseline project documentation for Copilot guidance and requirements scope.

**Acceptance Criteria:**
- [x] Add .github/copilot-instructions.md with project guidance
- [x] Add REQUIREMENTS.md with initial scope and constraints
- [x] Verify documents are committed to the repository

---

## Phase 1: Backend Foundation

### Ticket #2 (GH #5) - Set up ASP.NET Core 10 projects and dependency injection
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#5](https://github.com/akarthickarun/smart-todo-app/issues/5)  
**Dependencies:** None  
**Estimated:** 1 day

**Description:**
Create the foundational project structure and configure dependency injection for the entire backend.

**Acceptance Criteria:**
- [x] Create 5 projects: SmartTodoApp.Domain, SmartTodoApp.Application, SmartTodoApp.Infrastructure, SmartTodoApp.API, SmartTodoApp.Shared.Contracts
- [x] Configure Program.cs with MediatR registration
- [x] Configure Program.cs with AutoMapper registration
- [x] Configure Program.cs with Serilog registration
- [x] Configure Program.cs with EF Core SQL Server registration (placeholder)
- [x] Verify solution builds successfully
- [x] Create xUnit test projects: Tests.Application.UnitTests, Tests.API.IntegrationTests

---

### Ticket #3 (GH #6) - Implement base middleware and error handling
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#6](https://github.com/akarthickarun/smart-todo-app/issues/6)  
**Dependencies:** #2  
**Estimated:** 1 day

**Description:**
Add core middleware for request correlation tracking, exception handling, and structured logging.

**Acceptance Criteria:**
- [x] Create CorrelationIdMiddleware that generates/extracts X-Correlation-ID header
- [x] Create ExceptionHandlingMiddleware that returns RFC 7807 Problem Details
- [x] Register middleware in Program.cs in correct order
- [x] Configure Serilog with structured logging to console and file
- [x] Ensure CorrelationId is included in all logs
- [x] Test middleware with sample requests (manual or basic integration test)

---

## Phase 2: Domain & Data Layer

### Ticket #4 (GH #7) - Create Todo entity in Domain layer
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#7](https://github.com/akarthickarun/smart-todo-app/issues/7)  
**Dependencies:** None  
**Estimated:** 0.5 day

**Description:**
Define the core Todo entity with all business logic and factory methods.

**Acceptance Criteria:**
- [x] Create TodoItem entity in Domain/Entities/TodoItem.cs
- [x] Implement properties: Id, Title, Description, Status (enum Pending/Completed), DueDate, CreatedAt, UpdatedAt
- [x] Implement factory method TodoItem.Create(string title, string? description, DateOnly? dueDate)
- [x] Implement business methods: MarkAsComplete(), UpdateDetails(title, description, dueDate)
- [x] Add validation in constructor (Title required, length constraints)
- [x] Create Status enum in Domain/Enums/TodoStatus.cs
- [x] No external dependencies in Domain

---

### Ticket #5 (GH #8) - Create DTOs in Shared.Contracts
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#8](https://github.com/akarthickarun/smart-todo-app/issues/8)  
**Dependencies:** #4  
**Estimated:** 0.5 day

**Description:**
Define data transfer objects for API contracts and frontend consumption.

**Acceptance Criteria:**
- [x] Create TodoItemDto record in Shared.Contracts/TodoItems/TodoItemDto.cs
- [x] Create CreateTodoRequest record in Shared.Contracts/TodoItems/CreateTodoRequest.cs
- [x] Create UpdateTodoRequest record in Shared.Contracts/TodoItems/UpdateTodoRequest.cs
- [x] Ensure all DTOs match requirement specs for fields and types
- [x] Include data annotations for OpenAPI documentation if needed
- [x] All DTOs are immutable records

---

### Ticket #6 (GH #9) - Set up EF Core DbContext and configuration
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#9](https://github.com/akarthickarun/smart-todo-app/issues/9)  
**Dependencies:** #4, #5  
**Estimated:** 1 day

**Description:**
Create EF Core DbContext and configure TodoItem entity with Fluent API.

**Acceptance Criteria:**
- [x] Create ApplicationDbContext in Infrastructure/Persistence/ApplicationDbContext.cs
- [x] Add DbSet<TodoItem> TodoItems property
- [x] Create IApplicationDbContext interface in Application/Common/Interfaces/
- [x] Implement TodoItemConfiguration in Infrastructure/Persistence/Configurations/TodoItemConfiguration.cs
- [x] Configure all constraints: not null, max lengths, defaults
- [x] Configure indexes: IX_TodoItems_Status, IX_TodoItems_CreatedAt
- [x] Map DateOnly to SQL Server date type
- [x] Register in Program.cs with SQL Server connection string from appsettings

---

### Ticket #7 (GH #10) - Create and apply initial database migration
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#10](https://github.com/akarthickarun/smart-todo-app/issues/10)  
**Dependencies:** #6  
**Estimated:** 0.5 day

**Description:**
Generate and apply the first EF Core migration to create the TodoItems table.

**Acceptance Criteria:**
- [x] Run dotnet ef migrations add InitialCreate
- [x] Migration file created with up/down methods
- [x] Review migration SQL matches schema requirements
- [x] Run dotnet ef database update to apply locally
- [x] SQL Server database and TodoItems table successfully created
- [x] Verify table structure in SQL Server Management Studio

---

### Ticket #8 (GH #11) - Create AutoMapper MappingProfile
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#11](https://github.com/akarthickarun/smart-todo-app/issues/11)  
**Dependencies:** #4, #5, #6  
**Estimated:** 0.5 day

**Description:**
Configure AutoMapper for entity-to-DTO projections.

**Acceptance Criteria:**
- [x] Create MappingProfile in Application/Common/Mappings/MappingProfile.cs
- [x] Map TodoItem → TodoItemDto
- [x] MappingProfile is registered in Program.cs
- [x] Test mapping works with simple unit test

---

## Phase 3: Application Layer (CQRS)

### Ticket #9 (GH #12) - Implement todo CRUD commands and handlers
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#12](https://github.com/akarthickarun/smart-todo-app/issues/12)  
**Dependencies:** #4, #5, #8  
**Estimated:** 2 days

**Description:**
Implement CreateTodoItem, UpdateTodoItem, and DeleteTodoItem commands with handlers and validators.

**Acceptance Criteria:**
- [x] Create CreateTodoItemCommand record with Title, Description, DueDate
- [x] Create CreateTodoItemCommandValidator with FluentValidation (Title: required, 3-200 chars; Description: max 1000; DueDate: future)
- [x] Create CreateTodoItemCommandHandler that creates entity, adds to context, saves, returns Guid
- [x] Create UpdateTodoItemCommand record with Id, Title, Description, Status, DueDate
- [x] Create UpdateTodoItemCommandValidator with same rules
- [x] Create UpdateTodoItemCommandHandler that updates entity and saves
- [x] Create DeleteTodoItemCommand record with Id
- [x] Create DeleteTodoItemCommandHandler that deletes entity and saves
- [x] All handlers use async/await and CancellationToken
- [x] All handlers log with ILogger<T>
- [x] Handlers registered in ValidationBehavior pipeline

---

### Ticket #10 (GH #13) - Implement todo CRUD queries and handlers
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#13](https://github.com/akarthickarun/smart-todo-app/issues/13)  
**Dependencies:** #4, #5, #8  
**Estimated:** 1.5 days

**Description:**
Implement GetTodoItemById and GetTodoItems (list) queries with handlers.

**Acceptance Criteria:**
- [x] Create GetTodoItemByIdQuery record with Id
- [x] Create GetTodoItemByIdQueryHandler that uses AsNoTracking + ProjectTo<TodoItemDto>
- [x] Handler throws NotFoundException if todo not found
- [x] Create GetTodoItemsQuery record with optional Status filter
- [x] Create GetTodoItemsQueryHandler that filters by Status if provided, orders by CreatedAt desc
- [x] Handler uses AsNoTracking + ProjectTo<TodoItemDto>
- [x] Both handlers log with ILogger<T>
- [x] All queries use async/await and CancellationToken

---

## Phase 4: API Layer

### Ticket #11 (GH #14) - Create TodoItemsController with REST endpoints
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#14](https://github.com/akarthickarun/smart-todo-app/issues/14)  
**Dependencies:** #9, #10  
**Estimated:** 1.5 days

**Description:**
Create the API controller with all CRUD REST endpoints following REST conventions.

**Acceptance Criteria:**
- [x] Create TodoItemsController in API/Controllers/TodoItemsController.cs
- [x] Implement POST /api/todoitems (CreateTodoItemCommand) → 201 Created
- [x] Implement GET /api/todoitems/{id} (GetTodoItemByIdQuery) → 200 OK or 404
- [x] Implement PUT /api/todoitems/{id} (UpdateTodoItemCommand) → 200 OK or 404
- [x] Implement DELETE /api/todoitems/{id} (DeleteTodoItemCommand) → 204 No Content or 404
- [x] Implement GET /api/todoitems?status=Pending|Completed (GetTodoItemsQuery) → 200 OK
- [x] Implement PATCH /api/todoitems/{id}/complete (MarkTodoItemCompleteCommand) → 200 OK or 404
- [x] Add [ProducesResponseType] attributes for OpenAPI
- [x] Create requests return CreatedAtAction with location header
- [x] All endpoints delegate to IMediator
- [x] All endpoints use CancellationToken
- [x] Unit tests for controller endpoints
- [x] Manual check with Postman/curl

---

### Ticket #12 (GH #15) - Add JWT authentication and authorization
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#15](https://github.com/akarthickarun/smart-todo-app/issues/15)  
**Dependencies:** #11, #3  
**Estimated:** 1.5 days

**Description:**
Configure JWT Bearer authentication and add authorization to endpoints.

**Acceptance Criteria:**
- [x] Configure JWT in Program.cs with token validation parameters
- [x] Add appsettings.json values: Jwt:Key, Jwt:Issuer, Jwt:Audience, Jwt:ExpiryMinutes
- [x] Create a token generation service or helper for dev/testing
- [x] Add [Authorize] attributes to all command endpoints (POST, PUT, DELETE)
- [x] Query endpoints (GET) remain public for now
- [x] Test: unauthorized requests return 401
- [x] Test: authorized requests with valid token succeed
- [x] (Optional) Add authorization policy for advanced control

---

## Phase 5: Testing (Backend)

### Ticket #13 (GH #16) - Add xUnit unit tests for handlers and validators
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#16](https://github.com/akarthickarun/smart-todo-app/issues/16)  
**Dependencies:** #9, #10  
**Estimated:** 2 days

**Description:**
Write unit tests for all command/query handlers and validators covering success and failure paths.

**Acceptance Criteria:**
- [x] CreateTodoItemCommandValidator tests: valid input, empty title, title too long, invalid duedate
- [x] CreateTodoItemCommandHandler tests: creates entity, persists, returns id, logs
- [x] UpdateTodoItemCommandValidator tests: similar to create
- [x] UpdateTodoItemCommandHandler tests: updates entity, persists, logs
- [x] DeleteTodoItemCommandHandler tests: deletes entity, throws NotFoundException, logs
- [x] MarkTodoItemCompleteCommandValidator tests: valid ID validation
- [x] MarkTodoItemCompleteCommandHandler tests: marks complete, persists, logs
- [x] GetTodoItemByIdQueryHandler tests: returns DTO, throws NotFoundException if not found
- [x] GetTodoItemsQueryHandler tests: returns list, applies status filter, orders correctly
- [x] Minimum 3-5 tests per handler/validator
- [x] Use Moq for DbContext mocking
- [x] Use FluentAssertions for assertions

---

### Ticket #14 (GH #17) - Add integration tests for API endpoints
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#17](https://github.com/akarthickarun/smart-todo-app/issues/17)  
**Dependencies:** #11, #12, #13  
**Estimated:** 2 days

**Description:**
Write integration tests for all API endpoints using WebApplicationFactory.

**Acceptance Criteria:**
- [x] Create WebApplicationFactory<Program> fixture
- [x] Test POST /api/todoitems: create todo, verify 201 and location header
- [x] Test GET /api/todoitems/{id}: retrieve todo, verify 200 and DTO
- [x] Test GET /api/todoitems/{id}: not found, verify 404
- [x] Test PUT /api/todoitems/{id}: update todo, verify 200
- [x] Test DELETE /api/todoitems/{id}: delete todo, verify 204
- [x] Test GET /api/todoitems?status=Completed: filter by status
- [x] Test unauthorized POST (no token): verify 401
- [x] Test invalid requests: verify 400 with Problem Details
- [x] All tests use in-memory database with fresh database per test

---

## Phase 6: Frontend Foundation

### Ticket #15 (GH #18) - Set up React + TypeScript + Vite + Tailwind
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#18](https://github.com/akarthickarun/smart-todo-app/issues/18)  
**Dependencies:** None  
**Estimated:** 1 day

**Description:**
Initialize frontend project with all dev dependencies and configuration.

**Acceptance Criteria:**
- [ ] Create React 19 + TypeScript + Vite project structure
- [ ] Install dependencies: react-router-dom, axios, @tanstack/react-query, zustand, zod, react-hook-form, shadcn/ui
- [ ] Configure Tailwind CSS v4
- [ ] Configure path aliases (@/ → src/)
- [ ] Configure Vite with React plugin
- [ ] Verify build succeeds with `npm run build`
- [ ] Verify dev server starts with `npm run dev`

---

### Ticket #16 (GH #19) - Create layouts and routing
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#19](https://github.com/akarthickarun/smart-todo-app/issues/19)  
**Dependencies:** #15  
**Estimated:** 1 day

**Description:**
Set up React Router with main layout and core routes.

**Acceptance Criteria:**
- [ ] Create src/router/index.tsx with createBrowserRouter
- [ ] Create MainLayout.tsx with header, footer, Outlet for content
- [ ] Create Header.tsx component (placeholder: title and nav)
- [ ] Create Footer.tsx component (placeholder: copyright)
- [ ] Define routes: / (protected), /login, * (not found)
- [ ] Create ProtectedRoute wrapper component
- [ ] Create NotFoundPage component
- [ ] Create LoginPage component (stub)
- [ ] Create TodosPage component (stub)
- [ ] Router initialization in App.tsx

---

### Ticket #17 (GH #20) - Create auth store and login page (mock)
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#20](https://github.com/akarthickarun/smart-todo-app/issues/20)  
**Dependencies:** #16  
**Estimated:** 1 day

**Description:**
Implement Zustand auth store and basic login page for development.

**Acceptance Criteria:**
- [ ] Create src/features/auth/stores/authStore.ts with Zustand
- [ ] Store holds: token, user (id, email, name), isAuthenticated
- [ ] Implement login(token, user) and logout() actions
- [ ] Use persist middleware to save to localStorage
- [ ] Create LoginPage.tsx with mock email/password input
- [ ] LoginPage calls authStore.login() with hardcoded token for local testing
- [ ] ProtectedRoute now checks authStore.isAuthenticated
- [ ] Logout clears token and redirects to /login

---

## Phase 7: Frontend API & State

### Ticket #18 (GH #21) - Create axios instance and todo API client
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#21](https://github.com/akarthickarun/smart-todo-app/issues/21)  
**Dependencies:** #5, #17  
**Estimated:** 1.5 days

**Description:**
Build axios client with interceptors and todo API methods with Zod validation.

**Acceptance Criteria:**
- [ ] Create src/api/axios-instance.ts with base URL from VITE_API_BASE_URL
- [ ] Request interceptor adds Authorization header from authStore token
- [ ] Request interceptor adds X-Correlation-ID header (random UUID)
- [ ] Response interceptor handles 401 (clears auth, redirects to /login)
- [ ] Response interceptor handles 403 (logs error)
- [ ] Create src/api/todo-api.ts with methods: getAll, getById, create, update, delete
- [ ] Create src/features/todos/schemas/todoSchemas.ts with Zod schemas
- [ ] TodoItemDto schema matches backend
- [ ] CreateTodoInput schema with validation rules
- [ ] UpdateTodoInput schema with validation rules
- [ ] API methods validate responses with Zod before returning

---

### Ticket #19 (GH #22) - Create TanStack Query hooks
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#22](https://github.com/akarthickarun/smart-todo-app/issues/22)  
**Dependencies:** #18  
**Estimated:** 1.5 days

**Description:**
Implement custom hooks for querying and mutating todos with TanStack Query v5.

**Acceptance Criteria:**
- [ ] Create src/lib/query-client.ts with QueryClient configuration
- [ ] Configure defaults: retry, refetchOnWindowFocus, staleTime, gcTime
- [ ] Create src/features/todos/hooks/useTodos.ts (query all todos, optional filter)
- [ ] Create src/features/todos/hooks/useGetTodoById.ts (query single todo)
- [ ] Create src/features/todos/hooks/useCreateTodo.ts (mutation)
- [ ] Create src/features/todos/hooks/useUpdateTodo.ts (mutation)
- [ ] Create src/features/todos/hooks/useDeleteTodo.ts (mutation)
- [ ] All hooks use queryKey namespacing: ['todos'], ['todos', id]
- [ ] Mutations invalidate relevant queries on success
- [ ] Error handling provides user feedback via toast
- [ ] Wrap App with QueryClientProvider in main.tsx

---

## Phase 8: Frontend UI

### Ticket #20 (GH #23) - Create Todo components
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#23](https://github.com/akarthickarun/smart-todo-app/issues/23)  
**Dependencies:** #15, #19  
**Estimated:** 2 days

**Description:**
Build all reusable Todo components for list, create, edit, and delete operations.

**Acceptance Criteria:**
- [ ] Create src/components/todos/TodoList.tsx (displays filtered list)
- [ ] TodoList supports Status filter buttons (All, Pending, Completed)
- [ ] Create src/components/todos/TodoItem.tsx (single todo card)
- [ ] TodoItem displays title, description, status badge, duedate, action buttons
- [ ] TodoItem has edit and delete buttons (with confirm dialog for delete)
- [ ] Create src/components/todos/CreateTodoDialog.tsx (create form)
- [ ] CreateTodoDialog uses React Hook Form + Zod validation
- [ ] Form has Title, Description, DueDate inputs
- [ ] Form submission creates todo via useCreateTodo hook
- [ ] Create src/components/todos/UpdateTodoDialog.tsx (edit form)
- [ ] UpdateTodoDialog pre-populates with existing todo data
- [ ] All components use shadcn/ui: Dialog, Button, Card, Input, Textarea, Form
- [ ] All components show loading/error states
- [ ] All components are responsive

---

### Ticket #21 (GH #24) - Create TodosPage and integrate state
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#24](https://github.com/akarthickarun/smart-todo-app/issues/24)  
**Dependencies:** #20, #19  
**Estimated:** 1.5 days

**Description:**
Implement main TodosPage combining all components and connecting to API state.

**Acceptance Criteria:**
- [ ] Create src/pages/TodosPage.tsx
- [ ] Page composes: TodoList, CreateTodoDialog, UpdateTodoDialog
- [ ] Page uses useTodos hook to fetch and display list
- [ ] Page passes handler functions to TodoList for edit/delete
- [ ] Page displays loading spinner while fetching
- [ ] Page displays error message if fetch fails
- [ ] List updates immediately after create/update/delete (via query invalidation)
- [ ] Status filter in TodoList calls useTodos with filter parameter
- [ ] Empty state message when no todos exist
- [ ] All mutations show toast notifications (success/error)

---

## Phase 9: Testing (Frontend)

### Ticket #22 (GH #25) - Add frontend component tests
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#25](https://github.com/akarthickarun/smart-todo-app/issues/25)  
**Dependencies:** #20  
**Estimated:** 2 days

**Description:**
Write component tests for all Todo components using Vitest + React Testing Library.

**Acceptance Criteria:**
- [ ] Setup Vitest config with React and Testing Library
- [ ] Create src/components/todos/__tests__/TodoItem.test.tsx
- [ ] Test TodoItem renders title, description, duedate
- [ ] Test TodoItem edit button opens dialog
- [ ] Test TodoItem delete button shows confirm dialog
- [ ] Create src/components/todos/__tests__/CreateTodoDialog.test.tsx
- [ ] Test form renders and submits
- [ ] Test validation errors display
- [ ] Test create button disables while submitting
- [ ] Create src/components/todos/__tests__/TodoList.test.tsx
- [ ] Test list renders multiple todos
- [ ] Test status filter buttons work
- [ ] Use vi.fn() for mocked callbacks
- [ ] Minimum 3-5 tests per component

---

### Ticket #23 (GH #26) - Add frontend hook tests
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#26](https://github.com/akarthickarun/smart-todo-app/issues/26)  
**Dependencies:** #19  
**Estimated:** 1.5 days

**Description:**
Test TanStack Query hooks with renderHook and mocked axios.

**Acceptance Criteria:**
- [ ] Setup test utils with QueryClientProvider wrapper
- [ ] Create src/features/todos/hooks/__tests__/useTodos.test.ts
- [ ] Test useTodos returns loading state initially
- [ ] Test useTodos returns success data
- [ ] Test useTodos filters by status
- [ ] Create src/features/todos/hooks/__tests__/useCreateTodo.test.ts
- [ ] Test useCreateTodo triggers mutation
- [ ] Test useCreateTodo invalidates queries on success
- [ ] Test useCreateTodo shows error on failure
- [ ] Create src/features/todos/hooks/__tests__/useUpdateTodo.test.ts
- [ ] Similar tests for update mutation
- [ ] Mock axios with msw or vitest mocks
- [ ] Use waitFor() for async assertions

---

## Phase 10: Containerization & CI

### Ticket #24 (GH #27) - Add Docker setup
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#27](https://github.com/akarthickarun/smart-todo-app/issues/27)  
**Dependencies:** #11, #21  
**Estimated:** 1 day

**Description:**
Create Dockerfiles and docker-compose for full local development environment.

**Acceptance Criteria:**
- [ ] Create src/SmartTodoApp.API/Dockerfile (multi-stage: sdk build, aspnet runtime)
- [ ] Backend Dockerfile copies all projects, restores, builds, publishes
- [ ] Create src/frontend/Dockerfile (Node build stage, nginx serve stage)
- [ ] Frontend Dockerfile builds React app, copies to nginx /usr/share/nginx/html
- [ ] Create src/frontend/nginx.conf (proxy /api calls to backend)
- [ ] Create docker-compose.yml with 3 services: sqlserver, backend, frontend
- [ ] SQL Server service with healthcheck
- [ ] Backend service environment variables: connection string, JWT settings
- [ ] Frontend service environment variables: VITE_API_BASE_URL
- [ ] Test: docker-compose up --build succeeds
- [ ] Test: curl http://localhost:3000 returns frontend
- [ ] Test: curl http://localhost:5000/health returns 200

---

### Ticket #25 (GH #28) - Set up GitHub Actions CI pipeline
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#28](https://github.com/akarthickarun/smart-todo-app/issues/28)  
**Dependencies:** #13, #14, #22, #23  
**Estimated:** 1 day

**Description:**
Create CI workflows for automated testing and build verification.

**Acceptance Criteria:**
- [ ] Create .github/workflows/backend-ci.yml
- [ ] Backend workflow: checkout → setup dotnet → restore → build → test
- [ ] Fail workflow if any test fails
- [ ] Create .github/workflows/frontend-ci.yml
- [ ] Frontend workflow: checkout → setup node → install → lint → test → build
- [ ] Fail workflow if any test fails
- [ ] Add workflow triggers: on push to main/develop, on PR
- [ ] Optional: upload coverage to codecov
- [ ] Test: create PR and verify workflows run
- [ ] Verify workflow status blocks PR merge if tests fail

---

## Summary

| Phase | Tickets | Status |
|-------|---------|--------|
| 0. Project Documentation | #1 | ✅ #1 Completed |
| 1. Backend Foundation | #2-3 | ✅ #2-3 Completed |
| 2. Domain & Data | #4-8 | ✅ #4-8 Completed |
| 3. Application (CQRS) | #9-10 | ✅ #9-10 Completed |
| 4. API Layer | #11-12 | ✅ #11-12 Completed |
| 5. Backend Testing | #13-14 | ✅ #13-14 Completed |
| 6. Frontend Setup | #15-17 | Not Started |
| 7. Frontend API & State | #18-19 | Not Started |
| 8. Frontend UI | #20-21 | Not Started |
| 9. Frontend Testing | #22-23 | Not Started |
| 10. DevOps & CI | #24-25 | Not Started |

**Total:** 25 tickets
**Estimated Duration:** ~14 days (concurrent phases possible)

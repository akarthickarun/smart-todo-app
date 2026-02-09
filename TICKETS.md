# Implementation Tickets (Smart Todo App)

Comprehensive tracking document for all 25 implementation tickets. Each ticket is sequenced with clear dependencies and acceptance criteria. GitHub issues are linked below for collaborative tracking.

---

## Phase 1: Backend Foundation

### #2 - Set up ASP.NET Core 10 projects and dependency injection
**Status:** ✅ Completed  
**GitHub Issue:** [akarthickarun/smart-todo-app#2](https://github.com/akarthickarun/smart-todo-app/issues/2)  
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

### #3 - Implement base middleware and error handling
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#3](https://github.com/akarthickarun/smart-todo-app/issues/3)  
**Dependencies:** #2  
**Estimated:** 1 day

**Description:**
Add core middleware for request correlation tracking, exception handling, and structured logging.

**Acceptance Criteria:**
- [ ] Create CorrelationIdMiddleware that generates/extracts X-Correlation-ID header
- [ ] Create ExceptionHandlingMiddleware that returns RFC 7807 Problem Details
- [ ] Register middleware in Program.cs in correct order
- [ ] Configure Serilog with structured logging to console and file
- [ ] Ensure CorrelationId is included in all logs
- [ ] Test middleware with sample requests (manual or basic integration test)

---

## Phase 2: Domain & Data Layer

### #4 - Create Todo entity in Domain layer
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#4](https://github.com/akarthickarun/smart-todo-app/issues/4)  
**Dependencies:** None  
**Estimated:** 0.5 day

**Description:**
Define the core Todo entity with all business logic and factory methods.

**Acceptance Criteria:**
- [ ] Create TodoItem entity in Domain/Entities/TodoItem.cs
- [ ] Implement properties: Id, Title, Description, Status (enum Pending/Completed), DueDate, CreatedAt, UpdatedAt
- [ ] Implement factory method TodoItem.Create(string title, string? description, DateOnly? dueDate)
- [ ] Implement business methods: MarkAsComplete(), UpdateDetails(title, description, dueDate)
- [ ] Add validation in constructor (Title required, length constraints)
- [ ] Create Status enum in Domain/Enums/TodoStatus.cs
- [ ] No external dependencies in Domain

---

### #5 - Create DTOs in Shared.Contracts
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#5](https://github.com/akarthickarun/smart-todo-app/issues/5)  
**Dependencies:** #4  
**Estimated:** 0.5 day

**Description:**
Define data transfer objects for API contracts and frontend consumption.

**Acceptance Criteria:**
- [ ] Create TodoItemDto record in Shared.Contracts/TodoItems/TodoItemDto.cs
- [ ] Create CreateTodoRequest record in Shared.Contracts/TodoItems/CreateTodoRequest.cs
- [ ] Create UpdateTodoRequest record in Shared.Contracts/TodoItems/UpdateTodoRequest.cs
- [ ] Ensure all DTOs match requirement specs for fields and types
- [ ] Include data annotations for OpenAPI documentation if needed
- [ ] All DTOs are immutable records

---

### #6 - Set up EF Core DbContext and configuration
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#6](https://github.com/akarthickarun/smart-todo-app/issues/6)  
**Dependencies:** #4, #5  
**Estimated:** 1 day

**Description:**
Create EF Core DbContext and configure TodoItem entity with Fluent API.

**Acceptance Criteria:**
- [ ] Create ApplicationDbContext in Infrastructure/Persistence/ApplicationDbContext.cs
- [ ] Add DbSet<TodoItem> TodoItems property
- [ ] Create IApplicationDbContext interface in Application/Common/Interfaces/
- [ ] Implement TodoItemConfiguration in Infrastructure/Persistence/Configurations/TodoItemConfiguration.cs
- [ ] Configure all constraints: not null, max lengths, defaults
- [ ] Configure indexes: IX_TodoItems_Status, IX_TodoItems_CreatedAt
- [ ] Map DateOnly to SQL Server date type
- [ ] Register in Program.cs with SQL Server connection string from appsettings

---

### #7 - Create and apply initial database migration
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#7](https://github.com/akarthickarun/smart-todo-app/issues/7)  
**Dependencies:** #6  
**Estimated:** 0.5 day

**Description:**
Generate and apply the first EF Core migration to create the TodoItems table.

**Acceptance Criteria:**
- [ ] Run dotnet ef migrations add InitialCreate
- [ ] Migration file created with up/down methods
- [ ] Review migration SQL matches schema requirements
- [ ] Run dotnet ef database update to apply locally
- [ ] SQL Server database and TodoItems table successfully created
- [ ] Verify table structure in SQL Server Management Studio

---

### #8 - Create AutoMapper MappingProfile
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#8](https://github.com/akarthickarun/smart-todo-app/issues/8)  
**Dependencies:** #4, #5, #6  
**Estimated:** 0.5 day

**Description:**
Configure AutoMapper for entity-to-DTO projections.

**Acceptance Criteria:**
- [ ] Create MappingProfile in Application/Common/Mappings/MappingProfile.cs
- [ ] Map TodoItem → TodoItemDto
- [ ] MappingProfile is registered in Program.cs
- [ ] Test mapping works with simple unit test

---

## Phase 3: Application Layer (CQRS)

### #9 - Implement todo CRUD commands and handlers
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#9](https://github.com/akarthickarun/smart-todo-app/issues/9)  
**Dependencies:** #4, #5, #8  
**Estimated:** 2 days

**Description:**
Implement CreateTodoItem, UpdateTodoItem, and DeleteTodoItem commands with handlers and validators.

**Acceptance Criteria:**
- [ ] Create CreateTodoItemCommand record with Title, Description, DueDate
- [ ] Create CreateTodoItemCommandValidator with FluentValidation (Title: required, 3-200 chars; Description: max 1000; DueDate: future)
- [ ] Create CreateTodoItemCommandHandler that creates entity, adds to context, saves, returns Guid
- [ ] Create UpdateTodoItemCommand record with Id, Title, Description, Status, DueDate
- [ ] Create UpdateTodoItemCommandValidator with same rules
- [ ] Create UpdateTodoItemCommandHandler that updates entity and saves
- [ ] Create DeleteTodoItemCommand record with Id
- [ ] Create DeleteTodoItemCommandHandler that deletes entity and saves
- [ ] All handlers use async/await and CancellationToken
- [ ] All handlers log with ILogger<T>
- [ ] Handlers registered in ValidationBehavior pipeline

---

### #10 - Implement todo CRUD queries and handlers
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#10](https://github.com/akarthickarun/smart-todo-app/issues/10)  
**Dependencies:** #4, #5, #8  
**Estimated:** 1.5 days

**Description:**
Implement GetTodoItemById and GetTodoItems (list) queries with handlers.

**Acceptance Criteria:**
- [ ] Create GetTodoItemByIdQuery record with Id
- [ ] Create GetTodoItemByIdQueryHandler that uses AsNoTracking + ProjectTo<TodoItemDto>
- [ ] Handler throws NotFoundException if todo not found
- [ ] Create GetTodoItemsQuery record with optional Status filter
- [ ] Create GetTodoItemsQueryHandler that filters by Status if provided, orders by CreatedAt desc
- [ ] Handler uses AsNoTracking + ProjectTo<TodoItemDto>
- [ ] Both handlers log with ILogger<T>
- [ ] All queries use async/await and CancellationToken

---

## Phase 4: API Layer

### #11 - Create TodoItemsController with REST endpoints
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#11](https://github.com/akarthickarun/smart-todo-app/issues/11)  
**Dependencies:** #9, #10  
**Estimated:** 1.5 days

**Description:**
Create the API controller with all CRUD REST endpoints following REST conventions.

**Acceptance Criteria:**
- [ ] Create TodoItemsController in API/Controllers/TodoItemsController.cs
- [ ] Implement POST /api/todoitems (CreateTodoItemCommand) → 201 Created
- [ ] Implement GET /api/todoitems/{id} (GetTodoItemByIdQuery) → 200 OK or 404
- [ ] Implement PUT /api/todoitems/{id} (UpdateTodoItemCommand) → 200 OK or 404
- [ ] Implement DELETE /api/todoitems/{id} (DeleteTodoItemCommand) → 204 No Content or 404
- [ ] Implement GET /api/todoitems?status=Pending|Completed (GetTodoItemsQuery) → 200 OK
- [ ] Add [ProducesResponseType] attributes for OpenAPI
- [ ] Create requests return CreatedAtAction with location header
- [ ] All endpoints delegate to IMediator
- [ ] No business logic in controller

---

### #12 - Add JWT authentication and authorization
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#12](https://github.com/akarthickarun/smart-todo-app/issues/12)  
**Dependencies:** #11, #3  
**Estimated:** 1.5 days

**Description:**
Configure JWT Bearer authentication and add authorization to endpoints.

**Acceptance Criteria:**
- [ ] Configure JWT in Program.cs with token validation parameters
- [ ] Add appsettings.json values: Jwt:Key, Jwt:Issuer, Jwt:Audience, Jwt:ExpiryMinutes
- [ ] Create a token generation service or helper for dev/testing
- [ ] Add [Authorize] attributes to all command endpoints (POST, PUT, DELETE)
- [ ] Query endpoints (GET) remain public for now
- [ ] Test: unauthorized requests return 401
- [ ] Test: authorized requests with valid token succeed
- [ ] (Optional) Add authorization policy for advanced control

---

## Phase 5: Testing (Backend)

### #13 - Add xUnit unit tests for handlers and validators
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#13](https://github.com/akarthickarun/smart-todo-app/issues/13)  
**Dependencies:** #9, #10  
**Estimated:** 2 days

**Description:**
Write unit tests for all command/query handlers and validators covering success and failure paths.

**Acceptance Criteria:**
- [ ] CreateTodoItemCommandValidator tests: valid input, empty title, title too long, invalid duedate
- [ ] CreateTodoItemCommandHandler tests: creates entity, persists, returns id, logs
- [ ] UpdateTodoItemCommandValidator tests: similar to create
- [ ] UpdateTodoItemCommandHandler tests: updates entity, persists, logs
- [ ] GetTodoItemByIdQueryHandler tests: returns DTO, throws NotFoundException if not found
- [ ] GetTodoItemsQueryHandler tests: returns list, applies status filter, orders correctly
- [ ] Minimum 3-5 tests per handler/validator
- [ ] Use Moq for DbContext mocking
- [ ] Use FluentAssertions for assertions

---

### #14 - Add integration tests for API endpoints
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#14](https://github.com/akarthickarun/smart-todo-app/issues/14)  
**Dependencies:** #11, #12, #13  
**Estimated:** 2 days

**Description:**
Write integration tests for all API endpoints using WebApplicationFactory.

**Acceptance Criteria:**
- [ ] Create WebApplicationFactory<Program> fixture
- [ ] Test POST /api/todoitems: create todo, verify 201 and location header
- [ ] Test GET /api/todoitems/{id}: retrieve todo, verify 200 and DTO
- [ ] Test GET /api/todoitems/{id}: not found, verify 404
- [ ] Test PUT /api/todoitems/{id}: update todo, verify 200
- [ ] Test DELETE /api/todoitems/{id}: delete todo, verify 204
- [ ] Test GET /api/todoitems?status=Completed: filter by status
- [ ] Test unauthorized POST (no token): verify 401
- [ ] Test invalid requests: verify 400 with Problem Details
- [ ] All tests use in-memory database or fresh database per test

---

## Phase 6: Frontend Foundation

### #15 - Set up React + TypeScript + Vite + Tailwind
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#15](https://github.com/akarthickarun/smart-todo-app/issues/15)  
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

### #16 - Create layouts and routing
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#16](https://github.com/akarthickarun/smart-todo-app/issues/16)  
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

### #17 - Create auth store and login page (mock)
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#17](https://github.com/akarthickarun/smart-todo-app/issues/17)  
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

### #18 - Create axios instance and todo API client
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#18](https://github.com/akarthickarun/smart-todo-app/issues/18)  
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

### #19 - Create TanStack Query hooks
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#19](https://github.com/akarthickarun/smart-todo-app/issues/19)  
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

### #20 - Create Todo components
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#20](https://github.com/akarthickarun/smart-todo-app/issues/20)  
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

### #21 - Create TodosPage and integrate state
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#21](https://github.com/akarthickarun/smart-todo-app/issues/21)  
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

### #22 - Add frontend component tests
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#22](https://github.com/akarthickarun/smart-todo-app/issues/22)  
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

### #23 - Add frontend hook tests
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#23](https://github.com/akarthickarun/smart-todo-app/issues/23)  
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

### #24 - Add Docker setup
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#24](https://github.com/akarthickarun/smart-todo-app/issues/24)  
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

### #25 - Set up GitHub Actions CI pipeline
**Status:** Not Started  
**GitHub Issue:** [akarthickarun/smart-todo-app#25](https://github.com/akarthickarun/smart-todo-app/issues/25)  
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
| 1. Backend Foundation | #2-3 | ✅ #2 Completed |
| 2. Domain & Data | #4-8 | Not Started |
| 3. Application (CQRS) | #9-10 | Not Started |
| 4. API Layer | #11-12 | Not Started |
| 5. Backend Testing | #13-14 | Not Started |
| 6. Frontend Setup | #15-17 | Not Started |
| 7. Frontend API & State | #18-19 | Not Started |
| 8. Frontend UI | #20-21 | Not Started |
| 9. Frontend Testing | #22-23 | Not Started |
| 10. DevOps & CI | #24-25 | Not Started |

**Total:** 25 tickets
**Estimated Duration:** ~14 days (concurrent phases possible)

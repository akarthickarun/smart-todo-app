# PowerShell script to create all 25 GitHub issues using GitHub CLI
# Prerequisites: Install GitHub CLI from https://cli.github.com and authenticate with 'gh auth login'

Write-Host "Creating GitHub Issues for Smart Todo App..." -ForegroundColor Cyan
Write-Host "This will create 25 issues in akarthickarun/smart-todo-app" -ForegroundColor Yellow
Write-Host ""

# Check if gh CLI is installed
try {
    $ghVersion = gh --version
    Write-Host "✓ GitHub CLI found: $($ghVersion[0])" -ForegroundColor Green
} catch {
    Write-Host "✗ GitHub CLI not found. Please install from https://cli.github.com" -ForegroundColor Red
    exit 1
}

# Check if authenticated
try {
    gh auth status 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Not authenticated. Run 'gh auth login' first" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Authenticated with GitHub" -ForegroundColor Green
} catch {
    Write-Host "✗ Authentication check failed. Run 'gh auth login'" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Creating issues..." -ForegroundColor Cyan
Write-Host ""

# Issue #2
gh issue create --repo akarthickarun/smart-todo-app --title "Set up ASP.NET Core 10 projects and dependency injection" --body "**Phase:** 1 - Backend Foundation
**Status:** ✅ Completed
**Dependencies:** None
**Estimated:** 1 day

**Description:**
Set up Clean Architecture project structure with 5 backend projects and 2 test projects. Configure dependency injection, appsettings, and verify solution builds.

**Acceptance Criteria:**
- [x] Create SmartTodoApp.Domain project (no dependencies)
- [x] Create SmartTodoApp.Application project (depends on Domain)
- [x] Create SmartTodoApp.Infrastructure project (depends on Application)
- [x] Create SmartTodoApp.API project (depends on Application + Infrastructure)
- [x] Create SmartTodoApp.Shared.Contracts project (standalone DTOs)
- [x] Create test projects: Application.UnitTests and API.IntegrationTests
- [x] Add NuGet packages: MediatR, FluentValidation, AutoMapper, EF Core, Serilog
- [x] Create appsettings.json with database, JWT, and CORS configuration
- [x] Create Program.cs with DI registration and middleware pipeline
- [x] Solution builds successfully

**Related:** See TICKETS.md for full details" --label "phase-1,backend,completed"

Write-Host "✓ Created issue #2" -ForegroundColor Green

# Issue #3
gh issue create --repo akarthickarun/smart-todo-app --title "Implement middleware and error handling" --body "**Phase:** 1 - Backend Foundation
**Dependencies:** #2
**Estimated:** 1 day

**Description:**
Create middleware for correlation ID tracking and global exception handling with RFC 7807 Problem Details.

**Acceptance Criteria:**
- [ ] Create CorrelationIdMiddleware (extracts/generates X-Correlation-ID)
- [ ] Create ExceptionHandlingMiddleware (catches all exceptions)
- [ ] Map ValidationException → 400 with Problem Details
- [ ] Map NotFoundException → 404 with Problem Details
- [ ] Map all other exceptions → 500 with Problem Details
- [ ] Include traceId and correlationId in all error responses
- [ ] Register middleware in Program.cs
- [ ] Log exceptions with Serilog including CorrelationId
- [ ] Hide stack traces in production
- [ ] Test with manual exceptions in dev mode

**Related:** See TICKETS.md for full details" --label "phase-1,backend"

Write-Host "✓ Created issue #3" -ForegroundColor Green

# Issue #4
gh issue create --repo akarthickarun/smart-todo-app --title "Create TodoItem domain entity" --body "**Phase:** 2 - Domain & Data
**Dependencies:** #2
**Estimated:** 0.5 days

**Description:**
Implement TodoItem entity with business logic and domain rules.

**Acceptance Criteria:**
- [ ] Create Domain/Entities/TodoItem.cs
- [ ] Properties: Id, Title, Description, Status, CreatedAt, CompletedAt
- [ ] Factory method: TodoItem.Create(title, description)
- [ ] Method: MarkAsComplete() (validates state, sets CompletedAt)
- [ ] Method: UpdateDetails(title, description)
- [ ] Private constructor for EF Core
- [ ] Enforce business rules (title not empty, status transitions)
- [ ] No infrastructure dependencies in Domain layer

**Related:** See TICKETS.md for full details" --label "phase-2,backend,domain"

Write-Host "✓ Created issue #4" -ForegroundColor Green

# Issue #5
gh issue create --repo akarthickarun/smart-todo-app --title "Create DTOs in Shared.Contracts" --body "**Phase:** 2 - Domain & Data
**Dependencies:** #4
**Estimated:** 0.5 days

**Description:**
Create immutable record DTOs for API communication between frontend and backend.

**Acceptance Criteria:**
- [ ] Create Shared.Contracts/TodoItems/TodoItemDto.cs
- [ ] TodoItemDto: Id, Title, Description, Status, CreatedAt, CompletedAt
- [ ] Create CreateTodoItemRequest.cs
- [ ] Create UpdateTodoItemRequest.cs
- [ ] Use record types for immutability
- [ ] Include data annotations for OpenAPI docs
- [ ] All DTOs match domain entity structure

**Related:** See TICKETS.md for full details" --label "phase-2,backend,contracts"

Write-Host "✓ Created issue #5" -ForegroundColor Green

# Issue #6
gh issue create --repo akarthickarun/smart-todo-app --title "Set up EF Core DbContext and configurations" --body "**Phase:** 2 - Domain & Data
**Dependencies:** #4
**Estimated:** 1 day

**Description:**
Create ApplicationDbContext with entity configurations and IApplicationDbContext interface.

**Acceptance Criteria:**
- [ ] Create Application/Common/Interfaces/IApplicationDbContext.cs
- [ ] Create Infrastructure/Persistence/ApplicationDbContext.cs
- [ ] DbSet<TodoItem> property exposed
- [ ] Create Infrastructure/Persistence/Configurations/TodoItemConfiguration.cs
- [ ] Configure TodoItem: table name, PK, required fields, max lengths
- [ ] Add indexes: Status, CreatedAt
- [ ] Apply configurations via ApplyConfigurationsFromAssembly
- [ ] Register DbContext with SQL Server provider in DI
- [ ] ConnectionString from appsettings.json

**Related:** See TICKETS.md for full details" --label "phase-2,backend,infrastructure"

Write-Host "✓ Created issue #6" -ForegroundColor Green

# Issue #7
gh issue create --repo akarthickarun/smart-todo-app --title "Create database migrations" --body "**Phase:** 2 - Domain & Data
**Dependencies:** #6
**Estimated:** 0.5 days

**Description:**
Generate and test EF Core migrations for TodoItem table creation.

**Acceptance Criteria:**
- [ ] Run: dotnet ef migrations add InitialCreate
- [ ] Migration creates TodoItems table with correct schema
- [ ] Migration includes indexes on Status and CreatedAt
- [ ] Run: dotnet ef database update (verify in SQL Server)
- [ ] Verify table exists with correct columns and constraints
- [ ] Test: Insert sample data via SQL
- [ ] Test: Query sample data via EF Core
- [ ] Document migration commands in README

**Related:** See TICKETS.md for full details" --label "phase-2,backend,infrastructure"

Write-Host "✓ Created issue #7" -ForegroundColor Green

# Issue #8
gh issue create --repo akarthickarun/smart-todo-app --title "Add AutoMapper profile and MediatR behaviors" --body "**Phase:** 2 - Domain & Data
**Dependencies:** #5, #6
**Estimated:** 1 day

**Description:**
Configure AutoMapper for entity-to-DTO mapping and implement MediatR pipeline behaviors.

**Acceptance Criteria:**
- [ ] Create Application/Common/Mappings/MappingProfile.cs
- [ ] Mapping: TodoItem → TodoItemDto
- [ ] Mapping: CreateTodoItemRequest → TodoItem
- [ ] Create Application/Common/Behaviors/ValidationBehavior.cs
- [ ] ValidationBehavior runs all FluentValidation validators
- [ ] ValidationBehavior throws ValidationException on failure
- [ ] Create Application/Common/Behaviors/LoggingBehavior.cs
- [ ] LoggingBehavior logs request start/end with elapsed time
- [ ] Register behaviors in Application/DependencyInjection.cs
- [ ] Test: Trigger validation error, verify exception thrown

**Related:** See TICKETS.md for full details" --label "phase-2,backend,application"

Write-Host "✓ Created issue #8" -ForegroundColor Green

# Issue #9
gh issue create --repo akarthickarun/smart-todo-app --title "Implement todo CRUD commands and handlers" --body "**Phase:** 3 - Application (CQRS)
**Dependencies:** #4, #5, #8
**Estimated:** 1.5 days

**Description:**
Implement all CQRS commands with handlers and validators for todo operations.

**Acceptance Criteria:**
- [ ] CreateTodoItemCommand + Handler + Validator
- [ ] UpdateTodoItemCommand + Handler + Validator
- [ ] DeleteTodoItemCommand + Handler
- [ ] MarkTodoItemCompleteCommand + Handler + Validator
- [ ] All handlers use IApplicationDbContext
- [ ] All handlers log with structured logging
- [ ] All validators use FluentValidation rules
- [ ] Commands return void or ID only (no entities)
- [ ] Handlers call SaveChangesAsync with cancellation token
- [ ] Test: Send command via MediatR, verify DB persistence

**Related:** See TICKETS.md for full details" --label "phase-3,backend,application,cqrs"

Write-Host "✓ Created issue #9" -ForegroundColor Green

# Issue #10
gh issue create --repo akarthickarun/smart-todo-app --title "Implement todo CRUD queries and handlers" --body "**Phase:** 3 - Application (CQRS)
**Dependencies:** #4, #5, #8
**Estimated:** 1.5 days

**Description:**
Implement all CQRS queries with handlers for reading todo data.

**Acceptance Criteria:**
- [ ] GetTodoItemByIdQuery + Handler
- [ ] GetTodoItemsQuery + Handler (with optional Status filter)
- [ ] Queries use AsNoTracking() for read-only operations
- [ ] Handlers use AutoMapper ProjectTo for efficient DTO mapping
- [ ] GetTodoItemByIdQuery throws NotFoundException if not found
- [ ] GetTodoItemsQuery orders by CreatedAt descending
- [ ] All handlers log with structured logging
- [ ] Queries return DTOs only (never domain entities)
- [ ] Test: Send query via MediatR, verify DTO returned

**Related:** See TICKETS.md for full details" --label "phase-3,backend,application,cqrs"

Write-Host "✓ Created issue #10" -ForegroundColor Green

# Issue #11
gh issue create --repo akarthickarun/smart-todo-app --title "Create TodoItemsController with REST endpoints" --body "**Phase:** 4 - API Layer
**Dependencies:** #9, #10
**Estimated:** 1.5 days

**Description:**
Create REST API controller with all CRUD endpoints delegating to MediatR.

**Acceptance Criteria:**
- [ ] Create API/Controllers/TodoItemsController.cs
- [ ] POST /api/todoitems → CreateTodoItemCommand → 201 Created
- [ ] GET /api/todoitems/{id} → GetTodoItemByIdQuery → 200 OK
- [ ] GET /api/todoitems?status=Completed → GetTodoItemsQuery → 200 OK
- [ ] PUT /api/todoitems/{id} → UpdateTodoItemCommand → 200 OK
- [ ] DELETE /api/todoitems/{id} → DeleteTodoItemCommand → 204 No Content
- [ ] PATCH /api/todoitems/{id}/complete → MarkTodoItemCompleteCommand → 200 OK
- [ ] All endpoints delegate to IMediator
- [ ] All endpoints include ProducesResponseType attributes
- [ ] All endpoints use CancellationToken
- [ ] Test: Use Postman/curl to verify all endpoints

**Related:** See TICKETS.md for full details" --label "phase-4,backend,api"

Write-Host "✓ Created issue #11" -ForegroundColor Green

# Issue #12
gh issue create --repo akarthickarun/smart-todo-app --title "Add JWT authentication and authorization" --body "**Phase:** 4 - API Layer
**Dependencies:** #11, #3
**Estimated:** 1.5 days

**Description:**
Implement JWT Bearer authentication and role-based authorization.

**Acceptance Criteria:**
- [ ] Configure JWT Bearer authentication in Program.cs
- [ ] JWT settings in appsettings.json: Key, Issuer, Audience, ExpiryMinutes
- [ ] Create AuthController with /api/auth/login endpoint (mock user)
- [ ] Login endpoint generates JWT token with claims (id, email, roles)
- [ ] Add [Authorize] attribute to TodoItemsController
- [ ] Create authorization policy: CanManageTodos
- [ ] Test: Call protected endpoint without token → 401
- [ ] Test: Call protected endpoint with valid token → 200
- [ ] Test: Call protected endpoint with expired token → 401
- [ ] Add Swagger/OpenAPI Bearer security definition

**Related:** See TICKETS.md for full details" --label "phase-4,backend,api,security"

Write-Host "✓ Created issue #12" -ForegroundColor Green

# Issue #13
gh issue create --repo akarthickarun/smart-todo-app --title "Add xUnit unit tests for handlers and validators" --body "**Phase:** 5 - Backend Testing
**Dependencies:** #9, #10
**Estimated:** 2 days

**Description:**
Write comprehensive unit tests for all command/query handlers and validators.

**Acceptance Criteria:**
- [ ] CreateTodoItemCommandValidator tests: valid input, empty title, title too long
- [ ] CreateTodoItemCommandHandler tests: creates entity, persists, returns id, logs
- [ ] UpdateTodoItemCommandValidator tests: similar to create
- [ ] UpdateTodoItemCommandHandler tests: updates entity, persists, logs
- [ ] GetTodoItemByIdQueryHandler tests: returns DTO, throws NotFoundException
- [ ] GetTodoItemsQueryHandler tests: returns list, applies filter, orders correctly
- [ ] Minimum 3-5 tests per handler/validator
- [ ] Use Moq for DbContext mocking
- [ ] Use FluentAssertions for assertions

**Related:** See TICKETS.md for full details" --label "phase-5,backend,testing"

Write-Host "✓ Created issue #13" -ForegroundColor Green

# Issue #14
gh issue create --repo akarthickarun/smart-todo-app --title "Add integration tests for API endpoints" --body "**Phase:** 5 - Backend Testing
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

**Related:** See TICKETS.md for full details" --label "phase-5,backend,testing"

Write-Host "✓ Created issue #14" -ForegroundColor Green

# Issue #15
gh issue create --repo akarthickarun/smart-todo-app --title "Set up React + TypeScript + Vite + Tailwind" --body "**Phase:** 6 - Frontend Foundation
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
- [ ] Verify build succeeds with npm run build
- [ ] Verify dev server starts with npm run dev

**Related:** See TICKETS.md for full details" --label "phase-6,frontend,setup"

Write-Host "✓ Created issue #15" -ForegroundColor Green

# Issue #16
gh issue create --repo akarthickarun/smart-todo-app --title "Create layouts and routing" --body "**Phase:** 6 - Frontend Foundation
**Dependencies:** #15
**Estimated:** 1 day

**Description:**
Set up React Router with main layout and core routes.

**Acceptance Criteria:**
- [ ] Create src/router/index.tsx with createBrowserRouter
- [ ] Create MainLayout.tsx with header, footer, Outlet
- [ ] Create Header.tsx component (placeholder)
- [ ] Create Footer.tsx component (placeholder)
- [ ] Define routes: / (protected), /login, * (not found)
- [ ] Create ProtectedRoute wrapper component
- [ ] Create NotFoundPage component
- [ ] Create LoginPage component (stub)
- [ ] Create TodosPage component (stub)
- [ ] Router initialization in App.tsx

**Related:** See TICKETS.md for full details" --label "phase-6,frontend"

Write-Host "✓ Created issue #16" -ForegroundColor Green

# Issue #17
gh issue create --repo akarthickarun/smart-todo-app --title "Create auth store and login page (mock)" --body "**Phase:** 6 - Frontend Foundation
**Dependencies:** #16
**Estimated:** 1 day

**Description:**
Implement Zustand auth store and basic login page for development.

**Acceptance Criteria:**
- [ ] Create src/features/auth/stores/authStore.ts with Zustand
- [ ] Store holds: token, user, isAuthenticated
- [ ] Implement login(token, user) and logout() actions
- [ ] Use persist middleware to save to localStorage
- [ ] Create LoginPage.tsx with mock email/password input
- [ ] LoginPage calls authStore.login() with hardcoded token
- [ ] ProtectedRoute checks authStore.isAuthenticated
- [ ] Logout clears token and redirects to /login

**Related:** See TICKETS.md for full details" --label "phase-6,frontend,auth"

Write-Host "✓ Created issue #17" -ForegroundColor Green

# Issue #18
gh issue create --repo akarthickarun/smart-todo-app --title "Create axios instance and todo API client" --body "**Phase:** 7 - Frontend API & State
**Dependencies:** #5, #17
**Estimated:** 1.5 days

**Description:**
Build axios client with interceptors and todo API methods with Zod validation.

**Acceptance Criteria:**
- [ ] Create src/api/axios-instance.ts with base URL from VITE_API_BASE_URL
- [ ] Request interceptor adds Authorization header from authStore token
- [ ] Request interceptor adds X-Correlation-ID header (random UUID)
- [ ] Response interceptor handles 401 (clears auth, redirects)
- [ ] Response interceptor handles 403 (logs error)
- [ ] Create src/api/todo-api.ts with methods: getAll, getById, create, update, delete
- [ ] Create src/features/todos/schemas/todoSchemas.ts with Zod schemas
- [ ] TodoItemDto schema matches backend
- [ ] CreateTodoInput schema with validation rules
- [ ] UpdateTodoInput schema with validation rules
- [ ] API methods validate responses with Zod before returning

**Related:** See TICKETS.md for full details" --label "phase-7,frontend,api"

Write-Host "✓ Created issue #18" -ForegroundColor Green

# Issue #19
gh issue create --repo akarthickarun/smart-todo-app --title "Create TanStack Query hooks" --body "**Phase:** 7 - Frontend API & State
**Dependencies:** #18
**Estimated:** 1.5 days

**Description:**
Implement custom hooks for querying and mutating todos with TanStack Query v5.

**Acceptance Criteria:**
- [ ] Create src/lib/query-client.ts with QueryClient configuration
- [ ] Configure defaults: retry, refetchOnWindowFocus, staleTime, gcTime
- [ ] Create src/features/todos/hooks/useTodos.ts (query all todos)
- [ ] Create src/features/todos/hooks/useGetTodoById.ts (query single)
- [ ] Create src/features/todos/hooks/useCreateTodo.ts (mutation)
- [ ] Create src/features/todos/hooks/useUpdateTodo.ts (mutation)
- [ ] Create src/features/todos/hooks/useDeleteTodo.ts (mutation)
- [ ] All hooks use queryKey namespacing: ['todos'], ['todos', id]
- [ ] Mutations invalidate relevant queries on success
- [ ] Error handling provides user feedback via toast
- [ ] Wrap App with QueryClientProvider in main.tsx

**Related:** See TICKETS.md for full details" --label "phase-7,frontend,state"

Write-Host "✓ Created issue #19" -ForegroundColor Green

# Issue #20
gh issue create --repo akarthickarun/smart-todo-app --title "Create Todo components" --body "**Phase:** 8 - Frontend UI
**Dependencies:** #15, #19
**Estimated:** 2 days

**Description:**
Build all reusable Todo components for list, create, edit, and delete operations.

**Acceptance Criteria:**
- [ ] Create src/components/todos/TodoList.tsx (displays filtered list)
- [ ] TodoList supports Status filter buttons (All, Pending, Completed)
- [ ] Create src/components/todos/TodoItem.tsx (single todo card)
- [ ] TodoItem displays title, description, status badge, duedate, action buttons
- [ ] TodoItem has edit and delete buttons with confirm dialog
- [ ] Create src/components/todos/CreateTodoDialog.tsx (create form)
- [ ] CreateTodoDialog uses React Hook Form + Zod validation
- [ ] Form has Title, Description, DueDate inputs
- [ ] Form submission creates todo via useCreateTodo hook
- [ ] Create src/components/todos/UpdateTodoDialog.tsx (edit form)
- [ ] UpdateTodoDialog pre-populates with existing data
- [ ] All components use shadcn/ui components
- [ ] All components show loading/error states
- [ ] All components are responsive

**Related:** See TICKETS.md for full details" --label "phase-8,frontend,ui"

Write-Host "✓ Created issue #20" -ForegroundColor Green

# Issue #21
gh issue create --repo akarthickarun/smart-todo-app --title "Create TodosPage and integrate state" --body "**Phase:** 8 - Frontend UI
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
- [ ] List updates immediately after create/update/delete
- [ ] Status filter in TodoList calls useTodos with filter parameter
- [ ] Empty state message when no todos exist
- [ ] All mutations show toast notifications (success/error)

**Related:** See TICKETS.md for full details" --label "phase-8,frontend,ui"

Write-Host "✓ Created issue #21" -ForegroundColor Green

# Issue #22
gh issue create --repo akarthickarun/smart-todo-app --title "Add frontend component tests" --body "**Phase:** 9 - Testing (Frontend)
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

**Related:** See TICKETS.md for full details" --label "phase-9,frontend,testing"

Write-Host "✓ Created issue #22" -ForegroundColor Green

# Issue #23
gh issue create --repo akarthickarun/smart-todo-app --title "Add frontend hook tests" --body "**Phase:** 9 - Testing (Frontend)
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

**Related:** See TICKETS.md for full details" --label "phase-9,frontend,testing"

Write-Host "✓ Created issue #23" -ForegroundColor Green

# Issue #24
gh issue create --repo akarthickarun/smart-todo-app --title "Add Docker setup" --body "**Phase:** 10 - Containerization & CI
**Dependencies:** #11, #21
**Estimated:** 1 day

**Description:**
Create Dockerfiles and docker-compose for full local development environment.

**Acceptance Criteria:**
- [ ] Create src/SmartTodoApp.API/Dockerfile (multi-stage: sdk build, aspnet runtime)
- [ ] Backend Dockerfile copies all projects, restores, builds, publishes
- [ ] Create src/frontend/Dockerfile (Node build stage, nginx serve stage)
- [ ] Frontend Dockerfile builds React app, copies to nginx
- [ ] Create src/frontend/nginx.conf (proxy /api calls to backend)
- [ ] Create docker-compose.yml with 3 services: sqlserver, backend, frontend
- [ ] SQL Server service with healthcheck
- [ ] Backend service environment variables: connection string, JWT settings
- [ ] Frontend service environment variables: VITE_API_BASE_URL
- [ ] Test: docker-compose up --build succeeds
- [ ] Test: curl http://localhost:3000 returns frontend
- [ ] Test: curl http://localhost:5000/health returns 200

**Related:** See TICKETS.md for full details" --label "phase-10,devops,docker"

Write-Host "✓ Created issue #24" -ForegroundColor Green

# Issue #25
gh issue create --repo akarthickarun/smart-todo-app --title "Set up GitHub Actions CI pipeline" --body "**Phase:** 10 - Containerization & CI
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

**Related:** See TICKETS.md for full details" --label "phase-10,devops,ci-cd"

Write-Host "✓ Created issue #25" -ForegroundColor Green

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "✓ Successfully created all 25 GitHub issues!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "View issues at: https://github.com/akarthickarun/smart-todo-app/issues" -ForegroundColor Yellow
Write-Host ""

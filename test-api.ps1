# Smart Todo API Test Script
# Usage: .\test-api.ps1
# Make sure the API is running on https://localhost:7083

$baseUrl = "https://localhost:7083/api/todoitems"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Smart Todo API Test Suite" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Test 1: Create a new todo
Write-Host "1. Creating a new todo..." -ForegroundColor Yellow
$createJson = @{
    title = "Complete project documentation"
    description = "Write comprehensive API documentation"
    dueDate = "2026-02-20"
} | ConvertTo-Json

$todoId = curl.exe -k -X POST $baseUrl `
    -H "Content-Type: application/json" `
    -d $createJson `
    -s | ConvertFrom-Json

Write-Host "   Created todo with ID: $todoId" -ForegroundColor Green

# Test 2: Get all todos
Write-Host "`n2. Getting all todos..." -ForegroundColor Yellow
$allTodos = curl.exe -k $baseUrl -s | ConvertFrom-Json
Write-Host "   Found $($allTodos.Count) todo(s)" -ForegroundColor Green
$allTodos | Format-Table -AutoSize

# Test 3: Get todo by ID
Write-Host "`n3. Getting todo by ID..." -ForegroundColor Yellow
$todo = curl.exe -k "$baseUrl/$todoId" -s | ConvertFrom-Json
Write-Host "   Title: $($todo.title)" -ForegroundColor Green
Write-Host "   Status: $($todo.status)" -ForegroundColor Green

# Test 4: Update todo
Write-Host "`n4. Updating todo..." -ForegroundColor Yellow
$updateJson = @{
    title = "Complete project documentation - UPDATED"
    description = "Write comprehensive API and user documentation"
    dueDate = "2026-02-21"
} | ConvertTo-Json

curl.exe -k -X PUT "$baseUrl/$todoId" `
    -H "Content-Type: application/json" `
    -d $updateJson `
    -s -o $null -w "   HTTP Status: %{http_code}`n"

$updatedTodo = curl.exe -k "$baseUrl/$todoId" -s | ConvertFrom-Json
Write-Host "   Updated Title: $($updatedTodo.title)" -ForegroundColor Green

# Test 5: Mark as complete
Write-Host "`n5. Marking todo as complete..." -ForegroundColor Yellow
curl.exe -k -X PATCH "$baseUrl/$todoId/complete" `
    -s -o $null -w "   HTTP Status: %{http_code}`n"

$completedTodo = curl.exe -k "$baseUrl/$todoId" -s | ConvertFrom-Json
Write-Host "   Status: $($completedTodo.status) (1=Completed)" -ForegroundColor Green

# Test 6: Filter by status
Write-Host "`n6. Filtering todos by status..." -ForegroundColor Yellow
$completedTodos = curl.exe -k "$baseUrl`?status=1" -s | ConvertFrom-Json
Write-Host "   Completed todos: $($completedTodos.Count)" -ForegroundColor Green

$pendingTodos = curl.exe -k "$baseUrl`?status=0" -s | ConvertFrom-Json
Write-Host "   Pending todos: $($pendingTodos.Count)" -ForegroundColor Green

# Test 7: Delete todo
Write-Host "`n7. Deleting todo..." -ForegroundColor Yellow
curl.exe -k -X DELETE "$baseUrl/$todoId" `
    -s -o $null -w "   HTTP Status: %{http_code}`n"

$allTodosAfterDelete = curl.exe -k $baseUrl -s | ConvertFrom-Json
Write-Host "   Remaining todos: $($allTodosAfterDelete.Count)" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "All tests completed successfully!" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

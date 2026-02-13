# Smart Todo API - Curl Command Reference

Base URL: https://localhost:7083/api/todoitems

## 1. Create Todo (POST)
curl.exe -k -X POST https://localhost:7083/api/todoitems ^
  -H "Content-Type: application/json" ^
  -d "@create-todo.json"

# Or inline:
curl.exe -k -X POST https://localhost:7083/api/todoitems ^
  -H "Content-Type: application/json" ^
  -d "{\"title\":\"New Task\",\"description\":\"Task details\",\"dueDate\":\"2026-02-20\"}"

## 2. Get All Todos (GET)
curl.exe -k https://localhost:7083/api/todoitems

## 3. Get Todo by ID (GET)
curl.exe -k https://localhost:7083/api/todoitems/{id}

## 4. Update Todo (PUT)
curl.exe -k -X PUT https://localhost:7083/api/todoitems/{id} ^
  -H "Content-Type: application/json" ^
  -d "@update-todo.json"

## 5. Mark Todo Complete (PATCH)
curl.exe -k -X PATCH https://localhost:7083/api/todoitems/{id}/complete

## 6. Filter Todos by Status (GET)
# Get completed todos (status=1)
curl.exe -k "https://localhost:7083/api/todoitems?status=1"

# Get pending todos (status=0)
curl.exe -k "https://localhost:7083/api/todoitems?status=0"

## 7. Delete Todo (DELETE)
curl.exe -k -X DELETE https://localhost:7083/api/todoitems/{id}

## Note:
- Use -k flag to ignore SSL certificate warnings in development
- Status values: 0=Pending, 1=Completed, 2=Cancelled
- DueDate format: yyyy-MM-dd (e.g., "2026-02-20")
- Replace {id} with actual GUID from create response

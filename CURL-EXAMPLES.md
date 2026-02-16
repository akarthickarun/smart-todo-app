# Smart Todo API - Curl Command Reference

Base URL: https://localhost:7083/api

## Authentication

### 1. Login (POST) - Get JWT Token
```bash
curl.exe -k -X POST https://localhost:7083/api/auth/login ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"user@example.com\",\"password\":\"password\"}"
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "name": "user"
}
```

**Note:** For development/testing, any email and password combination will work. Save the token from the response to use in subsequent requests.

## Todo Items

Base URL: https://localhost:7083/api/todoitems

### 2. Create Todo (POST) - Requires Authentication
```bash
# First, get a token from /api/auth/login
# Then use the token in the Authorization header:
curl.exe -k -X POST https://localhost:7083/api/todoitems ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE" ^
  -d "@create-todo.json"

# Or inline:
curl.exe -k -X POST https://localhost:7083/api/todoitems ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE" ^
  -d "{\"title\":\"New Task\",\"description\":\"Task details\",\"dueDate\":\"2026-02-20\"}"
```

### 3. Get All Todos (GET) - Public
```bash
curl.exe -k https://localhost:7083/api/todoitems
```

### 4. Get Todo by ID (GET) - Public
```bash
curl.exe -k https://localhost:7083/api/todoitems/{id}
```

### 5. Update Todo (PUT) - Requires Authentication
```bash
curl.exe -k -X PUT https://localhost:7083/api/todoitems/{id} ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE" ^
  -d "@update-todo.json"
```

### 6. Mark Todo Complete (PATCH) - Requires Authentication
```bash
curl.exe -k -X PATCH https://localhost:7083/api/todoitems/{id}/complete ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### 7. Filter Todos by Status (GET) - Public
```bash
# Get completed todos (status=1)
curl.exe -k "https://localhost:7083/api/todoitems?status=1"

# Get pending todos (status=0)
curl.exe -k "https://localhost:7083/api/todoitems?status=0"
```

### 8. Delete Todo (DELETE) - Requires Authentication
```bash
curl.exe -k -X DELETE https://localhost:7083/api/todoitems/{id} ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Notes:
- **Authentication:** Command endpoints (POST, PUT, PATCH, DELETE) require a valid JWT token in the Authorization header
- **Getting a Token:** Use the `/api/auth/login` endpoint with any email/password (development mode accepts all credentials)
- **Token Format:** `Authorization: Bearer <your-token-here>`
- Use `-k` flag to ignore SSL certificate warnings in development
- Status values: 0=Pending, 1=Completed
- DueDate format: yyyy-MM-dd (e.g., "2026-02-20")
- Replace `{id}` with actual GUID from create response
- Replace `YOUR_TOKEN_HERE` with the token received from login

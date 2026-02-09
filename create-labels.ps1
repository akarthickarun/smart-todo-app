# PowerShell script to create GitHub labels for Smart Todo App issues

$repo = "akarthickarun/smart-todo-app"

Write-Host "Creating GitHub Labels for Smart Todo App..." -ForegroundColor Cyan
Write-Host "Repository: $repo" -ForegroundColor Yellow
Write-Host ""

# Phase labels (different colors)
$phases = @(
    @{ name = "phase-1"; color = "5DADE2"; description = "Phase 1: Backend Foundation" }
    @{ name = "phase-2"; color = "48C9B0"; description = "Phase 2: Domain & Data" }
    @{ name = "phase-3"; color = "76D7C4"; description = "Phase 3: Application (CQRS)" }
    @{ name = "phase-4"; color = "F8B88B"; description = "Phase 4: API Layer" }
    @{ name = "phase-5"; color = "F5B041"; description = "Phase 5: Backend Testing" }
    @{ name = "phase-6"; color = "F1948A"; description = "Phase 6: Frontend Foundation" }
    @{ name = "phase-7"; color = "E59866"; description = "Phase 7: Frontend API & State" }
    @{ name = "phase-8"; color = "D7BCCB"; description = "Phase 8: Frontend UI" }
    @{ name = "phase-9"; color = "A9DFBF"; description = "Phase 9: Testing (Frontend)" }
    @{ name = "phase-10"; color = "85C1E2"; description = "Phase 10: Containerization & CI" }
)

# Category labels
$categories = @(
    @{ name = "backend"; color = "0366D6"; description = "Backend development" }
    @{ name = "frontend"; color = "D73A49"; description = "Frontend development" }
    @{ name = "domain"; color = "28A745"; description = "Domain layer" }
    @{ name = "contracts"; color = "6F42C1"; description = "Shared contracts/DTOs" }
    @{ name = "application"; color = "6F42C1"; description = "Application layer (CQRS)" }
    @{ name = "infrastructure"; color = "1F6FEB"; description = "Infrastructure layer" }
    @{ name = "api"; color = "0366D6"; description = "API endpoints" }
    @{ name = "security"; color = "D73A49"; description = "Security/Authentication" }
    @{ name = "testing"; color = "A2AAAD"; description = "Testing/Test coverage" }
    @{ name = "devops"; color = "FD7E14"; description = "DevOps/Infrastructure" }
    @{ name = "docker"; color = "2496ED"; description = "Docker/Containerization" }
    @{ name = "ci-cd"; color = "FF6B6B"; description = "CI/CD Pipelines" }
    @{ name = "setup"; color = "B5A805"; description = "Project/Environment Setup" }
    @{ name = "auth"; color = "D73A49"; description = "Authentication/Authorization" }
    @{ name = "state"; color = "6F42C1"; description = "State Management" }
    @{ name = "ui"; color = "28A745"; description = "UI Components" }
    @{ name = "cqrs"; color = "6F42C1"; description = "CQRS (Commands/Queries)" }
)

# Status labels
$statuses = @(
    @{ name = "completed"; color = "28A745"; description = "Task completed" }
    @{ name = "in-progress"; color = "FFA500"; description = "Work in progress" }
    @{ name = "blocked"; color = "D73A49"; description = "Blocked by dependency" }
    @{ name = "ready"; color = "85C1E2"; description = "Ready to start" }
)

$allLabels = $phases + $categories + $statuses
$successCount = 0
$skipCount = 0

foreach ($label in $allLabels) {
    try {
        # Try to create the label
        gh label create $label.name `
            --repo $repo `
            --color $label.color `
            --description $label.description `
            2>&1 | Out-Null
        
        Write-Host "✓ Created label: $($label.name)" -ForegroundColor Green
        $successCount++
    }
    catch {
        # Label might already exist, try to update it
        $errorMsg = $_.Exception.Message
        if ($errorMsg -like "*already exists*") {
            Write-Host "✓ Label already exists: $($label.name)" -ForegroundColor Yellow
            $skipCount++
        }
        else {
            Write-Host "✗ Failed to create label: $($label.name)" -ForegroundColor Red
            Write-Host "  Error: $errorMsg" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Label Creation Summary:" -ForegroundColor Cyan
Write-Host "- Created: $successCount" -ForegroundColor Green
Write-Host "- Skipped (already exist): $skipCount" -ForegroundColor Yellow
Write-Host "- Total: $($allLabels.Count)" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Labels are now ready for issues!" -ForegroundColor Green

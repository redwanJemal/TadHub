#!/usr/bin/env bash
# =============================================================================
# TadHub Coding Standards Validator
# Checks that backend and frontend code follows project conventions.
#
# Usage:
#   ./scripts/validate-standards.sh              # Full validation
#   ./scripts/validate-standards.sh --changed    # Only validate changed files
#   ./scripts/validate-standards.sh --module Foo # Validate specific module
# =============================================================================

set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$PROJECT_ROOT"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

ERRORS=0
WARNINGS=0

error() { echo -e "${RED}[ERROR]${NC} $*"; ERRORS=$((ERRORS + 1)); }
warn()  { echo -e "${YELLOW}[WARN]${NC}  $*"; WARNINGS=$((WARNINGS + 1)); }
ok()    { echo -e "${GREEN}[OK]${NC}    $*"; }
info()  { echo -e "${BLUE}[INFO]${NC}  $*"; }

# Parse args
CHANGED_ONLY=false
TARGET_MODULE=""
while [[ $# -gt 0 ]]; do
    case "$1" in
        --changed) CHANGED_ONLY=true; shift ;;
        --module)  TARGET_MODULE="$2"; shift 2 ;;
        *)         shift ;;
    esac
done

# Get file list
if $CHANGED_ONLY; then
    CHANGED_FILES=$(git diff --name-only HEAD~1 2>/dev/null || git diff --name-only --cached)
else
    CHANGED_FILES=""
fi

should_check() {
    if $CHANGED_ONLY && [ -n "$CHANGED_FILES" ]; then
        echo "$CHANGED_FILES" | grep -q "$1" 2>/dev/null
        return $?
    fi
    return 0
}

echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "  TadHub Coding Standards Validator"
echo "═══════════════════════════════════════════════════════════════"
echo ""

# ─── 1. Controller Checks ────────────────────────────────────────────────────
info "Checking Controllers..."

for controller in src/TadHub.Api/Controllers/*.cs; do
    [ -f "$controller" ] || continue
    $CHANGED_ONLY && ! should_check "$controller" && continue

    name=$(basename "$controller" .cs)

    # Must have [ApiController]
    if ! grep -q '\[ApiController\]' "$controller"; then
        error "$name: Missing [ApiController] attribute"
    fi

    # Must have [Authorize] (except intentionally public controllers)
    PUBLIC_CONTROLLERS="CountriesController|JobCategoriesController|PlansController|StripeWebhookController|HealthController"
    if ! grep -q '\[Authorize' "$controller" && ! echo "$name" | grep -qE "$PUBLIC_CONTROLLERS"; then
        error "$name: Missing [Authorize] attribute"
    fi

    # Must return IActionResult (not ActionResult<T>)
    if grep -q 'Task<ActionResult<' "$controller"; then
        error "$name: Use Task<IActionResult> instead of Task<ActionResult<T>>"
    fi

    # Must have CancellationToken in async methods
    async_methods=$(grep -c 'public async' "$controller" 2>/dev/null || echo 0)
    ct_methods=$(grep -c 'CancellationToken' "$controller" 2>/dev/null || echo 0)
    if [ "$async_methods" -gt 0 ] && [ "$ct_methods" -lt "$async_methods" ]; then
        warn "$name: Some async methods may be missing CancellationToken parameter"
    fi

    # Check error mapping pattern
    if grep -q 'result\.IsSuccess' "$controller" && ! grep -q 'MapResultError\|MapError\|result\.ErrorCode' "$controller"; then
        warn "$name: Uses Result but may not have proper error mapping"
    fi
done
ok "Controllers checked"

# ─── 2. Service Checks ───────────────────────────────────────────────────────
info "Checking Services..."

for service in src/Modules/*/Core/Services/*.cs; do
    [ -f "$service" ] || continue
    $CHANGED_ONLY && ! should_check "$service" && continue

    name=$(basename "$service" .cs)

    # Check for Result<T> return types
    if grep -q 'public async Task<' "$service"; then
        # Services should use Result<T>, not raw types (except ListAsync which uses PagedList)
        raw_returns=$(grep -c 'public async Task<[A-Z][a-zA-Z]*Dto>' "$service" 2>/dev/null || echo 0)
        if [ "$raw_returns" -gt 0 ]; then
            warn "$name: Has methods returning raw DTOs instead of Result<Dto>"
        fi
    fi

    # Check CancellationToken
    async_methods=$(grep -c 'public async' "$service" 2>/dev/null || echo 0)
    ct_methods=$(grep -c 'CancellationToken' "$service" 2>/dev/null || echo 0)
    if [ "$async_methods" -gt 0 ] && [ "$ct_methods" -lt "$async_methods" ]; then
        warn "$name: Some async methods may be missing CancellationToken"
    fi

    # Check for throw statements (services should return Result, not throw)
    throw_count=$(grep -c 'throw new' "$service" 2>/dev/null || echo 0)
    if [ "$throw_count" -gt 0 ]; then
        warn "$name: Has $throw_count throw statement(s) — prefer returning Result.Failure()"
    fi
done
ok "Services checked"

# ─── 3. DTO Checks ───────────────────────────────────────────────────────────
info "Checking DTOs..."

for dto_dir in src/Modules/*/Contracts/DTOs; do
    [ -d "$dto_dir" ] || continue

    for dto in "$dto_dir"/*.cs; do
        [ -f "$dto" ] || continue
        $CHANGED_ONLY && ! should_check "$dto" && continue

        name=$(basename "$dto" .cs)

        # Must be sealed record
        if grep -q 'public class ' "$dto" || grep -q 'public record [A-Z]' "$dto" | grep -v 'sealed'; then
            if ! grep -q 'sealed record' "$dto" && ! grep -q 'sealed class' "$dto"; then
                warn "$name: DTOs should be 'sealed record' (found class or non-sealed record)"
            fi
        fi

        # Request DTOs should have validation attributes
        if [[ "$name" == Create* ]] || [[ "$name" == *Request* ]]; then
            if ! grep -q '\[Required\]\|\[MaxLength\]\|\[StringLength\]' "$dto"; then
                warn "$name: Create/Request DTO missing validation attributes ([Required], [MaxLength])"
            fi
        fi

        # Check for { get; init; } pattern
        if grep -q '{ get; set; }' "$dto"; then
            warn "$name: Use '{ get; init; }' instead of '{ get; set; }' in DTOs"
        fi
    done
done
ok "DTOs checked"

# ─── 4. Entity/EF Configuration Checks ───────────────────────────────────────
info "Checking Entity Configurations..."

for config in src/Modules/*/Core/Persistence/*Configuration.cs; do
    [ -f "$config" ] || continue
    $CHANGED_ONLY && ! should_check "$config" && continue

    name=$(basename "$config" .cs)

    # Check IEntityTypeConfiguration implementation
    if ! grep -q 'IEntityTypeConfiguration' "$config"; then
        warn "$name: Should implement IEntityTypeConfiguration<T>"
    fi

    # Check enum string conversion
    if grep -q 'HasConversion' "$config" && ! grep -q 'HasMaxLength' "$config"; then
        warn "$name: Enum conversion found but missing HasMaxLength"
    fi
done
ok "Entity configurations checked"

# ─── 5. Permission Seeder Check ──────────────────────────────────────────────
info "Checking Permission Seeder..."

SEEDER="src/Modules/Authorization/Authorization.Core/Seeds/PermissionSeeder.cs"
if [ -f "$SEEDER" ]; then
    # Check permission naming format: resource.action
    bad_perms=$(grep -oP '"[^"]*"' "$SEEDER" | grep -v '\.' | grep -v '^"$' | grep -v 'platform-admin\|tenant-admin\|Administrator\|Manager\|Viewer\|Staff\|description\|Permission\|Role\|Seed' | head -5)
    if [ -n "$bad_perms" ]; then
        warn "PermissionSeeder: Some permissions may not follow 'resource.action' format"
    fi
fi
ok "Permission seeder checked"

# ─── 6. Cross-Module Dependency Check ─────────────────────────────────────────
info "Checking cross-module dependencies..."

# Known legacy violations to whitelist (will be refactored later)
LEGACY_VIOLATIONS=(
    "Placement.Core:Candidate.Contracts"
    "Placement.Core:Worker.Contracts"
    "Placement.Core:Client.Contracts"
    "Placement.Core:Contract.Contracts"
    "Document.Core:Worker.Contracts"
    "Candidate.Core:Supplier.Contracts"
    "Worker.Core:Candidate.Contracts"
    "Worker.Core:Supplier.Contracts"
    "Contract.Core:Worker.Contracts"
    "Contract.Core:Client.Contracts"
)

is_legacy_violation() {
    local check="$1"
    for legacy in "${LEGACY_VIOLATIONS[@]}"; do
        if [ "$check" = "$legacy" ]; then
            return 0
        fi
    done
    return 1
}

for csproj in src/Modules/*/Core/*.csproj; do
    [ -f "$csproj" ] || continue

    module_name=$(basename "$(dirname "$(dirname "$csproj")")")
    core_name="$module_name.Core"

    # Check for cross-module project references
    while IFS= read -r ref_line; do
        # Extract the referenced project name
        ref_project=$(echo "$ref_line" | grep -oP '[A-Z][a-zA-Z]+\.(Contracts|Core)' || true)
        [ -z "$ref_project" ] && continue

        ref_module=$(echo "$ref_project" | sed 's/\.\(Contracts\|Core\)$//')

        # Skip self-references and SharedKernel/Infrastructure
        if [ "$ref_module" = "$module_name" ] || \
           [ "$ref_module" = "TadHub" ] || \
           [ "$ref_module" = "SharedKernel" ]; then
            continue
        fi

        # Skip system/infra modules (Authorization, Identity, Tenancy, Notification are infra)
        if [ "$ref_module" = "Authorization" ] || [ "$ref_module" = "Identity" ] || \
           [ "$module_name" = "Authorization" ] || [ "$module_name" = "Identity" ] || \
           [ "$module_name" = "Tenancy" ] || [ "$module_name" = "Notification" ]; then
            continue
        fi

        violation_key="$core_name:$ref_project"
        if is_legacy_violation "$violation_key"; then
            continue  # Known legacy, skip silently
        fi

        error "CROSS-MODULE VIOLATION: $core_name references $ref_project — modules must communicate via events, not direct references"
    done < <(grep 'ProjectReference' "$csproj" 2>/dev/null || true)
done

# Check services for cross-module service injections (in NEW files only if --changed)
for service in src/Modules/*/Core/Services/*.cs; do
    [ -f "$service" ] || continue
    $CHANGED_ONLY && ! should_check "$service" && continue

    service_module=$(echo "$service" | sed 's|src/Modules/\([^/]*\)/.*|\1|')
    service_name=$(basename "$service" .cs)

    # Look for injected interfaces from other modules
    while IFS= read -r using_line; do
        used_module=$(echo "$using_line" | grep -oP '(?<=using )[A-Z][a-zA-Z]+(?=\.Contracts)' || true)
        [ -z "$used_module" ] && continue

        if [ "$used_module" != "$service_module" ] && \
           [ "$used_module" != "TadHub" ] && \
           [ "$used_module" != "Authorization" ] && \
           [ "$used_module" != "Identity" ]; then
            warn "$service_name: Imports from $used_module.Contracts — cross-module service call detected"
        fi
    done < <(grep '^using ' "$service" 2>/dev/null || true)
done
ok "Cross-module dependencies checked"

# ─── 7. Frontend Skeleton Loading Check ───────────────────────────────────────
info "Checking frontend skeleton loading..."

for feature_dir in web/tenant-app/src/features/*/pages; do
    [ -d "$feature_dir" ] || continue

    feature=$(basename "$(dirname "$feature_dir")")

    # Check detail pages
    for page in "$feature_dir"/*DetailPage.tsx; do
        [ -f "$page" ] || continue
        $CHANGED_ONLY && ! should_check "$page" && continue

        page_name=$(basename "$page" .tsx)
        if ! grep -q 'Skeleton\|DetailSkeleton\|isLoading' "$page"; then
            error "Frontend $feature/$page_name: Missing skeleton loading — detail pages MUST have skeleton loading state"
        fi
    done

    # Check list pages
    for page in "$feature_dir"/*Page.tsx "$feature_dir"/*ListPage.tsx; do
        [ -f "$page" ] || continue
        $CHANGED_ONLY && ! should_check "$page" && continue

        page_name=$(basename "$page" .tsx)
        # Skip create/edit pages
        [[ "$page_name" == Create* ]] && continue
        [[ "$page_name" == Edit* ]] && continue
        [[ "$page_name" == *Detail* ]] && continue

        if ! grep -q 'isLoading\|Skeleton\|DataTableAdvanced' "$page"; then
            warn "Frontend $feature/$page_name: May be missing loading state — use DataTableAdvanced with isLoading or Skeleton"
        fi
    done
done
ok "Frontend skeleton loading checked"

# ─── 8. Frontend Type Checks ─────────────────────────────────────────────────
info "Checking Frontend Types..."

for types_file in web/tenant-app/src/features/*/types.ts; do
    [ -f "$types_file" ] || continue
    $CHANGED_ONLY && ! should_check "$types_file" && continue

    feature=$(basename "$(dirname "$types_file")")

    # Check for 'any' type usage
    any_count=$(grep -c ': any' "$types_file" 2>/dev/null || true)
    any_count=${any_count:-0}
    if [ "$any_count" -gt 0 ] 2>/dev/null; then
        warn "Frontend $feature/types.ts: Has $any_count 'any' type usage(s)"
    fi
done
ok "Frontend types checked"

# ─── 9. Frontend API Pattern Checks ──────────────────────────────────────────
info "Checking Frontend API patterns..."

for api_file in web/tenant-app/src/features/*/api.ts; do
    [ -f "$api_file" ] || continue
    $CHANGED_ONLY && ! should_check "$api_file" && continue

    feature=$(basename "$(dirname "$api_file")")

    # Must use apiClient (not raw fetch) — except file upload/download functions which may need raw fetch
    fetch_count=$(grep -c 'fetch(' "$api_file" 2>/dev/null || true)
    fetch_count=${fetch_count:-0}
    # Allow raw fetch for: file uploads (FormData), blob downloads (pdf, blob, arraybuffer)
    binary_context=$(grep -c 'FormData\|multipart\|upload\|Upload\|\.blob()\|pdf\|Pdf\|PDF\|arraybuffer\|Blob' "$api_file" 2>/dev/null || true)
    binary_context=${binary_context:-0}
    if [ "${fetch_count}" -gt 0 ] 2>/dev/null && [ "${binary_context}" -eq 0 ] 2>/dev/null; then
        error "Frontend $feature/api.ts: Uses raw fetch() without file/binary context — must use apiClient"
    fi

    # Must use tenantPath for tenant-scoped routes
    if grep -q "'/tenants/" "$api_file" && ! grep -q 'tenantPath' "$api_file"; then
        warn "Frontend $feature/api.ts: Hardcoded tenant path — use tenantPath() helper"
    fi
done
ok "Frontend API patterns checked"

# ─── 10. Frontend Hook Checks ────────────────────────────────────────────────
info "Checking Frontend Hooks..."

for hooks_file in web/tenant-app/src/features/*/hooks.ts; do
    [ -f "$hooks_file" ] || continue
    $CHANGED_ONLY && ! should_check "$hooks_file" && continue

    feature=$(basename "$(dirname "$hooks_file")")

    # Must use useQuery/useMutation
    if ! grep -q 'useQuery\|useMutation' "$hooks_file"; then
        warn "Frontend $feature/hooks.ts: Should use TanStack Query (useQuery/useMutation)"
    fi

    # Mutations should invalidate queries
    mutation_count=$(grep -c 'useMutation' "$hooks_file" 2>/dev/null || echo 0)
    invalidate_count=$(grep -c 'invalidateQueries' "$hooks_file" 2>/dev/null || echo 0)
    if [ "$mutation_count" -gt 0 ] && [ "$invalidate_count" -lt "$mutation_count" ]; then
        warn "Frontend $feature/hooks.ts: Some mutations may not invalidate queries ($mutation_count mutations, $invalidate_count invalidations)"
    fi
done
ok "Frontend hooks checked"

# ─── 11. i18n Completeness Check ─────────────────────────────────────────────
info "Checking i18n completeness..."

for en_file in web/tenant-app/src/features/*/i18n/en.json; do
    [ -f "$en_file" ] || continue
    $CHANGED_ONLY && ! should_check "$en_file" && continue

    feature=$(basename "$(dirname "$(dirname "$en_file")")")
    ar_file="$(dirname "$en_file")/ar.json"

    if [ ! -f "$ar_file" ]; then
        error "Frontend $feature: Missing Arabic translation file (ar.json)"
        continue
    fi

    # Compare key counts
    en_keys=$(python3 -c "
import json
def count_keys(obj, prefix=''):
    count = 0
    for k, v in obj.items():
        if isinstance(v, dict):
            count += count_keys(v, f'{prefix}{k}.')
        else:
            count += 1
    return count
with open('$en_file') as f:
    print(count_keys(json.load(f)))
" 2>/dev/null || echo 0)

    ar_keys=$(python3 -c "
import json
def count_keys(obj, prefix=''):
    count = 0
    for k, v in obj.items():
        if isinstance(v, dict):
            count += count_keys(v, f'{prefix}{k}.')
        else:
            count += 1
    return count
with open('$ar_file') as f:
    print(count_keys(json.load(f)))
" 2>/dev/null || echo 0)

    if [ "$en_keys" != "$ar_keys" ]; then
        warn "Frontend $feature i18n: Key count mismatch — en: $en_keys, ar: $ar_keys"
    fi
done
ok "i18n completeness checked"

# ─── 12. Build Verification ──────────────────────────────────────────────────
info "Checking builds..."

# Backend build
if dotnet build src/TadHub.Api/TadHub.Api.csproj --nologo -v q 2>&1 | grep -q 'Build succeeded'; then
    ok "Backend build: passed"
else
    error "Backend build: FAILED"
fi

# Frontend type check
if cd web/tenant-app && npx tsc --noEmit 2>&1 | tail -1 | grep -q 'error'; then
    error "Frontend TypeScript: FAILED"
else
    ok "Frontend TypeScript: passed"
fi
cd "$PROJECT_ROOT"

# ─── Summary ─────────────────────────────────────────────────────────────────
echo ""
echo "═══════════════════════════════════════════════════════════════"
if [ $ERRORS -eq 0 ] && [ $WARNINGS -eq 0 ]; then
    echo -e "  ${GREEN}All checks passed!${NC}"
elif [ $ERRORS -eq 0 ]; then
    echo -e "  ${YELLOW}Passed with $WARNINGS warning(s)${NC}"
else
    echo -e "  ${RED}FAILED: $ERRORS error(s), $WARNINGS warning(s)${NC}"
fi
echo "═══════════════════════════════════════════════════════════════"
echo ""

exit $ERRORS

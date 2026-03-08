# TadHub Known Issues & Solutions

This file is a knowledge base of issues encountered during development and their solutions.
When you encounter an issue, FIRST check this file. If found, follow the documented solution.
If not found, research and fix it properly, then document it here for future reference.

**RULE: No workarounds. Every issue must be fixed at the root cause.**

---

## Build & Tooling Issues

### `dotnet ef` command not found
**Symptom:** `dotnet ef migrations add` fails with "command not found"
**Root Cause:** EF tools installed globally but not on PATH
**Solution:** Use full path or export PATH:
```bash
export PATH="$HOME/.dotnet/tools:$PATH"
dotnet-ef migrations add MigrationName -p src/TadHub.Infrastructure -s src/TadHub.Api
```
**DO NOT:** Skip migration creation or use inline SQL as workaround.

### Frontend `tsc --noEmit` fails after adding new feature
**Symptom:** TypeScript compilation errors after adding new types/pages
**Root Cause:** Usually missing imports, wrong type names, or mismatched interfaces
**Solution:** Fix the actual type errors. Common causes:
- Missing export in types.ts
- Frontend DTO doesn't match backend DTO shape (camelCase vs PascalCase)
- Missing i18n namespace registration in `i18n/index.ts`
**DO NOT:** Use `// @ts-ignore`, `any` type, or skip the type check.

### `dotnet build` warns about nullable reference types
**Symptom:** CS8618 warnings for non-nullable properties
**Root Cause:** Properties declared as non-nullable without default values
**Solution:** Add `= string.Empty;` or `= default!;` for required properties, or make them nullable with `?`
**DO NOT:** Disable nullable warnings project-wide.

---

## EF Core / Database Issues

### Migration conflicts with existing snapshot
**Symptom:** `dotnet ef migrations add` fails because snapshot doesn't match database
**Root Cause:** Out-of-sync model snapshot
**Solution:**
1. Check if another migration was added but not applied
2. If snapshot is corrupt: remove last migration, rebuild, re-add
3. Never manually edit the snapshot file
**DO NOT:** Delete all migrations and start fresh.

### snake_case naming not applied to new entity
**Symptom:** EF generates PascalCase column names instead of snake_case
**Root Cause:** Missing or incorrect EF configuration, or entity not discovered
**Solution:**
1. Verify `IEntityTypeConfiguration<T>` exists in `{Module}.Core/Persistence/`
2. Verify the assembly containing the config has ".Core" in its name (auto-discovery)
3. Check that the convention is applied in `AppDbContext.OnModelCreating`
**DO NOT:** Manually rename columns in migration files.

### Enum stored as integer instead of string
**Symptom:** Database contains 0, 1, 2 instead of enum names
**Root Cause:** Missing `.HasConversion<string>()` in EF configuration
**Solution:** Add to entity configuration:
```csharp
builder.Property(x => x.Status)
    .HasConversion<string>()
    .HasMaxLength(30);
```
**DO NOT:** Store as int and convert in application code.

---

## Cross-Module Architecture Issues

### Need data from another module's entity
**Symptom:** Service needs to read data owned by another module
**Root Cause:** Cross-module data dependency
**Solution (in order of preference):**
1. **Include data in the event payload** — when the source module publishes an event, include a snapshot DTO with all fields consumers might need
2. **Raw SQL query** — `_db.Database.SqlQueryRaw<T>("SELECT ... FROM other_table WHERE ...")`
3. **BFF enrichment** — let the API controller compose data from multiple services
**DO NOT:** Add project references between modules or inject other module's services.

### Consumer needs more data than event provides
**Symptom:** MassTransit consumer receives event but needs additional fields
**Root Cause:** Event payload is incomplete
**Solution:** Expand the event DTO in `TadHub.SharedKernel/Events/` to include the missing fields. Update the publisher to populate them.
**DO NOT:** Inject the source module's service into the consumer.

---

## Frontend Issues

### Component shows blank screen while loading
**Symptom:** Page flashes white before data appears
**Root Cause:** Missing skeleton loading state
**Solution:**
- List pages: Use `DataTableAdvanced` with `isLoading` prop
- Detail pages: Create `DetailSkeleton()` component, return it when `isLoading` is true
- Import `Skeleton` from `@/shared/components/ui/skeleton`
**DO NOT:** Use a simple spinner or leave blank.

### i18n key shows raw key string instead of translation
**Symptom:** UI shows "candidates.create.fullName" instead of translated text
**Root Cause:** Missing namespace registration or key not in JSON file
**Solution:**
1. Check key exists in both `en.json` and `ar.json`
2. Check namespace is registered in `i18n/index.ts`
3. Check `useTranslation('namespace')` uses correct namespace
**DO NOT:** Hardcode strings instead of using i18n.

### `Textarea` component import fails
**Symptom:** `import { Textarea } from '@/shared/components/ui/textarea'` — module not found
**Root Cause:** No Textarea UI component exists in the project
**Solution:** Use plain HTML `<textarea>` with Tailwind classes:
```tsx
<textarea className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm" />
```
**DO NOT:** Create a new Textarea component wrapper.

---

## Docker / Deployment Issues

### Coolify build fails with "module not found"
**Symptom:** Docker build fails during `pnpm install` or `dotnet restore`
**Root Cause:** New project/package not added to Docker context or build references
**Solution:**
1. For new .NET projects: ensure they're referenced in `TadHub.Api.csproj` and the Dockerfile copies the right paths
2. For new npm packages: ensure `package.json` is updated and lockfile is committed
**DO NOT:** Add `--ignore-scripts` or skip dependency resolution.

### API container can't reach Keycloak
**Symptom:** Auth failures in production, token validation fails
**Root Cause:** Container DNS resolution for `auth.endlessmaker.com`
**Solution:** Check `extra_hosts` in docker-compose.coolify.yml maps the domain to the correct IP
**DO NOT:** Disable HTTPS metadata requirement or skip token validation.

---

## Git / Workflow Issues

### Pre-commit hook fails
**Symptom:** `git commit` fails due to hook
**Root Cause:** Code quality check failed (linting, formatting, etc.)
**Solution:** Fix the underlying issue the hook caught, then commit again
**DO NOT:** Use `--no-verify` to skip the hook.

### Context window exhausted during task
**Symptom:** Claude session ends without completing the task
**Root Cause:** Task too large for single session
**Solution:** Task runner detects this and keeps status as `in_progress`. Re-run the task — it will resume with subtask progress.
**DO NOT:** Mark the task as completed if it's not done.

---

## Adding New Issues

When you encounter and solve a new issue:
1. Add it under the appropriate section (or create a new section)
2. Follow the format: Symptom → Root Cause → Solution → DO NOT
3. Keep solutions concrete with code examples where helpful
4. The "DO NOT" line prevents future workarounds

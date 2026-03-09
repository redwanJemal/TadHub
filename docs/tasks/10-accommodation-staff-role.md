# Task 10: Accommodation Staff Role

## Summary
Add an Accommodation Staff role with permissions limited to accommodation management.

## Current State
- No accommodation staff role exists
- Accommodation module does not exist yet (see Task 07)

## Required Changes

### 10.1 Backend

- [ ] Add Accommodation Staff role seed
- [ ] Permissions:
  - `accommodation.view` — view daily list, current occupants, history
  - `accommodation.manage` — check in, check out, update room assignments
  - `arrivals.view` — view incoming arrivals (to prepare for new check-ins)
- [ ] Ensure role cannot access other modules (finance, contracts, candidates, etc.)

### 10.2 Frontend

- [ ] Accommodation-focused layout:
  - Daily occupant list (primary view)
  - Incoming arrivals (what to expect today)
  - Check-in / check-out actions
  - History / search
- [ ] Minimal navigation (no access to other modules)

## Acceptance Criteria

1. A user with Accommodation Staff role can log in and see only accommodation-related pages
2. They can view the daily maid list and current occupants
3. They can check in and check out maids
4. They can see incoming arrivals to prepare for new check-ins
5. They cannot access candidates, workers, finance, contracts, or other modules
6. All actions are audit-logged
7. Admin can assign the Accommodation Staff role to users

## Dependencies
- Task 07 (Accommodation Module) must be implemented first

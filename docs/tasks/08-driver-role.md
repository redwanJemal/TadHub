# Task 08: Driver Role

## Summary
Add a Driver user role with limited permissions to view assigned pickups, confirm pickup, and upload arrival photos.

## Current State
- No driver role exists
- No driver-specific UI or permissions
- Authorization module supports role-based access with granular permissions

## Required Changes

### 8.1 Backend

- [ ] Add Driver role seed in `PermissionSeeder.cs` or role seeding logic
- [ ] Driver permissions:
  - `arrivals.driver_actions` — view own assigned pickups, confirm pickup, upload photos
- [ ] Ensure driver can only see arrivals assigned to their user ID
- [ ] Driver-scoped API endpoints (filter by authenticated user)

### 8.2 Frontend

- [ ] Driver dashboard (minimal UI, mobile-friendly):
  - List of assigned pickups (today and upcoming)
  - Each pickup shows: maid name, flight number, airport, scheduled time
  - Confirm pickup button
  - Upload photo capability (camera integration for mobile)
- [ ] Driver does NOT have access to other system modules
- [ ] Consider a simplified layout (no full sidebar, just driver-specific nav)

### 8.3 Mobile Considerations

- [ ] Responsive design that works well on mobile browsers
- [ ] Camera capture for photo upload (use `<input type="file" accept="image/*" capture="environment">`)
- [ ] Large touch-friendly buttons for confirm/upload actions

## Acceptance Criteria

1. A user with Driver role can log in and see only their assigned pickups
2. Driver can see pickup details: maid name, flight info, airport, time
3. Driver can confirm maid pickup
4. Driver can upload a photo of the maid at the airport
5. Driver cannot access any other modules (candidates, workers, finance, etc.)
6. The driver interface is mobile-friendly with camera capture support
7. Driver actions are audit-logged
8. Admin can assign the Driver role to users

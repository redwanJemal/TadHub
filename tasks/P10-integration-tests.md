# Phase 10: End-to-End Integration Tests

Validate the complete business flows that span multiple modules, testing the event-driven architecture under realistic scenarios.

**Estimated Time:** 1 week

---

## P10-T01: Write Cross-Module Integration Tests

**Dependencies:** All previous phases

**Files:**
- `tests/TadHub.Tests.Integration/Tadbeer/TraditionalContractLifecycleTests.cs`
- `tests/TadHub.Tests.Integration/Tadbeer/FlexibleContractLifecycleTests.cs`
- `tests/TadHub.Tests.Integration/Tadbeer/WorkerAbscondingFlowTests.cs`
- `tests/TadHub.Tests.Integration/Tadbeer/RefundFlowTests.cs`
- `tests/TadHub.Tests.Integration/Tadbeer/WpsComplianceFlowTests.cs`
- `tests/TadHub.Tests.Integration/Tadbeer/MultiTenantIsolationTests.cs`
- `tests/TadHub.Tests.Integration/Tadbeer/SharedPoolTests.cs`
- `tests/TadHub.Tests.Integration/Tadbeer/PermissionEnforcementTests.cs`

**Instructions:**
1. `TraditionalContractLifecycleTests`: Register client > verify client > create worker > train > ready > create traditional contract > advance payment > PRO chain completes (medical + visa + insurance) > approve contract > worker Active > guarantee period > terminate > refund > credit note.
2. `FlexibleContractLifecycleTests`: Create flexible contract > book 4h slot > verify no double-book > book 8h slot next day > verify 12h limit > complete booking > invoice generated > pay > X-Report includes it.
3. `WorkerAbscondingFlowTests`: Worker Active > mark absconded > contract auto-terminated > refund triggered > WPS updated (excluded from SIF) > client notified > PRO visa cancellation created.
4. `RefundFlowTests`: Terminate within guarantee > refund triggered > 14-day countdown > approve > credit note > bank transfer. Also: terminate outside guarantee > no refund eligible.
5. `WpsComplianceFlowTests`: Create payroll records > generate SIF > validate format > submit > salary paid event > worker paid status updated.
6. `MultiTenantIsolationTests`: Agency A creates worker. Agency B queries workers. Zero results. Agency A and B create SharedPoolAgreement. Agency B now sees shared workers.
7. `PermissionEnforcementTests`: Cashier cannot approve contracts (403). Receptionist cannot process refunds (403). PRO officer can manage visas but not generate X-Reports.

**Tests:**
- [ ] All integration tests pass
- [ ] Complete business flows validated end-to-end

**Acceptance:** System behaves correctly across module boundaries. Events cascade properly.

**Status:** ⏳ Pending

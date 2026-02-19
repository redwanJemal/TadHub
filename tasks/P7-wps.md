# Phase 7: Wage Protection System (WPS)

SIF file generation, payroll management, WPS agent integration, and compliance tracking for workers under agency sponsorship.

**Estimated Time:** 2 weeks (parallel with P6)

---

## P7-T01: Implement WPS Module (Payroll, SIF Generation, Compliance)

**Dependencies:** P4-T01, P5-T01

**Files:**
- `src/Modules/Tadbeer/Wps/Wps.Core/Entities/PayrollRecord.cs`
- `src/Modules/Tadbeer/Wps/Wps.Core/Entities/SifFile.cs`
- `src/Modules/Tadbeer/Wps/Wps.Core/Entities/SifEmployeeRecord.cs`
- `src/Modules/Tadbeer/Wps/Wps.Core/Entities/GratuityCalculation.cs`
- `src/Modules/Tadbeer/Wps/Wps.Core/Persistence/` (4 configs)
- `src/Modules/Tadbeer/Wps/Wps.Core/Services/PayrollService.cs`
- `src/Modules/Tadbeer/Wps/Wps.Core/Services/SifGenerationService.cs`
- `src/Modules/Tadbeer/Wps/Wps.Core/Services/WpsComplianceService.cs`
- `src/Modules/Tadbeer/Wps/Wps.Core/Services/GratuityService.cs`
- `src/Modules/Tadbeer/Wps/Wps.Core/Consumers/ContractActivatedConsumer.cs`
- `src/Modules/Tadbeer/Wps/Wps.Core/Consumers/WorkerAbscondedConsumer.cs`
- `src/Modules/Tadbeer/Wps/Wps.Core/Consumers/SalaryPaidConsumer.cs`
- `src/Modules/Tadbeer/Wps/Wps.Core/Jobs/WpsDeadlineCheckJob.cs`
- `src/TadHub.Api/Controllers/Tadbeer/WpsController.cs`
- `src/Modules/Tadbeer/Wps/Wps.Core/WpsServiceRegistration.cs`

**Instructions:**
1. `PayrollRecord` : TenantScopedEntity. WorkerId FK, ContractId FK, Month (DateOnly, YYYY-MM-01), BasicSalary, HousingAllowance, TransportAllowance, OtherAllowances, Deductions, NetPay, Status (enum: Draft, Approved, Submitted, Paid, Failed), PaidAt, PaymentMethod.
2. `SifFile` : TenantScopedEntity. Month, FileContent (byte[]), RecordCount, TotalAmount, Status (enum: Generated, Validated, Submitted, Accepted, Rejected), SubmittedAt, RejectionReason.
3. `SifEmployeeRecord` : TenantScopedEntity. SifFileId FK, WorkerId FK, LaborCardNumber, BankRoutingCode, AccountNumber, SalaryAmount, StatusCode.
4. `SifGenerationService`: Generate SIF in Central Bank format. Include Employee Detail Records (EDR) and Salary Control Record (SCR). Validate: labor card format, bank routing codes, salary matches contract. Flag absconded workers with correct status code.
5. `WpsComplianceService`: Track payment deadlines (salary by 15th of month). If not submitted by 15th, publish WpsComplianceAlertEvent (new work permits suspended on 16th). Dashboard: red/amber/green status per worker.
6. `GratuityService`: Calculate end-of-service benefits based on contract duration and UAE labor law (21 days per year for first 5 years, 30 days per year after).
7. `WpsDeadlineCheckJob`: Daily Hangfire job. Check if current month's SIF is submitted. Alert at 10th, 13th, 14th of month.
8. API: GET `.../wps/payroll?filter[month]=2026-02&filter[status]=draft`, POST `.../wps/payroll/approve-batch`, POST `.../wps/sif/generate?month=2026-02`, POST `.../wps/sif/{id}/submit`, GET `.../wps/compliance` (dashboard).

**Tests:**
- [ ] Unit test: SIF file format matches Central Bank spec
- [ ] Unit test: Absconded worker excluded from SIF with correct status code
- [ ] Unit test: Compliance alert triggers on 14th if SIF not submitted
- [ ] Unit test: Gratuity calculation: 3 years = 63 days salary (21 * 3)

**Acceptance:** WPS compliance fully automated. SIF generation, deadline tracking, gratuity calculation.

**Status:** ⏳ Pending

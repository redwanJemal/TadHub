# Phase 4: Financial & Billing

Client-facing financials (separate from platform SaaS subscription). Multi-milestone payments, nationality-based pricing, VAT compliance, refund automation, X-Reports for daily cash reconciliation, and multi-payment-method support.

**Estimated Time:** 3 weeks (parallel with P5)

---

## P4-T01: Create Financial Entities (10+ Entities) and EF Config

**Dependencies:** P3-T01

**Files:**
- `src/Modules/Tadbeer/Financial/Financial.Core/Entities/Invoice.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Entities/InvoiceLineItem.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Entities/Payment.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Entities/CreditNote.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Entities/CreditNoteLineItem.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Entities/VatTransaction.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Entities/PaymentMilestone.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Entities/XReport.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Entities/XReportEntry.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Entities/RefundRequest.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Persistence/` (10 configs)

**Instructions:**
1. `Invoice` : TenantScopedEntity, IAuditable. InvoiceNumber (sequential per tenant, format: INV-{tenantPrefix}-{YYYYMM}-{seq}), Type (enum: Proforma, TaxInvoice, CreditNote), ContractId FK, ClientId FK, SubTotal (decimal), VatRate (decimal, 5%), VatAmount (decimal), TotalAmount, Currency ('AED'), Status (enum: Draft, Issued, PartiallyPaid, Paid, Overdue, Cancelled, Refunded), DueDate, PaidAt, TenantTrnNumber (Tax Registration Number), Notes.
2. `InvoiceLineItem` : TenantScopedEntity. InvoiceId FK, Description (LocalizedString), Quantity (int), UnitPrice (decimal), LineTotal, VatApplicable (bool). Items like: 'Recruitment Fee - Philippines Maid (Traditional)', 'VAT 5%', 'Advance Deposit (VAT-exempt)'.
3. `Payment` : TenantScopedEntity, IAuditable. InvoiceId FK, Amount, Method (enum: Cash, Card, BankTransfer, Cheque, EDirham), ReferenceNumber, ProcessedByUserId, ProcessedAt, Status (enum: Pending, Completed, Failed, Reversed). For cash: generate receipt. For card: POS reference. For cheque: tracking number + clearance status.
4. `CreditNote` : TenantScopedEntity. OriginalInvoiceId FK, CreditNoteNumber, Amount, Reason, IssuedAt.
5. `VatTransaction` : TenantScopedEntity. InvoiceId FK, Type (Output/Input), Amount, Period (YYYY-MM), IsReported (bool). For tax return preparation.
6. `PaymentMilestone` : TenantScopedEntity. ContractId FK, MilestoneType (enum: AdvanceDeposit, FullPayment, Installment1/2/3), Amount, DueDate, Status (Pending/Paid/Overdue), PaidAt, InvoiceId FK. Traditional contracts: advance on booking, full on activation. Installments for high-value packages.
7. `XReport` : TenantScopedEntity. ReportDate (DateOnly), GeneratedByUserId, GeneratedAt, TotalCash, TotalCard, TotalTransfer, TotalEDirham, GrandTotal, Status (Open/Closed). One per day per tenant.
8. `XReportEntry` : TenantScopedEntity. XReportId FK, PaymentId FK, InvoiceId FK, Amount, Method, Description, Timestamp.
9. `RefundRequest` : TenantScopedEntity. ContractId FK, ClientId FK, ReasonCode (enum), RequestedAmount, ApprovedAmount, Status (Requested/Approved/Processed/Rejected), DeadlineAt (14 days from request), ProcessedAt, BankDetails (JSONB).
10. Migration: `dotnet ef migrations add InitFinancial`

**Tests:**
- [ ] Integration test: Create invoice with line items, verify VAT calculation
- [ ] Integration test: Sequential invoice numbering per tenant

**Acceptance:** Financial data model supports all billing scenarios.

**Status:** ⏳ Pending

---

## P4-T02: Implement Financial Services (Invoicing, Payments, VAT, Refunds, X-Reports)

**Dependencies:** P4-T01

**Files:**
- `src/Modules/Tadbeer/Financial/Financial.Core/Services/InvoiceService.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Services/PaymentService.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Services/VatService.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Services/RefundService.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Services/XReportService.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Services/PaymentMilestoneService.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Consumers/ContractCreatedConsumer.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Consumers/ContractActivatedConsumer.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Consumers/RefundTriggeredConsumer.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/Consumers/BookingCreatedConsumer.cs`

**Instructions:**
1. `InvoiceService`: Generate proforma on contract creation (advance milestone). Generate tax invoice on contract activation (full milestone). Apply VAT correctly: advance deposits may be VAT-exempt if refundable; final payments are VAT-inclusive at 5%. Sequential numbering.
2. `PaymentService`: ProcessPaymentAsync(Guid invoiceId, PaymentMethod, amount, reference). On success, publish PaymentReceivedEvent. Check if all milestones are paid; if so, mark contract as payment-complete. X-Report entries auto-created on every payment.
3. `VatService`: Track output VAT per invoice. Monthly aggregation for tax returns. Export in UAE FTA format.
4. `RefundService`: Consume RefundTriggeredEvent. Create RefundRequest with 14-day deadline. On approval, generate CreditNote, initiate bank transfer workflow, publish RefundProcessedEvent. Hangfire job checks overdue refunds daily.
5. `XReportService`: GenerateAsync(DateOnly date). Aggregate all payments for the day by method. Close report (no more entries). Cashier-facing endpoint.
6. `PaymentMilestoneService`: Create milestones on contract creation. Track payment against milestones. Overdue milestones publish PaymentOverdueEvent.
7. Consumers: ContractCreatedConsumer generates advance invoice + milestones. BookingCreatedConsumer (flexible) generates per-booking invoice.
8. `ListAsync` for invoices: `filter[status]=overdue&filter[status]=issued`, `filter[clientId]=...`, `filter[contractId]=...`, `filter[issuedAt][gte]=...`, `sort=-issuedAt`.

**Tests:**
- [ ] Unit test: VAT calculation: SubTotal 12000 + 5% = TotalAmount 12600
- [ ] Unit test: Advance deposit marked as VAT-exempt
- [ ] Unit test: Refund deadline auto-set to 14 days
- [ ] Unit test: X-Report aggregates correctly by payment method
- [ ] Unit test: Flexible booking generates per-unit invoice at correct rate

**Acceptance:** Full financial lifecycle: invoicing, payments, VAT, refunds, X-Reports.

**Status:** ⏳ Pending

---

## P4-T03: Create Financial API Controllers and ServiceRegistration

**Dependencies:** P4-T02

**Files:**
- `src/TadHub.Api/Controllers/Tadbeer/InvoicesController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/PaymentsController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/RefundsController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/XReportsController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/PaymentMilestonesController.cs`
- `src/Modules/Tadbeer/Financial/Financial.Core/FinancialServiceRegistration.cs`

**Instructions:**
1. `InvoicesController`: GET `.../invoices` (with filters, including array status filter), GET `.../invoices/{id}`, POST `.../invoices/{id}/issue`.
2. `PaymentsController`: POST `.../payments` (process payment against invoice), GET `.../payments?filter[method]=cash&filter[method]=card&filter[processedAt][gte]=2026-01-01`.
3. `RefundsController`: GET `.../refunds?filter[status]=requested&filter[status]=approved`, POST `.../refunds/{id}/approve`, POST `.../refunds/{id}/process`.
4. `XReportsController`: POST `.../x-reports/generate` (today's report), GET `.../x-reports?filter[reportDate][gte]=2026-01-01`, GET `.../x-reports/{id}` (with entries).
5. Permissions: `financial.invoices.generate`, `financial.payments.process` (cashier), `financial.refunds.process` (admin), `financial.xreport.generate` (cashier).

**Tests:**
- [ ] Integration test: Full billing flow: contract created > advance invoice > pay > activate > full invoice > pay > X-Report shows both
- [ ] Integration test: Refund flow: terminate contract > refund triggered > approve > process > credit note generated

**Acceptance:** Financial module complete.

**Status:** ⏳ Pending

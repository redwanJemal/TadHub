# Task 11: Finance Enhancements

## Summary
Extend the financial module with refund calculation engine, amortization logic, guarantee-period cost recovery, and automatic supplier commission calculation on deployment.

## Current State
- Full financial module exists: invoices, payments, supplier payments, discount programs, cash reconciliation
- `SupplierPayment` entity tracks payments to suppliers
- `InvoiceType` has CreditNote for refunds
- No amortization/refund calculation logic
- No auto-commission calculation on deployment
- No guarantee-period cost recovery tracking

## Required Changes

### 11.1 Refund Calculation Engine

- [ ] Create `RefundCalculationService`:
  ```
  ValuePerMonth = TotalAmountPaid / 24
  RefundAmount = TotalAmountPaid - (MonthsWorked * ValuePerMonth)
  ```
- [ ] Support configurable contract duration (not hardcoded to 24 months)
- [ ] Support partial month calculation (configurable: pro-rata or round down)
- [ ] `CalculateRefundAsync(contractId, returnDate)` returns:
  - TotalPaid, ContractMonths, MonthsWorked, ValuePerMonth, RefundAmount
- [ ] Auto-generate CreditNote invoice on returnee approval
- [ ] Link refund to `ReturneeCase` (Task 03)

### 11.2 Supplier Commission Auto-Calculation

- [ ] When a maid is deployed (placement → Placed), auto-calculate supplier commission
- [ ] Commission rules (configurable per tenant in financial settings):
  - Fixed amount per maid, OR
  - Percentage of contract value, OR
  - Custom amount per supplier agreement
- [ ] Auto-create `SupplierPayment` record on deployment
- [ ] Add commission configuration to Financial Settings page

### 11.3 Guarantee Period Cost Recovery

- [ ] When a returnee or runaway case is within guarantee period:
  - Auto-generate supplier debit records for:
    - Commission refund
    - Ticket cost
    - Visa cost (optional)
    - Transportation cost (optional)
    - Medical cost (optional)
    - Other costs
  - Link debits to the case (ReturneeCase or RunawayCase)
  - Track payment status per debit
- [ ] Supplier payment module should show outstanding debits

### 11.4 Maid Cost Tracking

- [ ] Extend cost tracking to cover per-maid costs:
  - Maid procurement cost
  - Monthly accommodation cost
  - Transportation cost
  - Medical cost
  - Visa cost
  - Other operational costs
- [ ] `PlacementCostItem` already exists — ensure it covers all cost types
- [ ] Add cost summary on worker detail and placement detail pages

### 11.5 Financial Settings Expansion

- [ ] Add commission calculation settings (fixed/percentage/custom)
- [ ] Add refund calculation settings (pro-rata vs round-down for partial months)
- [ ] Add guarantee period default settings
- [ ] Add accommodation daily rate setting
- [ ] Frontend: expand Financial Settings page with new configuration sections

### 11.6 API Updates

- [ ] `GET /api/finance/refund-calculation?contractId=&returnDate=` — preview refund
- [ ] `POST /api/finance/commission/calculate?placementId=` — trigger commission calc
- [ ] `GET /api/finance/supplier-debits?supplierId=` — supplier outstanding debits
- [ ] Permissions: existing financial permissions cover these

### 11.7 Frontend Updates

- [ ] Refund calculation preview on returnee case detail
- [ ] Commission summary on supplier detail page
- [ ] Cost breakdown on placement and worker detail pages
- [ ] Enhanced Financial Settings page with new config sections
- [ ] Supplier debit tracking view

## Acceptance Criteria

1. When a maid is deployed, supplier commission is auto-calculated based on tenant settings
2. A `SupplierPayment` record is auto-created on deployment
3. Commission can be configured as fixed amount, percentage, or custom per supplier
4. Refund calculation correctly applies the amortization formula
5. Refund preview is available before finalizing returnee cases
6. CreditNote invoices are auto-generated for customer refunds
7. When returnee/runaway is within guarantee, supplier debits are auto-generated
8. All per-maid costs are tracked: procurement, accommodation, medical, visa, transport
9. Financial settings page allows configuring commission rules, refund rules, and default rates
10. Supplier payment module shows outstanding debits alongside regular payments

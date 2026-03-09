# Task 17: Contract Enhancements

## Summary
Enhance the Contract module to support guarantee periods, better alignment with spec requirements, and integration with returnee/runaway processes.

## Current State
- Contract entity has: ContractCode, Type, Status, WorkerId, ClientId, StartDate, EndDate, ProbationEndDate, GuaranteeEndDate, Rate, RatePeriod, Currency, TotalValue
- `ContractType`: Traditional, Temporary, Flexible
- `ContractStatus`: Draft, Confirmed, OnProbation, Active, Completed, Terminated, Cancelled, Closed
- `GuaranteeEndDate` exists but no guarantee period enum or logic

## Required Changes

### 17.1 Guarantee Period

- [ ] Add `GuaranteePeriod` enum: SixMonths, OneYear, TwoYears
- [ ] Add `GuaranteePeriod` field to Contract entity
- [ ] Auto-calculate `GuaranteeEndDate` from `StartDate` + guarantee period
- [ ] Default guarantee period configurable in tenant settings
- [ ] Guarantee period visible on contract detail page

### 17.2 Contract Type Alignment

- [ ] Add or map to spec contract types:
  - **Trial Contract**: 5-day trial period contract (linked to Trial entity)
  - **Two-Year Employment Contract**: standard 2-year maid contract
- [ ] Current `Traditional` type can map to "Two-Year Employment Contract"
- [ ] Add `TrialContract` type or handle trials separately

### 17.3 Contract Auto-Creation

- [ ] When a trial succeeds (inside-country), auto-create a 2-year contract
- [ ] When an outside-country placement is booked and payment received, prompt contract creation
- [ ] Contract should auto-populate worker and client from placement
- [ ] Contract duration default: 2 years (configurable)

### 17.4 Termination Enhancements

- [ ] Add termination reasons aligned with spec:
  - `ReturnToOffice` — maid returned, going back to inventory
  - `ReturnToCountry` — maid returned to home country
  - `Runaway` — maid absconded
  - `MutualAgreement`
  - `ContractExpiry`
  - `ClientRequest`
  - `WorkerRequest`
- [ ] Add `TerminationReason` enum (or expand existing)
- [ ] Link contract termination to ReturneeCase or RunawayCase
- [ ] On termination, update worker status appropriately

### 17.5 Replacement Contract Support

- [ ] `ReplacementContractId` and `OriginalContractId` already exist
- [ ] Ensure replacement flow works: when a returnee is replaced, create new contract linked to original
- [ ] UI for creating replacement contracts from terminated contracts

### 17.6 Frontend Updates

- [ ] Guarantee period selector on contract creation form
- [ ] Guarantee status indicator on contract detail (active/expired)
- [ ] Termination reason selection when terminating a contract
- [ ] Link to returnee/runaway case from terminated contract
- [ ] Replacement contract creation action

## Acceptance Criteria

1. Contracts have a configurable guarantee period (6 months, 1 year, 2 years)
2. Guarantee end date is auto-calculated from start date + guarantee period
3. Default guarantee period is configurable in tenant settings
4. Trial contracts can be generated for 5-day trial periods
5. 2-year employment contracts are auto-created on trial success or outside-country booking
6. Contract termination records the specific reason
7. Terminated contracts link to their ReturneeCase or RunawayCase
8. Replacement contracts are linked to original contracts
9. Guarantee status (active/expired) is visible on contract detail
10. All contract changes are audit-logged

## Dependencies
- Task 02 (Trial Period) for trial contracts
- Task 03 (Returnee Process) for termination integration
- Task 04 (Runaway Process) for termination integration

# Task 14: Notifications Expansion

## Summary
Expand the notification system to cover all business events specified in the Tadbeer spec.

## Current State
- `Notification` module exists
- Basic notification infrastructure in place
- Not all business events trigger notifications

## Required Notification Triggers

### 14.1 Maid Lifecycle Notifications

- [ ] **Maid Approval**: notify supplier when their maid is approved or rejected
- [ ] **Maid Status Change**: notify relevant parties on any significant status change

### 14.2 Booking & Deployment

- [ ] **Booking Confirmation**: notify customer (if system supports customer notifications) and office staff
- [ ] **Deployment Confirmation**: notify admin, supplier when maid is deployed

### 14.3 Arrival Notifications

- [ ] **Arrival Scheduled**: notify driver of new pickup assignment
- [ ] **Arrival Confirmed**: notify admin, accommodation staff
- [ ] **Missing Arrival / No-Show**: notify supplier AND admin when maid doesn't arrive
- [ ] **Pickup Confirmed**: notify admin when driver confirms pickup

### 14.4 Visa & Processing

- [ ] **Visa Status Updates**: notify office staff on visa approval, rejection, issuance
- [ ] **Emirates ID Issued**: notify office staff

### 14.5 Trial Period

- [ ] **Trial Started**: notify admin
- [ ] **Trial Completion Due**: remind office staff before trial ends (e.g., day 4)
- [ ] **Trial Outcome**: notify admin on success or failure

### 14.6 Returnee & Runaway

- [ ] **Returnee Case Filed**: notify admin
- [ ] **Returnee Approved**: notify office staff, supplier (if within guarantee)
- [ ] **Runaway Reported**: notify admin, supplier (if within guarantee)
- [ ] **Cost Recovery Required**: notify supplier of liability

### 14.7 Financial

- [ ] **Payment Received**: notify admin, cashier
- [ ] **Refund Processed**: notify admin
- [ ] **Supplier Commission Due**: notify finance team
- [ ] **Overdue Payment**: notify finance team

### 14.8 Implementation

- [ ] Each notification should specify:
  - Trigger event
  - Recipients (role-based and/or specific user)
  - Message template (i18n)
  - Priority (normal, urgent)
  - Channel (in-app, email — future: SMS)
- [ ] Use MassTransit event consumers to trigger notifications from domain events
- [ ] Notification templates should be configurable per tenant

### 14.9 Frontend

- [ ] Ensure notification panel displays all new notification types
- [ ] Notification preferences: allow users to configure which notifications they receive
- [ ] Urgent notifications should be visually distinct

## Acceptance Criteria

1. Maid approval/rejection triggers notification to supplier
2. Booking confirmation is notified to relevant parties
3. Arrival alerts are sent to drivers, admin, and accommodation staff
4. Missing arrival triggers urgent notification to supplier and admin
5. Visa status changes trigger notifications
6. Trial completion reminders are sent before trial ends
7. Returnee and runaway cases trigger notifications to admin and supplier
8. Cost recovery demands notify the supplier
9. Financial events (payment, refund, overdue) trigger appropriate notifications
10. All notifications are viewable in the notification panel
11. Notification templates support English and Arabic
12. Users can configure notification preferences

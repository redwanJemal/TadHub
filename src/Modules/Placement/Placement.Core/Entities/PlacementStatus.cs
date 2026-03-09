namespace Placement.Core.Entities;

public enum PlacementStatus
{
    // Step 1: Booking
    Booked = 0,

    // Step 2: Contract Creation
    ContractCreated = 13,

    // Step 3: Employment Visa
    EmploymentVisaProcessing = 14,

    // Step 4: Ticket Processing
    TicketArranged = 1,

    // Legacy (kept for backward compat)
    InTransit = 2,

    // Step 5: Arrival
    Arrived = 3,

    // Legacy statuses (kept for backward compat)
    MedicalInProgress = 4,
    MedicalCleared = 5,
    GovtProcessing = 6,
    GovtCleared = 7,
    Training = 8,
    ReadyForPlacement = 9,

    // Step 6: Deployment (replaces Placed for outside-country flow)
    Deployed = 15,
    Placed = 10, // Legacy alias

    // Step 7: Full Payment
    FullPaymentReceived = 16,

    // Step 8: Residence Visa
    ResidenceVisaProcessing = 17,

    // Step 9: Emirates ID
    EmiratesIdProcessing = 18,

    // Terminal
    Completed = 11,
    Cancelled = 12,
}

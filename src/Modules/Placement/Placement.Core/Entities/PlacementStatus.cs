namespace Placement.Core.Entities;

public enum PlacementStatus
{
    // Step 1: Booking
    Booked = 0,

    // Step 2: Contract Creation (outside-country) / Step 4 (inside-country)
    ContractCreated = 13,

    // Step 3: Employment Visa
    EmploymentVisaProcessing = 14,

    // Step 4: Ticket Processing (outside-country only)
    TicketArranged = 1,

    // Legacy (kept for backward compat)
    InTransit = 2,

    // Step 5: Arrival (outside-country only)
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

    // Inside-country flow statuses
    InTrial = 19,
    TrialSuccessful = 20,
    StatusChanged = 21,
}

public enum PlacementFlowType
{
    OutsideCountry = 0,
    InsideCountry = 1,
}

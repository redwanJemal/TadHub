namespace Worker.Core.Entities;

public enum WorkerStatus
{
    // Pool
    Available = 0,
    InTraining = 1,
    UnderMedicalTest = 2,

    // Arrival
    NewArrival = 3,

    // Placement
    Booked = 4,
    Hired = 5,
    OnProbation = 6,
    Active = 7,
    Renewed = 8,

    // Negative / Special
    PendingReplacement = 9,
    Transferred = 10,
    MedicallyUnfit = 11,
    Absconded = 12,
    Terminated = 13,
    Pregnant = 14,

    // Terminal
    Repatriated = 15,
    Deported = 16,
    Deceased = 17,
}

public enum WorkerLocation
{
    Abroad = 0,
    InCountry = 1,
}

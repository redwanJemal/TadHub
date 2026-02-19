namespace Worker.Core.Entities;

/// <summary>
/// Worker lifecycle status - 20-state finite state machine.
/// </summary>
public enum WorkerStatus
{
    /// <summary>Recently arrived from origin country</summary>
    NewArrival = 1,
    /// <summary>Undergoing orientation/training</summary>
    InTraining = 2,
    /// <summary>Available for client selection</summary>
    ReadyForMarket = 3,
    /// <summary>Reserved by a client, pending contract</summary>
    Booked = 4,
    /// <summary>Contract signed, awaiting visa/deployment</summary>
    Hired = 5,
    /// <summary>Deployed to client on probation period</summary>
    OnProbation = 6,
    /// <summary>Full-time active with employer</summary>
    Active = 7,
    /// <summary>Contract renewed</summary>
    Renewed = 8,
    /// <summary>Transferred to another employer</summary>
    Transferred = 9,
    /// <summary>Undergoing medical examination</summary>
    UnderMedicalTest = 10,
    /// <summary>Waiting for visa processing</summary>
    AwaitingVisa = 11,
    /// <summary>Probation period under review</summary>
    InProbationReview = 12,
    /// <summary>Client requested replacement</summary>
    PendingReplacement = 13,
    /// <summary>Worker absconded from employer</summary>
    Absconded = 14,
    /// <summary>Deported from UAE</summary>
    Deported = 15,
    /// <summary>Worker is pregnant (special handling)</summary>
    Pregnant = 16,
    /// <summary>Failed medical, cannot work</summary>
    MedicallyUnfit = 17,
    /// <summary>Contract terminated</summary>
    Terminated = 18,
    /// <summary>Sent back to origin country</summary>
    Repatriated = 19,
    /// <summary>Worker deceased</summary>
    Deceased = 20
}

/// <summary>
/// Passport custody location tracking.
/// </summary>
public enum PassportLocation
{
    WithAgency = 1,
    WithSponsor = 2,
    WithImmigration = 3,
    Surrendered = 4,
    WithWorker = 5
}

/// <summary>
/// Language proficiency levels.
/// </summary>
public enum LanguageProficiency
{
    Poor = 1,
    Fair = 2,
    Fluent = 3
}

/// <summary>
/// Worker media types.
/// </summary>
public enum MediaType
{
    Photo = 1,
    Video = 2,
    Document = 3
}

/// <summary>
/// Contract types for pricing.
/// </summary>
public enum ContractType
{
    Traditional = 1,
    Temporary = 2,
    Flexible = 3
}

/// <summary>
/// Education levels.
/// </summary>
public enum EducationLevel
{
    NoFormalEducation = 1,
    Elementary = 2,
    HighSchool = 3,
    Vocational = 4,
    College = 5,
    University = 6
}

/// <summary>
/// Marital status.
/// </summary>
public enum MaritalStatus
{
    Single = 1,
    Married = 2,
    Divorced = 3,
    Widowed = 4
}

/// <summary>
/// Gender.
/// </summary>
public enum Gender
{
    Female = 1,
    Male = 2
}

/// <summary>
/// Religion.
/// </summary>
public enum Religion
{
    Muslim = 1,
    Christian = 2,
    Hindu = 3,
    Buddhist = 4,
    Other = 99
}

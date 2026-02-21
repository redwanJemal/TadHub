using ReferenceData.Core.Entities;
using TadHub.SharedKernel.Entities;

namespace Worker.Core.Entities;

/// <summary>
/// Worker (domestic worker) entity.
/// Core of the CV/inventory management system.
/// </summary>
public class Worker : TenantScopedEntity, IAuditable
{
    #region Identity

    /// <summary>
    /// Passport number (unique within tenant).
    /// </summary>
    public string PassportNumber { get; set; } = string.Empty;

    /// <summary>
    /// UAE Emirates ID (assigned after visa).
    /// </summary>
    public string? EmiratesId { get; set; }

    /// <summary>
    /// Agency-assigned CV serial number.
    /// </summary>
    public string CvSerial { get; set; } = string.Empty;

    /// <summary>
    /// Full name in English.
    /// </summary>
    public string FullNameEn { get; set; } = string.Empty;

    /// <summary>
    /// Full name in Arabic.
    /// </summary>
    public string FullNameAr { get; set; } = string.Empty;

    #endregion

    #region Personal Details

    /// <summary>
    /// Nationality (e.g., "Philippines", "Indonesia").
    /// </summary>
    public string Nationality { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateOnly DateOfBirth { get; set; }

    /// <summary>
    /// Gender.
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Religion.
    /// </summary>
    public Religion Religion { get; set; }

    /// <summary>
    /// Marital status.
    /// </summary>
    public MaritalStatus MaritalStatus { get; set; }

    /// <summary>
    /// Number of children.
    /// </summary>
    public int? NumberOfChildren { get; set; }

    /// <summary>
    /// Highest education level.
    /// </summary>
    public EducationLevel Education { get; set; }

    /// <summary>
    /// Years of experience.
    /// </summary>
    public int? YearsOfExperience { get; set; }

    #endregion

    #region Status & Location

    /// <summary>
    /// Current lifecycle status (20-state FSM).
    /// </summary>
    public WorkerStatus CurrentStatus { get; set; } = WorkerStatus.NewArrival;

    /// <summary>
    /// Where the passport is currently held.
    /// </summary>
    public PassportLocation PassportLocation { get; set; } = PassportLocation.WithAgency;

    /// <summary>
    /// Whether worker is available for flexible (hourly) bookings.
    /// </summary>
    public bool IsAvailableForFlexible { get; set; }

    #endregion

    #region Job & Pricing

    /// <summary>
    /// Job category ID.
    /// </summary>
    public Guid JobCategoryId { get; set; }

    /// <summary>
    /// Job category navigation.
    /// </summary>
    public JobCategory? JobCategory { get; set; }

    /// <summary>
    /// Monthly base salary in AED.
    /// </summary>
    public decimal MonthlyBaseSalary { get; set; }

    #endregion

    #region Media

    /// <summary>
    /// Primary photo URL.
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Video introduction URL.
    /// </summary>
    public string? VideoUrl { get; set; }

    #endregion

    #region Notes

    /// <summary>
    /// Additional notes about the worker.
    /// </summary>
    public string? Notes { get; set; }

    #endregion

    #region IAuditable

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Worker skills.
    /// </summary>
    public ICollection<WorkerSkill> Skills { get; set; } = new List<WorkerSkill>();

    /// <summary>
    /// Languages spoken.
    /// </summary>
    public ICollection<WorkerLanguage> Languages { get; set; } = new List<WorkerLanguage>();

    /// <summary>
    /// Media files (photos, videos).
    /// </summary>
    public ICollection<WorkerMedia> Media { get; set; } = new List<WorkerMedia>();

    /// <summary>
    /// Passport custody history (append-only).
    /// </summary>
    public ICollection<WorkerPassportCustody> PassportCustodyHistory { get; set; } = new List<WorkerPassportCustody>();

    /// <summary>
    /// State transition history (append-only).
    /// </summary>
    public ICollection<WorkerStateHistory> StateHistory { get; set; } = new List<WorkerStateHistory>();

    #endregion

    /// <summary>
    /// Calculated age.
    /// </summary>
    public int Age => CalculateAge();

    private int CalculateAge()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - DateOfBirth.Year;
        if (DateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
}

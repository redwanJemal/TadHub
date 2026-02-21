using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReferenceData.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Localization;

namespace ReferenceData.Core.Seeds;

/// <summary>
/// Seeds the 19 official MoHRE job categories.
/// Run on application startup.
/// </summary>
public class JobCategorySeeder
{
    private readonly AppDbContext _db;
    private readonly ILogger<JobCategorySeeder> _logger;

    public JobCategorySeeder(AppDbContext db, ILogger<JobCategorySeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Set<JobCategory>().AnyAsync(ct))
        {
            _logger.LogDebug("Job categories already seeded, skipping");
            return;
        }

        _logger.LogInformation("Seeding 19 MoHRE job categories...");

        var categories = GetJobCategories();

        _db.Set<JobCategory>().AddRange(categories);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Seeded {Count} job categories", categories.Count);
    }

    /// <summary>
    /// 19 official MoHRE job categories for domestic workers.
    /// Source: UAE Ministry of Human Resources and Emiratisation.
    /// </summary>
    private static List<JobCategory> GetJobCategories() =>
    [
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            MoHRECode = "DMW",
            Name = new LocalizedString("Domestic Worker (General)", "عامل منزلي (عام)"),
            DisplayOrder = 1
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
            MoHRECode = "HSK",
            Name = new LocalizedString("Housekeeper", "مدبرة منزل"),
            DisplayOrder = 2
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
            MoHRECode = "COK",
            Name = new LocalizedString("Cook", "طباخ"),
            DisplayOrder = 3
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
            MoHRECode = "NAN",
            Name = new LocalizedString("Nanny / Babysitter", "مربية أطفال"),
            DisplayOrder = 4
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
            MoHRECode = "NRS",
            Name = new LocalizedString("Nurse (Home Care)", "ممرض منزلي"),
            DisplayOrder = 5
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000006"),
            MoHRECode = "ELC",
            Name = new LocalizedString("Elder Caregiver", "مرافق مسنين"),
            DisplayOrder = 6
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000007"),
            MoHRECode = "DRV",
            Name = new LocalizedString("Driver", "سائق"),
            DisplayOrder = 7
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000008"),
            MoHRECode = "GRD",
            Name = new LocalizedString("Gardener", "بستاني"),
            DisplayOrder = 8
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000009"),
            MoHRECode = "GUA",
            Name = new LocalizedString("Guard / Security", "حارس أمن"),
            DisplayOrder = 9
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000010"),
            MoHRECode = "LAU",
            Name = new LocalizedString("Laundry Worker", "عامل غسيل"),
            DisplayOrder = 10
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000011"),
            MoHRECode = "CLN",
            Name = new LocalizedString("Cleaner", "عامل نظافة"),
            DisplayOrder = 11
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000012"),
            MoHRECode = "TWN",
            Name = new LocalizedString("Tutor", "مدرس خصوصي"),
            DisplayOrder = 12
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000013"),
            MoHRECode = "PSC",
            Name = new LocalizedString("Personal Secretary", "سكرتير شخصي"),
            DisplayOrder = 13
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000014"),
            MoHRECode = "SWP",
            Name = new LocalizedString("Swimming Pool Attendant", "عامل مسبح"),
            DisplayOrder = 14
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000015"),
            MoHRECode = "SPA",
            Name = new LocalizedString("Spa / Massage Therapist", "معالج سبا"),
            DisplayOrder = 15
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000016"),
            MoHRECode = "PTK",
            Name = new LocalizedString("Pet Keeper", "مربي حيوانات أليفة"),
            DisplayOrder = 16
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000017"),
            MoHRECode = "HBS",
            Name = new LocalizedString("Horse / Stable Boy", "سائس خيل"),
            DisplayOrder = 17
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000018"),
            MoHRECode = "BTL",
            Name = new LocalizedString("Butler", "كبير الخدم"),
            DisplayOrder = 18
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000019"),
            MoHRECode = "OTH",
            Name = new LocalizedString("Other Domestic Service", "خدمة منزلية أخرى"),
            DisplayOrder = 99
        }
    ];
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReferenceData.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Localization;

namespace ReferenceData.Core.Seeds;

/// <summary>
/// Seeds countries with ISO 3166-1 codes, Arabic names, and nationality adjectives.
/// Prioritizes common Tadbeer source nationalities.
/// </summary>
public class CountrySeeder
{
    private readonly AppDbContext _db;
    private readonly ILogger<CountrySeeder> _logger;

    public CountrySeeder(AppDbContext db, ILogger<CountrySeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Set<Country>().AnyAsync(ct))
        {
            _logger.LogDebug("Countries already seeded, skipping");
            return;
        }

        _logger.LogInformation("Seeding countries...");

        var countries = GetCountries();

        _db.Set<Country>().AddRange(countries);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Seeded {Count} countries", countries.Count);
    }

    /// <summary>
    /// Returns countries with common Tadbeer nationalities prioritized.
    /// </summary>
    private static List<Country> GetCountries()
    {
        var countries = new List<Country>();
        var idCounter = 1;

        // Common Tadbeer source nationalities (DisplayOrder 1-10, IsCommonNationality = true)
        countries.AddRange(GetCommonNationalities(ref idCounter));

        // UAE and GCC countries (DisplayOrder 11-20)
        countries.AddRange(GetGccCountries(ref idCounter));

        // Other major countries (DisplayOrder 100+)
        countries.AddRange(GetOtherCountries(ref idCounter));

        return countries;
    }

    /// <summary>
    /// Common Tadbeer source nationalities - prioritized in worker registration.
    /// </summary>
    private static List<Country> GetCommonNationalities(ref int idCounter) =>
    [
        CreateCountry(ref idCounter, "PH", "PHL", "Philippines", "الفلبين", "Filipino", "فلبيني", "+63", 1, true),
        CreateCountry(ref idCounter, "ID", "IDN", "Indonesia", "إندونيسيا", "Indonesian", "إندونيسي", "+62", 2, true),
        CreateCountry(ref idCounter, "ET", "ETH", "Ethiopia", "إثيوبيا", "Ethiopian", "إثيوبي", "+251", 3, true),
        CreateCountry(ref idCounter, "IN", "IND", "India", "الهند", "Indian", "هندي", "+91", 4, true),
        CreateCountry(ref idCounter, "LK", "LKA", "Sri Lanka", "سريلانكا", "Sri Lankan", "سريلانكي", "+94", 5, true),
        CreateCountry(ref idCounter, "NP", "NPL", "Nepal", "نيبال", "Nepali", "نيبالي", "+977", 6, true),
        CreateCountry(ref idCounter, "BD", "BGD", "Bangladesh", "بنغلاديش", "Bangladeshi", "بنغلاديشي", "+880", 7, true),
        CreateCountry(ref idCounter, "UG", "UGA", "Uganda", "أوغندا", "Ugandan", "أوغندي", "+256", 8, true),
        CreateCountry(ref idCounter, "KE", "KEN", "Kenya", "كينيا", "Kenyan", "كيني", "+254", 9, true),
        CreateCountry(ref idCounter, "GH", "GHA", "Ghana", "غانا", "Ghanaian", "غاني", "+233", 10, true)
    ];

    /// <summary>
    /// UAE and GCC countries.
    /// </summary>
    private static List<Country> GetGccCountries(ref int idCounter) =>
    [
        CreateCountry(ref idCounter, "AE", "ARE", "United Arab Emirates", "الإمارات العربية المتحدة", "Emirati", "إماراتي", "+971", 11, false),
        CreateCountry(ref idCounter, "SA", "SAU", "Saudi Arabia", "المملكة العربية السعودية", "Saudi", "سعودي", "+966", 12, false),
        CreateCountry(ref idCounter, "KW", "KWT", "Kuwait", "الكويت", "Kuwaiti", "كويتي", "+965", 13, false),
        CreateCountry(ref idCounter, "QA", "QAT", "Qatar", "قطر", "Qatari", "قطري", "+974", 14, false),
        CreateCountry(ref idCounter, "BH", "BHR", "Bahrain", "البحرين", "Bahraini", "بحريني", "+973", 15, false),
        CreateCountry(ref idCounter, "OM", "OMN", "Oman", "عُمان", "Omani", "عماني", "+968", 16, false)
    ];

    /// <summary>
    /// Other countries alphabetically.
    /// </summary>
    private static List<Country> GetOtherCountries(ref int idCounter) =>
    [
        CreateCountry(ref idCounter, "AF", "AFG", "Afghanistan", "أفغانستان", "Afghan", "أفغاني", "+93", 100, false),
        CreateCountry(ref idCounter, "AL", "ALB", "Albania", "ألبانيا", "Albanian", "ألباني", "+355", 100, false),
        CreateCountry(ref idCounter, "DZ", "DZA", "Algeria", "الجزائر", "Algerian", "جزائري", "+213", 100, false),
        CreateCountry(ref idCounter, "AR", "ARG", "Argentina", "الأرجنتين", "Argentinian", "أرجنتيني", "+54", 100, false),
        CreateCountry(ref idCounter, "AU", "AUS", "Australia", "أستراليا", "Australian", "أسترالي", "+61", 100, false),
        CreateCountry(ref idCounter, "AT", "AUT", "Austria", "النمسا", "Austrian", "نمساوي", "+43", 100, false),
        CreateCountry(ref idCounter, "AZ", "AZE", "Azerbaijan", "أذربيجان", "Azerbaijani", "أذربيجاني", "+994", 100, false),
        CreateCountry(ref idCounter, "BR", "BRA", "Brazil", "البرازيل", "Brazilian", "برازيلي", "+55", 100, false),
        CreateCountry(ref idCounter, "CA", "CAN", "Canada", "كندا", "Canadian", "كندي", "+1", 100, false),
        CreateCountry(ref idCounter, "CN", "CHN", "China", "الصين", "Chinese", "صيني", "+86", 100, false),
        CreateCountry(ref idCounter, "CO", "COL", "Colombia", "كولومبيا", "Colombian", "كولومبي", "+57", 100, false),
        CreateCountry(ref idCounter, "EG", "EGY", "Egypt", "مصر", "Egyptian", "مصري", "+20", 100, false),
        CreateCountry(ref idCounter, "FR", "FRA", "France", "فرنسا", "French", "فرنسي", "+33", 100, false),
        CreateCountry(ref idCounter, "DE", "DEU", "Germany", "ألمانيا", "German", "ألماني", "+49", 100, false),
        CreateCountry(ref idCounter, "GB", "GBR", "United Kingdom", "المملكة المتحدة", "British", "بريطاني", "+44", 100, false),
        CreateCountry(ref idCounter, "GR", "GRC", "Greece", "اليونان", "Greek", "يوناني", "+30", 100, false),
        CreateCountry(ref idCounter, "HK", "HKG", "Hong Kong", "هونغ كونغ", "Hong Konger", "هونغ كونغي", "+852", 100, false),
        CreateCountry(ref idCounter, "IR", "IRN", "Iran", "إيران", "Iranian", "إيراني", "+98", 100, false),
        CreateCountry(ref idCounter, "IQ", "IRQ", "Iraq", "العراق", "Iraqi", "عراقي", "+964", 100, false),
        CreateCountry(ref idCounter, "IE", "IRL", "Ireland", "أيرلندا", "Irish", "أيرلندي", "+353", 100, false),
        CreateCountry(ref idCounter, "IT", "ITA", "Italy", "إيطاليا", "Italian", "إيطالي", "+39", 100, false),
        CreateCountry(ref idCounter, "JP", "JPN", "Japan", "اليابان", "Japanese", "ياباني", "+81", 100, false),
        CreateCountry(ref idCounter, "JO", "JOR", "Jordan", "الأردن", "Jordanian", "أردني", "+962", 100, false),
        CreateCountry(ref idCounter, "KZ", "KAZ", "Kazakhstan", "كازاخستان", "Kazakhstani", "كازاخستاني", "+7", 100, false),
        CreateCountry(ref idCounter, "KR", "KOR", "South Korea", "كوريا الجنوبية", "South Korean", "كوري جنوبي", "+82", 100, false),
        CreateCountry(ref idCounter, "LB", "LBN", "Lebanon", "لبنان", "Lebanese", "لبناني", "+961", 100, false),
        CreateCountry(ref idCounter, "LY", "LBY", "Libya", "ليبيا", "Libyan", "ليبي", "+218", 100, false),
        CreateCountry(ref idCounter, "MY", "MYS", "Malaysia", "ماليزيا", "Malaysian", "ماليزي", "+60", 100, false),
        CreateCountry(ref idCounter, "MV", "MDV", "Maldives", "المالديف", "Maldivian", "مالديفي", "+960", 100, false),
        CreateCountry(ref idCounter, "MX", "MEX", "Mexico", "المكسيك", "Mexican", "مكسيكي", "+52", 100, false),
        CreateCountry(ref idCounter, "MA", "MAR", "Morocco", "المغرب", "Moroccan", "مغربي", "+212", 100, false),
        CreateCountry(ref idCounter, "MM", "MMR", "Myanmar", "ميانمار", "Burmese", "بورمي", "+95", 100, false),
        CreateCountry(ref idCounter, "NL", "NLD", "Netherlands", "هولندا", "Dutch", "هولندي", "+31", 100, false),
        CreateCountry(ref idCounter, "NZ", "NZL", "New Zealand", "نيوزيلندا", "New Zealander", "نيوزيلندي", "+64", 100, false),
        CreateCountry(ref idCounter, "NG", "NGA", "Nigeria", "نيجيريا", "Nigerian", "نيجيري", "+234", 100, false),
        CreateCountry(ref idCounter, "PK", "PAK", "Pakistan", "باكستان", "Pakistani", "باكستاني", "+92", 100, false),
        CreateCountry(ref idCounter, "PS", "PSE", "Palestine", "فلسطين", "Palestinian", "فلسطيني", "+970", 100, false),
        CreateCountry(ref idCounter, "PE", "PER", "Peru", "بيرو", "Peruvian", "بيروفي", "+51", 100, false),
        CreateCountry(ref idCounter, "PL", "POL", "Poland", "بولندا", "Polish", "بولندي", "+48", 100, false),
        CreateCountry(ref idCounter, "PT", "PRT", "Portugal", "البرتغال", "Portuguese", "برتغالي", "+351", 100, false),
        CreateCountry(ref idCounter, "RO", "ROU", "Romania", "رومانيا", "Romanian", "روماني", "+40", 100, false),
        CreateCountry(ref idCounter, "RU", "RUS", "Russia", "روسيا", "Russian", "روسي", "+7", 100, false),
        CreateCountry(ref idCounter, "RW", "RWA", "Rwanda", "رواندا", "Rwandan", "رواندي", "+250", 100, false),
        CreateCountry(ref idCounter, "SN", "SEN", "Senegal", "السنغال", "Senegalese", "سنغالي", "+221", 100, false),
        CreateCountry(ref idCounter, "SG", "SGP", "Singapore", "سنغافورة", "Singaporean", "سنغافوري", "+65", 100, false),
        CreateCountry(ref idCounter, "ZA", "ZAF", "South Africa", "جنوب أفريقيا", "South African", "جنوب أفريقي", "+27", 100, false),
        CreateCountry(ref idCounter, "ES", "ESP", "Spain", "إسبانيا", "Spanish", "إسباني", "+34", 100, false),
        CreateCountry(ref idCounter, "SD", "SDN", "Sudan", "السودان", "Sudanese", "سوداني", "+249", 100, false),
        CreateCountry(ref idCounter, "SE", "SWE", "Sweden", "السويد", "Swedish", "سويدي", "+46", 100, false),
        CreateCountry(ref idCounter, "CH", "CHE", "Switzerland", "سويسرا", "Swiss", "سويسري", "+41", 100, false),
        CreateCountry(ref idCounter, "SY", "SYR", "Syria", "سوريا", "Syrian", "سوري", "+963", 100, false),
        CreateCountry(ref idCounter, "TW", "TWN", "Taiwan", "تايوان", "Taiwanese", "تايواني", "+886", 100, false),
        CreateCountry(ref idCounter, "TZ", "TZA", "Tanzania", "تنزانيا", "Tanzanian", "تنزاني", "+255", 100, false),
        CreateCountry(ref idCounter, "TH", "THA", "Thailand", "تايلاند", "Thai", "تايلاندي", "+66", 100, false),
        CreateCountry(ref idCounter, "TN", "TUN", "Tunisia", "تونس", "Tunisian", "تونسي", "+216", 100, false),
        CreateCountry(ref idCounter, "TR", "TUR", "Turkey", "تركيا", "Turkish", "تركي", "+90", 100, false),
        CreateCountry(ref idCounter, "UA", "UKR", "Ukraine", "أوكرانيا", "Ukrainian", "أوكراني", "+380", 100, false),
        CreateCountry(ref idCounter, "US", "USA", "United States", "الولايات المتحدة", "American", "أمريكي", "+1", 100, false),
        CreateCountry(ref idCounter, "UZ", "UZB", "Uzbekistan", "أوزبكستان", "Uzbekistani", "أوزبكستاني", "+998", 100, false),
        CreateCountry(ref idCounter, "VE", "VEN", "Venezuela", "فنزويلا", "Venezuelan", "فنزويلي", "+58", 100, false),
        CreateCountry(ref idCounter, "VN", "VNM", "Vietnam", "فيتنام", "Vietnamese", "فيتنامي", "+84", 100, false),
        CreateCountry(ref idCounter, "YE", "YEM", "Yemen", "اليمن", "Yemeni", "يمني", "+967", 100, false),
        CreateCountry(ref idCounter, "ZM", "ZMB", "Zambia", "زامبيا", "Zambian", "زامبي", "+260", 100, false),
        CreateCountry(ref idCounter, "ZW", "ZWE", "Zimbabwe", "زيمبابوي", "Zimbabwean", "زيمبابوي", "+263", 100, false)
    ];

    private static Country CreateCountry(
        ref int idCounter,
        string code,
        string alpha3,
        string nameEn,
        string nameAr,
        string nationalityEn,
        string nationalityAr,
        string dialingCode,
        int displayOrder,
        bool isCommon)
    {
        return new Country
        {
            Id = Guid.Parse($"20000000-0000-0000-0000-{idCounter++:D12}"),
            Code = code,
            Alpha3Code = alpha3,
            Name = new LocalizedString(nameEn, nameAr),
            Nationality = new LocalizedString(nationalityEn, nationalityAr),
            DialingCode = dialingCode,
            DisplayOrder = displayOrder,
            IsCommonNationality = isCommon
        };
    }
}

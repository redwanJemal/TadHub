using System.Diagnostics;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TadHub.Api.Documents;
using Worker.Contracts.DTOs;

namespace TadHub.Tests.Unit.Documents;

public class WorkerCvDocumentTests
{
    public WorkerCvDocumentTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [Fact]
    public void GeneratePdf_FullData_ProducesValidPdf()
    {
        // Arrange
        var data = CreateFullData();
        var document = new WorkerCvDocument(data);

        // Act
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeEmpty();
        pdfBytes.Length.Should().BeGreaterThan(1000, "a real PDF with content should be > 1KB");
        AssertValidPdfHeader(pdfBytes);
    }

    [Fact]
    public void GeneratePdf_MinimalData_NoPhotoNoLogoNoCollections_ProducesValidPdf()
    {
        // Arrange — bare minimum: just a name and worker code
        var cv = new WorkerCvDto
        {
            Id = Guid.NewGuid(),
            WorkerCode = "WRK-MIN-001",
            FullNameEn = "Minimal Worker",
            Nationality = "IN",
            Skills = new List<WorkerSkillDto>(),
            Languages = new List<WorkerLanguageDto>(),
        };
        var data = new WorkerCvPdfData(cv, "Test Center", null, null, null);
        var document = new WorkerCvDocument(data);

        // Act
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeEmpty();
        AssertValidPdfHeader(pdfBytes);
    }

    [Fact]
    public void GeneratePdf_EmptySkillsAndLanguages_ProducesValidPdf()
    {
        // Arrange — all personal/professional fields populated, but no skills/languages
        var cv = CreateCvDto(skills: new(), languages: new());
        var data = new WorkerCvPdfData(cv, "Al Tadbeer Center", "مركز التدبير", null, null);
        var document = new WorkerCvDocument(data);

        // Act
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeEmpty();
        AssertValidPdfHeader(pdfBytes);
    }

    [Fact]
    public void GeneratePdf_NullOptionalFields_DoesNotThrow()
    {
        // Arrange — every nullable field is null
        var cv = new WorkerCvDto
        {
            Id = Guid.NewGuid(),
            WorkerCode = "WRK-NUL-001",
            FullNameEn = "Null Fields Worker",
            FullNameAr = null,
            Nationality = "PH",
            DateOfBirth = null,
            Gender = null,
            PassportNumber = null,
            PassportExpiry = null,
            Phone = null,
            Email = null,
            Religion = null,
            MaritalStatus = null,
            EducationLevel = null,
            JobCategoryId = null,
            JobCategory = null,
            ExperienceYears = null,
            MonthlySalary = null,
            PhotoUrl = null,
            VideoUrl = null,
            PassportDocumentUrl = null,
            Skills = new List<WorkerSkillDto>(),
            Languages = new List<WorkerLanguageDto>(),
        };
        var data = new WorkerCvPdfData(cv, "Center", null, null, null);
        var document = new WorkerCvDocument(data);

        // Act
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeEmpty();
        AssertValidPdfHeader(pdfBytes);
    }

    [Fact]
    public void GeneratePdf_ManySkillsAndLanguages_ProducesValidPdf()
    {
        // Arrange — stress test with many items
        var skills = Enumerable.Range(1, 20).Select(i => new WorkerSkillDto
        {
            Id = Guid.NewGuid(),
            SkillName = $"Skill {i}",
            ProficiencyLevel = i % 2 == 0 ? "Advanced" : "Basic",
        }).ToList();

        var languages = Enumerable.Range(1, 10).Select(i => new WorkerLanguageDto
        {
            Id = Guid.NewGuid(),
            Language = $"Language {i}",
            ProficiencyLevel = i % 3 == 0 ? "Native" : "Conversational",
        }).ToList();

        var cv = CreateCvDto(skills: skills, languages: languages);
        var data = new WorkerCvPdfData(cv, "Big Center LLC", "شركة المركز الكبير", null, null);
        var document = new WorkerCvDocument(data);

        // Act
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeEmpty();
        AssertValidPdfHeader(pdfBytes);
    }

    [Fact]
    public void GeneratePdf_WithArabicNames_ProducesValidPdf()
    {
        // Arrange — Arabic content in names
        var cv = CreateCvDto();
        cv = cv with
        {
            FullNameAr = "محمد أحمد عبدالله",
        };
        var data = new WorkerCvPdfData(cv, "Al Tadbeer Center", "مركز التدبير للاستقدام", null, null);
        var document = new WorkerCvDocument(data);

        // Act
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeEmpty();
        AssertValidPdfHeader(pdfBytes);
    }

    [Fact]
    public void GeneratePdf_Performance_CompletesUnder500ms()
    {
        // Arrange — warm up QuestPDF engine
        var warmupData = new WorkerCvPdfData(
            CreateCvDto(),
            "Warmup", null, null, null);
        new WorkerCvDocument(warmupData).GeneratePdf();

        // Act — measure a fresh generation
        var data = CreateFullData();
        var document = new WorkerCvDocument(data);

        var sw = Stopwatch.StartNew();
        var pdfBytes = document.GeneratePdf();
        sw.Stop();

        // Assert
        pdfBytes.Should().NotBeEmpty();
        sw.ElapsedMilliseconds.Should().BeLessThan(500,
            "PDF generation for a single-page CV should be fast");
    }

    [Fact]
    public void GeneratePdf_Performance_MultipleGenerations_AverageUnder100ms()
    {
        // Arrange — warm up
        var data = CreateFullData();
        new WorkerCvDocument(data).GeneratePdf();

        // Act — generate 10 PDFs and measure average
        var sw = Stopwatch.StartNew();
        const int iterations = 10;
        for (var i = 0; i < iterations; i++)
        {
            new WorkerCvDocument(data).GeneratePdf();
        }
        sw.Stop();

        var avgMs = sw.ElapsedMilliseconds / (double)iterations;

        // Assert
        avgMs.Should().BeLessThan(100,
            "after warmup, each PDF should average under 100ms");
    }

    [Fact]
    public void GeneratePdf_FileSize_IsReasonable()
    {
        // Arrange
        var data = CreateFullData();
        var document = new WorkerCvDocument(data);

        // Act
        var pdfBytes = document.GeneratePdf();

        // Assert — without embedded images, PDF should be compact
        var sizeKb = pdfBytes.Length / 1024.0;
        sizeKb.Should().BeLessThan(200, "a text-only CV PDF should be under 200KB");
        sizeKb.Should().BeGreaterThan(1, "a real PDF should be at least 1KB");
    }

    #region Test Data Helpers

    private static WorkerCvPdfData CreateFullData()
    {
        var cv = CreateCvDto(
            skills: new List<WorkerSkillDto>
            {
                new() { Id = Guid.NewGuid(), SkillName = "Cleaning", ProficiencyLevel = "Advanced" },
                new() { Id = Guid.NewGuid(), SkillName = "Cooking", ProficiencyLevel = "Intermediate" },
                new() { Id = Guid.NewGuid(), SkillName = "Childcare", ProficiencyLevel = "Expert" },
            },
            languages: new List<WorkerLanguageDto>
            {
                new() { Id = Guid.NewGuid(), Language = "English", ProficiencyLevel = "Conversational" },
                new() { Id = Guid.NewGuid(), Language = "Arabic", ProficiencyLevel = "Basic" },
                new() { Id = Guid.NewGuid(), Language = "Tagalog", ProficiencyLevel = "Native" },
            }
        );

        return new WorkerCvPdfData(
            Cv: cv,
            TenantName: "Al Tadbeer Recruitment Center",
            TenantNameAr: "مركز التدبير للاستقدام",
            TenantLogo: null,
            WorkerPhoto: null);
    }

    private static WorkerCvDto CreateCvDto(
        List<WorkerSkillDto>? skills = null,
        List<WorkerLanguageDto>? languages = null)
    {
        return new WorkerCvDto
        {
            Id = Guid.NewGuid(),
            WorkerCode = "WRK-2026-0042",
            FullNameEn = "Maria Dela Cruz Santos",
            FullNameAr = "ماريا ديلا كروز سانتوس",
            Nationality = "PH",
            DateOfBirth = new DateOnly(1995, 3, 15),
            Gender = "Female",
            PassportNumber = "P1234567A",
            PassportExpiry = new DateOnly(2029, 6, 30),
            Phone = "+971501234567",
            Email = "maria.santos@example.com",
            Religion = "Christianity",
            MaritalStatus = "Single",
            EducationLevel = "Bachelor",
            JobCategoryId = Guid.NewGuid(),
            JobCategory = new JobCategoryInfoDto(Guid.NewGuid(), "Domestic Helper"),
            ExperienceYears = 5,
            MonthlySalary = 2500m,
            Skills = skills ?? new List<WorkerSkillDto>(),
            Languages = languages ?? new List<WorkerLanguageDto>(),
        };
    }

    private static void AssertValidPdfHeader(byte[] pdfBytes)
    {
        // PDF files must start with %PDF-
        pdfBytes.Length.Should().BeGreaterThanOrEqualTo(5);
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 5);
        header.Should().Be("%PDF-", "file must be a valid PDF");
    }

    #endregion
}

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Worker.Contracts.DTOs;

namespace TadHub.Api.Documents;

public sealed class WorkerCvDocument : IDocument
{
    private readonly WorkerCvPdfData _data;

    private const string FontFamily = "DejaVu Sans";
    private static readonly string PrimaryColor = "#1a365d";
    private static readonly string LightGray = "#f7fafc";
    private static readonly string MediumGray = "#718096";
    private static readonly string BorderColor = "#e2e8f0";

    public WorkerCvDocument(WorkerCvPdfData data)
    {
        _data = data;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.MarginVertical(30);
            page.MarginHorizontal(35);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily(FontFamily));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                if (_data.TenantLogo is { Length: > 0 })
                {
                    row.ConstantItem(60).Height(60).Image(_data.TenantLogo).FitArea();
                }

                row.RelativeItem().PaddingLeft(_data.TenantLogo is { Length: > 0 } ? 12 : 0).Column(nameCol =>
                {
                    nameCol.Item().Text(_data.TenantName)
                        .FontSize(18).Bold().FontColor(PrimaryColor);

                    if (!string.IsNullOrEmpty(_data.TenantNameAr))
                    {
                        nameCol.Item().Text(_data.TenantNameAr)
                            .FontSize(14).FontColor(MediumGray);
                    }

                    nameCol.Item().PaddingTop(4).Text("Worker CV")
                        .FontSize(11).FontColor(MediumGray);
                });
            });

            col.Item().PaddingTop(8).LineHorizontal(1).LineColor(BorderColor);
            col.Item().PaddingBottom(12);
        });
    }

    private void ComposeContent(IContainer container)
    {
        var cv = _data.Cv;

        container.Column(col =>
        {
            // Photo + Personal Info row
            col.Item().Row(row =>
            {
                // Photo column
                row.ConstantItem(130).Column(photoCol =>
                {
                    if (_data.WorkerPhoto is { Length: > 0 })
                    {
                        photoCol.Item().Width(120).Height(150).Image(_data.WorkerPhoto).FitArea();
                    }
                    else
                    {
                        photoCol.Item().Width(120).Height(150)
                            .Border(1).BorderColor(BorderColor)
                            .Background(LightGray)
                            .AlignCenter().AlignMiddle()
                            .Text("No Photo").FontSize(9).FontColor(MediumGray);
                    }
                });

                // Personal info
                row.RelativeItem().PaddingLeft(16).Column(infoCol =>
                {
                    infoCol.Item().Text(cv.FullNameEn).FontSize(16).Bold().FontColor(PrimaryColor);

                    if (!string.IsNullOrEmpty(cv.FullNameAr))
                        infoCol.Item().Text(cv.FullNameAr).FontSize(13).FontColor(MediumGray);

                    infoCol.Item().PaddingTop(10).Element(c => ComposeInfoGrid(c, new[]
                    {
                        ("Nationality", cv.Nationality),
                        ("Date of Birth", cv.DateOfBirth?.ToString("dd MMM yyyy")),
                        ("Gender", cv.Gender),
                        ("Passport No.", cv.PassportNumber),
                        ("Passport Expiry", cv.PassportExpiry?.ToString("dd MMM yyyy")),
                        ("Phone", cv.Phone),
                        ("Email", cv.Email),
                    }));
                });
            });

            col.Item().PaddingTop(16);

            // Professional Profile
            col.Item().Element(c => ComposeSection(c, "Professional Profile", section =>
            {
                ComposeInfoGrid(section, new[]
                {
                    ("Religion", cv.Religion),
                    ("Marital Status", cv.MaritalStatus),
                    ("Education", cv.EducationLevel),
                    ("Job Category", cv.JobCategory?.Name),
                    ("Experience", cv.ExperienceYears.HasValue ? $"{cv.ExperienceYears} years" : null),
                    ("Monthly Salary", cv.MonthlySalary.HasValue ? $"{cv.MonthlySalary:N0} AED" : null),
                }, columns: 3);
            }));

            col.Item().PaddingTop(12);

            // Skills
            if (cv.Skills.Count > 0)
            {
                col.Item().Element(c => ComposeSection(c, "Skills", section =>
                {
                    ComposeTable(section, new[] { "Skill", "Proficiency" },
                        cv.Skills.Select(s => new[] { s.SkillName, s.ProficiencyLevel }).ToList());
                }));
                col.Item().PaddingTop(12);
            }

            // Languages
            if (cv.Languages.Count > 0)
            {
                col.Item().Element(c => ComposeSection(c, "Languages", section =>
                {
                    ComposeTable(section, new[] { "Language", "Proficiency" },
                        cv.Languages.Select(l => new[] { l.Language, l.ProficiencyLevel }).ToList());
                }));
            }
        });
    }

    private void ComposeSection(IContainer container, string title, Action<IContainer> content)
    {
        container.Column(col =>
        {
            col.Item().PaddingBottom(6).Text(title)
                .FontSize(13).Bold().FontColor(PrimaryColor);

            col.Item().LineHorizontal(0.5f).LineColor(BorderColor);
            col.Item().PaddingTop(8).Element(content);
        });
    }

    private static void ComposeInfoGrid(IContainer container, (string Label, string? Value)[] items, int columns = 2)
    {
        var filtered = items.Where(i => !string.IsNullOrEmpty(i.Value)).ToArray();
        if (filtered.Length == 0) return;

        container.Table(table =>
        {
            table.ColumnsDefinition(def =>
            {
                for (int i = 0; i < columns; i++)
                    def.RelativeColumn();
            });

            foreach (var item in filtered)
            {
                table.Cell().PaddingBottom(6).Column(cell =>
                {
                    cell.Item().Text(item.Label).FontSize(8).FontColor(MediumGray);
                    cell.Item().Text(item.Value!).FontSize(10);
                });
            }
        });
    }

    private static void ComposeTable(IContainer container, string[] headers, List<string[]> rows)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(def =>
            {
                foreach (var _ in headers)
                    def.RelativeColumn();
            });

            // Header row
            foreach (var header in headers)
            {
                table.Cell().Background(LightGray).Padding(6)
                    .Text(header).FontSize(9).Bold().FontColor(PrimaryColor);
            }

            // Data rows
            foreach (var row in rows)
            {
                foreach (var cell in row)
                {
                    table.Cell().BorderBottom(0.5f).BorderColor(BorderColor).Padding(6)
                        .Text(cell).FontSize(9);
                }
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor(BorderColor);
            col.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span($"Generated by TadHub on {DateTime.UtcNow:dd MMM yyyy}")
                        .FontSize(8).FontColor(MediumGray);
                });
                row.RelativeItem().AlignRight().Text(_data.Cv.WorkerCode)
                    .FontSize(8).FontColor(MediumGray);
            });
        });
    }
}

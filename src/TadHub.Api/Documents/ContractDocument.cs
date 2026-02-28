using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TadHub.Api.Documents;

public sealed class ContractDocument : IDocument
{
    private readonly ContractPdfData _data;

    private const string FontFamily = "DejaVu Sans";
    private static readonly string PrimaryColor = "#1a365d";
    private static readonly string MediumGray = "#718096";
    private static readonly string BorderColor = "#e2e8f0";

    public ContractDocument(ContractPdfData data)
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

                    nameCol.Item().PaddingTop(4).Text("Employment Contract")
                        .FontSize(11).FontColor(MediumGray);
                });
            });

            col.Item().PaddingTop(8).LineHorizontal(1).LineColor(BorderColor);
            col.Item().PaddingBottom(12);
        });
    }

    private void ComposeContent(IContainer container)
    {
        var c = _data.Contract;

        container.Column(col =>
        {
            // Contract Code + Status
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(infoCol =>
                {
                    infoCol.Item().Text(c.ContractCode).FontSize(16).Bold().FontColor(PrimaryColor);
                    infoCol.Item().PaddingTop(4).Text($"Status: {c.Status}  |  Type: {c.Type}")
                        .FontSize(10).FontColor(MediumGray);
                });
            });

            col.Item().PaddingTop(16);

            // Parties
            col.Item().Element(s => ComposeSection(s, "Parties", section =>
            {
                ComposeInfoGrid(section, new[]
                {
                    ("Worker", c.Worker != null ? $"{c.Worker.FullNameEn} ({c.Worker.WorkerCode})" : c.WorkerId.ToString()),
                    ("Worker (Arabic)", c.Worker?.FullNameAr),
                    ("Client", c.Client != null ? c.Client.NameEn : c.ClientId.ToString()),
                    ("Client (Arabic)", c.Client?.NameAr),
                });
            }));

            col.Item().PaddingTop(12);

            // Dates & Terms
            col.Item().Element(s => ComposeSection(s, "Dates & Terms", section =>
            {
                ComposeInfoGrid(section, new[]
                {
                    ("Start Date", c.StartDate.ToString("dd MMM yyyy")),
                    ("End Date", c.EndDate?.ToString("dd MMM yyyy")),
                    ("Probation End Date", c.ProbationEndDate?.ToString("dd MMM yyyy")),
                    ("Guarantee End Date", c.GuaranteeEndDate?.ToString("dd MMM yyyy")),
                    ("Probation Passed", c.ProbationPassed ? "Yes" : "No"),
                });
            }));

            col.Item().PaddingTop(12);

            // Financial
            col.Item().Element(s => ComposeSection(s, "Financial", section =>
            {
                ComposeInfoGrid(section, new[]
                {
                    ("Rate", $"{c.Rate:N2} {c.Currency}"),
                    ("Rate Period", c.RatePeriod),
                    ("Total Value", c.TotalValue.HasValue ? $"{c.TotalValue:N2} {c.Currency}" : null),
                });
            }));

            // Termination (if applicable)
            if (c.TerminatedAt.HasValue)
            {
                col.Item().PaddingTop(12);
                col.Item().Element(s => ComposeSection(s, "Termination", section =>
                {
                    ComposeInfoGrid(section, new[]
                    {
                        ("Terminated At", c.TerminatedAt.Value.ToString("dd MMM yyyy HH:mm")),
                        ("Termination Reason", c.TerminationReason),
                        ("Terminated By", c.TerminatedBy),
                    });
                }));
            }

            // Notes
            if (!string.IsNullOrEmpty(c.Notes))
            {
                col.Item().PaddingTop(12);
                col.Item().Element(s => ComposeSection(s, "Notes", section =>
                {
                    section.Text(c.Notes).FontSize(10);
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
                row.RelativeItem().AlignRight().Text(_data.Contract.ContractCode)
                    .FontSize(8).FontColor(MediumGray);
            });
        });
    }
}

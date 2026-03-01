using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TadHub.Api.Documents;

public sealed class InvoiceDocument : IDocument
{
    private readonly InvoicePdfData _data;

    private const string FontFamily = "DejaVu Sans";
    private static readonly string LightGray = "#f7fafc";
    private static readonly string MediumGray = "#718096";
    private static readonly string BorderColor = "#e2e8f0";

    private string PrimaryColor => _data.Template.PrimaryColor;
    private string AccentColor => _data.Template.AccentColor;
    private bool ShowArabic => _data.Template.ShowArabicText;

    public InvoiceDocument(InvoicePdfData data)
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
        var inv = _data.Invoice;
        var isCredit = string.Equals(inv.Type, "CreditNote", StringComparison.OrdinalIgnoreCase);
        var title = isCredit ? "CREDIT NOTE" : "TAX INVOICE";
        var titleAr = isCredit ? "إشعار دائن" : "فاتورة ضريبية";

        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                // Left: Logo + company name
                row.RelativeItem().Column(left =>
                {
                    if (_data.Template.ShowLogo && _data.TenantLogo is { Length: > 0 })
                    {
                        left.Item().Height(50).Width(50).Image(_data.TenantLogo).FitArea();
                        left.Item().PaddingTop(4);
                    }

                    left.Item().Text(_data.TenantName)
                        .FontSize(16).Bold().FontColor(PrimaryColor);

                    if (ShowArabic && !string.IsNullOrEmpty(_data.TenantNameAr))
                    {
                        left.Item().Text(_data.TenantNameAr)
                            .FontSize(12).FontColor(MediumGray);
                    }

                    if (!string.IsNullOrEmpty(inv.TenantTrn))
                    {
                        left.Item().PaddingTop(2).Text($"TRN: {inv.TenantTrn}")
                            .FontSize(8).FontColor(MediumGray);
                    }

                    if (!string.IsNullOrEmpty(_data.TenantWebsite))
                    {
                        left.Item().Text(_data.TenantWebsite)
                            .FontSize(8).FontColor(MediumGray);
                    }

                    if (!string.IsNullOrEmpty(_data.Template.CompanyAddress))
                    {
                        left.Item().PaddingTop(2).Text(_data.Template.CompanyAddress)
                            .FontSize(8).FontColor(MediumGray);
                    }
                });

                // Right: Title
                row.RelativeItem().AlignRight().Column(right =>
                {
                    right.Item().AlignRight().Text(title)
                        .FontSize(20).Bold().FontColor(PrimaryColor);

                    if (ShowArabic)
                    {
                        right.Item().AlignRight().Text(titleAr)
                            .FontSize(14).FontColor(AccentColor);
                    }
                });
            });

            col.Item().PaddingTop(8).LineHorizontal(2).LineColor(PrimaryColor);
            col.Item().PaddingBottom(10);
        });
    }

    private void ComposeContent(IContainer container)
    {
        var inv = _data.Invoice;

        container.Column(col =>
        {
            // Invoice details + Client block side by side
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(c => ComposeInvoiceDetails(c, inv));
                row.ConstantItem(20);
                row.RelativeItem().Element(c => ComposeClientBlock(c, inv));
            });

            col.Item().PaddingTop(12);

            // Worker block (if applicable)
            if (inv.WorkerId.HasValue)
            {
                col.Item().Element(ComposeWorkerBlock);
                col.Item().PaddingTop(12);
            }

            // Line items table
            col.Item().Element(ComposeLineItems);
            col.Item().PaddingTop(12);

            // Financial summary
            col.Item().Element(ComposeFinancialSummary);

            // Discount info
            if (!string.IsNullOrEmpty(inv.DiscountProgramName))
            {
                col.Item().PaddingTop(12);
                col.Item().Element(ComposeDiscountInfo);
            }

            // Payment history
            if (inv.Payments is { Count: > 0 })
            {
                col.Item().PaddingTop(16);
                col.Item().Element(ComposePaymentHistory);
            }

            // Terms & Notes
            if (!string.IsNullOrEmpty(_data.Terms) || !string.IsNullOrEmpty(_data.FooterText)
                || !string.IsNullOrEmpty(inv.Notes))
            {
                col.Item().PaddingTop(16);
                col.Item().Element(ComposeTermsAndNotes);
            }
        });
    }

    private void ComposeInvoiceDetails(IContainer container, Financial.Contracts.DTOs.InvoiceDto inv)
    {
        container.Column(col =>
        {
            col.Item().PaddingBottom(4).Text("Invoice Details")
                .FontSize(11).Bold().FontColor(PrimaryColor);
            col.Item().LineHorizontal(0.5f).LineColor(BorderColor);
            col.Item().PaddingTop(6);

            ComposeField(col, "Invoice Number", inv.InvoiceNumber);
            ComposeField(col, "Issue Date", inv.IssueDate.ToString("dd MMM yyyy"));
            ComposeField(col, "Due Date", inv.DueDate.ToString("dd MMM yyyy"));
            ComposeField(col, "Status", inv.Status);
            if (!string.IsNullOrEmpty(inv.MilestoneType))
                ComposeField(col, "Milestone", inv.MilestoneType);
            ComposeField(col, "Currency", inv.Currency);
        });
    }

    private void ComposeClientBlock(IContainer container, Financial.Contracts.DTOs.InvoiceDto inv)
    {
        container.Column(col =>
        {
            col.Item().PaddingBottom(4).Text("Bill To")
                .FontSize(11).Bold().FontColor(PrimaryColor);
            col.Item().LineHorizontal(0.5f).LineColor(BorderColor);
            col.Item().PaddingTop(6);

            if (!string.IsNullOrEmpty(_data.ClientName))
                ComposeField(col, "Client", _data.ClientName);
            if (ShowArabic && !string.IsNullOrEmpty(_data.ClientNameAr))
                ComposeField(col, "العميل", _data.ClientNameAr);
            if (!string.IsNullOrEmpty(inv.ClientTrn))
                ComposeField(col, "Client TRN", inv.ClientTrn);
        });
    }

    private void ComposeWorkerBlock(IContainer container)
    {
        container.Background(LightGray).Padding(8).Column(col =>
        {
            col.Item().Text("Worker / Service Provider")
                .FontSize(9).Bold().FontColor(PrimaryColor);
            col.Item().PaddingTop(4).Row(row =>
            {
                if (!string.IsNullOrEmpty(_data.WorkerName))
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(_data.WorkerName).FontSize(10);
                        if (ShowArabic && !string.IsNullOrEmpty(_data.WorkerNameAr))
                            c.Item().Text(_data.WorkerNameAr).FontSize(9).FontColor(MediumGray);
                    });
                }
                if (!string.IsNullOrEmpty(_data.WorkerCode))
                {
                    row.ConstantItem(100).AlignRight()
                        .Text($"Code: {_data.WorkerCode}").FontSize(9).FontColor(MediumGray);
                }
            });
        });
    }

    private void ComposeLineItems(IContainer container)
    {
        var items = _data.Invoice.LineItems;
        if (items is not { Count: > 0 }) return;

        container.Column(col =>
        {
            col.Item().PaddingBottom(4).Text("Line Items")
                .FontSize(11).Bold().FontColor(PrimaryColor);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(def =>
                {
                    def.ConstantColumn(25);  // #
                    def.ConstantColumn(50);  // Code
                    def.RelativeColumn(3);   // Description
                    if (ShowArabic) def.RelativeColumn(2); // Description Ar
                    def.ConstantColumn(35);  // Qty
                    def.ConstantColumn(65);  // Unit Price
                    def.ConstantColumn(55);  // Discount
                    def.ConstantColumn(70);  // Total
                });

                // Header row
                HeaderCell(table, "#");
                HeaderCell(table, "Code");
                HeaderCell(table, "Description");
                if (ShowArabic) HeaderCell(table, "الوصف");
                HeaderCell(table, "Qty", true);
                HeaderCell(table, "Unit Price", true);
                HeaderCell(table, "Discount", true);
                HeaderCell(table, "Total", true);

                foreach (var item in items)
                {
                    BodyCell(table, item.LineNumber.ToString());
                    BodyCell(table, item.ItemCode ?? "—");
                    BodyCell(table, item.Description);
                    if (ShowArabic) BodyCell(table, item.DescriptionAr ?? "—");
                    BodyCellRight(table, item.Quantity.ToString("N2"));
                    BodyCellRight(table, item.UnitPrice.ToString("N2"));
                    BodyCellRight(table, item.DiscountAmount > 0 ? item.DiscountAmount.ToString("N2") : "—");
                    BodyCellRight(table, item.LineTotal.ToString("N2"));
                }
            });
        });
    }

    private void ComposeFinancialSummary(IContainer container)
    {
        var inv = _data.Invoice;

        container.Row(row =>
        {
            row.RelativeItem(); // spacer
            row.ConstantItem(250).Column(col =>
            {
                col.Item().PaddingBottom(4).Text("Financial Summary")
                    .FontSize(11).Bold().FontColor(PrimaryColor);
                col.Item().LineHorizontal(0.5f).LineColor(BorderColor);
                col.Item().PaddingTop(6);

                SummaryLine(col, "Subtotal", inv.Subtotal, inv.Currency);

                if (inv.DiscountAmount > 0)
                    SummaryLine(col, "Discount", -inv.DiscountAmount, inv.Currency);

                SummaryLine(col, "Taxable Amount", inv.TaxableAmount, inv.Currency);
                SummaryLine(col, $"VAT ({inv.VatRate}%)", inv.VatAmount, inv.Currency);

                col.Item().PaddingVertical(4).LineHorizontal(1).LineColor(PrimaryColor);

                // Total
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("Total").FontSize(12).Bold().FontColor(PrimaryColor);
                    r.ConstantItem(100).AlignRight()
                        .Text($"{inv.TotalAmount:N2} {inv.Currency}")
                        .FontSize(12).Bold().FontColor(PrimaryColor);
                });

                if (inv.PaidAmount > 0)
                {
                    col.Item().PaddingTop(4);
                    SummaryLine(col, "Paid", inv.PaidAmount, inv.Currency, "#38a169");
                }

                if (inv.BalanceDue > 0)
                {
                    SummaryLine(col, "Balance Due", inv.BalanceDue, inv.Currency, "#e53e3e");
                }
            });
        });
    }

    private void ComposeDiscountInfo(IContainer container)
    {
        var inv = _data.Invoice;
        container.Background("#fffff0").Border(0.5f).BorderColor(BorderColor).Padding(8).Column(col =>
        {
            col.Item().Text("Discount Applied")
                .FontSize(9).Bold().FontColor(AccentColor);
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text($"Program: {inv.DiscountProgramName}").FontSize(9);
                if (inv.DiscountPercentage.HasValue)
                    row.RelativeItem().Text($"Rate: {inv.DiscountPercentage}%").FontSize(9);
                if (!string.IsNullOrEmpty(inv.DiscountCardNumber))
                    row.RelativeItem().Text($"Card: {inv.DiscountCardNumber}").FontSize(9);
            });
        });
    }

    private void ComposePaymentHistory(IContainer container)
    {
        var payments = _data.Invoice.Payments!;

        container.Column(col =>
        {
            col.Item().PaddingBottom(4).Text("Payment History")
                .FontSize(11).Bold().FontColor(PrimaryColor);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(def =>
                {
                    def.RelativeColumn(2); // Payment #
                    def.RelativeColumn(2); // Date
                    def.RelativeColumn(2); // Method
                    def.RelativeColumn(2); // Amount
                    def.RelativeColumn(1); // Status
                });

                // Header row
                HeaderCell(table, "Payment #");
                HeaderCell(table, "Date");
                HeaderCell(table, "Method");
                HeaderCell(table, "Amount", true);
                HeaderCell(table, "Status");

                foreach (var p in payments)
                {
                    BodyCell(table, p.PaymentNumber);
                    BodyCell(table, p.PaymentDate.ToString("dd MMM yyyy"));
                    BodyCell(table, p.Method);
                    BodyCellRight(table, $"{p.Amount:N2} {p.Currency}");
                    BodyCell(table, p.Status);
                }
            });
        });
    }

    private void ComposeTermsAndNotes(IContainer container)
    {
        container.Column(col =>
        {
            if (!string.IsNullOrEmpty(_data.Invoice.Notes))
            {
                col.Item().PaddingBottom(4).Text("Notes")
                    .FontSize(9).Bold().FontColor(PrimaryColor);
                col.Item().Text(_data.Invoice.Notes).FontSize(8);
                col.Item().PaddingTop(8);
            }

            if (!string.IsNullOrEmpty(_data.Terms))
            {
                col.Item().PaddingBottom(4).Text("Terms & Conditions")
                    .FontSize(9).Bold().FontColor(PrimaryColor);
                col.Item().Text(_data.Terms).FontSize(8);

                if (ShowArabic && !string.IsNullOrEmpty(_data.TermsAr))
                {
                    col.Item().PaddingTop(2).Text(_data.TermsAr)
                        .FontSize(8).FontColor(MediumGray);
                }
                col.Item().PaddingTop(8);
            }

            if (!string.IsNullOrEmpty(_data.FooterText))
            {
                col.Item().Text(_data.FooterText).FontSize(8).FontColor(MediumGray);
                if (ShowArabic && !string.IsNullOrEmpty(_data.FooterTextAr))
                {
                    col.Item().Text(_data.FooterTextAr).FontSize(8).FontColor(MediumGray);
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
                    text.Span($"Generated on {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC")
                        .FontSize(7).FontColor(MediumGray);
                });
                row.RelativeItem().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(7).FontColor(MediumGray));
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
                row.RelativeItem().AlignRight()
                    .Text(_data.Invoice.InvoiceNumber)
                    .FontSize(7).FontColor(MediumGray);
            });
        });
    }

    #region Table Helpers

    private void HeaderCell(TableDescriptor table, string text, bool alignRight = false)
    {
        var cell = table.Cell().Background(PrimaryColor).Padding(5);
        if (alignRight)
            cell.AlignRight().Text(text).FontSize(8).Bold().FontColor(Colors.White);
        else
            cell.Text(text).FontSize(8).Bold().FontColor(Colors.White);
    }

    private static void BodyCell(TableDescriptor table, string text)
    {
        table.Cell().BorderBottom(0.5f).BorderColor(BorderColor).Padding(4)
            .Text(text).FontSize(8);
    }

    private static void BodyCellRight(TableDescriptor table, string text)
    {
        table.Cell().BorderBottom(0.5f).BorderColor(BorderColor).Padding(4)
            .AlignRight().Text(text).FontSize(8);
    }

    private static void ComposeField(ColumnDescriptor col, string label, string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        col.Item().PaddingBottom(4).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text(label).FontSize(8).FontColor(MediumGray);
                c.Item().Text(value).FontSize(10);
            });
        });
    }

    private static void SummaryLine(ColumnDescriptor col, string label, decimal amount, string currency, string? color = null)
    {
        col.Item().PaddingBottom(2).Row(r =>
        {
            r.RelativeItem().Text(label).FontSize(9).FontColor(MediumGray);
            var amountText = r.ConstantItem(100).AlignRight()
                .Text($"{amount:N2} {currency}").FontSize(9);
            if (color != null) amountText.FontColor(color);
        });
    }

    #endregion
}

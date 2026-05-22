using FreelancerManagementSystem.Interfaces;
using FreelancerManagementSystem.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FreelancerManagementSystem.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateInvoicePdf(Invoice invoice)
        {
            var totalPaid = invoice.Payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);
            var balance = Math.Max(0, invoice.Amount - totalPaid);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    page.Header().Element(header => Header(header, "Invoice", invoice.InvoiceNumber));
                    page.Content().PaddingVertical(24).Column(column =>
                    {
                        column.Spacing(18);
                        column.Item().Element(content => SummaryBand(content, invoice.Status, invoice.Amount, totalPaid, balance));
                        column.Item().Element(content => PartyGrid(
                            content,
                            "Client",
                            FormatUser(invoice.Client),
                            "Freelancer",
                            FormatUser(invoice.Freelancer)));
                        column.Item().Element(content => DetailTable(content, new[]
                        {
                            ("Contract", invoice.Contract?.Title ?? "Not assigned"),
                            ("Issue date", FormatDate(invoice.IssueDate)),
                            ("Due date", FormatDate(invoice.DueDate)),
                            ("Paid date", invoice.PaidDate.HasValue ? FormatDate(invoice.PaidDate.Value) : "Not paid"),
                            ("Description", invoice.Description)
                        }));
                        column.Item().Element(content => PaymentTable(content, invoice.Payments.OrderByDescending(p => p.PaymentDate).ToList()));
                    });
                    page.Footer().Element(Footer);
                });
            }).GeneratePdf();
        }

        public byte[] GenerateContractPdf(Contract contract)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    page.Header().Element(header => Header(header, "Contract", contract.Title));
                    page.Content().PaddingVertical(24).Column(column =>
                    {
                        column.Spacing(18);
                        column.Item().Element(content => ContractSummary(content, contract));
                        column.Item().Element(content => PartyGrid(
                            content,
                            "Client",
                            FormatUser(contract.Client),
                            "Freelancer",
                            FormatUser(contract.Freelancer)));
                        column.Item().Element(content => DetailTable(content, new[]
                        {
                            ("Project", contract.Project?.Name ?? "Not assigned"),
                            ("Rate type", contract.RateType),
                            ("Rate", FormatMoney(contract.Rate)),
                            ("Total amount", FormatMoney(contract.TotalAmount)),
                            ("Start date", FormatDate(contract.StartDate)),
                            ("End date", FormatDate(contract.EndDate)),
                            ("Terms", contract.Terms)
                        }));
                        column.Item().Element(content => NotesBlock(content, "Scope", contract.Content));
                        column.Item().Element(SignatureBlock);
                    });
                    page.Footer().Element(Footer);
                });
            }).GeneratePdf();
        }

        private static void ConfigurePage(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(42);
            page.DefaultTextStyle(style => style.FontSize(10).FontColor(Colors.Grey.Darken3));
        }

        private static void Header(IContainer container, string type, string title)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("FreelanceOS").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text("Freelancer management platform").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                row.RelativeItem().AlignRight().Column(column =>
                {
                    column.Item().AlignRight().Text(type).FontSize(16).Bold();
                    column.Item().AlignRight().Text(title).FontSize(9).FontColor(Colors.Grey.Darken1);
                    column.Item().AlignRight().Text($"Generated {DateTime.Now:MMM dd, yyyy}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        }

        private static void SummaryBand(IContainer container, string status, decimal amount, decimal totalPaid, decimal balance)
        {
            container.Background(Colors.Blue.Lighten5).Border(1).BorderColor(Colors.Blue.Lighten3).Padding(16).Row(row =>
            {
                row.RelativeItem().Element(content => Metric(content, "Status", status));
                row.RelativeItem().Element(content => Metric(content, "Amount", FormatMoney(amount)));
                row.RelativeItem().Element(content => Metric(content, "Paid", FormatMoney(totalPaid)));
                row.RelativeItem().Element(content => Metric(content, "Balance", FormatMoney(balance)));
            });
        }

        private static void ContractSummary(IContainer container, Contract contract)
        {
            container.Background(Colors.Blue.Lighten5).Border(1).BorderColor(Colors.Blue.Lighten3).Padding(16).Row(row =>
            {
                row.RelativeItem().Element(content => Metric(content, "Status", contract.Status));
                row.RelativeItem().Element(content => Metric(content, "Project", contract.Project?.Name ?? "Not assigned"));
                row.RelativeItem().Element(content => Metric(content, "Total", FormatMoney(contract.TotalAmount)));
            });
        }

        private static void Metric(IContainer container, string label, string value)
        {
            container.Column(column =>
            {
                column.Item().Text(label.ToUpperInvariant()).FontSize(8).SemiBold().FontColor(Colors.Grey.Darken1);
                column.Item().Text(value).FontSize(13).Bold().FontColor(Colors.Grey.Darken4);
            });
        }

        private static void PartyGrid(IContainer container, string leftTitle, string leftValue, string rightTitle, string rightValue)
        {
            container.Row(row =>
            {
                row.RelativeItem().Element(content => PartyCard(content, leftTitle, leftValue));
                row.ConstantItem(14);
                row.RelativeItem().Element(content => PartyCard(content, rightTitle, rightValue));
            });
        }

        private static void PartyCard(IContainer container, string title, string value)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(14).Column(column =>
            {
                column.Item().Text(title).FontSize(9).SemiBold().FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(4).Text(value).FontSize(12).Bold();
            });
        }

        private static void DetailTable(IContainer container, IEnumerable<(string Label, string Value)> rows)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(120);
                    columns.RelativeColumn();
                });

                foreach (var row in rows)
                {
                    table.Cell().Element(LabelCell).Text(row.Label);
                    table.Cell().Element(ValueCell).Text(row.Value);
                }
            });
        }

        private static void PaymentTable(IContainer container, IReadOnlyCollection<Payment> payments)
        {
            container.Column(column =>
            {
                column.Item().Text("Payment summary").FontSize(13).Bold();
                column.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    HeaderCell(table, "Date");
                    HeaderCell(table, "Method");
                    HeaderCell(table, "Status");
                    HeaderCell(table, "Amount");

                    if (!payments.Any())
                    {
                        table.Cell().ColumnSpan(4).Element(ValueCell).Text("No payments recorded yet.");
                        return;
                    }

                    foreach (var payment in payments)
                    {
                        table.Cell().Element(ValueCell).Text(FormatDate(payment.PaymentDate));
                        table.Cell().Element(ValueCell).Text(payment.PaymentMethod);
                        table.Cell().Element(ValueCell).Text(payment.Status);
                        table.Cell().Element(ValueCell).AlignRight().Text(FormatMoney(payment.Amount));
                    }
                });
            });
        }

        private static void NotesBlock(IContainer container, string title, string content)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(14).Column(column =>
            {
                column.Item().Text(title).FontSize(13).Bold();
                column.Item().PaddingTop(8).Text(string.IsNullOrWhiteSpace(content) ? "No additional notes." : content).LineHeight(1.35f);
            });
        }

        private static void SignatureBlock(IContainer container)
        {
            container.PaddingTop(18).Row(row =>
            {
                row.RelativeItem().Element(content => SignatureLine(content, "Client signature"));
                row.ConstantItem(28);
                row.RelativeItem().Element(content => SignatureLine(content, "Freelancer signature"));
            });
        }

        private static void SignatureLine(IContainer container, string label)
        {
            container.Column(column =>
            {
                column.Item().Height(38);
                column.Item().BorderTop(1).BorderColor(Colors.Grey.Darken1).PaddingTop(6).Text(label).FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        }

        private static IContainer LabelCell(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(8);
        }

        private static IContainer ValueCell(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8);
        }

        private static void HeaderCell(TableDescriptor table, string text)
        {
            table.Cell().Background(Colors.Grey.Lighten4).Padding(8).Text(text).SemiBold();
        }

        private static void Footer(IContainer container)
        {
            container.AlignCenter().DefaultTextStyle(style => style.FontSize(8).FontColor(Colors.Grey.Darken1)).Text(text =>
            {
                text.Span("Generated by FreelanceOS - ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        }

        private static string FormatUser(ApplicationUser? user)
        {
            return user == null ? "Not assigned" : $"{user.FirstName} {user.LastName}\n{user.Email}";
        }

        private static string FormatDate(DateTime date)
        {
            return date.ToString("MMM dd, yyyy");
        }

        private static string FormatMoney(decimal amount)
        {
            return amount.ToString("C");
        }
    }
}

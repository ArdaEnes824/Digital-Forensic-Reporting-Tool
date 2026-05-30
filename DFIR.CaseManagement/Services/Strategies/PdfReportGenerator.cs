using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DFIR.CaseManagement.Services.Strategies;

/// <summary>Concrete Strategy: renders a forensic case report as a PDF using QuestPDF.</summary>
public class PdfReportGenerator : IReportGenerator
{
    public string Format => "pdf";
    public string ContentType => "application/pdf";
    public string FileExtension => "pdf";

    public byte[] Generate(Case c)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Column(col =>
                {
                    col.Item().Text("DFIR CASE REPORT").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text("Case Summary").FontSize(14).Bold();
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(140);
                            c.RelativeColumn();
                        });
                        void Row(string k, string v)
                        {
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(k).Bold();
                            table.Cell().Padding(4).Text(v);
                        }
                        Row("Case Number", c.CaseNumber);
                        Row("Title", c.Title);
                        Row("Status", c.Status.ToString());
                        Row("Priority", c.Priority.ToString());
                        Row("ICAPAIR Stage", c.CurrentStage.ToString());
                        Row("Assigned To", string.IsNullOrWhiteSpace(c.AssignedTo) ? "-" : c.AssignedTo);
                        Row("Created", c.CreatedDate.ToString("yyyy-MM-dd HH:mm"));
                    });

                    col.Item().Text("Description").FontSize(14).Bold();
                    col.Item().Text(string.IsNullOrWhiteSpace(c.Description) ? "(no description)" : c.Description);

                    col.Item().Text($"Evidence Items ({c.EvidenceItems.Count})").FontSize(14).Bold();
                    if (c.EvidenceItems.Count == 0)
                    {
                        col.Item().Text("No evidence registered.").Italic();
                    }
                    else
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cd =>
                            {
                                cd.ConstantColumn(70);
                                cd.RelativeColumn();
                                cd.RelativeColumn();
                                cd.RelativeColumn(2);
                            });
                            void Head(string t) => table.Cell().Background(Colors.Blue.Lighten4).Padding(3).Text(t).Bold();
                            Head("Code"); Head("Type"); Head("Model"); Head("SHA256");
                            foreach (var e in c.EvidenceItems)
                            {
                                table.Cell().Padding(3).Text(e.EvidenceCode);
                                table.Cell().Padding(3).Text(e.DeviceType);
                                table.Cell().Padding(3).Text($"{e.Manufacturer} {e.Model}");
                                table.Cell().Padding(3).Text(e.SHA256Hash ?? "-").FontSize(7);
                            }
                        });
                    }

                    col.Item().Text($"Chain of Custody ({c.CustodyRecords.Count})").FontSize(14).Bold();
                    if (c.CustodyRecords.Count == 0)
                    {
                        col.Item().Text("No custody records.").Italic();
                    }
                    else
                    {
                        foreach (var cc in c.CustodyRecords.OrderBy(x => x.TransferDate))
                        {
                            col.Item().Text($"{cc.TransferDate:yyyy-MM-dd} | {cc.FromPerson} -> {cc.ToPerson} @ {cc.Location}");
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("DFIR Case Management System - Page ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
}

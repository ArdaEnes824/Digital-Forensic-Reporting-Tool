using ClosedXML.Excel;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;

namespace DFIR.CaseManagement.Services.Strategies;

/// <summary>Concrete Strategy: renders a case report as an .xlsx workbook using ClosedXML.</summary>
public class ExcelReportGenerator : IReportGenerator
{
    public string Format => "xlsx";
    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string FileExtension => "xlsx";

    public byte[] Generate(Case c)
    {
        using var workbook = new XLWorkbook();

        BuildSummarySheet(workbook, c);
        BuildEvidenceSheet(workbook, c);
        BuildCustodySheet(workbook, c);

        return Save(workbook);
    }

    /// <summary>Evidence-focused variant: leads with the evidence inventory.</summary>
    public byte[] GenerateEvidenceReport(Case c)
    {
        using var workbook = new XLWorkbook();

        BuildEvidenceSheet(workbook, c);
        BuildCustodySheet(workbook, c);

        return Save(workbook);
    }

    private static void BuildSummarySheet(XLWorkbook wb, Case c)
    {
        var ws = wb.Worksheets.Add("Case Summary");
        ws.Cell("A1").Value = "DFIR Case Report";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 16;

        var rows = new (string Key, string Value)[]
        {
            ("Case Number", c.CaseNumber),
            ("Title", c.Title),
            ("Status", c.Status.ToString()),
            ("Priority", c.Priority.ToString()),
            ("ICAPAIR Stage", c.CurrentStage.ToString()),
            ("Assigned To", string.IsNullOrWhiteSpace(c.AssignedTo) ? "-" : c.AssignedTo),
            ("Created (UTC)", c.CreatedDate.ToString("yyyy-MM-dd HH:mm")),
            ("Description", c.Description)
        };

        var r = 3;
        foreach (var (key, value) in rows)
        {
            ws.Cell(r, 1).Value = key;
            ws.Cell(r, 1).Style.Font.Bold = true;
            ws.Cell(r, 2).Value = value;
            r++;
        }

        ws.Columns().AdjustToContents();
    }

    private static void BuildEvidenceSheet(XLWorkbook wb, Case c)
    {
        var ws = wb.Worksheets.Add("Evidence");
        var headers = new[] { "Code", "Device Type", "Manufacturer", "Model", "Serial Number", "SHA256", "MD5" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        var row = 2;
        foreach (var e in c.EvidenceItems)
        {
            ws.Cell(row, 1).Value = e.EvidenceCode;
            ws.Cell(row, 2).Value = e.DeviceType;
            ws.Cell(row, 3).Value = e.Manufacturer;
            ws.Cell(row, 4).Value = e.Model;
            ws.Cell(row, 5).Value = e.SerialNumber;
            ws.Cell(row, 6).Value = e.SHA256Hash ?? "-";
            ws.Cell(row, 7).Value = e.MD5Hash ?? "-";
            row++;
        }

        ws.Columns().AdjustToContents();
    }

    private static void BuildCustodySheet(XLWorkbook wb, Case c)
    {
        var ws = wb.Worksheets.Add("Chain of Custody");
        var headers = new[] { "Transfer Date", "From", "To", "Location", "Description" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        var row = 2;
        foreach (var cc in c.CustodyRecords.OrderBy(x => x.TransferDate))
        {
            ws.Cell(row, 1).Value = cc.TransferDate.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, 2).Value = cc.FromPerson;
            ws.Cell(row, 3).Value = cc.ToPerson;
            ws.Cell(row, 4).Value = cc.Location;
            ws.Cell(row, 5).Value = cc.Description;
            row++;
        }

        ws.Columns().AdjustToContents();
    }

    private static byte[] Save(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

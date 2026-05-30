using System.Net;
using System.Text;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;

namespace DFIR.CaseManagement.Services.Strategies;

/// <summary>Concrete Strategy: renders the same case report as a standalone HTML file.</summary>
public class HtmlReportGenerator : IReportGenerator
{
    public string Format => "html";
    public string ContentType => "text/html";
    public string FileExtension => "html";

    public byte[] Generate(Case c)
    {
        static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'><title>DFIR Report</title>");
        sb.Append("<style>body{font-family:Arial,sans-serif;margin:40px;color:#222}h1{color:#1a4d8f}");
        sb.Append("table{border-collapse:collapse;width:100%;margin:10px 0}td,th{border:1px solid #ccc;padding:6px;text-align:left}");
        sb.Append("th{background:#eef}</style></head><body>");
        sb.Append($"<h1>DFIR Case Report</h1><p><small>Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</small></p>");

        sb.Append("<h2>Case Summary</h2><table>");
        sb.Append($"<tr><th>Case Number</th><td>{E(c.CaseNumber)}</td></tr>");
        sb.Append($"<tr><th>Title</th><td>{E(c.Title)}</td></tr>");
        sb.Append($"<tr><th>Status</th><td>{c.Status}</td></tr>");
        sb.Append($"<tr><th>Priority</th><td>{c.Priority}</td></tr>");
        sb.Append($"<tr><th>ICAPAIR Stage</th><td>{c.CurrentStage}</td></tr>");
        sb.Append($"<tr><th>Assigned To</th><td>{E(c.AssignedTo)}</td></tr>");
        sb.Append("</table>");

        sb.Append("<h2>Description</h2><p>").Append(E(c.Description)).Append("</p>");

        sb.Append($"<h2>Evidence Items ({c.EvidenceItems.Count})</h2>");
        sb.Append("<table><tr><th>Code</th><th>Type</th><th>Model</th><th>SHA256</th></tr>");
        foreach (var e in c.EvidenceItems)
            sb.Append($"<tr><td>{E(e.EvidenceCode)}</td><td>{E(e.DeviceType)}</td><td>{E(e.Manufacturer)} {E(e.Model)}</td><td><code>{E(e.SHA256Hash)}</code></td></tr>");
        sb.Append("</table>");

        sb.Append($"<h2>Chain of Custody ({c.CustodyRecords.Count})</h2>");
        sb.Append("<table><tr><th>Date</th><th>From</th><th>To</th><th>Location</th></tr>");
        foreach (var cc in c.CustodyRecords.OrderBy(x => x.TransferDate))
            sb.Append($"<tr><td>{cc.TransferDate:yyyy-MM-dd}</td><td>{E(cc.FromPerson)}</td><td>{E(cc.ToPerson)}</td><td>{E(cc.Location)}</td></tr>");
        sb.Append("</table>");

        sb.Append("</body></html>");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}

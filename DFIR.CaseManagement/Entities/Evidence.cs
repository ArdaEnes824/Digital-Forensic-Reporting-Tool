using System.Security.Cryptography;
using System.Text;
using DFIR.CaseManagement.Interfaces;

namespace DFIR.CaseManagement.Entities;

/// <summary>
/// A seized device / artifact. Implements IHashable (interface) so its integrity
/// can be verified cryptographically.
/// </summary>
public class Evidence : BaseEntity, IHashable
{
    public string EvidenceCode { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;

    public string? SHA256Hash { get; set; }
    public string? MD5Hash { get; set; }

    public int CaseId { get; set; }
    public Case? Case { get; set; }

    public void GenerateHashes(byte[] data)
    {
        SHA256Hash = Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
        MD5Hash = Convert.ToHexString(MD5.HashData(data)).ToLowerInvariant();
    }

    public bool VerifyIntegrity(byte[] data)
    {
        if (string.IsNullOrEmpty(SHA256Hash)) return false;
        var current = Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
        return string.Equals(current, SHA256Hash, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Allows hashing a known string (e.g. serial number) when no raw image is present.</summary>
    public void GenerateHashesFromText(string text) => GenerateHashes(Encoding.UTF8.GetBytes(text));

    public override string Describe() => $"Evidence {EvidenceCode} - {Manufacturer} {Model}";
}

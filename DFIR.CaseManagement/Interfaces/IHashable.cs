namespace DFIR.CaseManagement.Interfaces;

/// <summary>
/// Abstraction for entities whose integrity is verified through cryptographic hashes.
/// </summary>
public interface IHashable
{
    string? SHA256Hash { get; set; }
    string? MD5Hash { get; set; }

    void GenerateHashes(byte[] data);
    bool VerifyIntegrity(byte[] data);
}

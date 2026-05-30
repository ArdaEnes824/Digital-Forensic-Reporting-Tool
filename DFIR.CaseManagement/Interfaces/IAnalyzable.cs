namespace DFIR.CaseManagement.Interfaces;

/// <summary>
/// Abstraction for any artifact that can be put through a forensic analysis pass.
/// </summary>
public interface IAnalyzable
{
    string GetArtifactName();
    void RunAnalysis(byte[] data);
    double GetRiskScore();
}

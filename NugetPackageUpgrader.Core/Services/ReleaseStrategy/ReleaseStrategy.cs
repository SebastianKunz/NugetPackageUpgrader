namespace NugetPackageUpgrader.Core.Services.ReleaseStrategy;

public enum ReleaseStrategy
{
    /// <summary>
    /// Upgrades prerelease version
    /// </summary>
    Prerelease,
    
    ReleaseMajor,
    ReleaseMinor,
    ReleasePatch
}
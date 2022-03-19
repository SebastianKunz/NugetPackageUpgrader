using NugetPackageUpgrader.Core.Model;

namespace NugetPackageUpgrader.Core.Services.ReleaseStrategy;

public class PrereleaseUpgraderStrategy : IReleaseStrategy
{
    public string PrereleaseString { get; }

    public PrereleaseUpgraderStrategy(string prereleaseString = "beta")
    {
        PrereleaseString = prereleaseString;
    }
    
    public SolutionProjectVersion Upgrade(SolutionProjectVersion version)
    {
        if (string.IsNullOrEmpty(version.PrereleaseString))
        {
            version.PrereleaseString = PrereleaseString;
            version.PrereleaseNumber = 1;
        }
        else
        {
            version.PrereleaseNumber++;
        }

        return version;
    }
}
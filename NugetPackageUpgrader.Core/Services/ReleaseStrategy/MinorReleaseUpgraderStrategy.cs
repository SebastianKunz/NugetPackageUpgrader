using NugetPackageUpgrader.Core.Model;

namespace NugetPackageUpgrader.Core.Services.ReleaseStrategy;

public class MinorReleaseUpgraderStrategy : IReleaseStrategy
{
    public SolutionProjectVersion Upgrade(SolutionProjectVersion version)
    {
        version.Main.Minor++;
        version.Main.Patch = 0;
        version.PrereleaseString = null;
        version.PrereleaseNumber = null;

        return version;
    }
}
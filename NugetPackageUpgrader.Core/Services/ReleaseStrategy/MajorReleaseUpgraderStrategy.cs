using NugetPackageUpgrader.Core.Model;

namespace NugetPackageUpgrader.Core.Services.ReleaseStrategy;

public class MajorReleaseUpgraderStrategy : IReleaseStrategy
{
    public SolutionProjectVersion Upgrade(SolutionProjectVersion version)
    {
        version.Main.Major++;
        version.Main.Minor = 0;
        version.Main.Patch = 0;
        version.PrereleaseString = null;
        version.PrereleaseNumber = null;

        return version;
    }
}
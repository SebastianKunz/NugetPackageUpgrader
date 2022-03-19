using NugetPackageUpgrader.Core.Model;

namespace NugetPackageUpgrader.Core.Services.ReleaseStrategy;

public class PatchReleaseUpgraderStrategy : IReleaseStrategy
{
    public SolutionProjectVersion Upgrade(SolutionProjectVersion version)
    {
        version.Main.Patch++;
        version.PrereleaseString = null;
        version.PrereleaseNumber = null;

        return version;
    }
}
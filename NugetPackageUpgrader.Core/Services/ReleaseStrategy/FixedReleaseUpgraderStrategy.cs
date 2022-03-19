using NugetPackageUpgrader.Core.Model;

namespace NugetPackageUpgrader.Core.Services.ReleaseStrategy;

public class FixedReleaseUpgraderStrategy : IReleaseStrategy
{
    private readonly SolutionProjectVersion _version;

    public FixedReleaseUpgraderStrategy(SolutionProjectVersion version)
    {
        _version = version;
    }
    public SolutionProjectVersion Upgrade(SolutionProjectVersion version)
    {
        return _version;
    }
}
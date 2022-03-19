namespace NugetPackageUpgrader.Core.Services.ReleaseStrategy;

public class ReleaseStrategyFactory
{
    public IReleaseStrategy CreateStrategy(ReleaseStrategy strategy)
    {
        switch (strategy)
        {
            case ReleaseStrategy.Prerelease:
                return new PrereleaseUpgraderStrategy();
            case ReleaseStrategy.ReleaseMajor:
                return new MajorReleaseUpgraderStrategy();
            case ReleaseStrategy.ReleaseMinor:
                return new MinorReleaseUpgraderStrategy();
            case ReleaseStrategy.ReleasePatch:
                return new PatchReleaseUpgraderStrategy();
            default:
                return new PrereleaseUpgraderStrategy();
        }
    }
}
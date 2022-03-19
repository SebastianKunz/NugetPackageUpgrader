using NugetPackageUpgrader.Core.Model;

namespace NugetPackageUpgrader.Core.Services.ReleaseStrategy;

public interface IReleaseStrategy
{
   SolutionProjectVersion Upgrade(SolutionProjectVersion version);
}
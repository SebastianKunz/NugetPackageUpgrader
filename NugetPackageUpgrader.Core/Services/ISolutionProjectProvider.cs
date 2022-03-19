using NugetPackageUpgrader.Core.Model;

namespace NugetPackageUpgrader.Core.Services;

public interface ISolutionProjectProvider
{
    Task<SolutionProjectResult> GetSolutionInformation(string solutionFile);
}

public class SolutionProjectResult
{
   public ISet<SolutionProject> Projects { get; set; }
}
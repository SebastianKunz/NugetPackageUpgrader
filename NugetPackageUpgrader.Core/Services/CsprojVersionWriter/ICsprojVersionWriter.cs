using NugetPackageUpgrader.Core.Model;

namespace NugetPackageUpgrader.Core.Services.CsprojVersionWriter;

public interface ICsprojVersionWriter
{
    Task<bool> WriteVersion(SolutionProject project);
    
    Task<bool> WritePackageReferenceVersion(SolutionProject project, SolutionProject dependency);
}
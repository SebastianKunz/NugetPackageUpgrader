using NugetPackageUpgrader.Core.Model;

namespace NugetPackageUpgrader.Core.Services.Parser.VersionParser;

public interface IVersionParser
{
   SolutionProjectVersion ParseVersion(string versionStr, char prereleaseDelimiter = '-');

}
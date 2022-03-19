using NugetPackageUpgrader.Core.Model;

namespace NugetPackageUpgrader.Core.Services.Parser.VersionParser;

public class VersionParser : IVersionParser
{
   private MajorMinorPatchVersion ParseMajorMinorPatch(string majorMinorPatchStr)
   {
      var majorMinorPatch = majorMinorPatchStr.Split('.');

      if (majorMinorPatch.Length == 0 || majorMinorPatch.Length > 3)
      {
         throw new ArgumentException($"Unable to parse MajorMinorPatch part of version string [{majorMinorPatchStr}]");
      }

      var major = int.Parse(majorMinorPatch[0]);
      int minor = 0;
      if (majorMinorPatch.Length >= 2)
      {
         minor = int.Parse(majorMinorPatch[1]);
      }
      int patch = 0;
      if (majorMinorPatch.Length >= 3)
      {
         patch = int.Parse(majorMinorPatch[2]);
      }

      return new MajorMinorPatchVersion(major, minor, patch);
   }
   
   public SolutionProjectVersion ParseVersion(string versionStr, char prereleaseDelimiter = '-')
   {
      var segments = versionStr.Split(prereleaseDelimiter);

      if (segments.Length > 2 || segments.Length == 0)
      {
         throw new ArgumentException($"Unable to parse version string [{versionStr}]");
      }

      var majorMinorPatch = ParseMajorMinorPatch(segments[0]);

      if (segments.Length != 2)
      {
         return new SolutionProjectVersion(majorMinorPatch);
      }
      
      var input = segments[1];
      var numberStr = new string(input.SkipWhile(c=> !char.IsDigit(c))
         .TakeWhile(char.IsDigit)
         .ToArray());

      var prereleaseIndicator = new string(input.TakeWhile(char.IsLetter).ToArray());

      var preVersionNumber = int.Parse(numberStr);

      return new SolutionProjectVersion(majorMinorPatch, prereleaseIndicator, preVersionNumber);
   }
}
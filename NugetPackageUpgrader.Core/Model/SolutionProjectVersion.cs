namespace NugetPackageUpgrader.Core.Model;

public class SolutionProjectVersion
{
   public MajorMinorPatchVersion Main { get; }
   public string? PrereleaseString { get; set; }
   
   public int? PrereleaseNumber {get; set; }

   public SolutionProjectVersion(MajorMinorPatchVersion main, string? prereleaseString = null, int? prereleaseNumber = null)
   {
      Main = main;
      PrereleaseString = prereleaseString;
      PrereleaseNumber = prereleaseNumber;
   }

   public SolutionProjectVersion(int major, int minor, int patch, string? prereleaseString = null, int? prereleaseNumber = null)
      : this(new MajorMinorPatchVersion(major, minor, patch), prereleaseString, prereleaseNumber)
   {
   }

   public override string ToString()
   {
      if (PrereleaseString is null)
      {
         return Main.ToString();
      }

      return Main + "-" + PrereleaseString + PrereleaseNumber;
   }
}
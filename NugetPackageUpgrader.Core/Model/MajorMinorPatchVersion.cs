namespace NugetPackageUpgrader.Core.Model;

public class MajorMinorPatchVersion
{
    public int Major { get; set; } 
   
    public int Minor { get; set; }
   
    public int Patch { get; set; }

    public MajorMinorPatchVersion()
    {
        
    }

    public MajorMinorPatchVersion(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    public override string ToString()
    {
        return Major + "." + Minor + "." + Patch;
    }
}
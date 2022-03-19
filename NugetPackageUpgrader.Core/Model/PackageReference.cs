namespace NugetPackageUpgrader.Core.Model;

public class PackageReference
{
   public string Title { get; } 
   
   public SolutionProjectVersion Version { get; }

   public PackageReference(string title, SolutionProjectVersion version)
   {
       Title = title;
       Version = version;
   }
   
    public override int GetHashCode()
    {
        return Title.GetHashCode() + Version.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        var item = obj as PackageReference;
    
        if (item == null)
        {
            return false;
        }

        return item.Title == Title && item.Version == Version;
    }
}



namespace NugetPackageUpgrader.Core.Model;

public class SolutionProject
{
    public string Title { get; }
    public string Path { get; }
    public SolutionProjectVersion Version { get; set; }
    
    public ISet<PackageReference> PackageReferences { get; }
    
    public HashSet<SolutionProject> IsDependencyOf { get; set; }
    
    public SolutionProject(string title, string path, SolutionProjectVersion version, 
        ISet<PackageReference>? packageReferences = null,
        HashSet<SolutionProject>? isDependencyOf = null)
    {
        Title = title;
        Path = path;
        Version = version;
        IsDependencyOf = isDependencyOf ?? new HashSet<SolutionProject>();
        PackageReferences = packageReferences ?? new HashSet<PackageReference>();
    }

    public override string ToString()
    {
        return Title + "/" + Version;
    }

    public override int GetHashCode()
    {
        return Title.GetHashCode();
    }
}
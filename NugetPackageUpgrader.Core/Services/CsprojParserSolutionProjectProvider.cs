using NugetPackageUpgrader.Core.Model;
using NugetPackageUpgrader.Core.Services.Parser.VersionParser;
using System.Text.RegularExpressions;
using System.Xml;

namespace NugetPackageUpgrader.Core.Services;

public class CsprojParserSolutionProjectProvider : ISolutionProjectProvider
{
    private readonly IVersionParser _versionParser;
    const string VersionXpath = "Project/PropertyGroup/Version";
    const string ReferenceVersionXpath = "Project/ItemGroup/PackageReference";

    public CsprojParserSolutionProjectProvider(IVersionParser versionParser)
    {
        _versionParser = versionParser;
    }
    private IList<string> GetProjectsOfSolution(string solutionFile)
    {
        var content = File.ReadAllText(solutionFile);
        Regex projReg = new Regex(
            "Project\\(\"\\{[\\w-]*\\}\"\\) = \"([\\w _]*.*)\", \"(.*\\.csproj)\""
            , RegexOptions.Compiled);
        var matches = projReg.Matches(content).Cast<Match>();
        var projects = matches.Select(x => x.Groups[2].Value).ToList();
        for (int i = 0; i < projects.Count; ++i)
        {
            if (!Path.IsPathRooted(projects[i]))
                projects[i] = Path.Combine(Path.GetDirectoryName(solutionFile),
                    projects[i]);
            projects[i] = Path.GetFullPath(projects[i]);
        }

        return projects;
    }

    private SolutionProject ParseProjectXml(string projPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(projPath);

        var versionNode = doc.SelectSingleNode(VersionXpath);
        var packageReferences = doc.SelectNodes(ReferenceVersionXpath);
        var list = new HashSet<PackageReference>();
        for (int i = 0; i < packageReferences.Count; i++)
        {
            var title = packageReferences[i].Attributes["Include"].Value;
            var versionStr= packageReferences[i].Attributes["Version"].Value;
            var packageVersion = _versionParser.ParseVersion(versionStr);
            list.Add(new PackageReference(title, packageVersion));
        }

        SolutionProjectVersion version;
        if (versionNode is not null)
        {
            version = _versionParser.ParseVersion(versionNode.InnerText);
        }
        else
        {
            version = new SolutionProjectVersion(new MajorMinorPatchVersion(1, 0, 0));
        }

        var filename = Path.GetFileName(projPath);
        var projTitle = Path.ChangeExtension(filename, null);
        return new SolutionProject(projTitle, projPath, version, list);
    }
    
    public Task<SolutionProjectResult> GetSolutionInformation(string solutionFile)
    {
        var paths = GetProjectsOfSolution(solutionFile);
        var projects = new HashSet<SolutionProject>();

        foreach (var path in paths)
        {
           projects.Add(ParseProjectXml(path)); 
        }
        
        foreach (var project in projects)
        {
            var set = projects
                .Where(x => x.PackageReferences.FirstOrDefault(y => y.Title == project.Title) != null).ToHashSet();
            project.IsDependencyOf = set;
        }
        
        return Task.FromResult(new SolutionProjectResult()
        {
            Projects = projects
        });
    }
}
using Microsoft.Extensions.Logging;
using NugetPackageUpgrader.Core.Model;
using System.Xml;

namespace NugetPackageUpgrader.Core.Services.CsprojVersionWriter;

public class XmlCsprojVersionWriter : ICsprojVersionWriter
{
    private readonly ILogger<XmlCsprojVersionWriter> _logger;
    const string VersionXpath = "Project/PropertyGroup/Version";
    const string ReferenceVersionXpath = "Project/ItemGroup/PackageReference";

    public XmlCsprojVersionWriter(ILogger<XmlCsprojVersionWriter> logger)
    {
        _logger = logger;
    }
    
    public Task<bool> WriteVersion(SolutionProject project)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(project.Path);

        XmlNode? node = doc.SelectSingleNode(VersionXpath);
        if (node is null)
        {
            _logger.LogError($"Unable to upgrade {project.Title} Version to {project.Version}, because the Version attribute is missing in .csproj.");
            return Task.FromResult(false);
        }
        node.InnerText = project.Version.ToString();
        _logger.LogInformation($"Set Version of {project.Title} from {node.InnerText} to {project.Version}");

        doc.Save(project.Path);
        _logger.LogTrace($"Saved {project} to {project.Path}");
        return Task.FromResult(true);
    }

    public Task<bool> WritePackageReferenceVersion(SolutionProject project, SolutionProject dependency)
    {
        var doc = new XmlDocument();
        doc.Load(project.Path);

        XmlNodeList? nodes = doc.SelectNodes(ReferenceVersionXpath);
        if (nodes is null)
        {
            _logger.LogWarning($"Did not find any package references for project {project}");
            return Task.FromResult(true);
        }
        XmlNode? node = null;
        for (int i = 0; i < nodes.Count; i++)
        {
            node = nodes.Item(i);
            if (node?.Attributes?["Include"]?.Value == dependency.Title)
                break;
        }

        if (node is null)
        {
            _logger.LogError($"Unable to find a reference to {dependency} in project {project}");
            return Task.FromResult(false);
        }

        var versionAttr= node.Attributes?["Version"];
        if (versionAttr is null)
        {
            _logger.LogError($"Unable to upgrade {project.Title} Version to {project.Version}, because the Version attribute is missing in .csproj.");
            return Task.FromResult(false);
        }
        _logger.LogInformation($"On Project {project.Title} set Version of Dep {dependency.Title} from {versionAttr.Value} to {dependency.Version}");
        versionAttr.Value = dependency.Version.ToString();
        doc.Save(project.Path);

        _logger.LogTrace($"Saved {project} to {project.Path}");

        return Task.FromResult(true);
    }
}
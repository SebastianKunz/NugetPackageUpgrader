using Microsoft.Extensions.Logging;
using NugetPackageUpgrader.Core.Model;
using NugetPackageUpgrader.Core.Services.CsprojVersionWriter;
using NugetPackageUpgrader.Core.Services.DotnetRunner;
using NugetPackageUpgrader.Core.Services.ReleaseStrategy;

namespace NugetPackageUpgrader.Core.Services;

public class TransitiveProjectUpgrader
{
    private readonly ILogger<TransitiveProjectUpgrader> _logger;
    private readonly ResolverConfiguration _configuration;
    private readonly ICsprojVersionWriter _writer;
    private readonly IReleaseStrategy _depReleaseStrategy;
    private readonly IReleaseStrategy _parentReleaseStrategy;
    private readonly IDotnetCliRunner _dotnetCliRunner;

    public TransitiveProjectUpgrader(ILogger<TransitiveProjectUpgrader> logger,
        ResolverConfiguration configuration, ICsprojVersionWriter writer, IReleaseStrategy depReleaseStrategy,
        IReleaseStrategy parentReleaseStrategy,
        IDotnetCliRunner dotnetCliRunner)
    {
        _logger = logger;
        _configuration = configuration;
        _writer = writer;
        _depReleaseStrategy = depReleaseStrategy;
        _parentReleaseStrategy = parentReleaseStrategy;
        _dotnetCliRunner = dotnetCliRunner;
    }
    
    public async Task Upgrade(SolutionProject toUpgrade)
    {
        _logger.LogInformation($"Starting to upgrade {toUpgrade}");

        toUpgrade.Version = _depReleaseStrategy.Upgrade(toUpgrade.Version);

        await PerformRecursiveUpgrade(toUpgrade, false);
    }

    private async Task PerformRecursiveUpgrade(SolutionProject toUpgrade, bool upgradeVersion)
    {
        if (upgradeVersion)
        {
            toUpgrade.Version = _parentReleaseStrategy.Upgrade(toUpgrade.Version);
        }
        await _writer.WriteVersion(toUpgrade);
        if (await BuildPackage(toUpgrade) == false)
        {
            _logger.LogError($"Failed to build {toUpgrade}. Stopping...");
            return;
        }

        if (await PackPackage(toUpgrade) == false)
        {
            _logger.LogError($"Failed to pack {toUpgrade}. Stopping...");
            return;
        }
        
        foreach (var project in toUpgrade.IsDependencyOf)
        {
            await _writer.WritePackageReferenceVersion(project, toUpgrade);
            await PerformRecursiveUpgrade(project, true);
        }
        
    }

    private async Task<bool> BuildPackage(SolutionProject project)
    {
        return await _dotnetCliRunner.BuildAsync(project.Path);
    }
    
    private async Task<bool> PackPackage(SolutionProject project)
    {
        string args;
        if (_configuration.NugetOutput is null)
        {
            args = project.Path;
        }
        else
        {
            args = "-o " + _configuration.NugetOutput + " " + project.Path;
        }

        return await _dotnetCliRunner.PackAsync(args);
    }
}
using Microsoft.Extensions.Logging;
using NugetPackageUpgrader.Core.Model;
using System.Diagnostics;

namespace NugetPackageUpgrader.Core.Services.DotnetRunner;

public class DotnetCliRunnner : IDotnetCliRunner
{
    private readonly ILogger<DotnetCliRunnner> _logger;

    public DotnetCliRunnner(ILogger<DotnetCliRunnner> logger)
    {
        _logger = logger;
    }
    public async Task<bool> BuildAsync(string args)
    {
        Process? build = Process.Start(new ProcessStartInfo()
        {
            FileName = "dotnet",
            Arguments = "build " + args
        });
        if (build is null)
        {
            return false;
        }
        await build.WaitForExitAsync();
        return build.ExitCode == 0;
    }
    
    public async Task<bool> PackAsync(string args)
    {
        _logger.LogDebug("Starting Process. dotnet " + args);
        Process? pack = Process.Start(new ProcessStartInfo()
        {
            FileName = "dotnet",
            Arguments = "pack " + args,
            // RedirectStandardOutput = !_configuration.PrintProcess
        });
        if (pack is null)
        {
            return false;
        }
        await pack.WaitForExitAsync();
        return pack.ExitCode == 0;
    }
}
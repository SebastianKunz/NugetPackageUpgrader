namespace NugetPackageUpgrader.Core.Services.DotnetRunner;

public interface IDotnetCliRunner
{
   Task<bool> BuildAsync(string args);
   
   Task<bool> PackAsync(string args);
}
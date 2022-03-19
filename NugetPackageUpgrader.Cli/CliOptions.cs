using CommandLine;
using NugetPackageUpgrader.Core.Services.ReleaseStrategy;
using System.Runtime.CompilerServices;

namespace NugetPackageUpgrader.Cli;

public class CliOptions
{
    [Option('o', "output", Required = false, HelpText = "The directory, where generated nuget packages are copied to.")]
    public string? NugetOutput { get; set; } 
    
    [Option('s', "sln-path", Required = false, HelpText = "The solution, that contain the packages that should be upgraded.")]
    public string? SolutionFile { get; set; }
    
    [Option('p', "project", Required = true, HelpText = "The project, that should be upgraded.")]
    public string ProjectName { get; set; }
    
    [Option('r', "release-strategy", Required = false, HelpText = "Choose the release strategy.")]
    public ReleaseStrategy ReleaseStrategy { get; set; }
    
    [Option('c', "set-version", Required = false, HelpText = "Set a custom version.")]
    public string? SetVersion { get; set; }
    
    [Option('d', "sln-directory", Required = false, HelpText = "The directory to look for a solution file. Defaults to the directory of execution.")]
    public string? SolutionDirectory { get; set; }
}
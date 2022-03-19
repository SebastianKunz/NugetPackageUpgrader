using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NugetPackageUpgrader.Core.Model;
using NugetPackageUpgrader.Core.Services;
using NugetPackageUpgrader.Core.Services.CsprojVersionWriter;
using NugetPackageUpgrader.Core.Services.DotnetRunner;
using NugetPackageUpgrader.Core.Services.Parser.VersionParser;
using NugetPackageUpgrader.Core.Services.ReleaseStrategy;

namespace NugetPackageUpgrader.Cli;

static class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        var options = host.Services.GetService<CliOptions>();
        if (options is null)
        {
            await host.StopAsync();
            return;
        }
        
        var upgrader = host.Services.GetRequiredService<TransitiveProjectUpgrader>();
        var provider = host.Services.GetRequiredService<ISolutionProjectProvider>();
        var slnDir = options.SolutionDirectory ?? Directory.GetCurrentDirectory();
        string slnPath;
        if (options.SolutionFile is not null)
        {
            slnPath = options.SolutionFile;
        }
        else
        {
            var slnPaths = Directory.GetFiles(slnDir, "*.sln", SearchOption.TopDirectoryOnly);
            slnPath = slnPaths.First();
        }
            
        SolutionProjectResult result = await provider.GetSolutionInformation(slnPath);

        await upgrader.Upgrade(result.Projects.First(x => x.Title == options.ProjectName));
    }

    static IHostBuilder CreateHostBuilder(string[] args)
    {
        CliOptions? options = null;
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                var versionParser = new VersionParser();
                services.AddSingleton<IDotnetCliRunner, DotnetCliRunnner>();
                services.AddSingleton<IVersionParser>(versionParser);
                services.AddSingleton<ISolutionProjectProvider, CsprojParserSolutionProjectProvider>();
                services.AddSingleton<ICsprojVersionWriter, XmlCsprojVersionWriter>();
                var parser = new Parser(with =>
                {
                    with.HelpWriter = null;
                });
                var result = parser.ParseArguments<CliOptions>(args)
                    .WithParsed(o =>
                    {
                        options = o;
                        services.AddSingleton(o);
                        services.AddSingleton(new ResolverConfiguration()
                        {
                            NugetOutput = o.NugetOutput,
                        });
                    });
                result.WithNotParsed(errs => DisplayHelp(result));
                services.AddSingleton<TransitiveProjectUpgrader>(x =>
                {
                    var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                    var config = x.GetRequiredService<ResolverConfiguration>();
                    var writer = x.GetRequiredService<ICsprojVersionWriter>();
                    IReleaseStrategy dependencyStrategy = GetDependencyStrategy(options, versionParser);
                    IReleaseStrategy parentStrategy = GetParentReleaseStrategy(options);
                    var cliRunner = x.GetRequiredService<IDotnetCliRunner>();
                    return new TransitiveProjectUpgrader(loggerFactory.CreateLogger<TransitiveProjectUpgrader>(),
                        config, writer,
                        dependencyStrategy, parentStrategy, cliRunner);
                });
            })
            .ConfigureLogging((_, logging) =>
            {
                logging.ClearProviders();
                logging.AddCleanConsole();
            });
    }

    private static IReleaseStrategy GetDependencyStrategy(CliOptions o, IVersionParser versionParser)
    {
        IReleaseStrategy releaseStrategy;
        if (o.SetVersion is not null)
        {
            SolutionProjectVersion v = versionParser.ParseVersion(o.SetVersion);
            releaseStrategy = new FixedReleaseUpgraderStrategy(v);
        }
        else
        {
            var releaseFactory = new ReleaseStrategyFactory();
            releaseStrategy = releaseFactory.CreateStrategy(o.ReleaseStrategy);
        }

        return releaseStrategy;
    }
    
    private static IReleaseStrategy GetParentReleaseStrategy(CliOptions o)
    {
        IReleaseStrategy releaseStrategy;
        var releaseFactory = new ReleaseStrategyFactory();
        releaseStrategy = releaseFactory.CreateStrategy(o.ReleaseStrategy);

        return releaseStrategy;
    }
                
    static void DisplayHelp<T>(ParserResult<T> result)
    {  
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AddEnumValuesToHelpText = true;
            return h;
        });
        Console.WriteLine(helpText);
    }
}
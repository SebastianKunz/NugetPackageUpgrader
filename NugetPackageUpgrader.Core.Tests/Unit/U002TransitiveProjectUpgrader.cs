using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NugetPackageUpgrader.Core.Model;
using NugetPackageUpgrader.Core.Services;
using NugetPackageUpgrader.Core.Services.CsprojVersionWriter;
using NugetPackageUpgrader.Core.Services.DotnetRunner;
using NugetPackageUpgrader.Core.Services.ReleaseStrategy;
using NUnit.Framework;

namespace NugetPackageUpgrader.Core.Tests.Unit;

public class U002TransitiveProjectUpgrader
{
    [SetUp]
    public void SetUp()
    {
       _trueWriterMock = new Mock<ICsprojVersionWriter>();
       _trueWriterMock.Setup(x => x.WriteVersion(It.IsAny<SolutionProject>())).ReturnsAsync(true);
       _trueWriterMock.Setup(x => x.WritePackageReferenceVersion(It.IsAny<SolutionProject>(), It.IsAny<SolutionProject>())).ReturnsAsync(true);
       
       _trueCliRunnerMock = new Mock<IDotnetCliRunner>();
       _trueCliRunnerMock.Setup(x => x.BuildAsync(It.IsAny<string>())).ReturnsAsync(true);
       _trueCliRunnerMock.Setup(x => x.PackAsync(It.IsAny<string>())).ReturnsAsync(true);
       _anyStratMock = new Mock<IReleaseStrategy>();
       _anyStratMock.Setup(x => x.Upgrade(It.IsAny<SolutionProjectVersion>()))
           .Returns(() => It.IsAny<SolutionProjectVersion>());
    }
    
    public void SetupUpgrader(ResolverConfiguration config, Mock<IReleaseStrategy>? strat = null)
    {
        strat ??= _anyStratMock;
       _upgrader =
           new TransitiveProjectUpgrader(NullLogger<TransitiveProjectUpgrader>.Instance, config, _trueWriterMock.Object,
               strat.Object, strat.Object, _trueCliRunnerMock.Object);
    }

    private Mock<ICsprojVersionWriter> _trueWriterMock;
    private Mock<IDotnetCliRunner> _trueCliRunnerMock;
    private TransitiveProjectUpgrader _upgrader;
    private Mock<IReleaseStrategy> _anyStratMock;

    public static SolutionProject AnySolutionProject = It.IsAny<SolutionProject>();

    [Test]
   public void U002_001UpgradeWithoutDependencies()
   {
       var project = new SolutionProject("MyTitle", "Path", new SolutionProjectVersion(1, 0, 0));
       var config = new ResolverConfiguration();

       var strat = new Mock<IReleaseStrategy>();
       var upgradeVersion = new PrereleaseUpgraderStrategy().Upgrade(project.Version);
       strat.Setup(x => x.Upgrade(project.Version))
           .Returns(() => upgradeVersion);
       SetupUpgrader(config, strat);

       _upgrader.Upgrade(project);

       project.Version.Should().BeEquivalentTo(upgradeVersion);
       
       strat.Verify(x => x.Upgrade(upgradeVersion), Times.Once);
       _trueWriterMock.Verify(x => x.WriteVersion(project));
       _trueCliRunnerMock.Verify(x => x.BuildAsync(project.Path), Times.Once);
       _trueCliRunnerMock.Verify(x => x.PackAsync(project.Path), Times.Once);
       _trueWriterMock.Verify(x => x.WritePackageReferenceVersion(It.IsAny<SolutionProject>(), It.IsAny<SolutionProject>()), Times.Never);
   }

   [Test]
   public void U002_002UpgradeOutputToNugetSource()
   {
       var project = new SolutionProject("Project", "ProjectPath", new SolutionProjectVersion(1, 0, 0));
       var config = new ResolverConfiguration()
       {
           NugetOutput = "MyOutput"
       };
       
       SetupUpgrader(config);
       _upgrader.Upgrade(project);
       
       _trueWriterMock.Verify(x => x.WriteVersion(project));
       _trueCliRunnerMock.Verify(x => x.BuildAsync(project.Path), Times.Once);
       _trueCliRunnerMock.Verify(x => x.PackAsync("-o " + config.NugetOutput + " " + project.Path), Times.Once);
       _trueWriterMock.Verify(x => x.WritePackageReferenceVersion(It.IsAny<SolutionProject>(), It.IsAny<SolutionProject>()), Times.Never);
   }

   [Test]
   public void U002_003UpgradeDependencyAndParent()
   {
       var project = new SolutionProject("Project", "ProjectPath", new SolutionProjectVersion(1, 0, 0));
       var dependency = new SolutionProject("Dependency", "DepPath", new SolutionProjectVersion(0, 0, 1, "beta", 3));
       dependency.IsDependencyOf.Add(project);
       var config = new ResolverConfiguration();
       
       SetupUpgrader(config);
       _upgrader.Upgrade(dependency);
       
       _trueWriterMock.Verify(x => x.WriteVersion(dependency));
       
       _trueCliRunnerMock.Verify(x => x.BuildAsync(dependency.Path), Times.Once);
       _trueCliRunnerMock.Verify(x => x.PackAsync(dependency.Path), Times.Once);
       
       _trueWriterMock.Verify(x => x.WritePackageReferenceVersion(project, dependency), Times.Once);
       
       _trueWriterMock.Verify(x => x.WriteVersion(project));
       
       _trueCliRunnerMock.Verify(x => x.BuildAsync(project.Path), Times.Once);
       _trueCliRunnerMock.Verify(x => x.PackAsync(project.Path), Times.Once);
       
       _trueWriterMock.Verify(x => x.WritePackageReferenceVersion(AnySolutionProject, AnySolutionProject), Times.Never);
   }
}
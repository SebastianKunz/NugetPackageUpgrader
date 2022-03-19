using FluentAssertions;
using NugetPackageUpgrader.Core.Model;
using NugetPackageUpgrader.Core.Services.Parser.VersionParser;
using NUnit.Framework;

namespace NugetPackageUpgrader.Core.Tests.Unit;

public class U001VersionParser
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    [TestCase("1.0.0", 1, 0, 0, null, null)]
    [TestCase("1.2.3", 1, 2, 3, null, null)]
    [TestCase("0.0.0", 0, 0, 0, null, null)]
    [TestCase("1.0", 1, 0, 0, null, null)]
    [TestCase("1", 1, 0, 0, null, null)]
    [TestCase("1.5", 1, 5, 0, null, null)]
    [TestCase("1.5-beta2", 1, 5, 0, "beta", 2)]
    [TestCase("1.5-abc1337", 1, 5, 0, "abc", 1337)]
    [TestCase("1.5.1-beta2", 1, 5, 1, "beta", 2)]
    [TestCase("1-a24", 1, 0, 0, "a", 24)]
    [Parallelizable(ParallelScope.All)]
    public void U001_001ParseMajorMinorPatch(string versionStr, int major, int minor, int patch, string? prerelease, int? preNbr)
    {
        var parser = new VersionParser();
        var res = parser.ParseVersion(versionStr);

        res.Main.Should().BeEquivalentTo(new MajorMinorPatchVersion(major, minor, patch));
        res.PrereleaseString.Should().Be(prerelease);
        res.PrereleaseNumber.Should().Be(preNbr);
    }
}
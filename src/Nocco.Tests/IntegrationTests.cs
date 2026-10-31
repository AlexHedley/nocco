using Xunit;

namespace Nocco.Tests;

[Collection("cwd")]
public class GenerateIntegrationTests : IDisposable
{
    private readonly string _orig = Directory.GetCurrentDirectory();
    private readonly string _tmp = Directory.CreateTempSubdirectory().FullName;

    public GenerateIntegrationTests() => Directory.SetCurrentDirectory(_tmp);

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_orig);
        Directory.Delete(_tmp, true);
    }

    [Fact]
    public void GeneratesHtmlAndAssets()
    {
        File.WriteAllText("Sample.cs", "// Says **hi**\nConsole.WriteLine(\"hi\");\n");

        Nocco.Generate(new[] { "*.cs" });

        Assert.True(File.Exists(Path.Combine("docs", "nocco.css")));
        Assert.True(File.Exists(Path.Combine("docs", "prettify.js")));
        var html = File.ReadAllText(Path.Combine("docs", "sample.html"));
        Assert.Contains("<strong>hi</strong>", html);
        Assert.Contains("Console.WriteLine", html);
        Assert.Contains("Sample.cs", html);
    }

    [Fact]
    public void NoTargetsGeneratesNothing()
    {
        Nocco.Generate(Array.Empty<string>());
        Assert.False(Directory.Exists("docs"));
    }

    [Fact]
    public void UnsupportedAndIgnoredFilesAreSkipped()
    {
        File.WriteAllText("notes.txt", "// nope");
        File.WriteAllText("app.min.js", "// nope");

        Nocco.Generate(new[] { "*.*" });

        Assert.False(File.Exists(Path.Combine("docs", "notes.html")));
        Assert.False(File.Exists(Path.Combine("docs", "app.min.html")));
    }
}

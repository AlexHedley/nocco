using Xunit;

namespace Nocco.Tests;

public class ParseTests
{
    [Fact]
    public void SplitsCommentsAndCode()
    {
        var lines = new[] { "// Hello", "var x = 1;", "// Next", "var y = 2;" };
        var sections = Nocco.Parse("a.cs", lines);

        Assert.Equal(2, sections.Count);
        Assert.Contains("Hello", sections[0].DocsHtml);
        Assert.Contains("var x = 1;", sections[0].CodeHtml);
        Assert.Contains("Next", sections[1].DocsHtml);
        Assert.Contains("var y = 2;", sections[1].CodeHtml);
    }

    [Fact]
    public void ConsecutiveCommentsAreOneSection()
    {
        var sections = Nocco.Parse("a.cs", new[] { "// one", "// two", "code();" });
        Assert.Single(sections);
        Assert.Contains("one", sections[0].DocsHtml);
        Assert.Contains("two", sections[0].DocsHtml);
    }

    [Fact]
    public void CodeOnlyFileHasEmptyDocs()
    {
        var sections = Nocco.Parse("a.cs", new[] { "code();" });
        Assert.Single(sections);
        Assert.Equal("", sections[0].DocsHtml);
        Assert.Contains("code();", sections[0].CodeHtml);
    }

    [Fact]
    public void SqlUsesDoubleDashComments()
    {
        var sections = Nocco.Parse("q.sql", new[] { "-- note", "SELECT 1;" });
        Assert.Contains("note", sections[0].DocsHtml);
        Assert.Contains("SELECT 1;", sections[0].CodeHtml);
    }

    [Fact]
    public void CSharpXmlDocIsMappedToMarkdown()
    {
        var sections = Nocco.Parse("a.cs", new[]
        {
            "/// <summary>Does <c>x</c></summary>",
            "/// <param name=\"p\">the p</param>",
            "void F(int p) {}"
        });
        var docs = sections[0].DocsHtml;
        Assert.Contains("`x`", docs);
        Assert.Contains("**argument** *p*: the p", docs);
        Assert.DoesNotContain("<summary>", docs);
    }

    [Fact]
    public void ShebangLineIsTreatedAsCode()
    {
        var sections = Nocco.Parse("a.js", new[] { "#!/usr/bin/env node", "// doc", "run();" });
        Assert.Contains("#!/usr/bin/env node", sections[0].CodeHtml);
    }
}

public class HighlightTests
{
    [Fact]
    public void RunsDocsThroughMarkdown()
    {
        var sections = new List<Section> { new Section { DocsHtml = "**bold**", CodeHtml = "x" } };
        Nocco.Highlight(sections);
        Assert.Contains("<strong>bold</strong>", sections[0].DocsHtml);
        Assert.Equal("x", sections[0].CodeHtml);
    }
}

public class LanguageTests
{
    [Theory]
    [InlineData("a.cs", "csharp")]
    [InlineData("a.js", "javascript")]
    [InlineData("a.sql", "sql")]
    [InlineData("a.vb", "vb.net")]
    public void KnownExtensions(string file, string name) =>
        Assert.Equal(name, Nocco.GetLanguage(file).Name);

    [Fact]
    public void UnknownExtensionReturnsNull() => Assert.Null(Nocco.GetLanguage("a.txt"));

    [Theory]
    [InlineData("// text", true)]
    [InlineData("   // indented", true)]
    [InlineData("code(); // trailing", false)]
    public void CommentMatcherMatchesLeadingCommentsOnly(string line, bool expected) =>
        Assert.Equal(expected, Nocco.GetLanguage("a.cs").CommentMatcher.IsMatch(line));
}

[Collection("cwd")]
public class DestinationTests : IDisposable
{
    private readonly string _orig = Directory.GetCurrentDirectory();
    private readonly string _tmp = Directory.CreateTempSubdirectory().FullName;

    public DestinationTests() => Directory.SetCurrentDirectory(_tmp);

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_orig);
        Directory.Delete(_tmp, true);
    }

    [Fact]
    public void RootFileHasDepthZero()
    {
        var dest = Nocco.GetDestination("./Foo.cs", out var depth);
        Assert.Equal(0, depth);
        Assert.Equal(Path.Combine("docs", "foo.html"), Path.GetRelativePath(".", dest));
        Assert.True(Directory.Exists("docs"));
    }

    [Fact]
    public void NestedFileCreatesLowercaseDirectories()
    {
        var dest = Nocco.GetDestination("./Sub/Deep/Foo.cs", out var depth);
        Assert.Equal(2, depth);
        Assert.EndsWith(Path.Combine("docs", "sub", "deep", "foo.html"), dest.Replace("./", ""));
        Assert.True(Directory.Exists(Path.Combine("docs", "sub", "deep")));
    }
}

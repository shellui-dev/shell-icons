using ShellIcons.Generator;

namespace ShellIcons.Generator.Tests;

public class SvgParserTests
{
    [Fact]
    public void ExtractInner_SinglePath_ReturnsPathOnly()
    {
        // A minimal Lucide-format svg. Note the pretty-printed multi-attribute open tag.
        var svg = """
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="24"
              height="24"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
            >
              <path d="m9 18 6-6-6-6" />
            </svg>
            """;

        var inner = SvgParser.ExtractInner(svg);

        Assert.Equal("<path d=\"m9 18 6-6-6-6\" />", inner);
    }

    [Fact]
    public void ExtractInner_MultipleShapes_CollapsesToSingleLine()
    {
        var svg = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
              <circle cx="11" cy="11" r="8" />
              <path d="m21 21-4.3-4.3" />
            </svg>
            """;

        var inner = SvgParser.ExtractInner(svg);

        Assert.Equal("<circle cx=\"11\" cy=\"11\" r=\"8\" /><path d=\"m21 21-4.3-4.3\" />", inner);
    }

    [Fact]
    public void ExtractInner_ThreePathsWithArcCommand_Preserved()
    {
        var svg = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
              <path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3" />
              <path d="M12 9v4" />
              <path d="M12 17h.01" />
            </svg>
            """;

        var inner = SvgParser.ExtractInner(svg);

        Assert.Contains("m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3", inner);
        Assert.Contains("M12 9v4", inner);
        Assert.Contains("M12 17h.01", inner);
        Assert.DoesNotContain("<svg", inner);
        Assert.DoesNotContain("</svg>", inner);
        Assert.DoesNotContain("\n", inner);
    }

    [Fact]
    public void ExtractInner_Empty_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, SvgParser.ExtractInner(""));
    }

    [Fact]
    public void ExtractInner_NoSvgTag_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, SvgParser.ExtractInner("<div>not an svg</div>"));
    }

    [Fact]
    public void ExtractInner_SelfClosingSvg_ReturnsEmpty()
    {
        // No closing </svg> — regex-wise, our parser walks from svg-open to end-of-string.
        // For a truly malformed input we accept "empty-ish" as the answer.
        var svg = "<svg viewBox=\"0 0 24 24\" />";
        var inner = SvgParser.ExtractInner(svg);
        Assert.Equal(string.Empty, inner);
    }

    [Fact]
    public void ExtractInner_LeavesInnerAttributeWhitespaceAlone()
    {
        // The parser normalizes whitespace BETWEEN elements, but must preserve spaces
        // inside a path d="..." string (Lucide's path data uses meaningful spaces).
        var svg = """
            <svg xmlns="http://www.w3.org/2000/svg">
              <path d="M 3 10 a 2 2 0 0 1 .709 -1.528" />
            </svg>
            """;

        var inner = SvgParser.ExtractInner(svg);

        Assert.Contains("M 3 10 a 2 2 0 0 1 .709 -1.528", inner);
    }
}

namespace Pure.DI.Tests;

using System.Text;
using Core;
using Core.Models;

public class LinesTests
{
    [Fact]
    public void ShouldAppendText()
    {
        // Given
        var lines = new Lines();

        // When
        lines.Append("abc");
        lines.Append("def");
        lines.AppendLine();

        // Then
        lines.ToString().ShouldBe("abcdef" + Environment.NewLine);
    }

    [Fact]
    public void ShouldAppendLine()
    {
        // Given
        var lines = new Lines();

        // When
        lines.AppendLine("abc");
        lines.AppendLine("def");

        // Then
        lines.ToString().ShouldBe("abc" + Environment.NewLine + "def" + Environment.NewLine);
    }

    [Fact]
    public void ShouldSupportIndent()
    {
        // Given
        var lines = new Lines();

        // When
        lines.AppendLine("abc");
        using (lines.Indent())
        {
            lines.AppendLine("def");
            using (lines.Indent())
            {
                lines.AppendLine("ghi");
            }
            lines.AppendLine("jkl");
        }
        lines.AppendLine("mno");

        // Then
        lines.ToString().ShouldBe(
            "abc" + Environment.NewLine +
            "\tdef" + Environment.NewLine +
            "\t\tghi" + Environment.NewLine +
            "\tjkl" + Environment.NewLine +
            "mno" + Environment.NewLine);
    }

    [Fact]
    public void ShouldSupportIncDecIndent()
    {
        // Given
        var lines = new Lines();

        // When
        lines.AppendLine("abc");
        lines.IncIndent();
        lines.AppendLine("def");
        lines.DecIndent();
        lines.AppendLine("ghi");

        // Then
        lines.ToString().ShouldBe(
            "abc" + Environment.NewLine +
            "\tdef" + Environment.NewLine +
            "ghi" + Environment.NewLine);
    }

    [Fact]
    public void ShouldClone()
    {
        // Given
        var lines = new Lines();
        lines.AppendLine("abc");
        lines.IncIndent();
        lines.Append("def");

        // When
        var clone = lines.Clone();
        lines.AppendLine("ghi"); // Add to original
        clone.AppendLine("xyz"); // Add to clone

        // Then
        lines.ToString().ShouldBe(
            "abc" + Environment.NewLine +
            "\tdefghi" + Environment.NewLine);

        clone.ToString().ShouldBe(
            "abc" + Environment.NewLine +
            "\tdefxyz" + Environment.NewLine);
    }

    [Fact]
    public void ShouldCloneAndMaintainIndent()
    {
        // Given
        var lines = new Lines();
        lines.IncIndent();

        // When
        var clone = lines.Clone();
        clone.AppendLine("abc");

        // Then
        clone.ToString().ShouldBe("\tabc" + Environment.NewLine);
    }

    [Fact]
    public void ShouldCountLines()
    {
        // Given
        var lines = new Lines();

        // When
        lines.Count.ShouldBe(0);
        lines.AppendLine("abc");
        lines.Count.ShouldBe(1);
        lines.Append("def");
        lines.Count.ShouldBe(2); // _sb has "def", so it's 1 + 1
        lines.AppendLine();
        lines.Count.ShouldBe(2); // "def" flushed to _lines
    }

    [Fact]
    public void ShouldAppendLinesFromLines()
    {
        // Given
        var lines1 = new Lines();
        lines1.AppendLine("abc");
        
        var lines2 = new Lines();
        lines2.IncIndent();
        lines2.AppendLine("def");

        // When
        lines1.AppendLines(lines2);

        // Then
        lines1.ToString().ShouldBe(
            "abc" + Environment.NewLine +
            "\tdef" + Environment.NewLine);
    }

    [Fact]
    public void ShouldAppendLinesFromEnumerable()
    {
        // Given
        var lines = new Lines();
        lines.AppendLine("abc");
        var otherLines = new List<Line> { new(1, "def"), new(2, "ghi") };

        // When
        lines.AppendLines(otherLines);

        // Then
        lines.ToString().ShouldBe(
            "abc" + Environment.NewLine +
            "\tdef" + Environment.NewLine +
            "\t\tghi" + Environment.NewLine);
    }

    [Fact]
    public void ShouldHandleMixedAppendAndAppendLine()
    {
        // Given
        var lines = new Lines();

        // When
        lines.Append("abc");
        lines.AppendLine("def");
        lines.Append("ghi");
        lines.AppendLine("jkl");

        // Then
        lines.ToString().ShouldBe(
            "abcdef" + Environment.NewLine +
            "ghijkl" + Environment.NewLine);
    }

    [Fact]
    public void ShouldSaveToArray()
    {
        // Given
        var lines = new Lines();
        lines.AppendLine("abc");
        lines.IncIndent();
        lines.AppendLine("def");

        // When
        using (lines.SaveToArray(Encoding.UTF8, out var buffer, out var size))
        {
            var result = Encoding.UTF8.GetString(buffer, 0, size);

            // Then
            result.ShouldBe("abc" + Environment.NewLine + "\tdef" + Environment.NewLine);
        }
    }
}

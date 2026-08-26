// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Text;

namespace EricksonLopez.Mapper.Generator.Tests.Infrastructure;

public static class SourceNormalizer
{
    private static readonly string[] DefaultUsings =
    {
        "using EricksonLopez.Mapper;",
        "using System.Collections.Frozen;",
        "using System.Collections.Immutable;",
        "using System.Collections.Generic;",
        "using System.Collections;",
        "using System;"
    };

    public static string Normalize(string source)
    {
        var lines = source.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var usings = new List<string>(DefaultUsings);
        var assemblyLines = new List<string>();
        var otherLines = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("using ") && trimmed.EndsWith(";"))
            {
                if (!usings.Contains(trimmed))
                {
                    usings.Add(trimmed);
                }
            }
            else if (trimmed.StartsWith("[assembly:"))
            {
                assemblyLines.Add(line);
            }
            else
            {
                otherLines.Add(line);
            }
        }

        var sb = new StringBuilder();
        foreach (var u in usings)
        {
            sb.Append(u).Append('\n');
        }
        foreach (var a in assemblyLines)
        {
            sb.Append(a).Append('\n');
        }
        if (!source.Contains("namespace"))
        {
            sb.Append("namespace TestNamespace;\n");
        }
        foreach (var o in otherLines)
        {
            sb.Append(o).Append('\n');
        }

        return sb.ToString();
    }
}

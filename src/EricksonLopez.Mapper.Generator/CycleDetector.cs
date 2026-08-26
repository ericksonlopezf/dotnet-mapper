// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator;

internal static class CycleDetector
{
    public static void DetectCycles(List<Models.MethodMapping> methods, List<Models.DiagnosticInfo> diagnostics, Location location)
    {
        var adjacencyList = new Dictionary<string, List<string>>();
        var methodKeysByName = new Dictionary<string, List<string>>();

        foreach (var m in methods)
        {
            var deps = new List<string>();
            ExtractMethodDependencies(m.Construction, deps);
            foreach (var mem in m.Members) ExtractMethodDependencies(mem.Strategy, deps);

            string key = GetMethodKey(m);
            adjacencyList[key] = deps;

            if (!methodKeysByName.TryGetValue(m.MethodName, out var keys))
            {
                keys = new List<string>();
                methodKeysByName[m.MethodName] = keys;
            }
            keys.Add(key);
        }

        var recursionStack = new HashSet<string>(StringComparer.Ordinal);

        foreach (var m in methods)
        {
            string key = GetMethodKey(m);
            if (HasCycle(key, adjacencyList, methodKeysByName, recursionStack))
            {
                var lineSpan = location.GetLineSpan();
                string filePath = location.SourceTree?.FilePath ?? "";
                int line = lineSpan.StartLinePosition.Line;
                int column = lineSpan.StartLinePosition.Character;

                bool isDuplicate = diagnostics.Any(d =>
                    d.Id == DiagnosticDescriptors.CircularReference.Id &&
                    d.FilePath == filePath &&
                    d.Line == line &&
                    d.Column == column &&
                    d.Args.SequenceEqual(new[] { m.MethodName }));

                if (!isDuplicate)
                {
                    diagnostics.Add(new Models.DiagnosticInfo(
                        DiagnosticDescriptors.CircularReference.Id,
                        DiagnosticDescriptors.CircularReference.Title.ToString(),
                        DiagnosticDescriptors.CircularReference.MessageFormat.ToString(),
                        DiagnosticDescriptors.CircularReference.Category,
                        (int)DiagnosticDescriptors.CircularReference.DefaultSeverity,
                        DiagnosticDescriptors.CircularReference.IsEnabledByDefault,
                        filePath,
                        line,
                        column,
                        new[] { m.MethodName }.AsEquatableArray()));
                }
            }
        }
    }

    private static string GetMethodKey(Models.MethodMapping m)
    {
        if (m.SourceType == null || string.IsNullOrEmpty(m.SourceType.FullyQualifiedName))
            return m.MethodName;
        return $"{m.MethodName}({m.SourceType.FullyQualifiedName})";
    }

    private static bool HasCycle(
        string node,
        Dictionary<string, List<string>> graph,
        Dictionary<string, List<string>> methodKeysByName,
        HashSet<string> recStack)
    {
        if (!recStack.Add(node)) return true;

        try
        {
            if (graph.TryGetValue(node, out var deps))
            {
                foreach (var dep in deps)
                {
                    if (HasCycle(dep, graph, methodKeysByName, recStack)) return true;
                }
            }
            else if (methodKeysByName.TryGetValue(node, out var candidateKeys))
            {
                foreach (var cand in candidateKeys)
                {
                    if (HasCycle(cand, graph, methodKeysByName, recStack)) return true;
                }
            }

            return false;
        }
        finally
        {
            recStack.Remove(node);
        }
    }

    private static void ExtractMethodDependencies(Models.ConstructionStrategy construction, List<string> deps)
    {
        if (construction is Models.ConstructionStrategy.ParameterizedConstructor pc)
        {
            foreach (var p in pc.Parameters) ExtractMethodDependencies(p.Strategy, deps);
        }
        else if (construction is Models.ConstructionStrategy.FactoryMethod fm)
        {
            foreach (var p in fm.Parameters) ExtractMethodDependencies(p.Strategy, deps);
        }
    }

    private static void ExtractMethodDependencies(Models.ConversionStrategy strategy, List<string> deps)
    {
        if (strategy is Models.ConversionStrategy.MapMethodInvocation m)
        {
            deps.Add(!string.IsNullOrEmpty(m.MethodKey) ? m.MethodKey : m.MethodName);
        }
        else if (strategy is Models.ConversionStrategy.EnumerableMapping eMap)
        {
            ExtractMethodDependencies(eMap.ElementStrategy, deps);
        }
        else if (strategy is Models.ConversionStrategy.DictionaryMapping dMap)
        {
            ExtractMethodDependencies(dMap.KeyStrategy, deps);
            ExtractMethodDependencies(dMap.ValueStrategy, deps);
        }
        else if (strategy is Models.ConversionStrategy.ValueObjectMapping voMap)
        {
            ExtractMethodDependencies(voMap.InnerStrategy, deps);
        }
    }
}



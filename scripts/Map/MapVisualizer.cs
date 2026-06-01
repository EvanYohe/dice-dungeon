using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Enumerable = System.Linq.Enumerable;

namespace DiceDungeon.scripts.Map;

public static class MapVisualizer {
    public static void WriteDot(Graph graph, string filePath = "graph.dot") {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        List<Node> nodes = Enumerable.ToList(graph.Nodes);
        Dictionary<Node, string> nodeIds = new Dictionary<Node, string>();

        for (int index = 0; index < nodes.Count; index++) {
            nodeIds[nodes[index]] = $"n{index}";
        }

        StringBuilder builder = new StringBuilder();

        builder.AppendLine("graph G {");
        builder.AppendLine("  layout=neato;");
        builder.AppendLine("  overlap=false;");
        builder.AppendLine("  splines=true;");
        builder.AppendLine("  node [shape=circle, style=filled, fontname=\"Arial\"];");

        foreach (Node node in nodes) {
            string id = nodeIds[node];
            string label = EscapeLabel($"{node.Type}");
            string color = GetNodeColor(node.Type);

            builder.AppendLine($"  {id} [label=\"{label}\", fillcolor=\"{color}\"];");
        }

        HashSet<string> writtenEdges = new HashSet<string>();

        foreach (Node alpha in nodes)
        foreach (Node beta in graph.GetConnections(alpha)) {
            string alphaId = nodeIds[alpha];
            string betaId = nodeIds[beta];

            string edgeKey = string.CompareOrdinal(alphaId, betaId) < 0
                ? $"{alphaId}--{betaId}"
                : $"{betaId}--{alphaId}";

            if (writtenEdges.Add(edgeKey)) {
                builder.AppendLine($"  {alphaId} -- {betaId};");
            }
        }

        builder.AppendLine("}");

        File.WriteAllText(filePath, builder.ToString());
    }

    public static void WriteSvg(Graph graph, string dotFilePath = "graph.dot", string svgFilePath = "graph.svg") {
        WriteDot(graph, dotFilePath);

        ProcessStartInfo startInfo = new ProcessStartInfo {
            FileName = "dot",
            Arguments = $"-Tsvg \"{dotFilePath}\" -o \"{svgFilePath}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start Graphviz dot process.");

        process.WaitForExit();

        if (process.ExitCode != 0) {
            throw new InvalidOperationException("Graphviz failed to generate the SVG file.");
        }
    }

    private static string GetNodeColor(NodeType nodeType) {
        return nodeType switch {
            NodeType.Entrance => "palegreen",
            NodeType.Exit => "lightcoral",
            NodeType.Boss => "red",
            NodeType.Shop => "gold",
            NodeType.Treasure => "deepskyblue",
            NodeType.Miniboss => "orange",
            NodeType.Secret => "plum",
            NodeType.Event => "lightpink",
            NodeType.Challenge => "khaki",
            NodeType.Encounter => "lightsalmon",
            NodeType.Empty => "lightgray",
            _ => "white"
        };
    }

    private static string EscapeLabel(string label) {
        return label
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }
}
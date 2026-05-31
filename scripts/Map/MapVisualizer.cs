using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DiceDungeon.scripts.Map;

public static class MapVisualizer {
    public static void writeDot(Graph graph, string filePath = "graph.dot") {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        List<Node> nodes = graph.nodes.ToList();
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
            string label = escapeLabel($"{node.type}");
            string color = getNodeColor(node.type);

            builder.AppendLine($"  {id} [label=\"{label}\", fillcolor=\"{color}\"];");
        }

        HashSet<string> writtenEdges = new HashSet<string>();

        foreach (Node alpha in nodes)
        foreach (Node beta in graph.getConnections(alpha)) {
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

    public static void writeSvg(Graph graph, string dotFilePath = "graph.dot", string svgFilePath = "graph.svg") {
        writeDot(graph, dotFilePath);

        ProcessStartInfo startInfo = new ProcessStartInfo {
            FileName = "dot",
            Arguments = $"-Tsvg \"{dotFilePath}\" -o \"{svgFilePath}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = Process.Start(startInfo)
                                ?? throw new InvalidOperationException("Failed to start Graphviz dot process.");

        process.WaitForExit();

        if (process.ExitCode != 0) {
            throw new InvalidOperationException("Graphviz failed to generate the SVG file.");
        }
    }

    private static string getNodeColor(NodeType nodeType) {
        return nodeType switch {
            NodeType.ENTRANCE => "palegreen",
            NodeType.EXIT => "lightcoral",
            NodeType.BOSS => "red",
            NodeType.SHOP => "gold",
            NodeType.TREASURE => "deepskyblue",
            NodeType.MINIBOSS => "orange",
            NodeType.SECRET => "plum",
            NodeType.EVENT => "lightpink",
            NodeType.CHALLENGE => "khaki",
            NodeType.ENCOUNTER => "lightsalmon",
            NodeType.EMPTY => "lightgray",
            _ => "white"
        };
    }

    private static string escapeLabel(string label) {
        return label
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }
}
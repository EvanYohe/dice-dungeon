using System;
using System.Collections.Generic;
using Enumerable = System.Linq.Enumerable;

namespace DiceDungeon.scripts.Map;

public class MapValidator {
    private const int MinNodeCount = 22;
    private const int MaxNodeCount = 44;
    private static readonly (int, int) TreasureCountRange = (2, 3);

    public bool IsValid(Graph graph) {
        ArgumentNullException.ThrowIfNull(graph);

        if (!hasExactlyOneNodeOfType(graph, NodeType.Entrance)) {
            return false;
        }

        if (!hasExactlyOneNodeOfType(graph, NodeType.Exit)) {
            return false;
        }

        if (!hasExactlyOneNodeOfType(graph, NodeType.Shop)) {
            return false;
        }

        if (!hasExactlyOneNodeOfType(graph, NodeType.Boss)) {
            return false;
        }

        if (graph.Nodes.Count is < MinNodeCount or > MaxNodeCount) {
            return false;
        }

        if (!CheckEntryExitPathfinding(graph)) {
            return false;
        }

        if (!CheckEncounterRatio(graph)) {
            return false;
        }

        if (!NodesUnderMaxEdges(graph)) {
            return false;
        }

        if (!CheckExitConnections(graph)) {
            return false;
        }

        if (!CheckBossConnections(graph)) {
            return false;
        }

        if (!HasNodeCountInRange(graph, NodeType.Treasure, TreasureCountRange)) {
            return false;
        }

        if (!HasNodeCountAtMost(graph, NodeType.Event)) {
            return false;
        }

        if (!HasNodeCountAtMost(graph, NodeType.Challenge)) {
            return false;
        }

        if (!HasNodeCountAtMost(graph, NodeType.Secret)) {
            return false;
        }

        if (!HasNodeCountAtMost(graph, NodeType.Miniboss)) {
            return false;
        }

        return true;
    }

    public bool IsSubgraphValid(Graph graph) {
        int[] nodeConnectionCounts = Enumerable.ToArray(Enumerable.Select(graph.Nodes, graph.ConnectionCount));
        foreach (int t in nodeConnectionCounts) {
            if (t > 3) {
                return false;
            }
        }

        if (!Enumerable.Any(nodeConnectionCounts, count => count < 2)) {
            return false;
        }

        if (Enumerable.Any(nodeConnectionCounts, count => count == 0)) {
            return false;
        }

        HashSet<Guid> visited = TraverseFrom(graph, Enumerable.First(graph.Nodes));
        if (visited.Count != graph.Nodes.Count) {
            return false;
        }

        return true;
    }

    private static HashSet<Guid> TraverseFrom(Graph graph, Node start) {
        HashSet<Guid> visited = new HashSet<Guid>();
        Queue<Node> queue = new Queue<Node>();
        visited.Add(start.Id);
        queue.Enqueue(start);
        while (queue.Count > 0) {
            Node current = queue.Dequeue();
            foreach (Node connection in graph.GetConnections(current)) {
                if (visited.Add(connection.Id)) {
                    queue.Enqueue(connection);
                }
            }
        }

        return visited;
    }

    // This pathfinding algorithm should check for disconnected subgraphs or nodes
    private static bool CheckEntryExitPathfinding(Graph graph) {
        Node? entrance = graph.GetNodeByType(NodeType.Entrance);
        Node? exit = graph.GetNodeByType(NodeType.Exit);
        HashSet<Guid> visited = TraverseFrom(graph, entrance);
        return visited.Contains(exit.Id) && visited.Count == graph.Nodes.Count;
    }

    // An EXIT node should only have one connection to the BOSS node
    private static bool CheckExitConnections(Graph graph) {
        Node? exit = graph.GetNodeByType(NodeType.Exit);
        Node? boss = graph.GetNodeByType(NodeType.Boss);

        return graph.HasConnection(exit, boss) && graph.ConnectionCount(exit) == 1;
    }

    // A BOSS node should always have two connections,
    // One connection to the exit
    // One connection to an EMPTY node
    private static bool CheckBossConnections(Graph graph) {
        Node? boss = graph.GetNodeByType(NodeType.Boss);

        if (graph.ConnectionCount(boss) != 2 || !graph.HasConnectionOfType(boss, NodeType.Exit) || !graph.HasConnectionOfType(boss, NodeType.EmptyBeforeBoss)) {
            return false;
        }

        return true;
    }

    // The ratio of ENCOUNTER nodes to total nodes should be between 30% and 70%
    private static bool CheckEncounterRatio(Graph graph) {
        float encounter = graph.NodeTypeCount(NodeType.Encounter);
        float total = graph.Nodes.Count;

        if (total <= 0) {
            return false;
        }

        float ratio = encounter / total;

        return ratio is >= .3f and <= .7f;
    }

    private static bool NodesUnderMaxEdges(Graph graph) {
        return Enumerable.All(graph.Nodes, node => graph.ConnectionCount(node) <= 4);
    }

    private static bool hasExactlyOneNodeOfType(Graph graph, NodeType nodeType) {
        return graph.NodeTypeCount(nodeType) == 1;
    }

    private static bool HasNodeCountInRange(Graph graph, NodeType nodeType, (int minCount, int maxCount) range) {
        return graph.NodeTypeCount(nodeType) >= range.minCount && graph.NodeTypeCount(nodeType) <= range.maxCount;
    }

    private static bool HasNodeCountAtMost(Graph graph, NodeType nodeType) {
        return graph.NodeTypeCount(nodeType) <= 1;
    }
}
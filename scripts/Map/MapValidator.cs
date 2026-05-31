using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceDungeon.scripts.Map;

public class MapValidator {
    private static readonly (int, int) TREASURE_COUNT_RANGE = (2, 3);
    private const int MIN_NODE_COUNT = 22;
    private const int MAX_NODE_COUNT = 44;

    public bool isValid(Graph graph) {
        ArgumentNullException.ThrowIfNull(graph);

        if (!hasExactlyOneNodeOfType(graph, NodeType.ENTRANCE)) {
            return false;
        }

        if (!hasExactlyOneNodeOfType(graph, NodeType.EXIT)) {
            return false;
        }

        if (!hasExactlyOneNodeOfType(graph, NodeType.SHOP)) {
            return false;
        }

        if (!hasExactlyOneNodeOfType(graph, NodeType.BOSS)) {
            return false;
        }

        if (graph.nodes.Count < MIN_NODE_COUNT || graph.nodes.Count > MAX_NODE_COUNT) {
            return false;
        }

        if (!checkEntryExitPathfinding(graph)) {
            return false;
        }

        if (!checkEncounterRatio(graph)) {
            return false;
        }

        if (!nodesUnderMaxEdges(graph)) {
            return false;
        }

        if (!checkExitConnections(graph)) {
            return false;
        }

        if (!checkBossConnections(graph)) {
            return false;
        }

        if (!hasNodeCountInRange(graph, NodeType.TREASURE, TREASURE_COUNT_RANGE)) {
            return false;
        }

        if (!hasNodeCountAtMost(graph, NodeType.EVENT)) {
            return false;
        }

        if (!hasNodeCountAtMost(graph, NodeType.CHALLENGE)) {
            return false;
        }

        if (!hasNodeCountAtMost(graph, NodeType.SECRET)) {
            return false;
        }

        if (!hasNodeCountAtMost(graph, NodeType.MINIBOSS)) {
            return false;
        }

        return true;
    }

    public bool isSubgraphValid(Graph graph) {
        int[] nodeConnectionCounts = graph.nodes.Select(graph.connectionCount).ToArray();
        foreach (int t in nodeConnectionCounts) {
            if (t > 3) {
                return false;
            }
        }

        if (!nodeConnectionCounts.Any(count => count < 2)) {
            return false;
        }

        if (nodeConnectionCounts.Any(count => count == 0)) {
            return false;
        }

        HashSet<Guid> visited = traverseFrom(graph, graph.nodes.First());
        if (visited.Count != graph.nodes.Count) {
            return false;
        }

        return true;
    }

    private static HashSet<Guid> traverseFrom(Graph graph, Node start) {
        HashSet<Guid> visited = new HashSet<Guid>();
        Queue<Node> queue = new Queue<Node>();
        visited.Add(start.id);
        queue.Enqueue(start);
        while (queue.Count > 0) {
            Node current = queue.Dequeue();
            foreach (Node connection in graph.getConnections(current)) {
                if (visited.Add(connection.id)) {
                    queue.Enqueue(connection);
                }
            }
        }

        return visited;
    }

    // This pathfinding algorithm should check for disconnected subgraphs or nodes
    private static bool checkEntryExitPathfinding(Graph graph) {
        Node? entrance = graph.getNodeByType(NodeType.ENTRANCE);
        Node? exit = graph.getNodeByType(NodeType.EXIT);
        HashSet<Guid> visited = traverseFrom(graph, entrance);
        return visited.Contains(exit.id)
               && visited.Count == graph.nodes.Count;
    }

    // An EXIT node should only have one connection to the BOSS node
    private static bool checkExitConnections(Graph graph) {
        Node? exit = graph.getNodeByType(NodeType.EXIT);
        Node? boss = graph.getNodeByType(NodeType.BOSS);

        return graph.hasConnection(exit, boss)
               && graph.connectionCount(exit) == 1;
    }

    // A BOSS node should always have two connections,
    // One connection to the exit
    // One connection to an EMPTY node
    private static bool checkBossConnections(Graph graph) {
        Node? boss = graph.getNodeByType(NodeType.BOSS);

        if (graph.connectionCount(boss) != 2
            || !graph.hasConnectionOfType(boss, NodeType.EXIT)
            || !graph.hasConnectionOfType(boss, NodeType.EMPTY_BEFORE_BOSS)
           ) {
            return false;
        }

        return true;
    }

    // The ratio of ENCOUNTER nodes to total nodes should be between 30% and 70%
    private static bool checkEncounterRatio(Graph graph) {
        float encounter = graph.nodeTypeCount(NodeType.ENCOUNTER);
        float total = graph.nodes.Count;

        if (total <= 0) {
            return false;
        }

        float ratio = encounter / total;

        return ratio is >= .3f and <= .7f;
    }

    private static bool nodesUnderMaxEdges(Graph graph) {
        return graph.nodes.All(node => graph.connectionCount(node) <= 4);
    }

    private static bool hasExactlyOneNodeOfType(Graph graph, NodeType nodeType) {
        return graph.nodeTypeCount(nodeType) == 1;
    }

    private static bool hasNodeCountInRange(Graph graph, NodeType nodeType, (int minCount, int maxCount) range) {
        return graph.nodeTypeCount(nodeType) >= range.minCount
               && graph.nodeTypeCount(nodeType) <= range.maxCount;
    }

    private static bool hasNodeCountAtMost(Graph graph, NodeType nodeType) {
        return graph.nodeTypeCount(nodeType) <= 1;
    }
}
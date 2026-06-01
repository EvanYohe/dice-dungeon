using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceDungeon.scripts.Map;

public abstract class MapGenerator {
    private const int MaxConnectionsPerNode = 4;
    private const int MaxGenerationAttempts = 10;

    private static readonly MapValidator Validator = new MapValidator();
    private static readonly Random Random = new Random();

    private static readonly (int dx, int dy)[] CardinalDirections = [
        (1, 0), (-1, 0), (0, 1), (0, -1)
    ];

    public static Graph Generate(Graph seed) {
        // null check on seed graph
        ArgumentNullException.ThrowIfNull(seed);

        // create the function output from a clone of the seed graph
        Graph outputGraph = seed.Clone();

        // main iteration loop defines the maximum number of attempts to generate a valid graph
        for (int i = 0; i < MaxGenerationAttempts; i++) {
            // create a validated subgraph to be appended to the target graph
            Graph subgraph = CreateSubSubSubgraph();

            // grab the pre-appended nodes of the target graph
            List<Guid> outputGraphGuidList = outputGraph.Nodes.Where(node => node.Type is
                NodeType.Empty or
                NodeType.Encounter or
                NodeType.Treasure or
                NodeType.Miniboss or
                NodeType.Secret or
                NodeType.Event or
                NodeType.Challenge
            ).Select(node => node.Id).ToList();

            // create a list of the nodes from the source subgraph
            List<Guid> subgraphGuidList = subgraph.Nodes.Select(node => node.Id).ToList();

            // add all the nodes from the subgraph to the target graph
            foreach (Node node in subgraph.Nodes) {
                outputGraph.AddNode(node);
            }

            // copy/replicate the connections between the nodes in the subgraph to the nodes added to the target graph
            foreach (Node node in subgraph.Nodes) {
                foreach (Node connection in subgraph.GetConnections(node)) {
                    outputGraph.AddConnection(node, connection);
                }
            }

            if (outputGraphGuidList.Count == 0 || subgraphGuidList.Count == 0) {
                continue;
            }

            // connect the two subgraphs, denying connection if either random node has four connections
            bool connectionSuccess = false;
            while (!connectionSuccess) {
                // connectionSuccess = attachSubgraph(
                //     subgraph,
                //     outputGraph,
                //     subgraphGuidList,
                //     outputGraphGuidList
                //     );

                // testing subgraph generation by grid layout
                connectionSuccess = AttachSubgraphToGrid(outputGraph, subgraph, outputGraph.GetNodeCoordinates());
            }

            outputGraphGuidList.Clear();
            subgraphGuidList.Clear();

            // post-processing
            PostProcess(outputGraph);


            // if the target graph is now valid, break the loop early
            if (Validator.IsValid(outputGraph)) {
                MapVisualizer.WriteSvg(outputGraph, "success.dot", "success.svg");
                break;
            }
        }

        return outputGraph;
    }

    // First subgraph creation method
    private static Graph CreateSubgraph() {
        NodeType[] nodeTypePool = [
            NodeType.Empty,
            NodeType.Empty,
            NodeType.Encounter,
            NodeType.Encounter,
            NodeType.Encounter,
            NodeType.Encounter,
            NodeType.Treasure,
            NodeType.Miniboss,
            NodeType.Secret,
            NodeType.Event,
            NodeType.Challenge
        ];

        while (true) {
            Graph subgraph = new Graph();
            int nodeCount = Random.Next(3, 6);
            List<Node> nodeList = new List<Node>(nodeCount);

            for (int i = 0; i < nodeCount; i++) {
                Node node = new Node(nodeTypePool[Random.Next(nodeTypePool.Length)], Random.Next(4, 13));
                nodeList.Add(node);
                subgraph.AddNode(node);
            }

            // index of array has to match index of nodes in subgraph
            int[] nodeConnectionCounts =
                subgraph.Nodes.Select(subgraph.ConnectionCount).ToArray();
            while (nodeConnectionCounts.Any(count => count == 0)) {
                int alphaIndex = Random.Next(nodeCount);
                int betaIndex = Random.Next(nodeCount);
                Node alpha = nodeList[alphaIndex];
                Node beta = nodeList[betaIndex];

                if (subgraph.HasConnection(alpha, beta)) {
                    continue;
                }

                if (alpha.Id == beta.Id) {
                    continue;
                }

                subgraph.AddConnection(alpha, beta);
                nodeConnectionCounts[alphaIndex]++;
                nodeConnectionCounts[betaIndex]++;
            }

            if (Validator.IsSubgraphValid(subgraph)) {
                return subgraph;
            }
        }
    }

    // Second subgraph creation method
    private static Graph CreateSubSubgraph() {
        NodeType[] nodeTypePool = [
            NodeType.Empty,
            NodeType.Empty,
            NodeType.Encounter,
            NodeType.Encounter,
            NodeType.Encounter,
            NodeType.Encounter,
            NodeType.Treasure,
            NodeType.Miniboss,
            NodeType.Secret,
            NodeType.Event,
            NodeType.Challenge
        ];

        while (true) {
            Graph subgraph = new Graph();
            int nodeCount = Random.Next(3, 6);
            List<Node> nodeList = new List<Node>(nodeCount);

            for (int i = 0; i < nodeCount; i++) {
                Node node = new Node(nodeTypePool[Random.Next(nodeTypePool.Length)], Random.Next(4, 13));
                nodeList.Add(node);
                subgraph.AddNode(node);
            }

            // Shuffle the node list and chain them together into a spanning tree.
            // This guarantees every node has at least one connection and the
            // subgraph is fully connected before any extra edges are added.
            List<Node> shuffled =
                nodeList.OrderBy(_ => Random.Next()).ToList();
            for (int i = 0; i < shuffled.Count - 1; i++) {
                subgraph.AddConnection(shuffled[i], shuffled[i + 1]);
            }

            // Opportunistically add extra edges between nodes that still have
            // capacity, giving the subgraph more structural variety.
            for (int i = 0; i < nodeCount; i++) {
                for (int j = i + 1; j < nodeCount; j++) {
                    Node alpha = nodeList[i];
                    Node beta = nodeList[j];

                    if (subgraph.HasConnection(alpha, beta)) {
                        continue;
                    }

                    if (subgraph.ConnectionCount(alpha) >= MaxConnectionsPerNode) {
                        continue;
                    }

                    if (subgraph.ConnectionCount(beta) >= MaxConnectionsPerNode) {
                        continue;
                    }

                    if (Random.NextDouble() > 0.5) {
                        subgraph.AddConnection(alpha, beta);
                    }
                }
            }

            if (Validator.IsSubgraphValid(subgraph)) {
                return subgraph;
            }
        }
    }

    // Third subgraph creation method
    private static Graph CreateSubSubSubgraph() {
        NodeType[] nodeTypePool = [
            NodeType.Empty,
            NodeType.Empty,
            NodeType.Encounter,
            NodeType.Encounter,
            NodeType.Encounter,
            NodeType.Encounter,
            NodeType.Treasure,
            NodeType.Miniboss,
            NodeType.Secret,
            NodeType.Event,
            NodeType.Challenge
        ];

        while (true) {
            Graph subgraph = new Graph();
            int targetCount = Random.Next(3, 6);

            // Track which grid cells are occupied in this subgraph's local space
            Dictionary<(int x, int y), Node> occupiedCells = new Dictionary<(int x, int y), Node>();

            // Seed node at local origin
            Node seed = new Node(nodeTypePool[Random.Next(nodeTypePool.Length)], Random.Next(4, 13), 0, 0);
            subgraph.AddNode(seed);
            occupiedCells[(0, 0)] = seed;

            int attempts = 0;
            while (subgraph.Nodes.Count < targetCount && attempts++ < 200) {
                // Pick a random existing node as the expansion point
                Node parent = subgraph.Nodes[Random.Next(subgraph.Nodes.Count)];
                // Pick a random cardinal direction
                (int dx, int dy) = CardinalDirections[Random.Next(4)];
                (int nx, int ny) = (parent.X + dx, parent.Y + dy);
                if (occupiedCells.ContainsKey((nx, ny))) {
                    continue;
                }

                Node child = new Node(nodeTypePool[Random.Next(nodeTypePool.Length)], Random.Next(4, 13), nx, ny);
                subgraph.AddNode(child);
                subgraph.AddConnection(parent, child); // always adjacent = valid grid edge
                occupiedCells[(nx, ny)] = child;
            }

            foreach (Node a in subgraph.Nodes) {
                foreach ((int dx, int dy) in CardinalDirections) {
                    if (!occupiedCells.TryGetValue((a.X + dx, a.Y + dy), out Node? b)) {
                        continue;
                    }

                    if (subgraph.HasConnection(a, b)) {
                        continue;
                    }

                    if (subgraph.ConnectionCount(a) >= MaxConnectionsPerNode) {
                        continue;
                    }

                    if (subgraph.ConnectionCount(b) >= MaxConnectionsPerNode) {
                        continue;
                    }

                    if (Random.NextDouble() > 0.5) {
                        subgraph.AddConnection(a, b);
                    }
                }
            }

            if (Validator.IsSubgraphValid(subgraph)) {
                return subgraph;
            }
        }
    }

    // function to link subgraphs from createSubgraph and createSubSubgraph
    private static bool AttachSubgraph(
        Graph source,
        Graph target, List<Guid> sourceCandidates,
        List<Guid> targetCandidates) {

        bool connectionSuccess = false;
        int attempts = 0;
        while (!connectionSuccess) {
            if (++attempts > 100) {
                break;
            }

            int outputGraphCandidateGuidIndex = Random.Next(targetCandidates.Count);
            int subgraphCandidateGuidIndex = Random.Next(sourceCandidates.Count);

            if (target.ConnectionCount(targetCandidates[outputGraphCandidateGuidIndex]) <
                MaxConnectionsPerNode
                && source.ConnectionCount(sourceCandidates[subgraphCandidateGuidIndex]) < MaxConnectionsPerNode) {
                // randomly select a candidate from each list and connect them
                target.AddConnection(
                    targetCandidates[outputGraphCandidateGuidIndex],
                    sourceCandidates[subgraphCandidateGuidIndex]
                );
                connectionSuccess = true;
            }
        }

        return true;
    }

    // function to link subgraph from createSubSubSubgraph
    private static bool AttachSubgraphToGrid(
        Graph outputGraph,
        Graph subgraph, HashSet<(int x, int y)> occupiedCells) {

        // Find all candidate attachment points on the output graph boundary:
        // cells adjacent to an existing node that are currently unoccupied
        List<(Node outputNode, int tx, int ty)> attachmentPoints = new List<(Node outputNode, int tx, int ty)>();

        foreach (Node node in outputGraph.Nodes) {
            if (outputGraph.ConnectionCount(node) >= MaxConnectionsPerNode) {
                continue;
            }

            foreach ((int dx, int dy) in CardinalDirections) {
                (int tx, int ty) = (node.X + dx, node.Y + dy);
                if (!occupiedCells.Contains((tx, ty))) {
                    attachmentPoints.Add((node, tx, ty));
                }
            }
        }

        if (attachmentPoints.Count == 0) {
            return false;
        }

        // Pick a random attachment point and a random entry node from the subgraph
        (Node anchorNode, int targetX, int targetY) = attachmentPoints[Random.Next(attachmentPoints.Count)];

        Node subgraphEntry = subgraph.Nodes.Where(n => subgraph.ConnectionCount(n) < MaxConnectionsPerNode).OrderBy(_ => Random.Next()).FirstOrDefault();
        if (subgraphEntry is null) {
            return false;
        }

        // Calculate the offset needed to place subgraphEntry at (targetX, targetY)
        int offsetX = targetX - subgraphEntry.X;
        int offsetY = targetY - subgraphEntry.Y;

        // Check for collisions before committing
        foreach (Node n in subgraph.Nodes) {
            if (occupiedCells.Contains((n.X + offsetX, n.Y + offsetY))) {
                return false;
            }
        }

        // Apply offset and add to output graph
        foreach (Node n in subgraph.Nodes) {
            n.X += offsetX;
            n.Y += offsetY;
            outputGraph.AddNode(n);
            occupiedCells.Add((n.X, n.Y));
        }

        foreach (Node n in subgraph.Nodes) {
            foreach (Node conn in subgraph.GetConnections(n)) {
                outputGraph.AddConnection(n, conn);
            }
        }

        // Bridge the two subgraphs
        outputGraph.AddConnection(anchorNode, subgraphEntry);
        return true;
    }

    private static void PostProcess(Graph graph) {
        CleanupLimitedNodeTypes(graph);

        float encounterRatio = (float)graph.NodeTypeCount(NodeType.Encounter) / graph.Nodes.Count;
        while (encounterRatio < .5f) {
            List<Node> emptyNodes =
                graph.Nodes.Where(n => n.Type == NodeType.Empty).ToList();

            if (emptyNodes.Count == 0) {
                break;
            }

            graph.ChangeNodeType(emptyNodes[Random.Next(emptyNodes.Count)], NodeType.Encounter);
            encounterRatio = (float)graph.NodeTypeCount(NodeType.Encounter) / graph.Nodes.Count;
        }

        float emptyRatio = (float)graph.NodeTypeCount(NodeType.Empty) / graph.Nodes.Count;
        while (emptyRatio < .25f) {
            List<Node> encounterNodes =
                graph.Nodes.Where(n => n.Type == NodeType.Encounter).ToList();

            if (encounterNodes.Count == 0) {
                break;
            }

            graph.ChangeNodeType(encounterNodes[Random.Next(encounterNodes.Count)], NodeType.Empty);
            emptyRatio = (float)graph.NodeTypeCount(NodeType.Empty) / graph.Nodes.Count;
        }
    }

    private static void CleanupLimitedNodeTypes(Graph graph) {
        ConvertExcessNodesOfType(graph, NodeType.Challenge, 1);
        ConvertExcessNodesOfType(graph, NodeType.Secret, 1);
        ConvertExcessNodesOfType(graph, NodeType.Event, 1);
        ConvertExcessNodesOfType(graph, NodeType.Miniboss, 1);
        ConvertExcessNodesOfType(graph, NodeType.Treasure, 4);
    }

    private static void ConvertExcessNodesOfType(Graph graph, NodeType type, int maxCount) {
        while (graph.NodeTypeCount(type) > maxCount) {
            Node? nodeToConvert = graph.Nodes.Where(node => node.Type == type).OrderBy(graph.ConnectionCount).FirstOrDefault();
            if (nodeToConvert is null) {
                return;
            }

            graph.ChangeNodeType(nodeToConvert, NodeType.Empty);
        }
    }
}
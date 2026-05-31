using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceDungeon.scripts.Map;

public abstract class MapGenerator {
    private const int MAX_CONNECTIONS_PER_NODE = 4;
    private const int MAX_GENERATION_ATTEMPTS = 10;

    private static readonly MapValidator VALIDATOR = new MapValidator();
    private static readonly Random RANDOM = new Random();

    private static readonly (int dx, int dy)[] CARDINAL_DIRECTIONS = [
        (1, 0), (-1, 0), (0, 1), (0, -1)
    ];

    public static Graph generate(Graph seed) {
        // null check on seed graph
        ArgumentNullException.ThrowIfNull(seed);

        // create the function output from a clone of the seed graph
        Graph outputGraph = seed.clone();

        // main iteration loop defines the maximum number of attempts to generate a valid graph
        for (int i = 0; i < MAX_GENERATION_ATTEMPTS; i++) {
            // create a validated subgraph to be appended to the target graph
            Graph subgraph = createSubSubSubgraph();

            // grab the pre-appended nodes of the target graph
            List<Guid> outputGraphGuidList = outputGraph.nodes
                .Where(node => node.type is NodeType.EMPTY
                    or NodeType.ENCOUNTER
                    or NodeType.TREASURE
                    or NodeType.MINIBOSS // potentially add shop to this list
                    or NodeType.SECRET
                    or NodeType.EVENT
                    or NodeType.CHALLENGE)
                .Select(node => node.id)
                .ToList();

            // create a list of the nodes from the source subgraph
            List<Guid> subgraphGuidList = subgraph.nodes.Select(node => node.id).ToList();

            // add all the nodes from the subgraph to the target graph
            foreach (Node node in subgraph.nodes) {
                outputGraph.addNode(node);
            }

            // copy/replicate the connections between the nodes in the subgraph to the nodes added to the target graph
            foreach (Node node in subgraph.nodes) {
                foreach (Node connection in subgraph.getConnections(node)) {
                    outputGraph.addConnection(node, connection);
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
                connectionSuccess = attachSubgraphToGrid(outputGraph, subgraph, outputGraph.getNodeCoordinates());
            }

            outputGraphGuidList.Clear();
            subgraphGuidList.Clear();
            
            // post-processing
            postProcess(outputGraph);
            
            
            // if the target graph is now valid, break the loop early
            if (VALIDATOR.isValid(outputGraph)) {
                MapVisualizer.writeSvg(outputGraph, "success.dot", "success.svg");
                break;
            }
        }

        return outputGraph;
    }

    // First subgraph creation method
    private static Graph createSubgraph() {
        NodeType[] nodeTypePool = [
            NodeType.EMPTY,
            NodeType.EMPTY,
            NodeType.ENCOUNTER,
            NodeType.ENCOUNTER,
            NodeType.ENCOUNTER,
            NodeType.ENCOUNTER,
            NodeType.TREASURE,
            NodeType.MINIBOSS,
            NodeType.SECRET,
            NodeType.EVENT,
            NodeType.CHALLENGE
        ];

        while (true) {
            Graph subgraph = new Graph();
            int nodeCount = RANDOM.Next(3, 6);
            List<Node> nodeList = new List<Node>(nodeCount);

            for (int i = 0; i < nodeCount; i++) {
                Node node = new Node(nodeTypePool[RANDOM.Next(nodeTypePool.Length)], RANDOM.Next(4, 13));
                nodeList.Add(node);
                subgraph.addNode(node);
            }

            // index of array has to match index of nodes in subgraph
            int[] nodeConnectionCounts = subgraph.nodes.Select(subgraph.connectionCount).ToArray();
            while (nodeConnectionCounts.Any(count => count == 0)) {
                int alphaIndex = RANDOM.Next(nodeCount);
                int betaIndex = RANDOM.Next(nodeCount);
                Node alpha = nodeList[alphaIndex];
                Node beta = nodeList[betaIndex];

                if (subgraph.hasConnection(alpha, beta)) {
                    continue;
                }

                if (alpha.id == beta.id) {
                    continue;
                }

                subgraph.addConnection(alpha, beta);
                nodeConnectionCounts[alphaIndex]++;
                nodeConnectionCounts[betaIndex]++;
            }

            if (VALIDATOR.isSubgraphValid(subgraph)) {
                return subgraph;
            }
        }
    }

    // Second subgraph creation method
    private static Graph createSubSubgraph() {
        NodeType[] nodeTypePool = [
            NodeType.EMPTY,
            NodeType.EMPTY,
            NodeType.ENCOUNTER,
            NodeType.ENCOUNTER,
            NodeType.ENCOUNTER,
            NodeType.ENCOUNTER,
            NodeType.TREASURE,
            NodeType.MINIBOSS,
            NodeType.SECRET,
            NodeType.EVENT,
            NodeType.CHALLENGE
        ];

        while (true) {
            Graph subgraph = new Graph();
            int nodeCount = RANDOM.Next(3, 6);
            List<Node> nodeList = new List<Node>(nodeCount);

            for (int i = 0; i < nodeCount; i++) {
                Node node = new Node(nodeTypePool[RANDOM.Next(nodeTypePool.Length)], RANDOM.Next(4, 13));
                nodeList.Add(node);
                subgraph.addNode(node);
            }

            // Shuffle the node list and chain them together into a spanning tree.
            // This guarantees every node has at least one connection and the
            // subgraph is fully connected before any extra edges are added.
            List<Node> shuffled = nodeList.OrderBy(_ => RANDOM.Next()).ToList();
            for (int i = 0; i < shuffled.Count - 1; i++) {
                subgraph.addConnection(shuffled[i], shuffled[i + 1]);
            }

            // Opportunistically add extra edges between nodes that still have
            // capacity, giving the subgraph more structural variety.
            for (int i = 0; i < nodeCount; i++) {
                for (int j = i + 1; j < nodeCount; j++) {
                    Node alpha = nodeList[i];
                    Node beta = nodeList[j];

                    if (subgraph.hasConnection(alpha, beta)) {
                        continue;
                    }

                    if (subgraph.connectionCount(alpha) >= MAX_CONNECTIONS_PER_NODE) {
                        continue;
                    }

                    if (subgraph.connectionCount(beta) >= MAX_CONNECTIONS_PER_NODE) {
                        continue;
                    }

                    if (RANDOM.NextDouble() > 0.5) {
                        subgraph.addConnection(alpha, beta);
                    }
                }
            }

            if (VALIDATOR.isSubgraphValid(subgraph)) {
                return subgraph;
            }
        }
    }

    // Third subgraph creation method
    private static Graph createSubSubSubgraph() {
        NodeType[] nodeTypePool = [
            NodeType.EMPTY,
            NodeType.EMPTY,
            NodeType.ENCOUNTER,
            NodeType.ENCOUNTER,
            NodeType.ENCOUNTER,
            NodeType.ENCOUNTER,
            NodeType.TREASURE,
            NodeType.MINIBOSS,
            NodeType.SECRET,
            NodeType.EVENT,
            NodeType.CHALLENGE
        ];

        while (true) {
            Graph subgraph = new Graph();
            int targetCount = RANDOM.Next(3, 6);

            // Track which grid cells are occupied in this subgraph's local space
            Dictionary<(int x, int y), Node> occupiedCells = new Dictionary<(int x, int y), Node>();

            // Seed node at local origin
            Node seed = new Node(nodeTypePool[RANDOM.Next(nodeTypePool.Length)], RANDOM.Next(4, 13), 0, 0);
            subgraph.addNode(seed);
            occupiedCells[(0, 0)] = seed;

            int attempts = 0;
            while (subgraph.nodes.Count < targetCount && attempts++ < 200) {
                // Pick a random existing node as the expansion point
                Node parent = subgraph.nodes[RANDOM.Next(subgraph.nodes.Count)];
                // Pick a random cardinal direction
                (int dx, int dy) = CARDINAL_DIRECTIONS[RANDOM.Next(4)];
                (int nx, int ny) = (parent.x + dx, parent.y + dy);
                if (occupiedCells.ContainsKey((nx, ny))) {
                    continue;
                }
                Node child = new Node(nodeTypePool[RANDOM.Next(nodeTypePool.Length)], RANDOM.Next(4, 13), nx, ny);
                subgraph.addNode(child);
                subgraph.addConnection(parent, child); // always adjacent = valid grid edge
                occupiedCells[(nx, ny)] = child;
            }
            foreach (Node a in subgraph.nodes) {
                foreach ((int dx, int dy) in CARDINAL_DIRECTIONS) {
                    if (!occupiedCells.TryGetValue((a.x + dx, a.y + dy), out Node? b)) {
                        continue;
                    }
                    if (subgraph.hasConnection(a, b)) {
                        continue;
                    }
                    if (subgraph.connectionCount(a) >= MAX_CONNECTIONS_PER_NODE) {
                        continue;
                    }
                    if (subgraph.connectionCount(b) >= MAX_CONNECTIONS_PER_NODE) {
                        continue;
                    }
                    if (RANDOM.NextDouble() > 0.5) {
                        subgraph.addConnection(a, b);
                    }
                }
            }
            if (VALIDATOR.isSubgraphValid(subgraph)) {
                return subgraph;
            }
        }
    }

    // function to link subgraphs from createSubgraph and createSubSubgraph
    private static bool attachSubgraph(
        Graph source, 
        Graph target, 
        List<Guid> sourceCandidates,
        List<Guid> targetCandidates) {
        
        bool connectionSuccess = false;
        int attempts = 0;
        while (!connectionSuccess) {
            if (++attempts > 100) {
                break;
            }

            int outputGraphCandidateGuidIndex = RANDOM.Next(targetCandidates.Count);
            int subgraphCandidateGuidIndex = RANDOM.Next(sourceCandidates.Count);

            if (target.connectionCount(targetCandidates[outputGraphCandidateGuidIndex]) <
                MAX_CONNECTIONS_PER_NODE
                && source.connectionCount(sourceCandidates[subgraphCandidateGuidIndex]) < MAX_CONNECTIONS_PER_NODE) {
                // randomly select a candidate from each list and connect them
                target.addConnection(
                    targetCandidates[outputGraphCandidateGuidIndex],
                    sourceCandidates[subgraphCandidateGuidIndex]
                );
                connectionSuccess = true;
            }
        }
        
        return true;
    }
    
    // function to link subgraph from createSubSubSubgraph
    private static bool attachSubgraphToGrid(
    Graph outputGraph,
    Graph subgraph,
    HashSet<(int x, int y)> occupiedCells) {

        // Find all candidate attachment points on the output graph boundary:
        // cells adjacent to an existing node that are currently unoccupied
        List<(Node outputNode, int tx, int ty)> attachmentPoints = new List<(Node outputNode, int tx, int ty)>();

        foreach (Node node in outputGraph.nodes) {
            if (outputGraph.connectionCount(node) >= MAX_CONNECTIONS_PER_NODE) continue;
            foreach (var (dx, dy) in CARDINAL_DIRECTIONS) {
                (int tx, int ty) = (node.x + dx, node.y + dy);
                if (!occupiedCells.Contains((tx, ty)))
                    attachmentPoints.Add((node, tx, ty));
            }
        }

        if (attachmentPoints.Count == 0) {
            return false;
        }

        // Pick a random attachment point and a random entry node from the subgraph
        (Node anchorNode, int targetX, int targetY) = attachmentPoints[RANDOM.Next(attachmentPoints.Count)];

        Node subgraphEntry = subgraph.nodes
            .Where(n => subgraph.connectionCount(n) < MAX_CONNECTIONS_PER_NODE)
            .OrderBy(_ => RANDOM.Next())
            .FirstOrDefault();
        if (subgraphEntry is null) {
            return false;
        }

        // Calculate the offset needed to place subgraphEntry at (targetX, targetY)
        int offsetX = targetX - subgraphEntry.x;
        int offsetY = targetY - subgraphEntry.y;

        // Check for collisions before committing
        foreach (Node n in subgraph.nodes) {
            if (occupiedCells.Contains((n.x + offsetX, n.y + offsetY))) {
                return false;
            }
        }

        // Apply offset and add to output graph
        foreach (Node n in subgraph.nodes) {
            n.x += offsetX;
            n.y += offsetY;
            outputGraph.addNode(n);
            occupiedCells.Add((n.x, n.y));
        }

        foreach (Node n in subgraph.nodes) {
            foreach (Node conn in subgraph.getConnections(n)) {
                outputGraph.addConnection(n, conn);
            }
        }
        
        // Bridge the two subgraphs
        outputGraph.addConnection(anchorNode, subgraphEntry);
        return true;
    }

    private static void postProcess(Graph graph) {
        
        cleanupLimitedNodeTypes(graph);

        float encounterRatio = (float)graph.nodeTypeCount(NodeType.ENCOUNTER) / graph.nodes.Count;
        while (encounterRatio < .5f) {
            List<Node> emptyNodes = graph.nodes
                .Where(n => n.type == NodeType.EMPTY)
                .ToList();

            if (emptyNodes.Count == 0) {
                break;
            }

            graph.changeNodeType(emptyNodes[RANDOM.Next(emptyNodes.Count)], NodeType.ENCOUNTER);
            encounterRatio = (float)graph.nodeTypeCount(NodeType.ENCOUNTER) / graph.nodes.Count;
        }

        float emptyRatio = (float)graph.nodeTypeCount(NodeType.EMPTY) / graph.nodes.Count;
        while (emptyRatio < .25f) {
            List<Node> encounterNodes = graph.nodes
                .Where(n => n.type == NodeType.ENCOUNTER)
                .ToList();

            if (encounterNodes.Count == 0) {
                break;
            }

            graph.changeNodeType(encounterNodes[RANDOM.Next(encounterNodes.Count)], NodeType.EMPTY);
            emptyRatio = (float)graph.nodeTypeCount(NodeType.EMPTY) / graph.nodes.Count;
        }
    }

    private static void cleanupLimitedNodeTypes(Graph graph) {
        convertExcessNodesOfType(graph, NodeType.CHALLENGE, 1);
        convertExcessNodesOfType(graph, NodeType.SECRET, 1);
        convertExcessNodesOfType(graph, NodeType.EVENT, 1);
        convertExcessNodesOfType(graph, NodeType.MINIBOSS, 1);
        convertExcessNodesOfType(graph, NodeType.TREASURE, 4);
    }

    private static void convertExcessNodesOfType(Graph graph, NodeType type, int maxCount) {
        while (graph.nodeTypeCount(type) > maxCount) {
            Node? nodeToConvert = graph.nodes
                .Where(node => node.type == type)
                .OrderBy(graph.connectionCount)
                .FirstOrDefault();
            if (nodeToConvert is null) {
                return;
            }
            graph.changeNodeType(nodeToConvert, NodeType.EMPTY);
        }
    }
}
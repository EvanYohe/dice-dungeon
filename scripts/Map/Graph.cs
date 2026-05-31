using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceDungeon.scripts.Map;

public class Graph {
    private readonly Dictionary<Guid, HashSet<Guid>> _edges = new Dictionary<Guid, HashSet<Guid>>();

    private readonly List<Node> _nodes = new List<Node>();
    private readonly Dictionary<Guid, Node> _nodesById = new Dictionary<Guid, Node>();

    public Graph() {
    }

    public Graph(IEnumerable<Node> nodes) {
        ArgumentNullException.ThrowIfNull(nodes);
        foreach (Node node in nodes) {
            this.addNode(node);
        }
    }

    public IReadOnlyList<Node> nodes => this._nodes;

    public void addNode(Node node) {
        ArgumentNullException.ThrowIfNull(node);

        if (this._nodesById.ContainsKey(node.id)) {
            return;
        }

        this._nodes.Add(node);
        this._nodesById[node.id] = node;
        this._edges[node.id] = new HashSet<Guid>();
    }

    public void removeNode(Node node) {
        ArgumentNullException.ThrowIfNull(node);

        Node? nodeToRemove = this.findNode(node);

        if (nodeToRemove is null) {
            return;
        }

        this._edges.Remove(nodeToRemove.id);

        foreach (HashSet<Guid> connections in this._edges.Values) {
            connections.Remove(nodeToRemove.id);
        }

        this._nodes.Remove(nodeToRemove);
        this._nodesById.Remove(nodeToRemove.id);
    }

    public void clear() {
        this._edges.Clear();
        this._nodes.Clear();
        this._nodesById.Clear();
    }

    public Graph clone() {
        Graph clone = new Graph();
        Dictionary<Guid, Node> clonedNodesByOriginalId = new Dictionary<Guid, Node>();
        foreach (Node node in this._nodes) {
            Node clonedNode = new Node(node.type, node.gridArea);
            clone.addNode(clonedNode);
            clonedNodesByOriginalId[node.id] = clonedNode;
        }

        foreach (Node node in this._nodes)
        foreach (Node connection in this.getConnections(node)) {
            Node clonedNode = clonedNodesByOriginalId[node.id];
            Node clonedConnection = clonedNodesByOriginalId[connection.id];

            clone.addConnection(clonedNode, clonedConnection);
        }

        return clone;
    }

    public Node? getNodeById(Guid id) {
        return this._nodesById.GetValueOrDefault(id);
    }

    public Node? getNodeByType(NodeType type) {
        return this._nodes.FirstOrDefault(node => node.type == type);
    }

    public IReadOnlyCollection<Node> getConnections(Node node) {
        ArgumentNullException.ThrowIfNull(node);
        Node? storedNode = this.findNode(node);
        if (storedNode is null || !this._edges.TryGetValue(storedNode.id, out HashSet<Guid>? connectedIds)) {
            return Array.Empty<Node>();
        }

        List<Node> connections = new List<Node>(connectedIds.Count);
        foreach (Guid connectedId in connectedIds) {
            if (this._nodesById.TryGetValue(connectedId, out Node? connectedNode)) {
                connections.Add(connectedNode);
            }
        }

        return connections;
    }

    public HashSet<(int x, int y)> getNodeCoordinates() {
        HashSet<(int x, int y)> coordinateSet = new HashSet<(int x, int y)>();

        foreach (Node node in this.nodes) {
            coordinateSet.Add((node.x, node.y));
        }
        
        return coordinateSet;
    }

    public void addConnection(Node alpha, Node beta) {
        ArgumentNullException.ThrowIfNull(alpha);
        ArgumentNullException.ThrowIfNull(beta);
        Node? storedAlpha = this.findNode(alpha);
        Node? storedBeta = this.findNode(beta);
        if (storedAlpha is null || storedBeta is null) {
            return;
        }

        if (storedAlpha.id == storedBeta.id) {
            return;
        }

        this._edges[storedAlpha.id].Add(storedBeta.id);
        this._edges[storedBeta.id].Add(storedAlpha.id);
    }

    public void addConnection(Guid alphaId, Guid betaId) {
        if (!this._nodesById.ContainsKey(alphaId) || !this._nodesById.ContainsKey(betaId)) {
            return;
        }

        if (alphaId == betaId) {
            return;
        }

        this._edges[alphaId].Add(betaId);
        this._edges[betaId].Add(alphaId);
    }

    public void removeConnection(Node alpha, Node beta) {
        ArgumentNullException.ThrowIfNull(alpha);
        ArgumentNullException.ThrowIfNull(beta);
        Node? storedAlpha = this.findNode(alpha);
        Node? storedBeta = this.findNode(beta);
        if (storedAlpha is null || storedBeta is null) {
            return;
        }

        this._edges[storedAlpha.id].Remove(storedBeta.id);
        this._edges[storedBeta.id].Remove(storedAlpha.id);
    }

    public int connectionCount(Node node) {
        ArgumentNullException.ThrowIfNull(node);
        Node? storedNode = this.findNode(node);
        if (storedNode is null || !this._edges.TryGetValue(storedNode.id, out HashSet<Guid>? connections)) {
            return 0;
        }

        return connections.Count;
    }

    public int connectionCount(Guid id) {
        Node? storedNode = this.findNode(id);
        ArgumentNullException.ThrowIfNull(storedNode);
        if (!this._edges.TryGetValue(storedNode.id, out HashSet<Guid>? connections)) {
            return 0;
        }

        return connections.Count;
    }

    public int nodeTypeCount(NodeType type) {
        return this._nodes.Count(node => node.type == type);
    }

    public bool hasConnection(Node alpha, Node beta) {
        ArgumentNullException.ThrowIfNull(alpha);
        ArgumentNullException.ThrowIfNull(beta);
        Node? storedAlpha = this.findNode(alpha);
        Node? storedBeta = this.findNode(beta);
        if (storedAlpha is null || storedBeta is null) {
            return false;
        }

        return this._edges.TryGetValue(storedAlpha.id, out HashSet<Guid>? connections)
               && connections.Contains(storedBeta.id);
    }

    public bool hasConnectionOfType(Node node, NodeType type) {
        ArgumentNullException.ThrowIfNull(node);
        return this.getConnections(node).Any(connection => connection.type == type);
    }

    public void changeNodeType(Node node, NodeType type) {
        ArgumentNullException.ThrowIfNull(node);
        Node? storedNode = this.findNode(node);
        if (storedNode is null) {
            return;
        }

        storedNode.type = type;
    }

    public int[,] adjacencyMatrix() {
        int[,] matrix = new int[this._nodes.Count, this._nodes.Count];
        Dictionary<Guid, int> nodeIndexes = new Dictionary<Guid, int>();
        for (int index = 0; index < this._nodes.Count; index++) {
            nodeIndexes[this._nodes[index].id] = index;
        }

        for (int row = 0; row < this._nodes.Count; row++) {
            Guid nodeId = this._nodes[row].id;
            if (!this._edges.TryGetValue(nodeId, out HashSet<Guid>? connections)) {
                continue;
            }

            foreach (Guid connectionId in connections) {
                if (nodeIndexes.TryGetValue(connectionId, out int column)) {
                    matrix[row, column] = 1;
                }
            }
        }

        return matrix;
    }

    private Node? findNode(Node node) {
        ArgumentNullException.ThrowIfNull(node);
        return this._nodesById.GetValueOrDefault(node.id);
    }

    private Node? findNode(Guid id) {
        return this._nodesById.GetValueOrDefault(id);
    }
}
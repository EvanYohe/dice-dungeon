using System;
using System.Collections.Generic;
using Enumerable = System.Linq.Enumerable;

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
            AddNode(node);
        }
    }

    public IReadOnlyList<Node> Nodes { get { return _nodes; } }

    public void AddNode(Node node) {
        ArgumentNullException.ThrowIfNull(node);

        if (_nodesById.ContainsKey(node.Id)) {
            return;
        }

        _nodes.Add(node);
        _nodesById[node.Id] = node;
        _edges[node.Id] = new HashSet<Guid>();
    }

    public void RemoveNode(Node node) {
        ArgumentNullException.ThrowIfNull(node);

        Node? nodeToRemove = FindNode(node);

        if (nodeToRemove is null) {
            return;
        }

        _edges.Remove(nodeToRemove.Id);

        foreach (HashSet<Guid> connections in _edges.Values) {
            connections.Remove(nodeToRemove.Id);
        }

        _nodes.Remove(nodeToRemove);
        _nodesById.Remove(nodeToRemove.Id);
    }

    public void Clear() {
        _edges.Clear();
        _nodes.Clear();
        _nodesById.Clear();
    }

    public Graph Clone() {
        Graph clone = new Graph();
        Dictionary<Guid, Node> clonedNodesByOriginalId = new Dictionary<Guid, Node>();
        foreach (Node node in _nodes) {
            Node clonedNode = new Node(node.Type, node.GridArea);
            clone.AddNode(clonedNode);
            clonedNodesByOriginalId[node.Id] = clonedNode;
        }

        foreach (Node node in _nodes)
        foreach (Node connection in GetConnections(node)) {
            Node clonedNode = clonedNodesByOriginalId[node.Id];
            Node clonedConnection = clonedNodesByOriginalId[connection.Id];

            clone.AddConnection(clonedNode, clonedConnection);
        }

        return clone;
    }

    public Node? GetNodeById(Guid id) {
        return _nodesById.GetValueOrDefault(id);
    }

    public Node? GetNodeByType(NodeType type) {
        return Enumerable.FirstOrDefault(_nodes, node => node.Type == type);
    }

    public IReadOnlyCollection<Node> GetConnections(Node node) {
        ArgumentNullException.ThrowIfNull(node);
        Node? storedNode = FindNode(node);
        if (storedNode is null || !_edges.TryGetValue(storedNode.Id, out HashSet<Guid>? connectedIds)) {
            return Array.Empty<Node>();
        }

        List<Node> connections = new List<Node>(connectedIds.Count);
        foreach (Guid connectedId in connectedIds) {
            if (_nodesById.TryGetValue(connectedId, out Node? connectedNode)) {
                connections.Add(connectedNode);
            }
        }

        return connections;
    }

    public HashSet<(int x, int y)> GetNodeCoordinates() {
        HashSet<(int x, int y)> coordinateSet = new HashSet<(int x, int y)>();

        foreach (Node node in Nodes) {
            coordinateSet.Add((node.X, node.Y));
        }

        return coordinateSet;
    }

    public void AddConnection(Node alpha, Node beta) {
        ArgumentNullException.ThrowIfNull(alpha);
        ArgumentNullException.ThrowIfNull(beta);
        Node? storedAlpha = FindNode(alpha);
        Node? storedBeta = FindNode(beta);
        if (storedAlpha is null || storedBeta is null) {
            return;
        }

        if (storedAlpha.Id == storedBeta.Id) {
            return;
        }

        _edges[storedAlpha.Id].Add(storedBeta.Id);
        _edges[storedBeta.Id].Add(storedAlpha.Id);
    }

    public void AddConnection(Guid alphaId, Guid betaId) {
        if (!_nodesById.ContainsKey(alphaId) || !_nodesById.ContainsKey(betaId)) {
            return;
        }

        if (alphaId == betaId) {
            return;
        }

        _edges[alphaId].Add(betaId);
        _edges[betaId].Add(alphaId);
    }

    public void RemoveConnection(Node alpha, Node beta) {
        ArgumentNullException.ThrowIfNull(alpha);
        ArgumentNullException.ThrowIfNull(beta);
        Node? storedAlpha = FindNode(alpha);
        Node? storedBeta = FindNode(beta);
        if (storedAlpha is null || storedBeta is null) {
            return;
        }

        _edges[storedAlpha.Id].Remove(storedBeta.Id);
        _edges[storedBeta.Id].Remove(storedAlpha.Id);
    }

    public int ConnectionCount(Node node) {
        ArgumentNullException.ThrowIfNull(node);
        Node? storedNode = FindNode(node);
        if (storedNode is null || !_edges.TryGetValue(storedNode.Id,
                out HashSet<Guid>? connections)) {
            return 0;
        }

        return connections.Count;
    }

    public int ConnectionCount(Guid id) {
        Node? storedNode = FindNode(id);
        ArgumentNullException.ThrowIfNull(storedNode);
        if (!_edges.TryGetValue(storedNode.Id, out HashSet<Guid>? connections)) {
            return 0;
        }

        return connections.Count;
    }

    public int NodeTypeCount(NodeType type) {
        return Enumerable.Count(_nodes, node => node.Type == type);
    }

    public bool HasConnection(Node alpha, Node beta) {
        ArgumentNullException.ThrowIfNull(alpha);
        ArgumentNullException.ThrowIfNull(beta);
        Node? storedAlpha = FindNode(alpha);
        Node? storedBeta = FindNode(beta);
        if (storedAlpha is null || storedBeta is null) {
            return false;
        }

        return _edges.TryGetValue(storedAlpha.Id, out HashSet<Guid>? connections) && connections.Contains(storedBeta.Id);
    }

    public bool HasConnectionOfType(Node node, NodeType type) {
        ArgumentNullException.ThrowIfNull(node);
        return Enumerable.Any(GetConnections(node), connection => connection.Type == type);
    }

    public void ChangeNodeType(Node node, NodeType type) {
        ArgumentNullException.ThrowIfNull(node);
        Node? storedNode = FindNode(node);
        if (storedNode is null) {
            return;
        }

        storedNode.Type = type;
    }

    public int[,] AdjacencyMatrix() {
        int[,] matrix = new int[_nodes.Count, _nodes.Count];
        Dictionary<Guid, int> nodeIndexes = new Dictionary<Guid, int>();
        for (int index = 0; index < _nodes.Count; index++) {
            nodeIndexes[_nodes[index].Id] = index;
        }

        for (int row = 0; row < _nodes.Count; row++) {
            Guid nodeId = _nodes[row].Id;
            if (!_edges.TryGetValue(nodeId, out HashSet<Guid>? connections)) {
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

    private Node? FindNode(Node node) {
        ArgumentNullException.ThrowIfNull(node);
        return _nodesById.GetValueOrDefault(node.Id);
    }

    private Node? FindNode(Guid id) {
        return _nodesById.GetValueOrDefault(id);
    }
}
using System;

namespace DiceDungeon.scripts.Map;

public class Node {
    
    public Guid Id { get; } = Guid.NewGuid();
    public NodeType Type { get; internal set; }
    public int GridArea { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    
    public Node(NodeType type, int gridArea) {
        
        Type = type;
        GridArea = gridArea;
        X = 0;
        Y = 0;
    }

    public Node(NodeType type, int gridArea, int x, int y) {
        
        Type = type;
        GridArea = gridArea;
        X = x;
        Y = y;
    }
}
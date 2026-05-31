using System;

namespace DiceDungeon.scripts.Map;

public class Node {
    
    public Node(NodeType type, int gridArea) {
        this.type = type;
        this.gridArea = gridArea;
        this.x = 0;
        this.y = 0;
    }

    public Node(NodeType type, int gridArea, int x, int y) {
        this.type = type;
        this.gridArea = gridArea;
        this.x = x;
        this.y = y;
    }

    public Guid id { get; } = Guid.NewGuid();
    public NodeType type { get; internal set; }
    public int gridArea { get; set; }
    public int x { get; set; }
    public int y { get; set; }
}
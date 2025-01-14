using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node
{
    public Vector3Int Position { get; set; }
    public Node Parent { get; set; }
    public bool IsWalkable { get; set; }
    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost => GCost + HCost;
    
    public Node(Vector3Int position, bool isWalkable)
    {
        Position = position;
        IsWalkable = isWalkable;
    }
}
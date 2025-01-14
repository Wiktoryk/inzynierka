using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding
{
    private static Pathfinding instance;
    public static Pathfinding Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Pathfinding();
            }
            return instance;
        }
    }
    
    private Dictionary<Vector3Int, Node> defaultGrid = new Dictionary<Vector3Int, Node>();
    public void InitGrid(Vector3 center)
    {
        for (int x = -6; x <= 6; x++)
        {
            for (int y = -4; y <= 4; y++)
            {
                Vector3Int position = Vector3Int.FloorToInt(new Vector3(x * 0.64f + 0.32f, y * 0.64f + 0.32f, 0) + center);
                defaultGrid[position] = new Node(position, true);
            }
        }
    }
    public List<Vector3> FindPath(Vector3Int start, Vector3Int target, HashSet<Vector3Int> dynamic)
    {
        Dictionary<Vector3Int, Node> grid = new Dictionary<Vector3Int, Node>(defaultGrid);
        foreach (Vector3Int obstacle in dynamic)
        {
            if (grid.ContainsKey(obstacle))
            {
                grid[obstacle].IsWalkable = false;
            }
        }
        grid[target].IsWalkable = true;
        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        Node startNode = grid[start];
        Node targetNode = grid[target];

        openList.Add(startNode);
        int iterations = 0;
        while (openList.Count > 0 && iterations < 100)
        {
            Node currentNode = openList.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (Node neighbor in GetNeighbors(currentNode, grid))
            {
                if (!neighbor.IsWalkable || closedList.Contains(neighbor))
                    continue;

                int tentativeGCost = currentNode.GCost + GetDistance(currentNode, neighbor);

                if (tentativeGCost < neighbor.GCost || !openList.Contains(neighbor))
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = GetDistance(neighbor, targetNode);
                    neighbor.Parent = currentNode;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
            iterations++;
        }
        return new List<Vector3>();
    }
    
    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        for (int i = 0; i < path.Count; i++)
        {
            path[i] = new Vector3(path[i].x * 0.64f + 0.32f, path[i].y * 0.64f + 0.32f, 0);
        }
        return path;
    }
    
    private List<Node> GetNeighbors(Node node, Dictionary<Vector3Int, Node> grid)
    {
        List<Node> neighbors = new List<Node>();
        Vector3Int[] directions = 
        {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };

        foreach (Vector3Int direction in directions)
        {
            Vector3Int neighborPos = node.Position + direction;
            if (grid.TryGetValue(neighborPos, out Node neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.Position.x - b.Position.x);
        int dstY = Mathf.Abs(a.Position.y - b.Position.y);

        return dstX + dstY;
    }
}
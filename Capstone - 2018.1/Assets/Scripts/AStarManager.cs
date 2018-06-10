using System.Collections;
using System.Collections.Generic;
using MapClasses;
using UnityEngine;

namespace AStar
{
    public interface INode
    {
        int Index { get; }
        int X { get; }
        int Z { get; }
        NodeInfo NodeInfo { get; }
    }
    public class NodeInfo
    {
        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost { get { return GCost + HCost; } }
        public INode Parent { get; set; }
    }
    public class Path
    {
        List<INode> pathList;
        int iter;

        public int Cost
        {
            get
            {
                int pathCost = 0;
                foreach (INode node in pathList)
                {
                    pathCost += node.NodeInfo.FCost;
                }
                return pathCost;
            }
        }

        public Path()
        {
            pathList = new List<INode>();
            ResetNext();
        }

        public Path(List<INode> path)
        {
            pathList = new List<INode>();
            pathList.AddRange(path);
            ResetNext();
        }

        public void Add(INode node)
        {
            pathList.Add(node);
            ResetNext();
        }
        public INode Next()
        {
            if (iter < 0 || pathList.Count <= 0)
            {
                // Debug.LogError("ERROR: Attempting to get next node while path is empty.");
                return null;
            }

            return pathList[iter--];
        }
        public void ResetNext() => iter = pathList.Count - 1;
    }

    public class AStarManager
    {
        public Level Level { get; set; }
        private bool ExecuteAStar(INode origin, INode destination)
        {
            if (Level == null)
            {
                Debug.LogError("ERROR: Level not assigned in AStarManager.");
                return false;
            }
            // Instantiates open set and closed set.
            // TODO: Replace with a heap.
            List<INode> openSet = new List<INode>();
            Dictionary<int, INode> closedSet = new Dictionary<int, INode>();

            // Adds the origin node as the initial node.
            origin.NodeInfo.Parent = null;
            origin.NodeInfo.GCost = 0;
            origin.NodeInfo.HCost = CalculateHCost(origin, destination);

            openSet.Add(origin);

            // While there are valid nodes to be explored.
            bool pathFound = false;
            while (openSet.Count > 0)
            {
                // Get the node in the open set with the lowest F cost.
                // TODO: Replace with a Heap. Currently iterates through the entire openSet and searches for the one with the lowest FCost.
                INode currNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].NodeInfo.FCost < currNode.NodeInfo.FCost ||
                        (openSet[i].NodeInfo.FCost == currNode.NodeInfo.FCost && openSet[i].NodeInfo.HCost < currNode.NodeInfo.HCost))
                    {
                        currNode = openSet[i];
                    }
                }

                // Remove the current node from the open set and add it to the closed set.
                openSet.Remove(currNode);
                closedSet.Add(closedSet.Count, currNode);

                // If it's the destination, exit the loop.
                if (currNode.Index == destination.Index)
                {
                    pathFound = true;
                    break;
                }

                Cell currCell = currNode as Cell;
                // For each neighbor of the current node.

                // foreach (Direction direction in typeof(Direction).GetEnumValues())
                // {
                // Cell neighbor = Level.GetNeighbor(currCell, direction);
                // Skip neighbors that are impassable or already in the closed set.
                // if (neighbor == null || closedSet.ContainsValue(neighbor))
                //     continue;
                foreach (Cell neighbor in Level.GetNeighbors(currCell))
                {
                    if (!Level.HasConnection(currCell, neighbor) ||
                        neighbor.Locked ||
                        (neighbor.Occupant != null && !neighbor.Occupant.IsPlayer) ||
                        closedSet.ContainsValue(neighbor))
                        continue;

                    // If neighbor is in the open set and the new path is shorter, update it.
                    // Or the neighbor is not in the open set, update it and add it.
                    if (openSet.Contains(neighbor) &&
                        (currCell.NodeInfo.GCost + 1 + neighbor.NodeInfo.HCost) < neighbor.NodeInfo.FCost)
                    {
                        neighbor.NodeInfo.Parent = currCell;
                        neighbor.NodeInfo.GCost = currCell.NodeInfo.GCost + 1;
                        neighbor.NodeInfo.HCost = CalculateHCost(neighbor, destination);
                    }
                    else if (!openSet.Contains(neighbor))
                    {
                        neighbor.NodeInfo.Parent = currCell;
                        neighbor.NodeInfo.GCost = currCell.NodeInfo.GCost + 1;
                        neighbor.NodeInfo.HCost = CalculateHCost(neighbor, destination);

                        openSet.Add(neighbor);
                    }
                }

            }

            return pathFound;
        }

        public bool BestPathExists(INode origin, INode destination)
        {
            return ExecuteAStar(origin, destination);
        }

        public Path GetBestPath(INode origin, INode destination)
        {
            // Execute A*.
            bool pathFound = ExecuteAStar(origin, destination);

            // Create path.
            Path path = new Path();

            // If a path was found, start at the destination node, add it to the list,
            // and move up to the Node.Parent until at the starting location.
            if (pathFound)
            {
                // path = new Path();
                INode node = destination;
                while (node != null && node != origin)
                {
                    path.Add(node);

                    node = node.NodeInfo.Parent;
                }
            }

            return path;
        }

        public int CalculateHCost(INode node1, INode node2)
        {
            int distX = Mathf.Abs(node1.X - node2.X);
            int distZ = Mathf.Abs(node1.Z - node2.Z);
            return (distX + distZ);
        }
    }
}

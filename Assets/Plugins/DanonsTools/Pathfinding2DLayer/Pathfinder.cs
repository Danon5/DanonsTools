using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DanonsTools.Pathfinding2DLayer
{
    public sealed class Pathfinder
    {
        private readonly Dictionary<Vector2Int, PathNode> _openList = new  Dictionary<Vector2Int, PathNode>();
        private readonly Dictionary<Vector2Int, PathNode> _closedList = new Dictionary<Vector2Int, PathNode>();
        private readonly INodeGraph _nodeGraph;

        public Pathfinder(in INodeGraph nodeGraph)
        {
            _nodeGraph = nodeGraph;
        }

        public PathResult FindPathNonAlloc(in Vector2 startPos, in Vector2 goalPos, in List<Vector2> path, 
            in bool allowPartialPaths = false, in float minWalkableRadius = .5f, in Heuristic heuristic = Heuristic.Euclidean)
        {
            _openList.Clear();
            _closedList.Clear();

            _nodeGraph.OnFindPathStarted(minWalkableRadius);

            var startCell = new Vector2Int((int)startPos.x, (int)startPos.y);
            var goalCell = new Vector2Int((int)goalPos.x, (int)goalPos.y);
            var startNode = new PathNode(startCell);
            _openList.Add(startCell, startNode);
            _nodeGraph.OnNodeAddedToOpenList(startNode);

            var nodesVisited = 0;
            
            while (_openList.Count > 0)
            {
                var currentNode = _openList.Values.OrderBy(node => node.F).First();
                
                _openList.Remove(currentNode.Cell);
                _closedList.Add(currentNode.Cell, currentNode);

                if (currentNode.Cell == goalCell)
                {
                    _nodeGraph.TracePathNonAlloc(currentNode, path);
                    return PathResult.FullPath;
                }

                if (nodesVisited > _nodeGraph.SearchLimit)
                {
                    if (allowPartialPaths)
                    {
                        _nodeGraph.TracePathNonAlloc(GetNodeClosestToCell(goalCell, _closedList.Values), path);
                        return PathResult.PartialPath;
                    }
                    
                    _nodeGraph.TracePathNonAlloc(null, path);
                    return PathResult.NoPath;
                }

                for (var x = -1; x <= 1; x++)
                {
                    for (var y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        
                        var cell = currentNode.Cell + new Vector2Int(x, y);

                        if (_closedList.ContainsKey(cell))
                            continue;
                        
                        if (!_nodeGraph.IsValidNode(cell))
                            continue;

                        if (!_nodeGraph.IsWalkableNode(cell))
                            continue;

                        var child = new PathNode(cell);
                        var isDiagonal = Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1;
                        var additiveGCost = _nodeGraph.GetNodeAdditiveGCost(cell);
                        
                        child.G = currentNode.G + additiveGCost + (isDiagonal ? _nodeGraph.DiagonalCost : _nodeGraph.CardinalCost);
                        child.H = CalculateHeuristic(_nodeGraph, heuristic, cell, goalCell);
                        child.UpdateFValue();
                        child.ParentNode = currentNode;

                        if (_openList.TryGetValue(cell, out var nodeAlreadyInOpenList) && child.F > nodeAlreadyInOpenList.F)
                            continue;

                        _openList[cell] = child;
                        _nodeGraph.OnNodeAddedToOpenList(child);
                    }
                }

                nodesVisited++;
            }

            return PathResult.NoPath;
        }

        private static PathNode GetNodeClosestToCell(in Vector2Int cell, in IEnumerable<PathNode> nodes)
        {
            var closestDist = float.MaxValue;
            PathNode closestNode = default;
            
            foreach (var node in nodes)
            {
                var dist = Vector2.SqrMagnitude(node.Cell - cell);

                if (dist > closestDist) continue;

                closestDist = dist;
                closestNode = node;
            }

            return closestNode;
        }

        private static float CalculateHeuristic(in INodeGraph nodeGraph, in Heuristic heuristic, in Vector2Int cell, in Vector2Int goalCell)
        {
            var dx = Mathf.Abs(cell.x - goalCell.x);
            var dy = Mathf.Abs(cell.y - goalCell.y);

            return heuristic switch
            {
                Heuristic.Manhattan => dx + dy,
                Heuristic.Diagonal => nodeGraph.CardinalCost * (dx + dy) + (nodeGraph.DiagonalCost - 2f * nodeGraph.CardinalCost) * Mathf.Min(dx, dy),
                Heuristic.Euclidean => Mathf.Sqrt(Mathf.Pow(cell.x - goalCell.x, 2) + Mathf.Pow(cell.y - goalCell.y, 2)),
                _ => default
            };
        }
    }
}
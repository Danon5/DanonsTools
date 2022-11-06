using System.Collections.Generic;
using UnityEngine;

namespace DanonsTools.Pathfinding2D
{
    public interface INodeGraph
    {
        public float CardinalCost { get; }
        public float DiagonalCost { get; }
        public int SearchLimit { get; }

        public void OnFindPathStarted(in float minWalkableRadius);
        public void OnNodeAddedToOpenList(in PathNode pathNode);
        public bool IsValidNode(in Vector2Int cell);
        public bool IsWalkableNode(in Vector2Int cell);
        public float GetNodeAdditiveGCost(in Vector2Int cell);
        public void TracePathNonAlloc(in PathNode fromNode, in List<Vector2> path);
    }
}
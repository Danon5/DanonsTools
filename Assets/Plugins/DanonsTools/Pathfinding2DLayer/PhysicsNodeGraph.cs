using System.Collections.Generic;
using UnityEngine;

namespace DanonsTools.Pathfinding2DLayer
{
    public sealed class PhysicsNodeGraph : INodeGraph
    {
        public float CardinalCost { get; } = 1f;
        public float DiagonalCost { get; } = Mathf.Sqrt(2f);
        public int SearchLimit { get; } = 5000;

        private readonly Dictionary<Vector2Int, bool> _walkableCache = new Dictionary<Vector2Int, bool>();
        private readonly Vector2 _halfCell = new Vector2(.5f, .5f);

        private float _physicsCheckRadius;

        public void OnFindPathStarted(in float minWalkableRadius)
        {
            _walkableCache.Clear();
            _physicsCheckRadius = minWalkableRadius;
        }

        public void OnNodeAddedToOpenList(in PathNode pathNode)
        {
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    var cell = pathNode.Cell + new Vector2Int(x, y);

                    if (_walkableCache.ContainsKey(cell))
                        continue;
                    
                    UpdateWalkableStateForCell(cell);
                }
            }
        }

        public bool IsValidNode(in Vector2Int cell)
        {
            return true;
        }

        public bool IsWalkableNode(in Vector2Int cell)
        {
            return _walkableCache.TryGetValue(cell, out var walkable) && walkable;
        }

        public float GetNodeAdditiveGCost(in Vector2Int cell)
        {
            return 0f;
        }

        public void TracePathNonAlloc(in PathNode fromNode, in List<Vector2> path)
        {
            path.Clear();

            if (fromNode == null) return;
            
            var node = fromNode;

            while (node.ParentNode != null)
            {
                path.Add(node.Cell + _halfCell);
                node = node.ParentNode;
            }
        }

        private void UpdateWalkableStateForCell(in Vector2Int cell)
        {
            var checkPos = cell + _halfCell;
            _walkableCache[cell] = Physics2D.OverlapCircle(checkPos, _physicsCheckRadius) == null;
        }
    }
}
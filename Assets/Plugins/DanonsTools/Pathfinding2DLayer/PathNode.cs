using UnityEngine;

namespace DanonsTools.Pathfinding2DLayer
{
    public sealed class PathNode
    {
        public Vector2Int Cell { get; }
        public PathNode ParentNode { set; get; }
        public float H { get; set; }
        public float G { get; set;  }
        public float F { get; private set; }

        public PathNode(in Vector2Int cell)
        {
            Cell = cell;
        }

        public void UpdateFValue()
        {
            F = H + G;
        }
    }
}
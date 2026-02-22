using UnityEngine;

namespace ProjectLightsOut.Managers
{
    public struct WallBlockData
    {
        public RectInt area;

        public WallEdgeData top;
        public WallEdgeData bottom;
        public WallEdgeData left;
        public WallEdgeData right;
    }
}

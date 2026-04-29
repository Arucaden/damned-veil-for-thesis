
namespace ProjectLightsOut.Managers
{
    public struct WallEdgeData
    {
        public WallPhysics physics;
        public WallRole role;
    }

    [System.Flags]
    public enum WallPhysics
    {
        None        = 0,
        Solid       = 1 << 0,
        Reflective  = 1 << 1
    }

    public enum WallRole
    {
        Structural,
        Noise
    }
}

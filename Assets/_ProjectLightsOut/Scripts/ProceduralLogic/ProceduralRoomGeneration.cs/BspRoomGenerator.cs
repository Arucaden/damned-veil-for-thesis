using UnityEngine;
using System.Collections.Generic;

/// Simple BSP room generator for 2D orthogonal dungeons.
/// Deterministic, inspector-friendly, with Gizmos preview.
public class BspRoomGenerator : MonoBehaviour
{
    [Header("Map")]
    public Vector2Int mapSize = new Vector2Int(64, 40);
    public int seed = 12345;
    [Range(1,8)] public int maxDepth = 4;
    public int minLeafSize = 12;           // minimal width/height of a leaf
    public int corridorWidth = 2;          // tiles
    public int roomPadding = 1;            // inner margin from leaf border

    [Header("Rooms")]
    public Vector2Int minRoomSize = new Vector2Int(6, 6);
    public Vector2Int maxRoomSize = new Vector2Int(14, 10);

    [Header("Obstacles (optional)")]
    [Range(0,3)] public int maxObstaclesPerRoom = 1;
    public Vector2Int minObsSize = new Vector2Int(2, 2);
    public Vector2Int maxObsSize = new Vector2Int(4, 3);
    public int obsClearance = 2; // min distance from room walls and corridor

    [Header("Runtime")]
    public bool generateOnStart = true;

    [Header("Debug")]
    public bool drawGizmos = true;

    // Outputs (read-only at runtime)
    public RectInt OuterBounds => new RectInt(0, 0, mapSize.x, mapSize.y);
    public List<RectInt> Rooms { get; private set; } = new List<RectInt>();
    public List<RectInt> Corridors { get; private set; } = new List<RectInt>();
    public List<RectInt> Obstacles { get; private set; } = new List<RectInt>();

    System.Random rng;

    // ---------- Public API ----------
    [ContextMenu("Generate BSP")]
    public void Generate()
    {
        rng = new System.Random(seed);
        Rooms.Clear(); Corridors.Clear(); Obstacles.Clear();

        var root = new Leaf(new RectInt(0, 0, mapSize.x, mapSize.y));
        SplitRecursive(root, 0);
        CreateRooms(root);
        ConnectSiblings(root);
        AddObstacles();
    }

    // Build wall segments for your specular-path spawner:
    // outer rectangle + all obstacle rectangles (rooms & corridors are open).
    public struct WallSeg { public Vector2 a,b; public bool vertical;
        public WallSeg(Vector2 A, Vector2 B, bool v){ a=A; b=B; vertical=v; } }

    public List<WallSeg> BuildWallSegments()
    {
        var segs = new List<WallSeg>(128);
        // Outer bounds
        AddRectSegments(segs, OuterBounds);
        // Obstacles as solid blocks
        foreach (var r in Obstacles) AddRectSegments(segs, r);
        return segs;
    }

    // ---------- Unity lifecycle ----------
    void Start(){ if (generateOnStart) Generate(); }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
        Color cRoom = new Color(0.2f, 0.85f, 0.4f, 0.35f);
        Color cCorr = new Color(1f, 0.9f, 0.2f, 0.35f);
        Color cObs  = new Color(0.85f, 0.2f, 0.25f, 0.5f);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(new Vector3(OuterBounds.center.x, OuterBounds.center.y, 0f), new Vector3(OuterBounds.size.x, OuterBounds.size.y, 0.1f));

        Gizmos.color = cRoom;
        foreach (var r in Rooms) Gizmos.DrawCube(Center3(r), Size3(r));

        Gizmos.color = cCorr;
        foreach (var r in Corridors) Gizmos.DrawCube(Center3(r), Size3(r));

        Gizmos.color = cObs;
        foreach (var r in Obstacles) Gizmos.DrawCube(Center3(r), Size3(r));
    }

    // ---------- Core BSP ----------
    class Leaf {
        public RectInt rect;
        public Leaf left, right;
        public RectInt? room; // set for leaves
        public Leaf(RectInt r){ rect=r; }
        public bool IsLeaf => left==null && right==null;
    }

    void SplitRecursive(Leaf node, int depth)
    {
        if (depth >= maxDepth || node.rect.width < minLeafSize*2 && node.rect.height < minLeafSize*2)
            return;

        bool splitVert;
        if (node.rect.width >= node.rect.height*1.25f) splitVert = true;
        else if (node.rect.height >= node.rect.width*1.25f) splitVert = false;
        else splitVert = rng.NextDouble() < 0.5;

        if (splitVert) {
            int minX = node.rect.xMin + minLeafSize;
            int maxX = node.rect.xMax - minLeafSize;
            if (maxX <= minX) return;
            int x = rng.Next(minX, maxX);
            node.left  = new Leaf(new RectInt(node.rect.xMin, node.rect.yMin, x - node.rect.xMin, node.rect.height));
            node.right = new Leaf(new RectInt(x, node.rect.yMin, node.rect.xMax - x, node.rect.height));
        } else {
            int minY = node.rect.yMin + minLeafSize;
            int maxY = node.rect.yMax - minLeafSize;
            if (maxY <= minY) return;
            int y = rng.Next(minY, maxY);
            node.left  = new Leaf(new RectInt(node.rect.xMin, node.rect.yMin, node.rect.width, y - node.rect.yMin));
            node.right = new Leaf(new RectInt(node.rect.xMin, y, node.rect.width, node.rect.yMax - y));
        }
        SplitRecursive(node.left,  depth+1);
        SplitRecursive(node.right, depth+1);
    }

    void CreateRooms(Leaf node)
    {
        if (node == null) return;
        if (node.IsLeaf) {
            // choose room size with padding
            int maxW = Mathf.Min(maxRoomSize.x, node.rect.width  - 2*roomPadding);
            int maxH = Mathf.Min(maxRoomSize.y, node.rect.height - 2*roomPadding);
            int minW = Mathf.Min(minRoomSize.x, maxW);
            int minH = Mathf.Min(minRoomSize.y, maxH);
            if (maxW < minRoomSize.x || maxH < minRoomSize.y) { node.room = null; return; }

            int w = rng.Next(minW, maxW+1);
            int h = rng.Next(minH, maxH+1);
            int x = rng.Next(node.rect.xMin + roomPadding, node.rect.xMax - roomPadding - w + 1);
            int y = rng.Next(node.rect.yMin + roomPadding, node.rect.yMax - roomPadding - h + 1);
            var r = new RectInt(x, y, w, h);
            Rooms.Add(r);
            node.room = r;
        } else {
            CreateRooms(node.left);
            CreateRooms(node.right);
        }
    }

    void ConnectSiblings(Leaf node)
    {
        if (node == null || node.IsLeaf) return;
        ConnectSiblings(node.left);
        ConnectSiblings(node.right);

        if (!TryGetAnyRoom(node.left, out RectInt a)) return;
        if (!TryGetAnyRoom(node.right, out RectInt b)) return;

        // L-shaped corridor between centers
        Vector2Int ca = Vector2Int.RoundToInt(a.center); Vector2Int cb = Vector2Int.RoundToInt(b.center);
        if (rng.NextDouble() < 0.5) {
            // horizontal then vertical
            var h = RectFromToX(ca, cb, corridorWidth);
            var v = RectFromToY(new Vector2Int(cb.x, ca.y), cb, corridorWidth);
            Corridors.Add(h); Corridors.Add(v);
        } else {
            // vertical then horizontal
            var v = RectFromToY(ca, cb, corridorWidth);
            var h = RectFromToX(new Vector2Int(ca.x, cb.y), cb, corridorWidth);
            Corridors.Add(v); Corridors.Add(h);
        }
    }

    bool TryGetAnyRoom(Leaf node, out RectInt r)
    {
        if (node == null) { r = default; return false; }
        if (node.room.HasValue) { r = node.room.Value; return true; }
        // climb down: prefer left, then right
        if (TryGetAnyRoom(node.left, out r)) return true;
        if (TryGetAnyRoom(node.right, out r)) return true;
        r = default; return false;
    }

    // ---------- Obstacles ----------
    void AddObstacles()
    {
        if (maxObstaclesPerRoom <= 0) return;
        foreach (var room in Rooms)
        {
            int k = rng.Next(0, maxObstaclesPerRoom+1);
            for (int i=0;i<k;i++)
            {
                int maxW = Mathf.Min(maxObsSize.x, room.width  - 2*obsClearance);
                int maxH = Mathf.Min(maxObsSize.y, room.height - 2*obsClearance);
                if (maxW < minObsSize.x || maxH < minObsSize.y) break;

                const int MAX_ATTEMPTS = 24;
                bool placed = false;
                for (int attempt=0; attempt<MAX_ATTEMPTS && !placed; attempt++)
                {
                    int w = rng.Next(minObsSize.x, maxW+1);
                    int h = rng.Next(minObsSize.y, maxH+1);
                    int x = rng.Next(room.xMin + obsClearance, room.xMax - obsClearance - w + 1);
                    int y = rng.Next(room.yMin + obsClearance, room.yMax - obsClearance - h + 1);
                    var obs = new RectInt(x, y, w, h);

                    if (!OverlapsAnyInflated(obs, Corridors, obsClearance))
                    {
                        Obstacles.Add(obs);
                        placed = true;
                    }
                }
                // kalau gagal 24x, skip obstacle ini tanpa i--
            }
        }
    }

    // ---------- Helpers ----------
    static RectInt RectFromToX(Vector2Int a, Vector2Int b, int width)
    {
        int y = a.y - width/2;
        int x0 = Mathf.Min(a.x, b.x); int x1 = Mathf.Max(a.x, b.x);
        return new RectInt(x0, y, Mathf.Max(1, x1 - x0 + 1), Mathf.Max(1, width));
    }
    static RectInt RectFromToY(Vector2Int a, Vector2Int b, int width)
    {
        int x = a.x - width/2;
        int y0 = Mathf.Min(a.y, b.y); int y1 = Mathf.Max(a.y, b.y);
        return new RectInt(x, y0, Mathf.Max(1, width), Mathf.Max(1, y1 - y0 + 1));
    }
    static bool OverlapsAnyInflated(RectInt r, List<RectInt> list, int pad)
    {
        var rr = new RectInt(r.xMin - pad, r.yMin - pad, r.width + 2*pad, r.height + 2*pad);
        foreach (var q in list) {
            var qq = new RectInt(q.xMin - pad, q.yMin - pad, q.width + 2*pad, q.height + 2*pad);
            if (rr.Overlaps(qq)) return true;
        }
        return false;
    }
    static void AddRectSegments(List<WallSeg> segs, RectInt r)
    {
        Vector2 bl = new Vector2(r.xMin, r.yMin);
        Vector2 br = new Vector2(r.xMax, r.yMin);
        Vector2 tl = new Vector2(r.xMin, r.yMax);
        Vector2 tr = new Vector2(r.xMax, r.yMax);
        segs.Add(new WallSeg(bl, br, false)); // bottom
        segs.Add(new WallSeg(tl, tr, false)); // top
        segs.Add(new WallSeg(bl, tl, true));  // left
        segs.Add(new WallSeg(br, tr, true));  // right
    }
    static Vector3 Center3(RectInt r) => new Vector3(r.center.x, r.center.y, 0f);
    static Vector3 Size3(RectInt r)   => new Vector3(r.width, r.height, 0.1f);
}

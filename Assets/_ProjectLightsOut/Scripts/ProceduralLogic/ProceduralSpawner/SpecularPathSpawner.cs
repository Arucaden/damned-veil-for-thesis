using UnityEngine;
using System.Collections.Generic;

/// Simplified specular-path enemy spawner.
/// - Traces candidate rays (K trials) and picks the first valid one.
/// - Places enemies along the polyline at fixed spacings.
/// - Valid means: clearance from walls and bounce budget respected.
/// - If none valid, falls back to the most clear candidate (or 45°).
public class SpecularPathSpawner : MonoBehaviour
{
    [Header("Scene References")]
    public BspRoomGenerator bsp;           // optional, to fetch walls
    public Transform player;               // shot origin
    public GameObject enemyPrefab;         // enemy to spawn
    public Transform enemyParent;          // optional parent

    [Header("Enemy Placement")]
    [Range(1, 64)] public int enemyCount = 5;
    public float s0 = 2.5f;                // first enemy distance along path
    public float ds = 2.0f;                // spacing between enemies
    public float rEnemy = 0.3f;
    public float rProj  = 0.1f;
    public float wallMargin = 0.06f;       // min distance from walls

    [Header("Shot Mechanics")]
    public int B0 = 4;                     // initial bounce cap
    public bool bonusBounceOnHit = true;   // +1 to bounce cap per enemy hit
    [Range(1, 128)] public int Bmax = 32;  // absolute safety cap on wall bounces

    [Header("Angle Selection (load-time)")]
    [Range(1, 256)] public int angleTrials = 36; // K candidates
    public float minAxisCos = 0.15f;             // avoid grazing (|cos| and |sin| >= this)
    public int rngSeed = 2025;                   // determinism for angle sampling

    [Header("Runtime Controls")]
    public bool generateOnStart = true;
    public bool clearOldEnemiesOnSpawn = true;

    // internal
    struct WallSeg { public Vector2 a, b; public Vector2 n; public WallSeg(Vector2 A, Vector2 B){ a=A; b=B; var t=(B-A).normalized; n=new Vector2(-t.y, t.x); } }
    List<WallSeg> walls = new List<WallSeg>();
    List<Vector2> chosenPath = new List<Vector2>();      // polyline points
    List<Vector2> placed = new List<Vector2>();          // enemy positions
    List<int>     bounceAtEnemy = new List<int>();       // bounce count before each enemy hit
    float chosenTheta = -1f;
    const float EPS = 1e-4f;

    // --------------- Public API ---------------
    [ContextMenu("Generate & Spawn")]
    public void GenerateAndSpawn()
    {
        BuildWalls();
        PickAngleAndPlace();
        SpawnEnemies();
    }

    // Get chosen angle in radians (for logging)
    public float GetChosenAngle() => chosenTheta;

    // --------------- Unity lifecycle ---------------
    void Start()
    {
        if (generateOnStart) {
            GenerateAndSpawn();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        // draw walls
        Gizmos.color = Color.gray;
        foreach (var s in walls) Gizmos.DrawLine((Vector3)s.a, (Vector3)s.b);

        // draw path
        if (chosenPath != null && chosenPath.Count > 1) {
            Gizmos.color = new Color(0.2f,0.7f,1f,1f);
            for (int i=1;i<chosenPath.Count;i++) Gizmos.DrawLine((Vector3)chosenPath[i-1], (Vector3)chosenPath[i]);
        }
        // draw enemies
        Gizmos.color = new Color(1f,0.3f,0.3f,1f);
        foreach (var p in placed) Gizmos.DrawWireSphere((Vector3)p, rEnemy);
    }

    // --------------- Core steps ---------------
    void BuildWalls()
    {
        walls.Clear();

        // Priority: take from BSP if present
        if (bsp != null) {
            var segs = bsp.BuildWallSegments();
            foreach (var s in segs) walls.Add(new WallSeg(s.a, s.b));
        }

        // If there is no BSP, you can manually add walls here or
        // detect colliders at runtime and convert to segments.
        if (walls.Count == 0) {
            Debug.LogWarning("[SpecularPathSpawner] No walls found. Assign a BSP or add walls manually.");
        }
    }

    void PickAngleAndPlace()
    {
        chosenPath.Clear();
        placed.Clear(); bounceAtEnemy.Clear();
        chosenTheta = -1f;

        if (player == null) { Debug.LogError("Player transform is missing."); return; }
        if (enemyPrefab == null) { Debug.LogError("Enemy prefab is missing."); return; }

        // Required path length for placement
        float neededLen = s0 + (enemyCount - 1) * ds + 3f;

        // Try a handful of angles, pick the first valid.
        System.Random rng = new System.Random(rngSeed);

        float bestClr = -1f;
        float bestTheta = -1f;
        List<Vector2> bestPath = null;
        List<Vector2> bestEnemies = null;
        List<int>     bestBounceIdx = null;

        for (int i=0;i<angleTrials;i++) {
            float theta = SampleAngleAvoidAxis(rng);

            if (TryBuildForAngle(theta, neededLen,
                                 out var path, out var enemies, out var bounceIdx,
                                 out float minClr))
            {
                // Found a valid candidate; stop early.
                chosenTheta = theta;
                chosenPath  = path;
                placed      = enemies;
                bounceAtEnemy = bounceIdx;
                return;
            }

            // Track most clear candidate for fallback
            if (minClr > bestClr) {
                bestClr = minClr; bestTheta = theta;
                bestPath = path; bestEnemies = enemies; bestBounceIdx = bounceIdx;
            }
        }

        // No strictly valid candidate; fallback to the most clear one if any
        if (bestPath != null && bestEnemies != null) {
            Debug.LogWarning("[SpecularPathSpawner] Using fallback candidate (best clearance).");
            chosenTheta = bestTheta;
            chosenPath  = bestPath;
            placed      = bestEnemies;
            bounceAtEnemy = bestBounceIdx;
            return;
        }

        // Final fallback: straight 45° without walls consideration
        {
            float theta = 0.25f * Mathf.PI;
            var path = new List<Vector2>() { (Vector2)player.position, (Vector2)player.position + new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * (neededLen+1f) };
            var enemies = new List<Vector2>(enemyCount);
            var bIdx = new List<int>(enemyCount);
            PlaceAlongPath(path, s0, ds, enemyCount, enemies, bIdx);
            chosenTheta = theta;
            chosenPath = path;
            placed = enemies;
            bounceAtEnemy = bIdx;
        }
    }

    void SpawnEnemies()
    {
        if (clearOldEnemiesOnSpawn && enemyParent != null) {
            var toKill = new List<Transform>();
            foreach (Transform ch in enemyParent) toKill.Add(ch);
            foreach (var t in toKill) { if (Application.isEditor) DestroyImmediate(t.gameObject); else Destroy(t.gameObject); }
        }

        foreach (var p in placed) {
            var go = Instantiate(enemyPrefab, new Vector3(p.x, p.y, 0f), Quaternion.identity);
            if (enemyParent != null) go.transform.SetParent(enemyParent, true);
        }
        Debug.Log($"[SpecularPathSpawner] Spawned {placed.Count} enemies. Theta={chosenTheta * Mathf.Rad2Deg:F2}°");
    }

    // --------------- Simple candidate building ---------------
    bool TryBuildForAngle(float theta, float neededLen,
                          out List<Vector2> path,
                          out List<Vector2> enemies,
                          out List<int> bounceIdx,
                          out float minClearance)
    {
        Vector2 S = player.position;
        Vector2 dir = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)).normalized;

        path = TracePath(S, dir, Bmax, neededLen);
        enemies = new List<Vector2>(enemyCount);
        bounceIdx = new List<int>(enemyCount);
        minClearance = -1f;

        if (path.Count < 2) return false;

        // Place enemies along polyline
        PlaceAlongPath(path, s0, ds, enemyCount, enemies, bounceIdx);

        // Compute clearance and budget validity
        minClearance = float.PositiveInfinity;
        foreach (var p in enemies) {
            float d = MinDistanceToWalls(p);
            if (d < minClearance) minClearance = d;
        }

        bool clearanceOK = (minClearance >= rEnemy + wallMargin - 1e-5f);
        bool budgetOK = CheckBounceBudget(bounceIdx);

        return clearanceOK && budgetOK;
    }

    bool CheckBounceBudget(List<int> bounceIdx)
    {
        for (int i=0;i<bounceIdx.Count;i++) {
            int allowed = B0 + (bonusBounceOnHit ? i : 0);
            if (bounceIdx[i] > allowed) return false;
        }
        return true;
    }

    // --------------- Geometry ---------------
    List<Vector2> TracePath(Vector2 start, Vector2 dir, int maxBounces, float minLen)
    {
        List<Vector2> poly = new List<Vector2>(64){ start };
        Vector2 p = start;
        Vector2 d = dir.normalized;
        float acc = 0f;
        int b = 0;

        while (b <= maxBounces && acc < minLen) {
            bool hitAny = false;
            float bestT = float.PositiveInfinity;
            Vector2 bestQ = Vector2.zero;
            Vector2 bestN = Vector2.zero;

            foreach (var s in walls) {
                Vector2 q; float t; Vector2 n;
                if (RaySegIntersect(p, d, s.a, s.b, out t, out q)) {
                    if (t > EPS && t < bestT) {
                        bestT = t; bestQ = q; bestN = s.n; hitAny = true;
                    }
                }
            }

            if (!hitAny || float.IsInfinity(bestT)) break;

            Vector2 next = p + bestT * d;
            poly.Add(next);
            acc += bestT;

            // reflect
            d = Reflect(d, bestN).normalized;

            // Nudge off the wall to avoid re-hitting due to numeric noise
            p = next + d * 1e-3f;
            b++;
        }
        return poly;
    }

    static Vector2 Reflect(Vector2 d, Vector2 nUnit)
    {
        // sign of n does not matter for reflection
        float dot = Vector2.Dot(d, nUnit);
        return (d - 2f * dot * nUnit);
    }

    static bool RaySegIntersect(Vector2 p, Vector2 d, Vector2 a, Vector2 b, out float t, out Vector2 q)
    {
        // Solve p + t d = a + u (b-a)
        Vector2 e = b - a;
        float denom = Cross(d, e);
        if (Mathf.Abs(denom) < 1e-8f) { t = 0; q = default; return false; } // parallel

        Vector2 ap = a - p;
        t = Cross(ap, e) / denom;
        float u = Cross(ap, d) / denom;
        if (t < 0f) { q = default; return false; }
        if (u < -1e-6f || u > 1f + 1e-6f) { q = default; return false; }
        q = p + t * d;
        return true;
    }

    static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;

    void PlaceAlongPath(List<Vector2> path, float s0, float ds, int count,
                        List<Vector2> outPts, List<int> outBounceIdx)
    {
        outPts.Clear(); outBounceIdx.Clear();
        if (path.Count < 2) return;

        float run = 0f;
        int segIdx = 1;
        float segLen = Vector2.Distance(path[0], path[1]);

        for (int i=0;i<count;i++) {
            float target = s0 + i * ds;

            while (segIdx < path.Count && run + segLen < target - 1e-6f) {
                run += segLen;
                segIdx++;
                if (segIdx >= path.Count) { // clamp at end
                    segIdx = path.Count - 1;
                    segLen = Vector2.Distance(path[segIdx-1], path[segIdx]);
                    break;
                }
                segLen = Vector2.Distance(path[segIdx-1], path[segIdx]);
            }

            float t = Mathf.Clamp01((target - run) / Mathf.Max(1e-5f, segLen));
            Vector2 p = Vector2.Lerp(path[segIdx-1], path[segIdx], t);
            p = NudgeFromWalls(p, rEnemy + wallMargin, 3);
            outPts.Add(p);
            int bBefore = Mathf.Max(0, (segIdx - 1));
            outBounceIdx.Add(bBefore);
        }
    }

    Vector2 NudgeFromWalls(Vector2 p, float minDist, int iters)
    {
        for (int k=0;k<iters;k++) {
            float bestPush = 0f; Vector2 pushDir = Vector2.zero;
            foreach (var s in walls) {
                float d = DistPointSeg(p, s.a, s.b);
                if (d < minDist) {
                    // push direction approx along wall normal
                    Vector2 dir = s.n.normalized;
                    float need = (minDist - d);
                    if (need > bestPush) { bestPush = need; pushDir = dir; }
                }
            }
            if (bestPush <= 1e-4f) break;
            p += pushDir * bestPush;
        }
        return p;
    }

    float MinDistanceToWalls(Vector2 p)
    {
        float best = float.PositiveInfinity;
        foreach (var s in walls) {
            float d = DistPointSeg(p, s.a, s.b);
            if (d < best) best = d;
        }
        return best;
    }

    static float DistPointSeg(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / Mathf.Max(1e-8f, ab.sqrMagnitude);
        t = Mathf.Clamp01(t);
        Vector2 proj = a + t * ab;
        return Vector2.Distance(p, proj);
    }

    float SampleAngleAvoidAxis(System.Random rng)
    {
        for (int k=0;k<64;k++) {
            float theta = (float)rng.NextDouble() * Mathf.PI; // [0,pi)
            float c = Mathf.Abs(Mathf.Cos(theta));
            float s = Mathf.Abs(Mathf.Sin(theta));
            if (c >= minAxisCos && s >= minAxisCos) return theta;
        }
        return 0.25f * Mathf.PI; // fallback 45°
    }

    static float RandomSign(System.Random rng) => rng.NextDouble() < 0.5 ? -1f : 1f;
}

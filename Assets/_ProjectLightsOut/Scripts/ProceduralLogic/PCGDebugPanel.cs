using DamnedVeil.ProceduralLogic.Orchestrator;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace DamnedVeil.ProceduralLogic
{
    public class PCGDebugPanel : MonoBehaviour
    {
        [SerializeField] private ProceduralEnemySpawner spawner;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
        [SerializeField] private Vector2 panelPosition = new Vector2(10, 10);

        private bool isVisible = false;

        private GUIStyle boxStyle;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle passStyle;
        private GUIStyle failStyle;
        private GUIStyle disabledStyle;
        private bool stylesInitialized = false;

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                isVisible = !isVisible;
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(12, 12, 10, 10)
            };
            boxStyle.normal.background = MakeTex(1, 1, new Color(0f, 0f, 0f, 0.78f));

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(0.9f, 0.85f, 0.3f);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                richText = true
            };
            labelStyle.normal.textColor = Color.white;

            passStyle = new GUIStyle(labelStyle);
            passStyle.normal.textColor = new Color(0.4f, 1f, 0.4f);

            failStyle = new GUIStyle(labelStyle);
            failStyle.normal.textColor = new Color(1f, 0.35f, 0.35f);

            disabledStyle = new GUIStyle(labelStyle);
            disabledStyle.normal.textColor = new Color(0.55f, 0.55f, 0.55f);

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!isVisible || spawner == null) return;

            InitStyles();

            bool valid = spawner.HasSpawned;
            var settings = spawner.LastSettings;
            var path = spawner.CurrentPath;
            int segmentCount = path != null ? path.SegmentCount : 0;
            int bounceCount = Mathf.Max(0, segmentCount - 1);

            // Build content to measure panel size
            float panelWidth = 260f;
            float lineH = 20f;
            int lineCount = 20;
            float panelHeight = lineH * lineCount + 30f;

            Rect panelRect = new Rect(panelPosition.x, panelPosition.y, panelWidth, panelHeight);
            GUI.Box(panelRect, GUIContent.none, boxStyle);

            GUILayout.BeginArea(panelRect);
            GUILayout.Space(6);

            GUILayout.Label("=== PCG DEBUG PANEL ===", titleStyle);
            GUILayout.Space(4);

            // --- Trial & Enemies ---
            GUILayout.Label($"Trial         : <b>{spawner.LastAttemptCount}</b> of {spawner.MaxAttempts}", labelStyle);
            GUILayout.Label($"Target        : <b>{settings.EnemyCount}</b> enemies", labelStyle);
            GUILayout.Label($"Spawned       : <b>{spawner.SpawnedEnemyCount}</b> enemies", labelStyle);

            GUILayout.Space(6);

            // --- Timing ---
            GUILayout.Label($"Gen Time      : <b>{spawner.GenerationTimeMs:F2} ms</b>", labelStyle);
            GUILayout.Label($"  SP  Time    : {spawner.SpTimeMs:F2} ms", labelStyle);
            GUILayout.Label($"  CSP Time    : {spawner.CspTimeMs:F2} ms", labelStyle);

            GUILayout.Space(6);

            // --- Status ---
            GUIStyle statusStyle = valid ? passStyle : failStyle;
            string statusText = valid ? "VALID" : "FAILED";
            GUILayout.Label($"Status        : <b>{statusText}</b>", statusStyle);
            GUILayout.Label($"Path Segments : <b>{segmentCount}</b>", labelStyle);
            GUILayout.Label($"Bounce Count  : <b>{bounceCount}</b> / {settings.MaxBounces}", labelStyle);

            GUILayout.Space(6);

            // --- CSP Constraints ---
            GUILayout.Label("CSP Constraints:", labelStyle);
            DrawConstraint("Enemy Spacing",     settings.MinEnemySpacing > 0,      valid);
            DrawConstraint("Safe Zone",         settings.SafeZoneRadius > 0,       valid);
            DrawConstraint("Wall Buffer",       settings.WallBufferRadius > 0,     valid);
            DrawConstraint("End Path Buffer",   settings.EndPathBuffer > 0,        valid);
            DrawConstraint("Max Per Segment",   settings.MaxEnemiesPerSegment > 0, valid);
            DrawConstraint("Enemy Count Met",   true,                              spawner.SpawnedEnemyCount >= settings.EnemyCount);

            GUILayout.Space(4);
            GUILayout.Label($"<color=#888888>[{toggleKey}] to hide</color>", labelStyle);

            GUILayout.EndArea();
        }

        private void DrawConstraint(string label, bool isActive, bool passed)
        {
            if (!isActive)
            {
                GUILayout.Label($"  [<color=#555555>—</color>] {label}", labelStyle);
                return;
            }

            if (passed)
                GUILayout.Label($"  [<color=#66ff66>✓</color>] {label}", labelStyle);
            else
                GUILayout.Label($"  [<color=#ff5555>✗</color>] {label}", labelStyle);
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}

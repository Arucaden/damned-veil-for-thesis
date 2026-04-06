using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using ProjectLightsOut.Managers;
using DamnedVeil.ProceduralLogic.Orchestrator;
using Debug = UnityEngine.Debug;

namespace DamnedVeil.ProceduralLogic.Testing
{
    /// <summary>
    /// A testing utility for evaluating procedural generation across 4 thesis metrics:
    /// Speed, Reliability, Variability, and Expressivity.
    /// </summary>
    public class PCGMetricsTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProceduralEnemySpawner spawner;

        [Header("Test Configuration")]
        [Tooltip("Number of generations to run for the test")]
        [SerializeField] private int testIterations = 100;

        [Tooltip("The wave settings to use for this test")]
        [SerializeField] private ProceduralWaveSettings waveSettings;

        [ContextMenu("Run PCG Metrics Test")]
        public void RunTest()
        {
            if (spawner == null)
            {
                spawner = FindObjectOfType<ProceduralEnemySpawner>();
                if (spawner == null)
                {
                    Debug.LogError("[PCGMetricsTester] Spawner reference is missing!");
                    return;
                }
            }

            if (waveSettings.EnemyPool == null || waveSettings.EnemyPool.Count == 0 || waveSettings.EnemyCount <= 0)
            {
                Debug.LogError("[PCGMetricsTester] WaveSettings are invalid (Missing EnemyPool or EnemyCount <= 0). Please assign a valid configuration.");
                return;
            }

            Debug.Log($"--- Starting PCG Metrics Test: {testIterations} Iterations ---");

            // --- Metrics Tracking Variables ---
            
            // 1. Reliability
            int successfulGenerations = 0;
            int failedGenerations = 0;

            // 2. Speed
            List<long> generationTimesMs = new List<long>();

            // 3 & 4. Variability & Expressivity (Tracking Path Lengths & Enemy Spawns)
            List<float> successfulPathLengths = new List<float>();
            List<int> successfulEnemyCounts = new List<int>();

            Stopwatch stopwatch = new Stopwatch();

            // Run the iteration loop
            for (int i = 0; i < testIterations; i++)
            {
                // Clear state from previous run
                spawner.ClearSpawnedEnemies();

                // Measure Time (Speed)
                stopwatch.Restart();
                bool success = spawner.SpawnWave(waveSettings);
                stopwatch.Stop();

                generationTimesMs.Add(stopwatch.ElapsedMilliseconds);

                // Reliability tracking
                if (success)
                {
                    successfulGenerations++;
                    
                    if (spawner.CurrentPath != null)
                        successfulPathLengths.Add(spawner.CurrentPath.TotalLength);
                    
                    successfulEnemyCounts.Add(spawner.SpawnedEnemyCount);
                }
                else
                {
                    failedGenerations++;
                }
            }

            // Clean up visual clutter off the last run
            spawner.ClearSpawnedEnemies();

            // --- Compile Results ---
            CompileAndLogResults(successfulGenerations, failedGenerations, generationTimesMs, successfulPathLengths, successfulEnemyCounts);
        }

        private void CompileAndLogResults(
            int successes, 
            int failures, 
            List<long> allTimesMs, 
            List<float> pathLengths, 
            List<int> enemyCounts)
        {
            // 1. Reliability Calculation
            float reliabilityPercent = ((float)successes / testIterations) * 100f;

            // 2. Speed Calculation
            double averageTimeMs = allTimesMs.Average();
            long maxTimeMs = allTimesMs.Max();
            long minTimeMs = allTimesMs.Min();

            // 3. Variability Calculation (Standard Deviation of Path Lengths)
            double meanPathLength = 0;
            double pathLengthStdDev = 0;
            
            if (pathLengths.Count > 0)
            {
                meanPathLength = pathLengths.Average();
                double sumOfSquaresOfDifferences = pathLengths.Select(val => (val - meanPathLength) * (val - meanPathLength)).Sum();
                // Sample standard deviation
                pathLengthStdDev = pathLengths.Count > 1 ? System.Math.Sqrt(sumOfSquaresOfDifferences / (pathLengths.Count - 1)) : 0;
            }

            // 4. Expressivity Calculation (Range of outputs)
            float minPathGenerated = pathLengths.Count > 0 ? pathLengths.Min() : 0;
            float maxPathGenerated = pathLengths.Count > 0 ? pathLengths.Max() : 0;

            int minEnemiesSpawned = enemyCounts.Count > 0 ? enemyCounts.Min() : 0;
            int maxEnemiesSpawned = enemyCounts.Count > 0 ? enemyCounts.Max() : 0;

            // Final Output Formatting
            string report = "<b>=== PCG METRICS TEST REPORT ===</b>\n\n";

            report += $"<b>1. RELIABILITY: {reliabilityPercent:F2}% Success Rate</b>\n";
            report += $"   - Successful Generations: {successes} / {testIterations}\n";
            report += $"   - Failed Generations: {failures} / {testIterations}\n\n";

            report += $"<b>2. SPEED: {averageTimeMs:F2} ms Average</b>\n";
            report += $"   - Fastest Generation: {minTimeMs} ms\n";
            report += $"   - Slowest Generation: {maxTimeMs} ms\n\n";

            report += $"<b>3. VARIABILITY (Standard Deviation)</b>\n";
            if (pathLengths.Count > 1) {
                report += $"   - Path Length Std. Dev: {pathLengthStdDev:F2} units\n";
                report += $"   <i>(High values indicate structurally diverse paths across runs)</i>\n\n";
            } else {
                report += $"   - N/A (Requires > 1 successful generation)\n\n";
            }

            report += $"<b>4. EXPRESSIVITY (Possibility Space Boundaries)</b>\n";
            if (pathLengths.Count > 0)
            {
                report += $"   - Path Length Range: {minPathGenerated:F2} to {maxPathGenerated:F2} units\n";
                report += $"   - Enemy Count Range: {minEnemiesSpawned} to {maxEnemiesSpawned} enemies placed\n";
                report += $"   - Target Settings: Min Path={waveSettings.MinPathLength}, Target Enemies={waveSettings.EnemyCount}\n";
            }
            else
            {
                report += $"   - N/A (0 successful paths generated)\n";
            }

            Debug.Log(report);
        }
    }
}

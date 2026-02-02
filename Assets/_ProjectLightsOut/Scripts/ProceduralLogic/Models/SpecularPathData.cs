using System.Collections.Generic;
using UnityEngine;

namespace DamnedVeil.ProceduralLogic.Models
{
    /// <summary>
    /// Contains the complete data for a specular (ricochet) bullet path.
    /// </summary>
    [System.Serializable]
    public class SpecularPathData
    {
        /// <summary>
        /// All trajectory points along the path (including origin and bounce points).
        /// </summary>
        public List<TrajectoryPoint> PathPoints;

        /// <summary>
        /// Total length of the path in world units.
        /// </summary>
        public float TotalLength;

        /// <summary>
        /// Whether this path forms a valid closed loop (hits the final target if any).
        /// </summary>
        public bool IsClosedLoop;

        public SpecularPathData()
        {
            PathPoints = new List<TrajectoryPoint>();
            TotalLength = 0f;
            IsClosedLoop = false;
        }

        /// <summary>
        /// Gets all segment positions as Vector2 array for easy iteration.
        /// </summary>
        public Vector2[] GetPositions()
        {
            Vector2[] positions = new Vector2[PathPoints.Count];
            for (int i = 0; i < PathPoints.Count; i++)
            {
                positions[i] = PathPoints[i].Position;
            }
            return positions;
        }

        /// <summary>
        /// Gets the segment between two consecutive points.
        /// </summary>
        public (Vector2 start, Vector2 end) GetSegment(int segmentIndex)
        {
            if (segmentIndex < 0 || segmentIndex >= PathPoints.Count - 1)
            {
                Debug.LogWarning($"Invalid segment index: {segmentIndex}");
                return (Vector2.zero, Vector2.zero);
            }
            return (PathPoints[segmentIndex].Position, PathPoints[segmentIndex + 1].Position);
        }

        /// <summary>
        /// Returns the number of path segments (points - 1).
        /// </summary>
        public int SegmentCount => PathPoints.Count > 0 ? PathPoints.Count - 1 : 0;
    }
}

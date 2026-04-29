using System.Collections.Generic;
using UnityEngine;

namespace DamnedVeil.ProceduralLogic.Models
{
    [System.Serializable]
    public class SpecularPathData
    {
        public List<TrajectoryPoint> PathPoints;
        public float TotalLength;
        public bool IsClosedLoop;

        public SpecularPathData()
        {
            PathPoints = new List<TrajectoryPoint>();
            TotalLength = 0f;
            IsClosedLoop = false;
        }

        public Vector2[] GetPositions()
        {
            Vector2[] positions = new Vector2[PathPoints.Count];
            for (int i = 0; i < PathPoints.Count; i++)
            {
                positions[i] = PathPoints[i].Position;
            }
            return positions;
        }

        public (Vector2 start, Vector2 end) GetSegment(int segmentIndex)
        {
            if (segmentIndex < 0 || segmentIndex >= PathPoints.Count - 1)
            {
                Debug.LogWarning($"Invalid segment index: {segmentIndex}");
                return (Vector2.zero, Vector2.zero);
            }
            return (PathPoints[segmentIndex].Position, PathPoints[segmentIndex + 1].Position);
        }

        public int SegmentCount => PathPoints.Count > 0 ? PathPoints.Count - 1 : 0;
    }
}

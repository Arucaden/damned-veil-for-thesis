using UnityEngine;

namespace ProjectLightsOut.Managers
{
    /// <summary>
    /// Base class for all optional puzzle/riddle constraints in a level.
    /// LevelProgressionValidator will ensure all active riddles are IsSolved before completing the level.
    /// </summary>
    public abstract class BaseRiddle : MonoBehaviour
    {
        public bool IsSolved { get; protected set; } = false;

        /// <summary>
        /// Call this from your inherited class when the puzzle conditions are met.
        /// </summary>
        protected void CompleteRiddle()
        {
            if (IsSolved) return;
            IsSolved = true;
            DevUtils.EventManager.Broadcast(new OnRiddleSolved(this));
        }
    }

    public class OnRiddleSolved : DevUtils.GameEvent
    {
        public BaseRiddle Riddle;
        public OnRiddleSolved(BaseRiddle riddle) { Riddle = riddle; }
    }
}

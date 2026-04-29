using UnityEngine;

namespace ProjectLightsOut.Managers
{
    public abstract class BaseRiddle : MonoBehaviour
    {
        public bool IsSolved { get; protected set; } = false;

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

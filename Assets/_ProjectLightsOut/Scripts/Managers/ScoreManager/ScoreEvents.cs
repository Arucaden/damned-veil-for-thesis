using ProjectLightsOut.DevUtils;

namespace ProjectLightsOut.Managers
{
    public class OnScoreChange : GameEvent
    {
        public int Score;
        public OnScoreChange(int score)
        {
            Score = score;
        }
    }

    public class OnAddScore : GameEvent
    {
        public int Score;
        public OnAddScore(int score)
        {
            Score = score;
        }
    }

    public class OnPostScore : GameEvent
    {
        public int Score;
        public int TimeBonus;
        public int LevelBonus;
        public int BulletBonus;

        public OnPostScore(int score, int timeBonus = 0, int levelBonus = 0, int bulletBonus = 0)
        {
            Score = score;
            TimeBonus = timeBonus;
            LevelBonus = levelBonus;
            BulletBonus = bulletBonus;
        }
    }

    public class OnCompleteCountingScore : GameEvent
    {}

    public class OnResetScore : GameEvent
    {}

    public class OnRollbackScore : GameEvent
    {}
}
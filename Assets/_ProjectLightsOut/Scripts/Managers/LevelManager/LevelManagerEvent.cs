using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    public class OnEnemyRegister : GameEvent
    {
        public Enemy Enemy;

        public OnEnemyRegister(Enemy enemy)
        {
            Enemy = enemy;
        }
    }

    public class OnBossRegister : GameEvent
    {
        public Enemy Boss;

        public OnBossRegister(Enemy boss)
        {
            Boss = boss;
        }
    }

    public class OnEnemyDead : GameEvent
    {
        public Enemy Enemy;

        public OnEnemyDead(Enemy enemy)
        {
            Enemy = enemy;
        }
    }

    public class OnProjectileShoot : GameEvent
    {
        public int BulletLeft;

        public OnProjectileShoot(int bulletLeft)
        {
            BulletLeft = bulletLeft;
        }
    }

    public class OnTriggerGameOver : GameEvent
    {}

    public class OnTriggerLevelComplete : GameEvent
    {}

    public class OnBossDead : GameEvent
    {}

    public class OnProjectileDestroy : GameEvent
    {}

    public class OnLevelComplete : GameEvent
    {
        public int LevelBonus;
        public int BulletRemaining;
        public float LevelTimeRemaining;

        public OnLevelComplete(int levelBonus, int bulletRemaining, float levelTimeRemaining)
        {
            LevelBonus = levelBonus;
            BulletRemaining = bulletRemaining;
            LevelTimeRemaining = levelTimeRemaining;
        }
    }

    public class OnBulletReload : GameEvent
    {
        public int Bullets;

        public OnBulletReload(int bullets)
        {
            Bullets = bullets;
        }
    }

    public class OnGrantReload : GameEvent
    {
        public int Bullets;

        public OnGrantReload(int bullets)
        {
            Bullets = bullets;
        }
    }

    public class OnPlayerMove : GameEvent
    {
        public bool IsMoving;
        public List<Transform> Waypoints;
        public OnPlayerMove(bool isMoving, List<Transform> waypoints)
        {
            IsMoving = isMoving;
            Waypoints = waypoints;
        }
    }

    public class OnPlayerFinishMove : GameEvent
    {}

    public class OnPlayerEnableShooting : GameEvent
    {
        public bool IsEnabled;

        public OnPlayerEnableShooting(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }

    public class OnBossLevel : GameEvent
    {}

    public class OnBossReady : GameEvent
    {
        public Enemy Boss;

        public OnBossReady(Enemy boss)
        {
            Boss = boss;
        }
    }

    public class OnReadyBoss : GameEvent
    {}
}

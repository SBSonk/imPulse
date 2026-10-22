using System.Collections.Generic;
using UnityEngine;

namespace ImPulse.Core
{
    [System.Serializable]
    public struct RoundCheckpoint
    {
        [Tooltip("The round number at which these enemy settings take effect.")]
        public int roundTrigger;
        public List<EnemySpawnData> enemyPool;
    }

    [CreateAssetMenu(fileName = "New Difficulty Settings", menuName = "ImPulse/Data/Difficulty Settings")]
    public class DifficultySettings : ScriptableObject
    {
        [Header("Wave Configuration")]
        public float timeBetweenWaves = 3f;
        public float spawnRadius = 15f;

        [Header("Base Stats (Round 1)")]
        public int baseTotalEnemies = 10;
        public int baseMaxAlive = 5;
        public float baseSpawnInterval = 2f;
        public int baseGroupSize = 1;

        [Header("Scaling Multipliers")]
        public float totalEnemiesMultiplier = 1.2f;
        public int maxAliveIncrement = 2;
        public float spawnIntervalDecay = 0.9f;
        public float minSpawnInterval = 0.2f;
        [Tooltip("How many rounds before the group size increases by 1.")]
        public int groupSizeIncreaseRate = 3;
        public float healthScalingRate = 0.15f;

        [Header("Round Checkpoints")]
        [Tooltip("Define checkpoints to change the enemy pool at specific rounds. Make sure to define a checkpoint for Round 1.")]
        public List<RoundCheckpoint> roundCheckpoints = new List<RoundCheckpoint>();
    }
}
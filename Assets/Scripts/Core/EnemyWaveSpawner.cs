using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImPulse.Core.Pooling; 
using ImPulse.Enemies;
using ImPulse.Player;
using System;

namespace ImPulse.Core
{
    [System.Serializable]
    public struct EnemySpawnData
    {
        public Enemy enemyPrefab;
        [Range(0, 100)] public float spawnWeight;
        public int xpYield;
        public int scoreYield;
        [Tooltip("The maximum amount of this specific enemy type allowed alive at once. 0 means unlimited.")]
        public int maxAliveAtOnce;
    }

    public class EnemyWaveSpawner : MonoBehaviour
    {
        public static EnemyWaveSpawner Instance;

        [Header("Wave Configuration")]
        [SerializeField] private List<EnemySpawnData> enemyTypes;
        [SerializeField] private float timeBetweenWaves = 3f;
        [SerializeField] private float spawnRadius = 15f;

        [Header("Base Stats (Round 1)")]
        [SerializeField] private int baseTotalEnemies = 10;
        [SerializeField] private int baseMaxAlive = 5;
        [SerializeField] private float baseSpawnInterval = 2f;
        [SerializeField] private int baseGroupSize = 1;

        [Header("Scaling Multipliers")]
        [SerializeField] private float totalEnemiesMultiplier = 1.2f;
        [SerializeField] private int maxAliveIncrement = 2;
        [SerializeField] private float spawnIntervalDecay = 0.9f;
        [SerializeField] private float minSpawnInterval = 0.2f;
        [Tooltip("How many rounds before the group size increases by 1.")]
        [SerializeField] private int groupSizeIncreaseRate = 3;
        [SerializeField] private float healthScalingRate = 0.15f;

        [Header("State (Read Only)")]
        public int currentRound = 1;
        [SerializeField] private int _enemiesSpawnedThisRound = 0;
        [SerializeField] private int _enemiesCurrentlyAlive = 0;
        [SerializeField] private bool _isRoundActive = false;

        public Action<int> OnRoundChange;

        GameManager _gameManager;
        Transform _playerTransform;

        private int _currentTotalEnemies;
        private int _currentMaxAlive;
        private float _currentSpawnInterval;
        private int _currentGroupSize;
        private float _currentHealthModifier;

        private float _spawnTimer = 0f;
        private bool _isWaitingForNextRound = false;

        // Tracking active instances per type
        private int[] _aliveCountPerType;
        private Dictionary<Enemy, int> _activeEnemyTypeMap = new Dictionary<Enemy, int>();

        void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            _gameManager = FindAnyObjectByType<GameManager>();
            _playerTransform = FindAnyObjectByType<PlayerMovement>().transform;
            
            _aliveCountPerType = new int[enemyTypes.Count];
            
            StartWave(1);
        }

        private void Update()
        {
            if (!_isRoundActive || _isWaitingForNextRound || _playerTransform == null) return;

            if (_enemiesSpawnedThisRound >= _currentTotalEnemies && _enemiesCurrentlyAlive <= 0)
            {
                StartCoroutine(NextRoundRoutine());
                return;
            }

            if (_enemiesSpawnedThisRound < _currentTotalEnemies && _enemiesCurrentlyAlive < _currentMaxAlive)
            {
                _spawnTimer -= Time.deltaTime;
                if (_spawnTimer <= 0f)
                {
                    SpawnGroup();
                    _spawnTimer = _currentSpawnInterval;
                }
            }
        }

        private void StartWave(int round)
        {
            currentRound = round;
            OnRoundChange?.Invoke(round);
            _enemiesSpawnedThisRound = 0;
            
            ApplyRoundScaling();
            
            _spawnTimer = _currentSpawnInterval;
            _isRoundActive = true;
            _isWaitingForNextRound = false;
        }

        private void ApplyRoundScaling()
        {
            _currentTotalEnemies = Mathf.RoundToInt(baseTotalEnemies * Mathf.Pow(totalEnemiesMultiplier, currentRound - 1));
            _currentMaxAlive = baseMaxAlive + ((currentRound - 1) * maxAliveIncrement);
            _currentSpawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval * Mathf.Pow(spawnIntervalDecay, currentRound - 1));
            _currentGroupSize = baseGroupSize + ((currentRound - 1) / Mathf.Max(1, groupSizeIncreaseRate));
            _currentHealthModifier = 1f + ((currentRound - 1) * healthScalingRate);
        }

        private void SpawnGroup()
        {
            int amountToSpawn = Mathf.Min(_currentGroupSize, _currentTotalEnemies - _enemiesSpawnedThisRound, _currentMaxAlive - _enemiesCurrentlyAlive);

            for (int i = 0; i < amountToSpawn; i++)
            {
                // Break out early if no eligible enemies can be spawned due to type caps
                if (!SpawnSingleEnemy()) break; 
            }
        }

        private bool SpawnSingleEnemy()
        {
            if (!GetRandomValidEnemyType(out int typeIndex, out EnemySpawnData dataToSpawn)) 
            {
                return false;
            }
            
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            Vector3 spawnPos = _playerTransform.position + new Vector3(randomDir.x, randomDir.y, 0f) * spawnRadius;

            GameObject enemyObj = null;
            if (ObjectPooler.Instance != null)
            {
                enemyObj = ObjectPooler.Instance.Get(dataToSpawn.enemyPrefab.gameObject, spawnPos, Quaternion.identity);
            }
            else
            {
                enemyObj = Instantiate(dataToSpawn.enemyPrefab.gameObject, spawnPos, Quaternion.identity);
            }

            Enemy enemyScript = enemyObj.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.InitializeWaveStats(_currentHealthModifier, dataToSpawn.xpYield, dataToSpawn.scoreYield);
                enemyScript.OnEnemyDied += HandleEnemyDeath;
                
                _enemiesCurrentlyAlive++;
                _enemiesSpawnedThisRound++;
                
                // Track this specific enemy type
                _aliveCountPerType[typeIndex]++;
                _activeEnemyTypeMap.TryAdd(enemyScript, typeIndex);
            }
            
            return true;
        }

        private bool GetRandomValidEnemyType(out int selectedIndex, out EnemySpawnData selectedData)
        {
            selectedIndex = -1;
            selectedData = default;
            float totalWeight = 0;
            
            // Only weigh types that haven't hit their max local cap
            for (int i = 0; i < enemyTypes.Count; i++)
            {
                if (enemyTypes[i].maxAliveAtOnce == 0 || _aliveCountPerType[i] < enemyTypes[i].maxAliveAtOnce)
                {
                    totalWeight += enemyTypes[i].spawnWeight;
                }
            }

            if (totalWeight <= 0) return false;

            float randomVal = UnityEngine.Random.Range(0f, totalWeight);
            
            for (int i = 0; i < enemyTypes.Count; i++)
            {
                if (enemyTypes[i].maxAliveAtOnce == 0 || _aliveCountPerType[i] < enemyTypes[i].maxAliveAtOnce)
                {
                    if (randomVal < enemyTypes[i].spawnWeight)
                    {
                        selectedIndex = i;
                        selectedData = enemyTypes[i];
                        return true;
                    }
                    randomVal -= enemyTypes[i].spawnWeight;
                }
            }

            // Fallback to first available uncapped type
            for (int i = 0; i < enemyTypes.Count; i++)
            {
                if (enemyTypes[i].maxAliveAtOnce == 0 || _aliveCountPerType[i] < enemyTypes[i].maxAliveAtOnce)
                {
                    selectedIndex = i;
                    selectedData = enemyTypes[i];
                    return true;
                }
            }

            return false;
        }

        private void HandleEnemyDeath(Enemy deadEnemy, int xpYield, int scoreYield)
        {
            deadEnemy.OnEnemyDied -= HandleEnemyDeath;
            _enemiesCurrentlyAlive--;

            // Free up the local cap limit for this enemy type
            if (_activeEnemyTypeMap.TryGetValue(deadEnemy, out int typeIndex))
            {
                _aliveCountPerType[typeIndex]--;
                _activeEnemyTypeMap.Remove(deadEnemy);
            }

            if (_gameManager != null)
            {
                _gameManager.AddXP(xpYield);
                _gameManager.AddScore(scoreYield);
            }
        }

        private IEnumerator NextRoundRoutine()
        {
            _isRoundActive = false;
            _isWaitingForNextRound = true;
            
            yield return new WaitForSeconds(timeBetweenWaves);

            StartWave(currentRound + 1);
        }
    }
}
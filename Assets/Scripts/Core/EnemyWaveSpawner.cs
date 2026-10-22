using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImPulse.Core.Pooling; 
using ImPulse.Enemies;
using ImPulse.Player;
using ImPulse.Collectibles;
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

        [Header("Configuration")]
        [SerializeField] private DifficultySettings difficultySettings;

        [Header("Drops")]
        [SerializeField] private GameObject xpPickupPrefab;
        [SerializeField] private GameObject healthPickupPrefab;
        [Tooltip("Percentage chance (0 to 1) an enemy drops a health pickup.")]
        [SerializeField] private float healthDropChance = 0.05f;

        [Header("State (Read Only)")]
        public int currentRound = 1;
        [SerializeField] private int _enemiesSpawnedThisRound = 0;
        [SerializeField] private int _enemiesCurrentlyAlive = 0;
        [SerializeField] private bool _isRoundActive = false;

        public Action<int> OnRoundChange;

        GameManager _gameManager;
        Transform _playerTransform;

        // Current Scaling 
        private int _currentTotalEnemies;
        private int _currentMaxAlive;
        private float _currentSpawnInterval;
        private int _currentGroupSize;
        private float _currentHealthModifier;

        private float _spawnTimer = 0f;
        private bool _isWaitingForNextRound = false;

        // Current Active Pool
        private List<EnemySpawnData> _currentEnemyPool = new List<EnemySpawnData>();
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
            
            if (difficultySettings == null)
            {
                UnityEngine.Debug.LogError("[EnemyWaveSpawner] Difficulty Settings not assigned!");
                return;
            }

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
            CheckForPoolUpdate(round);
            
            _spawnTimer = _currentSpawnInterval;
            _isRoundActive = true;
            _isWaitingForNextRound = false;
        }

        private void ApplyRoundScaling()
        {
            _currentTotalEnemies = Mathf.RoundToInt(difficultySettings.baseTotalEnemies * Mathf.Pow(difficultySettings.totalEnemiesMultiplier, currentRound - 1));
            _currentMaxAlive = difficultySettings.baseMaxAlive + ((currentRound - 1) * difficultySettings.maxAliveIncrement);
            _currentSpawnInterval = Mathf.Max(difficultySettings.minSpawnInterval, difficultySettings.baseSpawnInterval * Mathf.Pow(difficultySettings.spawnIntervalDecay, currentRound - 1));
            _currentGroupSize = difficultySettings.baseGroupSize + ((currentRound - 1) / Mathf.Max(1, difficultySettings.groupSizeIncreaseRate));
            _currentHealthModifier = 1f + ((currentRound - 1) * difficultySettings.healthScalingRate);
        }

        private void CheckForPoolUpdate(int round)
        {
            if (difficultySettings.roundCheckpoints == null) return;

            foreach (var checkpoint in difficultySettings.roundCheckpoints)
            {
                if (checkpoint.roundTrigger == round)
                {
                    UpdateEnemyPool(checkpoint.enemyPool);
                    break; 
                }
            }
        }

        private void UpdateEnemyPool(List<EnemySpawnData> newPool)
        {
            _currentEnemyPool = new List<EnemySpawnData>(newPool);
            
            _aliveCountPerType = new int[_currentEnemyPool.Count];
            _activeEnemyTypeMap.Clear();
            _enemiesCurrentlyAlive = 0; 
        }

        private void SpawnGroup()
        {
            int amountToSpawn = Mathf.Min(_currentGroupSize, _currentTotalEnemies - _enemiesSpawnedThisRound, _currentMaxAlive - _enemiesCurrentlyAlive);

            for (int i = 0; i < amountToSpawn; i++)
            {
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
            Vector3 spawnPos = _playerTransform.position + new Vector3(randomDir.x, randomDir.y, 0f) * difficultySettings.spawnRadius;

            GameObject enemyObj = null;
            if (ObjectPooler.Instance != null)
            {
                enemyObj = ObjectPooler.Instance.Get(dataToSpawn.enemyPrefab.gameObject, spawnPos, Quaternion.identity);
            }
            else
            {
                enemyObj = Instantiate(dataToSpawn.enemyPrefab.gameObject, spawnPos, Quaternion.identity);
            }

            if (enemyObj.TryGetComponent<Enemy>(out var enemyScript))
            {
                enemyScript.InitializeWaveStats(_currentHealthModifier, dataToSpawn.xpYield, dataToSpawn.scoreYield);
                enemyScript.OnEnemyDied += HandleEnemyDeath;
                
                _enemiesCurrentlyAlive++;
                _enemiesSpawnedThisRound++;
                
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
            
            for (int i = 0; i < _currentEnemyPool.Count; i++)
            {
                if (_currentEnemyPool[i].maxAliveAtOnce == 0 || _aliveCountPerType[i] < _currentEnemyPool[i].maxAliveAtOnce)
                {
                    totalWeight += _currentEnemyPool[i].spawnWeight;
                }
            }

            if (totalWeight <= 0) return false;

            float randomVal = UnityEngine.Random.Range(0f, totalWeight);
            
            for (int i = 0; i < _currentEnemyPool.Count; i++)
            {
                if (_currentEnemyPool[i].maxAliveAtOnce == 0 || _aliveCountPerType[i] < _currentEnemyPool[i].maxAliveAtOnce)
                {
                    if (randomVal < _currentEnemyPool[i].spawnWeight)
                    {
                        selectedIndex = i;
                        selectedData = _currentEnemyPool[i];
                        return true;
                    }
                    randomVal -= _currentEnemyPool[i].spawnWeight;
                }
            }

            for (int i = 0; i < _currentEnemyPool.Count; i++)
            {
                if (_currentEnemyPool[i].maxAliveAtOnce == 0 || _aliveCountPerType[i] < _currentEnemyPool[i].maxAliveAtOnce)
                {
                    selectedIndex = i;
                    selectedData = _currentEnemyPool[i];
                    return true;
                }
            }

            return false;
        }

        private void HandleEnemyDeath(Enemy deadEnemy, int xpYield, int scoreYield)
        {
            deadEnemy.OnEnemyDied -= HandleEnemyDeath;
            _enemiesCurrentlyAlive--;

            if (xpPickupPrefab != null && ObjectPooler.Instance != null)
            {
                GameObject xpObj = ObjectPooler.Instance.Get(xpPickupPrefab, deadEnemy.transform.position, Quaternion.identity);
                if (xpObj.TryGetComponent<XPPickup>(out var xpPickup))
                {
                    xpPickup.Initialize(xpYield);
                }
            }
            else if (_gameManager != null)
            {
                _gameManager.AddXP(xpYield); // Fallback if no prefab is assigned
            }

            if (healthPickupPrefab != null && ObjectPooler.Instance != null)
            {
                if (UnityEngine.Random.value <= healthDropChance)
                {
                    Vector3 offset = (Vector3)UnityEngine.Random.insideUnitCircle * 0.5f;
                    ObjectPooler.Instance.Get(healthPickupPrefab, deadEnemy.transform.position + offset, Quaternion.identity);
                }
            }

            if (_activeEnemyTypeMap.TryGetValue(deadEnemy, out int typeIndex))
            {
                _aliveCountPerType[typeIndex]--;
                _activeEnemyTypeMap.Remove(deadEnemy);
            }

            if (_gameManager != null)
            {
                _gameManager.AddScore(scoreYield);
            }
        }

        private IEnumerator NextRoundRoutine()
        {
            _isRoundActive = false;
            _isWaitingForNextRound = true;
            
            yield return new WaitForSeconds(difficultySettings.timeBetweenWaves);

            StartWave(currentRound + 1);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace ImPulse.Core.Pooling
{
    public interface IPoolable
    {
        void OnPoolGet(GameObject sourcePrefab);
        void OnPoolReturn();
    }

    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            public GameObject prefab;
            public int pregenCount = 10;
        }

        [Header("Pool Configuration")]
        public List<Pool> pools;

        private Dictionary<GameObject, Queue<GameObject>> _poolDictionary;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            _poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

            // pregen items
            foreach (Pool pool in pools)
            {
                if (pool.prefab == null) continue;

                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.pregenCount; i++)
                {
                    GameObject obj = Instantiate(pool.prefab, transform);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                _poolDictionary.Add(pool.prefab, objectPool);
            }
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            // create a new pool if not already pooled
            if (!_poolDictionary.ContainsKey(prefab))
            {
                _poolDictionary.Add(prefab, new Queue<GameObject>());
            }

            GameObject objToSpawn;

            if (_poolDictionary[prefab].Count > 0)
            {
                objToSpawn = _poolDictionary[prefab].Dequeue();
            }
            else
            {
                objToSpawn = Instantiate(prefab, transform);
            }

            objToSpawn.SetActive(true);
            objToSpawn.transform.position = position;
            objToSpawn.transform.rotation = rotation;

            IPoolable[] poolables = objToSpawn.GetComponentsInChildren<IPoolable>();
            foreach (var p in poolables) p.OnPoolGet(prefab);

            return objToSpawn;
        }

        public void Return(GameObject prefab, GameObject obj)
        {
            if (prefab == null || !_poolDictionary.ContainsKey(prefab))
            {
                UnityEngine.Debug.LogWarning("Trying to return an object to an invalid or untracked prefab pool.");
                Destroy(obj);
                return;
            }

            IPoolable[] poolables = obj.GetComponentsInChildren<IPoolable>();
            foreach (var p in poolables) p.OnPoolReturn();

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            
            _poolDictionary[prefab].Enqueue(obj);
        }
    }
}
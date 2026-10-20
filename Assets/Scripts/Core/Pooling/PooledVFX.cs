using System.Collections;
using UnityEngine;

namespace ImPulse.Core.Pooling
{
    [RequireComponent(typeof(ParticleSystem))]
    public class PooledVFX : MonoBehaviour, IPoolable
    {
        private GameObject _sourcePrefab;
        private ParticleSystem _particleSystem;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        public void OnPoolGet(GameObject sourcePrefab)
        {
            _sourcePrefab = sourcePrefab;
            
            _particleSystem.Play(true);
            StartCoroutine(WaitAndReturn());
        }

        public void OnPoolReturn()
        {
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private IEnumerator WaitAndReturn()
        {
            float totalDuration = _particleSystem.main.duration + _particleSystem.main.startLifetime.constantMax;
            
            yield return new WaitForSeconds(totalDuration);
            
            if (ObjectPooler.Instance != null && _sourcePrefab != null)
            {
                ObjectPooler.Instance.Return(_sourcePrefab, gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
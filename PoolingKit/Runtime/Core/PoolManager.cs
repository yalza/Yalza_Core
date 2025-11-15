using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Yalza_Core.PoolingKit.Runtime.Core
{
    [DefaultExecutionOrder(-500)]
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [Serializable]
        public class PoolDefinition
        {
            public string key;
            public GameObject prefab;
            [Min(0)] public int initialSize = 0;
            [Min(1)] public int maxSize = 100;
            public bool autoExpand = true;
        }

        [SerializeField] private List<PoolDefinition> _pools = new();

        private readonly Dictionary<string, GameObjectPool> _runtimePools = new();
        private readonly Dictionary<GameObject, GameObjectPool> _prefabLookup = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Tạo các pool được khai báo sẵn trong inspector
            foreach (var def in _pools)
            {
                if (def.prefab == null || string.IsNullOrWhiteSpace(def.key))
                    continue;

                CreatePool(def.key, def.prefab, def.initialSize, def.maxSize, def.autoExpand);
            }
        }

        // -------- Internal ----------

        public GameObjectPool GetPool(string key)
        {
            if (_runtimePools.TryGetValue(key, out var pool))
                return pool;

            Debug.LogError($"[PoolManager] No pool registered with key '{key}'");
            return null;
        }

        public GameObjectPool GetOrCreatePool(GameObject prefab,
                                              int initialSize = 0,
                                              int maxSize = 100,
                                              bool autoExpand = true)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));

            if (_prefabLookup.TryGetValue(prefab, out var pool))
                return pool;

            // Create a unique key for the new pool
            var key = prefab.name;
            var counter = 1;
            while (_runtimePools.ContainsKey(key))
            {
                key = $"{prefab.name}_{counter++}";
            }

            return CreatePool(key, prefab, initialSize, maxSize, autoExpand);
        }

        private GameObjectPool CreatePool(string key,
                                          GameObject prefab,
                                          int initialSize,
                                          int maxSize,
                                          bool autoExpand)
        {
            if (_runtimePools.TryGetValue(key, out var pool1))
            {
                Debug.LogWarning($"[PoolManager] Pool with key '{key}' already exists");
                return pool1;
            }

            var rootGo = new GameObject($"[Pool] {key}");
            rootGo.transform.SetParent(transform);
            var root = rootGo.transform;

            var pool = new GameObjectPool(key, prefab, root, initialSize, maxSize, autoExpand);

            _runtimePools.Add(key, pool);
            _prefabLookup[prefab] = pool;

            return pool;
        }

        // -------- Public API (instance) ----------

        public void ReleaseAllOf(string key)
        {
            if (_runtimePools.TryGetValue(key, out var pool))
            {
                pool.ReleaseAll();
            }
        }

        public void ClearAll()
        {
            foreach (var pool in _runtimePools.Values)
            {
                pool.Clear();
            }

            _runtimePools.Clear();
            _prefabLookup.Clear();
        }

        // -------- Static helpers (dễ gọi) ----------

        public static GameObject Spawn(string key,
                                       Vector3 position,
                                       Quaternion rotation,
                                       Transform parent = null)
        {
            var pool = Instance.GetPool(key);
            return pool?.Get(position, rotation, parent);
        }

        public static T Spawn<T>(string key,
                                 Vector3 position,
                                 Quaternion rotation,
                                 Transform parent = null)
            where T : Component
        {
            var pool = Instance.GetPool(key);
            return pool != null ? pool.Get<T>(position, rotation, parent) : null;
        }

        public static GameObject Spawn(GameObject prefab,
                                       Vector3 position,
                                       Quaternion rotation,
                                       Transform parent = null,
                                       int initialSize = 0,
                                       int maxSize = 100,
                                       bool autoExpand = true)
        {
            var pool = Instance.GetOrCreatePool(prefab, initialSize, maxSize, autoExpand);
            return pool.Get(position, rotation, parent);
        }

        public static void Despawn(GameObject instance)
        {
            if (instance == null) return;

            var pooled = instance.GetComponent<PooledObject>();
            if (pooled != null && pooled.Pool != null)
            {
                pooled.Pool.Release(instance);
            }
            else
            {
                Destroy(instance);
            }
        }
    }
}

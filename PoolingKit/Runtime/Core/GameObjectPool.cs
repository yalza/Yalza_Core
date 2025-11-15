using System.Collections.Generic;
using UnityEngine;

namespace Yalza_Core.PoolingKit.Runtime.Core
{
    public class GameObjectPool
    {
        public string Key { get; }
        public GameObject Prefab { get; }
        public Transform Root { get; }

        private readonly Stack<GameObject> _inactive = new();
        private readonly HashSet<GameObject> _active = new();

        private readonly int _maxSize;
        private readonly bool _autoExpand;

        public int CountInactive => _inactive.Count;
        public int CountActive   => _active.Count;
        public int CountAll      => CountInactive + CountActive;

        public GameObjectPool(string key,
                              GameObject prefab,
                              Transform root,
                              int initialSize,
                              int maxSize,
                              bool autoExpand)
        {
            Key = key;
            Prefab = prefab;
            Root = root;
            _maxSize = Mathf.Max(1, maxSize);
            _autoExpand = autoExpand;

            Prewarm(initialSize);
        }

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var go = CreateInstance();
                ReturnToPool(go);
            }
        }

        private GameObject CreateInstance()
        {
            var go = Object.Instantiate(Prefab, Root);
            go.name = $"{Prefab.name}_Pooled";

            var pooled = go.GetComponent<PooledObject>();
            if (pooled == null)
                pooled = go.AddComponent<PooledObject>();

            pooled.Pool = this;
            go.SetActive(false);
            return go;
        }

        private void ReturnToPool(GameObject go)
        {
            go.SetActive(false);
            go.transform.SetParent(Root, false);
            _inactive.Push(go);
        }

        public GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject instance;

            if (_inactive.Count > 0)
            {
                instance = _inactive.Pop();
            }
            else if (_autoExpand || CountAll < _maxSize)
            {
                instance = CreateInstance();
            }
            else
            {
                // Pool full: vẫn cho tạo thêm (tuỳ policy bạn có thể đổi lại).
                instance = CreateInstance();
            }

            _active.Add(instance);

            var t = instance.transform;
            t.SetParent(parent, false);
            t.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);

            // Gọi IPoolable
            if (instance.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnSpawned();
            }

            return instance;
        }

        public T Get<T>(Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component
        {
            return Get(position, rotation, parent).GetComponent<T>();
        }

        public void Release(GameObject instance)
        {
            if (instance == null) return;

            if (!_active.Remove(instance))
            {
                // Không thuộc active set -> destroy tránh giữ zombie ngoài pool
                Object.Destroy(instance);
                return;
            }

            if (instance.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnDeSpawned();
            }

            ReturnToPool(instance);
        }

        public void ReleaseAll()
        {
            var temp = new List<GameObject>(_active);
            foreach (var go in temp)
            {
                Release(go);
            }
        }

        public void Clear()
        {
            foreach (var go in _inactive)
            {
                Object.Destroy(go);
            }
            _inactive.Clear();

            foreach (var go in _active)
            {
                Object.Destroy(go);
            }
            _active.Clear();

            if (Root != null)
            {
                Object.Destroy(Root.gameObject);
            }
        }
    }
}

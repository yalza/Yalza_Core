using System;
using UnityEngine;

namespace Yalza_Core.SingletonKit.Runtime.Core
{
    public interface IMonoSingleton
    {
        bool IsInitialized { get; }
    }

    public abstract class MonoSingleton<T> : MonoBehaviour, IMonoSingleton
        where T : MonoSingleton<T>
    {
        private static T _instance;
        private static bool _applicationIsQuitting;

        private bool _isInitialized;

        protected virtual bool UseDontDestroyOnLoad => true;

        public static bool HasInstance => _instance != null && !_applicationIsQuitting;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    return null;
                }

                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<T>();
                if (_instance != null)
                {
                    _instance.InternalInitIfNeeded();
                    return _instance;
                }

                var go = new GameObject(typeof(T).Name);
                _instance = go.AddComponent<T>();
                _instance.InternalInitIfNeeded();
                return _instance;
            }
        }

        public bool IsInitialized => _isInitialized;

        protected virtual void Awake()
        {
            if (_applicationIsQuitting)
            {
                // Đang quit, không giữ thêm instance.
                Destroy(gameObject);
                return;
            }

            if (_instance == null)
            {
                _instance = (T)this;
                InternalInitIfNeeded();
            }
            else if (_instance != this)
            {
                // Duplicate -> destroy bản mới.
                Debug.LogWarning($"[MonoSingleton<{typeof(T).Name}>] Duplicate found on {gameObject.name}, destroying this one.");
                Destroy(gameObject);
            }
        }

        private void InternalInitIfNeeded()
        {
            if (_isInitialized) return;

            _isInitialized = true;

            if (UseDontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            SingletonRegistry.Register(this);
            OnSingletonInit();
        }

        /// <summary>
        /// Chỉ gọi một lần khi instance được khởi tạo.
        /// </summary>
        protected virtual void OnSingletonInit() { }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                SingletonRegistry.Unregister(this);
                _instance = null;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}

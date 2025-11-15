using UnityEngine;

namespace Yalza_Core.SingletonKit.Runtime.Core
{
    public abstract class MonoSingletonInScene<T> : MonoBehaviour, IMonoSingleton
        where T : MonoSingletonInScene<T>
    {
        private static T _instance;
        private bool _isInitialized;

        public static bool HasInstance => _instance != null;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = FindObjectOfType<T>();
                if (_instance != null)
                {
                    _instance.InternalInitIfNeeded();
                }

                return _instance;
            }
        }

        public bool IsInitialized => _isInitialized;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = (T)this;
                InternalInitIfNeeded();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[MonoSingletonInScene<{typeof(T).Name}>] Duplicate found on {gameObject.name}, destroying this one.");
                Destroy(gameObject);
            }
        }

        private void InternalInitIfNeeded()
        {
            if (_isInitialized) return;

            _isInitialized = true;
            SingletonRegistry.Register(this);
            OnSingletonInit();
        }

        protected virtual void OnSingletonInit() { }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                SingletonRegistry.Unregister(this);
                _instance = null;
            }
        }
    }
}

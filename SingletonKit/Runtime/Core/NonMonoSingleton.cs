namespace Yalza_Core.SingletonKit.Runtime.Core
{
    public interface INonMonoSingleton
    {
        bool IsInitialized { get; }
        void OnSingletonInit();
    }

    public abstract class NonMonoSingleton<T> : INonMonoSingleton
        where T : NonMonoSingleton<T>, new()
    {
        private static readonly object Lock = new object();
        private static T _instance;

        private bool _isInitialized;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                        _instance.InternalInit();
                    }
                }

                return _instance;
            }
        }

        public bool IsInitialized => _isInitialized;

        private void InternalInit()
        {
            if (_isInitialized) return;

            _isInitialized = true;

            // Đăng ký với Registry để debug / lấy instance theo type.
            SingletonRegistry.Register(this);

            OnSingletonInit();
        }

        /// <summary>
        /// Gọi một lần duy nhất sau khi instance được tạo.
        /// </summary>
        public virtual void OnSingletonInit() { }

        /// <summary>
        /// Nếu bạn muốn reset singleton (ví dụ khi đổi profile, reload game),
        /// gọi từ chính instance hiện tại.
        /// </summary>
        protected void DestroySingleton()
        {
            if (ReferenceEquals(_instance, this))
            {
                SingletonRegistry.Unregister(this);
                _instance = null;
                _isInitialized = false;
            }
        }
    }
}

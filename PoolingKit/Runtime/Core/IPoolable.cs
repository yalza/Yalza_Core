namespace Yalza_Core.PoolingKit.Runtime.Core
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDeSpawned();
    }
}

// Assets/Yalza_Core/Pooling/Runtime/Core/PooledObject.cs

using UnityEngine;

namespace Yalza_Core.PoolingKit.Runtime.Core
{
    [DisallowMultipleComponent]
    public class PooledObject : MonoBehaviour
    {
        internal GameObjectPool Pool { get; set; }

        public bool IsSpawned => Pool != null && gameObject.activeSelf;

        // Despawn this object back to its pool
        public void Despawn()
        {
            if (Pool != null)
            {
                Pool.Release(gameObject);
            }
            else
            {
                // If the object is not associated with any pool, just destroy it
                Destroy(gameObject);
            }
        }
    }
}

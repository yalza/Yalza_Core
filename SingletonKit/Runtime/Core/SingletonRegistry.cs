using System.Collections.Generic;
using System.Linq;

namespace Yalza_Core.SingletonKit.Runtime.Core
{
    public static class SingletonRegistry
    {
        private static readonly HashSet<object> _instances = new HashSet<object>();
        public static IReadOnlyCollection<object> Instances => _instances;

        internal static void Register(object instance)
        {
            if (instance == null) return;
            _instances.Add(instance);
        }

        internal static void Unregister(object instance)
        {
            if (instance == null) return;
            _instances.Remove(instance);
        }

        public static T Get<T>() where T : class
        {
            return _instances.OfType<T>().FirstOrDefault();
        }
    }
}

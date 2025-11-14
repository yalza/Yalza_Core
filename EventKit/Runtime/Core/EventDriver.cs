using UnityEngine;

namespace Yalza_Core.EventKit.Runtime.Core
{
    [DefaultExecutionOrder(-10000)]
    internal sealed class EventBusDriver : MonoBehaviour
    {
        private static bool _created;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetFlag() => _created = false;

        public static void Bootstrap()
        {
            if (_created) return;
            var go = new GameObject("~EventBus")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            DontDestroyOnLoad(go);
            go.AddComponent<EventBusDriver>();
            _created = true;
        }

        private void Update() => EventBus.Drain();
    }
}

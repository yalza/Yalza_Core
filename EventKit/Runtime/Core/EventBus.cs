using System;
using System.Collections.Generic;

namespace Yalza_Core.EventKit.Runtime.Core
{
    public sealed class EventHandle : IDisposable
    {
        private Type _t;
        private Delegate _d;
        internal EventHandle(Type t, Delegate d) { _t = t; _d = d; }
        public void Dispose()
        {
            if (_t == null || _d == null) return;
            EventBus.Unsubscribe(_t, _d);
            _t = null; _d = null;
        }
    }
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Map = new();
        private static readonly Queue<object> Queue = new();
        private static readonly Dictionary<Type, object> Sticky = new();
        private static bool _driverReady;

        #region Subscribe / Unsubscribe
        public static EventHandle Subscribe<T>(Action<T> listener)
        {
            var t = typeof(T);
            if (Map.TryGetValue(t, out var del)) Map[t] = (Action<T>)del + listener;
            else Map[t] = listener;
            return new EventHandle(t, listener);
        }

        public static EventHandle SubscribeSticky<T>(Action<T> listener)
        {
            var h = Subscribe(listener);
            if (Sticky.TryGetValue(typeof(T), out var obj))
                listener((T)obj);
            return h;
        }

        internal static void Unsubscribe(Type t, Delegate listener)
        {
            if (!Map.TryGetValue(t, out var del)) return;
            var cur = Delegate.Remove(del, listener);
            if (cur == null) Map.Remove(t);
            else Map[t] = cur;
        }
        #endregion

        #region Publish / Post
        public static void Publish<T>(T evt)
        {
            if (Map.TryGetValue(typeof(T), out var del))
                ((Action<T>)del)?.Invoke(evt);
        }

        public static void Post<T>(T evt)
        {
            Queue.Enqueue(evt);
            EnsureDriver();
        }

        public static void PublishSticky<T>(T evt)
        {
            Sticky[typeof(T)] = evt!;
            Publish(evt);
        }
        #endregion

        #region Driver (đảm bảo chạy trên main thread)
        internal static void Drain()
        {
            while (Queue.Count > 0)
            {
                var e = Queue.Dequeue();
                var t = e.GetType();
                if (Map.TryGetValue(t, out var del))
                    del.DynamicInvoke(e);
            }
        }

        private static void EnsureDriver()
        {
            if (_driverReady) return;
            _driverReady = true;
            EventBusDriver.Bootstrap();
        }
        #endregion
    }
}

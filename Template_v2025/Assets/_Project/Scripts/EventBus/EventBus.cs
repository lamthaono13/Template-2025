// EventBus.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EventBus - instance, typed publish/subscribe event bus.
/// Non-static so it can be registered as a service, mocked in tests, swapped, etc.
/// Feature parity with your AdvancedEventBus: subscribe token, priority, once, sticky, safe unsubscribe during dispatch.
/// </summary>
public class EventBus : IEventBus
{
    class Subscriber
    {
        public Guid id;
        public Delegate handler;
        public int priority;
        public bool once;
    }

    readonly Dictionary<Type, List<Subscriber>> subscribers = new Dictionary<Type, List<Subscriber>>();
    readonly Dictionary<Type, object> stickyEvents = new Dictionary<Type, object>();
    readonly object sync = new object();

    public Action Subscribe<T>(Action<T> handler, int priority = 0, bool once = false, bool receiveSticky = false)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        var t = typeof(T);
        var sub = new Subscriber { id = Guid.NewGuid(), handler = handler, priority = priority, once = once };

        lock (sync)
        {
            if (!subscribers.TryGetValue(t, out var list))
            {
                list = new List<Subscriber>();
                subscribers[t] = list;
            }
            // insert by priority descending (higher priority first)
            int idx = list.FindIndex(s => s.priority < priority);
            if (idx == -1) list.Add(sub); else list.Insert(idx, sub);
        }

        // handle sticky immediately outside lock
        if (receiveSticky)
        {
            if (stickyEvents.TryGetValue(t, out var last))
            {
                try
                {
                    handler((T)last);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                // if once requested, unsubscribe immediately
                if (once) UnsubscribeById<T>(sub.id);
            }
        }

        // return unsubscribe token
        return () => UnsubscribeById<T>(sub.id);
    }

    public void Publish<T>(T payload, bool sticky = false)
    {
        var t = typeof(T);

        // set sticky outside lock to avoid handler calls within lock
        if (sticky)
        {
            lock (sync)
            {
                stickyEvents[t] = payload;
            }
        }

        List<Subscriber> snapshot = null;
        lock (sync)
        {
            if (!subscribers.TryGetValue(t, out var list) || list.Count == 0) return;
            // copy to avoid mutation during iteration (safe unsubscribe in handlers)
            snapshot = list.ToList();
        }

        foreach (var sub in snapshot)
        {
            var handler = sub.handler as Action<T>;
            if (handler == null) continue;
            try
            {
                handler(payload);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            if (sub.once) UnsubscribeById<T>(sub.id);
        }
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        if (handler == null) return;
        var t = typeof(T);
        lock (sync)
        {
            if (!subscribers.TryGetValue(t, out var list)) return;
            list.RemoveAll(s => s.handler == (Delegate)handler);
            if (list.Count == 0) subscribers.Remove(t);
        }
    }

    void UnsubscribeById<T>(Guid id)
    {
        var t = typeof(T);
        lock (sync)
        {
            if (!subscribers.TryGetValue(t, out var list)) return;
            list.RemoveAll(s => s.id == id);
            if (list.Count == 0) subscribers.Remove(t);
        }
    }

    public void ClearSticky<T>()
    {
        var t = typeof(T);
        lock (sync)
        {
            stickyEvents.Remove(t);
        }
    }

    public void ClearAllSticky()
    {
        lock (sync)
        {
            stickyEvents.Clear();
        }
    }

    public bool HasSubscribers<T>()
    {
        var t = typeof(T);
        lock (sync)
        {
            return subscribers.TryGetValue(t, out var list) && list.Count > 0;
        }
    }

    public int SubscriberCount<T>()
    {
        var t = typeof(T);
        lock (sync)
        {
            if (!subscribers.TryGetValue(t, out var list)) return 0;
            return list.Count;
        }
    }

    public void ClearAll()
    {
        lock (sync)
        {
            subscribers.Clear();
            stickyEvents.Clear();
        }
    }
}

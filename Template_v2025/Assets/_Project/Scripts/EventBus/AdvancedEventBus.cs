// AdvancedEventBus.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// AdvancedEventBus - type-safe publish/subscribe event bus for Unity.
/// - Subscribe returns an Action (unsubscribe token).
/// - Supports sticky events, priority, once-only subscription.
/// - Safe to unsubscribe during dispatch (copy-on-iterate).
/// - Basic thread-safety for registrations/publishes (locks).
/// NOTE: Unity API must be called on main thread. If you publish from background thread,
/// marshal to main thread before invoking UnityEngine calls inside handlers.
/// </summary>
public static class AdvancedEventBus
{
    // internal subscriber record
    class Subscriber
    {
        public Guid id;
        public Delegate handler;
        public int priority;
        public bool once;
    }

    // map event type -> list of subscribers
    static readonly Dictionary<Type, List<Subscriber>> subscribers = new Dictionary<Type, List<Subscriber>>();

    // sticky events storage
    static readonly Dictionary<Type, object> stickyEvents = new Dictionary<Type, object>();

    // locking object for thread-safety across register/unregister/publish
    static readonly object sync = new object();

    /// <summary>
    /// Subscribe to event type T. Returns an Action which, when invoked, unsubscribes.
    /// Parameters:
    /// - handler: Action<T>
    /// - priority: higher value called earlier (default 0)
    /// - once: if true, handler is automatically removed after first invocation
    /// - receiveSticky: if true and a sticky value exists it will be invoked immediately (synchronously) with that value
    /// </summary>
    public static Action Subscribe<T>(Action<T> handler, int priority = 0, bool once = false, bool receiveSticky = false)
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

    /// <summary>
    /// Publish an event payload of type T.
    /// If sticky==true, value is stored and future subscribers with receiveSticky=true will receive it.
    /// </summary>
    public static void Publish<T>(T payload, bool sticky = false)
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

        // iterate snapshot
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
            // if subscriber wanted once, remove it
            if (sub.once) UnsubscribeById<T>(sub.id);
        }
    }

    /// <summary>
    /// Unsubscribe by providing the same handler reference (optional).
    /// Use the token returned by Subscribe for safest removal.
    /// </summary>
    public static void Unsubscribe<T>(Action<T> handler)
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

    // internal: remove subscriber by id
    static void UnsubscribeById<T>(Guid id)
    {
        var t = typeof(T);
        lock (sync)
        {
            if (!subscribers.TryGetValue(t, out var list)) return;
            list.RemoveAll(s => s.id == id);
            if (list.Count == 0) subscribers.Remove(t);
        }
    }

    /// <summary>
    /// Clear sticky event for type T.
    /// </summary>
    public static void ClearSticky<T>()
    {
        var t = typeof(T);
        lock (sync)
        {
            stickyEvents.Remove(t);
        }
    }

    /// <summary>
    /// Clear all sticky events.
    /// </summary>
    public static void ClearAllSticky()
    {
        lock (sync)
        {
            stickyEvents.Clear();
        }
    }

    /// <summary>
    /// Check if there are subscribers for type T.
    /// </summary>
    public static bool HasSubscribers<T>()
    {
        var t = typeof(T);
        lock (sync)
        {
            return subscribers.TryGetValue(t, out var list) && list.Count > 0;
        }
    }

    /// <summary>
    /// Debug helper: returns number of subscribers for type T (editor only recommended).
    /// </summary>
    public static int SubscriberCount<T>()
    {
        var t = typeof(T);
        lock (sync)
        {
            if (!subscribers.TryGetValue(t, out var list)) return 0;
            return list.Count;
        }
    }

    /// <summary>
    /// Completely clear all subscribers and sticky events.
    /// USE WITH CARE (for example when resetting game).
    /// </summary>
    public static void ClearAll()
    {
        lock (sync)
        {
            subscribers.Clear();
            stickyEvents.Clear();
        }
    }
}
using System;
using System.Collections.Generic;

public static class EventBus
{
    [System.NonSerialized]
    private static readonly Dictionary<Type, List<Action<IGameEvent>>> _listeners
        = new Dictionary<Type, List<Action<IGameEvent>>>();

    public static void AddListener<T>(Action<T> callback) where T : IGameEvent
    {
        Type t = typeof(T);

        if (!_listeners.ContainsKey(t))
            _listeners[t] = new List<Action<IGameEvent>>();

        // wrap: IGameEvent → T
        _listeners[t].Add((e) => callback((T)e));
    }

    public static void RemoveListener<T>(Action<T> callback) where T : IGameEvent
    {
        Type t = typeof(T);
        if (!_listeners.ContainsKey(t)) return;

        _listeners[t].RemoveAll(l => l.Equals((Action<IGameEvent>)((e) => callback((T)e))));
    }

    public static void Raise(IGameEvent evt)
    {
        Type t = evt.GetType();
        if (!_listeners.ContainsKey(t)) return;

        foreach (var listener in _listeners[t])
            listener.Invoke(evt);
    }
}

public interface IGameEvent { }
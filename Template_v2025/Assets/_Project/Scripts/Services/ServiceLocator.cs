using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    // Registry map interface/type -> instance
    private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
    private static readonly object syncRoot = new object();

    // Register an instance for TService. Overwrite if exists.
    public static void Register<TService>(TService instance) where TService : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        var type = typeof(TService);
        lock (syncRoot)
        {
            services[type] = instance;
        }
    }

    // Try register only if not exists
    public static bool TryRegister<TService>(TService instance) where TService : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        var type = typeof(TService);
        lock (syncRoot)
        {
            if (services.ContainsKey(type)) return false;
            services[type] = instance;
            return true;
        }
    }

    // Resolve — will throw if not found
    public static TService Get<TService>() where TService : class
    {
        var type = typeof(TService);
        lock (syncRoot)
        {
            if (services.TryGetValue(type, out var obj))
                return obj as TService;
        }
        throw new InvalidOperationException($"Service not registered: {type.FullName}");
    }

    // Safe resolve — return null if not found
    public static TService TryGet<TService>() where TService : class
    {
        var type = typeof(TService);
        lock (syncRoot)
        {
            if (services.TryGetValue(type, out var obj))
                return obj as TService;
        }
        return null;
    }

    // Unregister
    public static bool Unregister<TService>() where TService : class
    {
        var type = typeof(TService);
        lock (syncRoot)
        {
            return services.Remove(type);
        }
    }

    // Clear all (useful for tests)
    public static void ClearAll()
    {
        lock (syncRoot)
            services.Clear();
    }
}
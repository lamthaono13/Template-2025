using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();

    public static void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (service == null)
        {
            services.Remove(type);    // ⬅ hủy đăng ký
            return;
        }
        services[type] = service;    // ⬅ đăng ký
    }

    public static void Unregister<T>()
    {
        services.Remove(typeof(T));
    }

    public static T Get<T>() where T : class
    {
        services.TryGetValue(typeof(T), out var s);
        return s as T;
    }
}
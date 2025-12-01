using Lean.Pool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] private LeanGameObjectPool leanGameObjectPool;

    //private Dictionary<Type, IPool<Component>> pools = new Dictionary<Type, IPool<Component>>();

    public void Init()
    {

    }

    public T GetPool<T>(Transform parent) where T : Component
    {
        return leanGameObjectPool.Spawn(parent).GetComponent<T>();
    }

    public void TakeToPool<T>(GameObject objTake)
    {
        leanGameObjectPool.Despawn(objTake);
    }
}

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServicesBootstrap : MonoBehaviour
{
    [SerializeField] private List<GameObject> objInitializables;

    void Awake()
    {
        // Nếu đã bootstrap rồi thì hủy (tránh double-init khi có nhiều bản)
        if (ServiceLocator.TryGet<ServicesBootstrap>() != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        //

        for(int i = 0; i < objInitializables.Count; i++)
        {
            var initializables = objInitializables[i].GetComponents<IInitializable>();
            for (int j = 0; j < initializables.Length; j++)
            {
                initializables[j].Initialize();
            }
        }

        //
        ServiceLocator.Register<ServicesBootstrap>(this);

        Debug.Log("ServicesBootstrap: All services registered.");
    }

    void OnDestroy()
    {
        ServiceLocator.ClearAll();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServicesBootstrap : MonoBehaviour
{
    void Awake()
    {
        // Nếu đã bootstrap rồi thì hủy (tránh double-init khi có nhiều bản)
        if (ServiceLocator.TryGet<ServicesBootstrap>() != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        var dataService = new DataService();
        ServiceLocator.Register<IDataService>(dataService);


        ServiceLocator.Register<ServicesBootstrap>(this);

        Debug.Log("ServicesBootstrap: All services registered.");
    }

    void OnDestroy()
    {
        ServiceLocator.ClearAll();
    }
}

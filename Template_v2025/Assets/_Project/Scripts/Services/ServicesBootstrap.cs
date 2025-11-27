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

        // 1) Core low-level services (không phụ thuộc gì)
        //var playerPrefsService = new PlayerPrefsService();
        //ServiceLocator.Register<IPlayerPrefsService>(playerPrefsService);

        var dataService = new DataService();
        ServiceLocator.Register<IDataService>(dataService);

        // 2) Mid-level services (phụ thuộc vào core)
        //var configManager = new DataConfigManager(dataService);
        //configManager.LoadAll(); // nếu cần load ngay
        //ServiceLocator.Register<IDataConfigManager>(configManager);

        // 3) Optional - audio, save, analytics...
        // var audio = new AudioService();
        // ServiceLocator.Register<IAudioService>(audio);

        // 4) Register the bootstrap itself so TryGet check works later (optional)
        ServiceLocator.Register<ServicesBootstrap>(this);

        Debug.Log("ServicesBootstrap: All services registered.");
    }

    // Nếu bạn muốn hỗ trợ cleanup khi game đóng:
    void OnDestroy()
    {
        // optional: call Dispose on IDisposable services then clear registry
        ServiceLocator.ClearAll();
    }
}

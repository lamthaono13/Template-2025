// EventBusService.cs
using UnityEngine;

/// <summary>
/// MonoBehaviour wrapper that instantiates a singleton EventBus and registers it with ServiceLocator.
/// Attach this to a GameObject in your initial scene (or PoolRoot/Bootstrapper).
/// </summary>
public class EventBusService : MonoBehaviour
{
    // the instance created for this running app; kept for convenience if you want to access directly
    public EventBus Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = new EventBus();
            ServiceLocator.Register<IEventBus>(Instance);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // cleanup: clear subscriptions & sticky events to avoid dangling references
        Instance?.ClearAll();
        ServiceLocator.Unregister<IEventBus>();

        // optional: unregister from ServiceLocator if you implement unregister. If ServiceLocator doesn't support unregister,
        // keep in mind the instance is left in registry (good to implement Unregister in ServiceLocator if needed).
    }
}
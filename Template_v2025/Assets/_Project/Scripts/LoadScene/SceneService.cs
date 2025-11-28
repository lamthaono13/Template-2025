using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System;

public class SceneService : MonoBehaviour, ISceneService, IInitializable
{
    public bool IsLoading { get; private set; }

    void Awake()
    {

    }

    public async UniTask LoadSceneAsync(string sceneName, bool additive = false)
    {
        if (IsLoading) return;
        IsLoading = true;

        var mode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
        var op = SceneManager.LoadSceneAsync(sceneName, mode);

        var bus = ServiceLocator.Get<IEventBus>();
        bus?.Publish(new SceneLoadingEvent(sceneName, 0f));

        while (!op.isDone)
        {
            float normalized = (op.progress < 0.9f) ? (op.progress / 0.9f) : 1f;
            bus?.Publish(new SceneLoadingEvent(sceneName, normalized));
            await UniTask.Yield(); // next frame
        }

        bus?.Publish(new SceneLoadedEvent(sceneName));

        IsLoading = false;
    }

    public async UniTask ReloadSceneAsync()
    {
        await LoadSceneAsync(SceneManager.GetActiveScene().name, false);
    }

    public async UniTask UnloadSceneAsync(string sceneName)
    {
        var op = SceneManager.UnloadSceneAsync(sceneName);
        await op.ToUniTask();
        ServiceLocator.Get<IEventBus>()?.Publish(new SceneUnloadedEvent(sceneName));
    }

    public void Initialize()
    {
        if (ServiceLocator.TryGet<ISceneService>() == null)
            ServiceLocator.Register<ISceneService>(this);

        DontDestroyOnLoad(gameObject);
    }
}

public readonly struct SceneLoadingEvent
{
    public readonly string SceneName;
    public readonly float Progress;

    public SceneLoadingEvent(string sceneName, float progress)
    {
        SceneName = sceneName;
        Progress = progress;
    }
}

public readonly struct SceneLoadedEvent
{
    public readonly string SceneName;

    public SceneLoadedEvent(string sceneName)
    {
        SceneName = sceneName;
    }
}

public readonly struct SceneUnloadedEvent
{
    public readonly string SceneName;

    public SceneUnloadedEvent(string sceneName)
    {
        SceneName = sceneName;
    }
}
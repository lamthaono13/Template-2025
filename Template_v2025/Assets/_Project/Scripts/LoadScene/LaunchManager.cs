using Cysharp.Threading.Tasks;
using UnityEngine;

public class LaunchManager : MonoBehaviour, IInitializable
{
    //// optional: expose in inspector to override behavior quickly
    //[Tooltip("If true, use save to decide next scene; otherwise load fallbackScene")]
    //public bool useSave = true;
    //public string fallbackScene = "MainMenu";

    //ISceneService sceneSvc;
    //IEventBus bus;
    //ISaveService saveSvc; // assume you have one
    //bool initialized = false;

    public void Initialize()
    {
        //if (initialized) return;
        //initialized = true;

        //// Resolve services from ServiceLocator (they should be registered by Bootstrapper)
        //sceneSvc = ServiceLocator.Get<ISceneService>();
        //bus = ServiceLocator.Get<IEventBus>();
        //saveSvc = ServiceLocator.Get<ISaveService>(); // may be null if you didn't register

        //// Start decide-and-load (use UniTask) but don't block the Initialize caller unless you want to.
        //// We will await here to ensure deterministic flow — Bootstrapper controls ordering.
        //_ = DecideAndLoadAsync().Forget(); // fire-and-forget is okay OR await if Bootstrapper expects completion
    }

    //async UniTaskVoid DecideAndLoadAsync()
    //{
    //    // publish event about launch start if you want
    //    bus?.Publish(new LaunchStartedEvent());

    //    string nextScene = fallbackScene;

    //    try
    //    {
    //        // Example decision logic:
    //        if (useSave && saveSvc != null)
    //        {
    //            var state = saveSvc.LoadLastSaveMeta(); // your save API: could return null or object
    //            if (state != null && state.lastLevelName != null)
    //            {
    //                nextScene = state.lastLevelName; // resume
    //            }
    //            else
    //            {
    //                nextScene = fallbackScene; // new game or menu
    //            }
    //        }
    //        else
    //        {
    //            // fallback: maybe deep link, remote config, A/B test, etc.
    //            nextScene = fallbackScene;
    //        }

    //        // Optional: notify loading UI via EventBus
    //        bus?.Publish(new SceneLoadingEvent(nextScene, 0f), sticky: true);

    //        // Await load so you can do post-load init if needed
    //        await sceneSvc.LoadSceneAsync(nextScene, additive: false);

    //        // Optionally publish more events
    //        bus?.Publish(new SceneLoadedEvent(nextScene));

    //        // If you need to do scene-specific initialization:
    //        // await WaitForSceneInitializablesToComplete(nextScene);
    //        bus?.Publish(new LaunchCompletedEvent(nextScene));
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Debug.LogError($"LaunchManager DecideAndLoadAsync failed: {ex}");
    //        // fallback: load fallbackScene if not already
    //        if (nextScene != fallbackScene)
    //        {
    //            await sceneSvc.LoadSceneAsync(fallbackScene);
    //            bus?.Publish(new SceneLoadedEvent(fallbackScene));
    //        }
    //    }
    //}
}

// events used above
public readonly struct LaunchStartedEvent { }
public readonly struct LaunchCompletedEvent { public readonly string SceneName; public LaunchCompletedEvent(string name) { SceneName = name; } }
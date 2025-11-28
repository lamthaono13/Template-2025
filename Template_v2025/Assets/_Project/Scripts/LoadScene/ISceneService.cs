using Cysharp.Threading.Tasks;
using System;

public interface ISceneService
{
    UniTask LoadSceneAsync(string sceneName, bool additive = false);
    UniTask ReloadSceneAsync();
    UniTask UnloadSceneAsync(string sceneName);
}
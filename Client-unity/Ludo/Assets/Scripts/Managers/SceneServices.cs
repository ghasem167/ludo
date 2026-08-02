using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneService
{
    #region Events

    public event Action<GameScene> BeforeSceneLoad;
    public event Action<GameScene> AfterSceneLoad;

    public event Action<GameScene> BeforeSceneUnload;
    public event Action<GameScene> AfterSceneUnload;

    public event Action LoadingStarted;
    public event Action<float> LoadingProgressChanged;
    public event Action LoadingFinished;

    #endregion

    #region Scene Listeners

    private readonly Dictionary<GameScene, HashSet<Action>> _beforeLoadListeners = new();
    private readonly Dictionary<GameScene, HashSet<Action>> _afterLoadListeners = new();

    private readonly Dictionary<GameScene, HashSet<Action>> _beforeUnloadListeners = new();
    private readonly Dictionary<GameScene, HashSet<Action>> _afterUnloadListeners = new();

    #endregion

    #region Register

    public void RegisterBeforeSceneLoad(GameScene scene, Action listener)
        => Register(_beforeLoadListeners, scene, listener);

    public void RegisterAfterSceneLoad(GameScene scene, Action listener)
        => Register(_afterLoadListeners, scene, listener);

    public void RegisterBeforeSceneUnload(GameScene scene, Action listener)
        => Register(_beforeUnloadListeners, scene, listener);

    public void RegisterAfterSceneUnload(GameScene scene, Action listener)
        => Register(_afterUnloadListeners, scene, listener);

    #endregion

    #region Unregister

    public void UnregisterBeforeSceneLoad(GameScene scene, Action listener)
        => Unregister(_beforeLoadListeners, scene, listener);

    public void UnregisterAfterSceneLoad(GameScene scene, Action listener)
        => Unregister(_afterLoadListeners, scene, listener);

    public void UnregisterBeforeSceneUnload(GameScene scene, Action listener)
        => Unregister(_beforeUnloadListeners, scene, listener);

    public void UnregisterAfterSceneUnload(GameScene scene, Action listener)
        => Unregister(_afterUnloadListeners, scene, listener);

    #endregion

    #region Load

    public async Task LoadSceneAsync(GameScene scene,
        LoadSceneMode mode = LoadSceneMode.Single,
        bool showLoading = true)
    {
        if (showLoading)
            LoadingStarted?.Invoke();

        InvokeBeforeSceneLoad(scene);

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(scene.ToString(), mode);

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            LoadingProgressChanged?.Invoke(operation.progress);

            await Task.Yield();
        }

        InvokeAfterSceneLoad(scene);

        if (showLoading)
            LoadingFinished?.Invoke();
    }

    #endregion

    #region Unload

    public async Task UnloadSceneAsync(GameScene scene)
    {
        InvokeBeforeSceneUnload(scene);

        AsyncOperation operation =
            SceneManager.UnloadSceneAsync(scene.ToString());

        if (operation != null)
        {
            while (!operation.isDone)
                await Task.Yield();
        }

        InvokeAfterSceneUnload(scene);
    }

    #endregion

    #region Invoke

    private void InvokeBeforeSceneLoad(GameScene scene)
    {
        BeforeSceneLoad?.Invoke(scene);

        InvokeListeners(_beforeLoadListeners, scene);
    }

    private void InvokeAfterSceneLoad(GameScene scene)
    {
        AfterSceneLoad?.Invoke(scene);

        InvokeListeners(_afterLoadListeners, scene);
    }

    private void InvokeBeforeSceneUnload(GameScene scene)
    {
        BeforeSceneUnload?.Invoke(scene);

        InvokeListeners(_beforeUnloadListeners, scene);
    }

    private void InvokeAfterSceneUnload(GameScene scene)
    {
        AfterSceneUnload?.Invoke(scene);

        InvokeListeners(_afterUnloadListeners, scene);
    }

    #endregion

    #region Helpers

    private static void Register(
        Dictionary<GameScene, HashSet<Action>> map,
        GameScene scene,
        Action listener)
    {
        if (!map.TryGetValue(scene, out HashSet<Action> listeners))
        {
            listeners = new HashSet<Action>();
            map.Add(scene, listeners);
        }

        listeners.Add(listener);
    }

    private static void Unregister(
        Dictionary<GameScene, HashSet<Action>> map,
        GameScene scene,
        Action listener)
    {
        if (map.TryGetValue(scene, out HashSet<Action> listeners))
        {
            listeners.Remove(listener);

            if (listeners.Count == 0)
                map.Remove(scene);
        }
    }

    private static void InvokeListeners(
        Dictionary<GameScene, HashSet<Action>> map,
        GameScene scene)
    {
        if (!map.TryGetValue(scene, out HashSet<Action> listeners))
            return;

        foreach (Action listener in listeners)
        {
            try
            {
                listener?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    #endregion
}
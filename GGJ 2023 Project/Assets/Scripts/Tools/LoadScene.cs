using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads a scene based off the supplied string
/// Can load immediately or asynchronously
/// </summary>

public class LoadScene : MonoBehaviour
{
    // name of scene to load
    [SerializeField] private string _sceneName;
    // whether the scene should be loaded immediately or asynchronously
    [SerializeField] private bool _async;

    void OnEnable ()
    {
        if (_async)
        {
            LoadAsync();
        } else 
        {
            Load();
        }
    }

    // loads scene with specified name
    public void Load (string p_sceneName)
    {
        SceneManager.LoadScene(p_sceneName);
    }

    // loads scene name set in the inspector
    public void Load ()
    {
        Load(_sceneName);
    }

    // loads scene with specified name asynchronously
    public void LoadAsync (string p_sceneName)
    {
        SceneManager.LoadSceneAsync(p_sceneName);
    }

    // loads scene name set in the inspector asynchronously
    public void LoadAsync ()
    {
        LoadAsync(_sceneName);
    }
}
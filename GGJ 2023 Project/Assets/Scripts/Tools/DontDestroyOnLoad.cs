using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Prevents the Game Object this is attached to from being destroyed when loading scenes
/// </summary>
public class DontDestroyOnLoad : MonoBehaviour
{
    void OnEnable ()
    {
        GameObject.DontDestroyOnLoad(gameObject);
    }
}
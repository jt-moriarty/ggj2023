using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Invokes UnityEvents when enabled
/// </summary>
public class InvokeEvents : MonoBehaviour
{
    /// <summary>
    /// These events will fire when the component is enabled
    /// </summary>
    [SerializeField] private UnityEvent Events = new UnityEvent();

    void OnEnable ()
    {
        Invoke();
    }

    public void Invoke ()
    {
        Events.Invoke();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sets Timescale to supplied value
/// </summary>
public class SetTimescale : MonoBehaviour
{
    [SerializeField] private float _timescale;

    void OnEnable ()
    {
        SetTime(_timescale);
        enabled = false;
    }

    public void SetTime (float p_timescale)
    {
        Time.timeScale = p_timescale;
    }
}
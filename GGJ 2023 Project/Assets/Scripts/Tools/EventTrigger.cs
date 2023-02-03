using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Fires Unity Events when OnTriggerEnter fires with an object with a specified tag
/// </summary>

public class EventTrigger : MonoBehaviour
{
    /// <summary>
    /// If any of these tags are found on the colliding object the events will fire
    /// </summary>
    [SerializeField] private string[] _triggerTags;

    /// <summary>
    /// These events will fire when entering the trigger
    /// </summary>
    [SerializeField] private UnityEvent TriggerEvents = new UnityEvent();

    /// <summary>
    /// Collision logic for 2D triggers
    /// </summary>
    void OnTriggerEnter2D (Collider2D p_other)
    {
        bool p_validTag = false;

        for (int i = 0; i < _triggerTags.Length; i++)
        {
            if (p_other.CompareTag(_triggerTags[i]))
            {
                p_validTag = true;
                break;
            }
        }

        if (p_validTag)
        {
            TriggerEvents.Invoke();
        }
    }

    /// <summary>
    /// Collision logic for 3D triggers
    /// </summary>
    void OnTriggerEnter (Collider p_other)
    {
        bool p_validTag = false;

        for (int i = 0; i < _triggerTags.Length; i++)
        {
            if (p_other.CompareTag(_triggerTags[i]))
            {
                p_validTag = true;
                break;
            }
        }

        if (p_validTag)
        {
            TriggerEvents.Invoke();
        }
    }
}
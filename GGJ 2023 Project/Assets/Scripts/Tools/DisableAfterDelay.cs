using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterDelay : MonoBehaviour
{
    [SerializeField] private float _delay;

    private float _currentDelay;

    void OnEnable ()
    {
        _currentDelay = _delay;
    }

    // Update is called once per frame
    void Update()
    {
        _currentDelay -= Time.deltaTime;

        if (_currentDelay <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
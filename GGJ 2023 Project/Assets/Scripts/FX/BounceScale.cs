using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceScale : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve bounceCurve;

    [SerializeField]
    private Vector3 baseScale = Vector3.one;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newScale = baseScale;
        newScale *= bounceCurve.Evaluate(Time.time);
        transform.localScale = newScale;
    }
}

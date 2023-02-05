using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenToTarget : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve moveCurve;

    public IEnumerator TweenPosition(Vector3 source, Vector3 destination, float tweenTime, Action onEnd)
    {
        float startTime = Time.time;
        while (Time.time - startTime < tweenTime)
        {
            float t = moveCurve.Evaluate((Time.time - startTime) / tweenTime);
            transform.localPosition = Vector3.Lerp(source, destination, t);
            yield return null;
        }

        transform.localPosition = destination;
        onEnd();
    }
}

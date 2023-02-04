using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainEmission : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var particlesystem = GetComponent<ParticleSystem>();
        var emission = particlesystem.emission;
        emission.rate = 55.0f;
    }

    // Update is called once per frame
    void Update()
    {
     
    }
}

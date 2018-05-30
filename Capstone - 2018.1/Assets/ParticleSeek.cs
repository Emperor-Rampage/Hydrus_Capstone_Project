using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSeek : MonoBehaviour {

    public Transform attractor;
    public float force = 10.0f;

    ParticleSystem ps;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update ()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
	}
}

﻿using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSeek : MonoBehaviour
{
    public Transform target;
    public float force = 10.0f;
    private float timer = 0.0f;
    public AnimationCurve forceCurve;

    new ParticleSystem particleSystem;
    ParticleSystem.Particle[] particles;

    ParticleSystem.MainModule particleSystemMainModule;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        particleSystem = GetComponent<ParticleSystem>();
        particleSystemMainModule = particleSystem.main;   
    }

    void Update()
    {
        timer += Time.deltaTime;
       // if (timer >= 1.0f)
       //     timer += -1.0f;
    }

    void LateUpdate()
    {
        int maxParticles = particleSystemMainModule.maxParticles;

        if (particles == null || particles.Length < maxParticles)
        {
            particles = new ParticleSystem.Particle[maxParticles];
        }

        force = forceCurve.Evaluate(timer);

        particleSystem.GetParticles(particles);
        float forceDeltaTime = force * Time.deltaTime;

        Vector3 targetTransformedPosition;

        switch (particleSystemMainModule.simulationSpace)
        {
            case ParticleSystemSimulationSpace.Local:
                {
                    targetTransformedPosition = transform.InverseTransformPoint(target.position);
                    break;
                }
            case ParticleSystemSimulationSpace.Custom:
                {
                    targetTransformedPosition = particleSystemMainModule.customSimulationSpace.InverseTransformPoint(target.position);
                    break;
                }
            case ParticleSystemSimulationSpace.World:
                {
                    targetTransformedPosition = target.position;
                    break;
                }
            default:
                {
                    throw new System.NotSupportedException(

                        string.Format("Unsupported simulation space '{0}'.",
                        System.Enum.GetName(typeof(ParticleSystemSimulationSpace), particleSystemMainModule.simulationSpace)));
                }
        }

        int particleCount = particleSystem.particleCount;

        for (int i = 0; i < particleCount; i++)
        {
            Vector3 directionToTarget = Vector3.Normalize(targetTransformedPosition - particles[i].position);
            Vector3 seekForce = directionToTarget * forceDeltaTime;

            particles[i].velocity += seekForce;
        }

        particleSystem.SetParticles(particles, particleCount);
    }
}

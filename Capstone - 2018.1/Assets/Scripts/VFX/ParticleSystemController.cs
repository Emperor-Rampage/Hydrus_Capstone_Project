using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

using EntityClasses;
using AbilityClasses;
using MapClasses;

namespace ParticleClasses
{
    //TODO: Build this out to handle the particle effect creation and destruction for the whole game. Telling things where to spawn, how long, and handling the garbage of all of this
    // -- General hitspark method to display a hit spark when an enemy is hit.
    // -- Play player effects depending on a transform given.
    // -- Play enemy particle effects depending on more complex info given (Transform, cast time, activation, etc)
    // -- Handle Status effect particles to help the player know when something is affecting the target.
    // -- Expose Enemy prefab components somewhere(probably in the Entity script) to make grabbing the pieces a lot faster and less taxing.
    // -- Set the Level to properly track the player so I can grab it for effects and such. Gone on for FAR too long.

    public class ParticleSystemController : MonoBehaviour
    {
        [SerializeField]
        public ParticleSystem hitSpark;
        [SerializeField]
        public ParticleSystem coreEffect;
        [SerializeField]
        public AnimationCurve hurtCurve;

        private LevelManager level;

        [SerializeField]
        public float colorDecayTime = 0.5f;

        //Used to display the hitspark effect
        //TODO: Get a point slightly closer to the player and make the effect happen there. Should help convey that the target was hit.
        public void PlayHitSpark(Entity hurtTarget)
        {
            //Debug.Log("Generating HitSpark for " + hurtTarget.Name + " at Cell: " + hurtTarget.Cell.X + "," + hurtTarget.Cell.Z);
            Vector3 sparkVec = new Vector3(hurtTarget.Instance.transform.position.x, 0.5f, hurtTarget.Instance.transform.position.z);

            Instantiate(hitSpark, sparkVec, GameObject.FindGameObjectWithTag("Player").transform.localRotation);
        }

        //Instantiates a Core Effect Particle System when prompted.
        public void PlayCoreGather(Entity spawnTarget)
        {
            if (spawnTarget == null || spawnTarget.Instance == null)
                return;

            //Debug.Log("Generating Core Gather Effect for " + spawnTarget.Name + "at Cell " + spawnTarget.Cell.X + "," + spawnTarget.Cell.Z + " with " + spawnTarget.Cores + " Cores. Spawning " + (spawnTarget.Cores / 5) + " particles." );
            Vector3 spawnVec = new Vector3(spawnTarget.Instance.transform.position.x, 0.5f, spawnTarget.Instance.transform.position.z);

            coreEffect.emission.SetBurst(0,
                new ParticleSystem.Burst(0.0f, (spawnTarget.Cores / 5))
                );

            Instantiate(coreEffect, spawnVec, GameObject.FindGameObjectWithTag("Player").transform.localRotation);

        }

        public void PlayPlayerAnimation(float castTime, float delay, float timing, Animator anim, string trigger)
        {
            Tween.Value(0, 1, (i) => {}, (castTime - timing), delay, completeCallback: () => anim.SetTrigger(trigger));
        }

        //Plays an enemy animation with a delay that makes the ability sync-up
        public void PlayEnemyAnimation(AbilityObject abil, float delay, float castPercent, Animator anim)
        {

        }

        public void PlayPlayerVFX(AbilityObject abil)
        {
            Debug.Log("Playing " + abil.Name + " at Origin Point " + abil.ParticleOrigin);
        }

        public void PlayEnemyVFX(AbilityObject abil, Entity caster)
        {
            Debug.Log("Playing " + abil.Name + " at Origin Point " + abil.ParticleOrigin + " for enemy " + caster.Name);
        }

        //Causes the enemy to flash the HitColor specified in the material for the target.
        public void HitColor(Entity hurtTarget)
        {
            if (hurtTarget == null)
                return;

            Material mat;
            if (hurtTarget.Name == "Flower Spider")
                mat = hurtTarget.Instance.GetComponentInChildren<SkinnedMeshRenderer>().material;
            else
                mat = hurtTarget.Instance.GetComponentInChildren<MeshRenderer>().material;

            if (mat == null)
                return;

            mat.SetFloat("_HurtScale", 1.0f);
            Tween.ShaderFloat(mat, "_HurtScale", 0.0f, 0.5f, 0.0f);
        }

        //Needed to set the layer weight using a tween... God, I hope nobody sees this.
        //private Animator CurrentHurtAnimator;

        public void PlayHurtAnim(Entity hurtTarget)
        {
            Animator anim = hurtTarget.Instance.GetComponent<Animator>();
            if (anim == null)
                return;

            anim.SetTrigger("Hurt");
            //Tween.Value(1.0f, 0.0f,HandleHurtAnimChange, 0.5f,0.0f, hurtCurve);
        }

        /*
        void HandleHurtAnimChange(float value)
        {
            CurrentHurtAnimator.SetLayerWeight(1, value);
        }
        */

        public void DissolveEnemy(Entity target, Level level)
        {
            if (target == null || target.Index == null || level == null)
                return;

            Material mat;
            if (target.Name == "Flower Spider")
                mat = target.Instance.GetComponentInChildren<SkinnedMeshRenderer>().material;
            else
                mat = target.Instance.GetComponentInChildren<MeshRenderer>().material;

            if (mat == null)
                return;

            Tween.ShaderFloat(mat, "_DissolveScale", 1.0f, 1.0f, 0.0f, completeCallback: () => GameObject.Destroy(target.Instance));
        }

        //Used to apply a visual effect to a target during an ability.
        public void MaterialEffect(Entity target)
        {

        }

        //Blanket method that is called to display an effect for each ability effect currently affecting the player.
        //Will hookup to effect dictionary of the entity and play the particles/change the material to display that things are happening.
        public void StatusEffectVFX(Entity target)
        {

        }
    }
}

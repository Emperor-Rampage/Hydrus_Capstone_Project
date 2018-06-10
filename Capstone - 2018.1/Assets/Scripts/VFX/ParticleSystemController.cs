using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pixelplacement;
using Pixelplacement.TweenSystem;

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

        private GameManager gameManager;
        private Player player;

        [SerializeField]
        public ParticleSystem hitSpark;

        [SerializeField]
        public ParticleSystem coreEffect;

        [SerializeField]
        public AnimationCurve hurtCurve;
        /*
        //Setup for possible status effect activation.
        [Header("Status Effect Indicators")]

        [SerializeField]
        public ParticleSystem Bleed;

        [SerializeField]
        public ParticleSystem MoveSlow;

        [SerializeField]
        public ParticleSystem Stun;

        [SerializeField]
        public ParticleSystem Silence;

        [SerializeField]
        public ParticleSystem Root;

        [SerializeField]
        public ParticleSystem Nani;

        [SerializeField]
        */

        [SerializeField]
        public float colorDecayTime = 0.5f;

        private void Start()
        {
            gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("Scene Loaded");

            player = gameManager.LevelManager.CurrentLevel.Player;
            player.Anim = player.Instance.GetComponent<Animator>();

            if (player == null)
                Debug.LogError("Player Not Found On Scene Load");
            else
                Debug.Log("Scene Loaded and found " + player.Name);

            if (player.Anim != null)
                Debug.Log(player.Name + " has an animator");
            else
                Debug.LogError("Cannot Find Animator on " + player.Name);
        }

        //Used to display the hitspark effect
        //TODO: Get a point slightly closer to the player and make the effect happen there. Should help convey that the target was hit.
        public void PlayHitSpark(Entity hurtTarget)
        {
            //Debug.Log("Generating HitSpark for " + hurtTarget.Name + " at Cell: " + hurtTarget.Cell.X + "," + hurtTarget.Cell.Z);
            Vector3 sparkVec = new Vector3(hurtTarget.Instance.transform.position.x, 0.5f, hurtTarget.Instance.transform.position.z);

            Instantiate(hitSpark, sparkVec, player.Instance.transform.localRotation);
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

            Instantiate(coreEffect, spawnVec, player.Instance.transform.localRotation);

        }

        public void SetPlayerCastAnimation(float CTScale, AbilityObject abil, string trigger)
        {
            Animator playerAnim = player.Anim;

            if (playerAnim == null)
                return;
            
            for(int i = 0; i < playerAnim.parameterCount - 1; i++)
            {
                //if(playerAnim.parameters[i] == )
                    playerAnim.ResetTrigger(i);
            }


            playerAnim.SetFloat("CastTimeScale", CTScale);
            playerAnim.SetTrigger(trigger);
            PlaySyncedPlayerAnimation(player.GetAdjustedCastTime(abil.CastTime), abil.AnimDelay, abil.AnimTiming, player.Anim, "CastActivate");
        }

        public void PlayerMove(Direction direction, float adjustedMovespeed)
        {
            
            if (player.Facing == direction)
            {
                player.Anim.SetFloat("MoveSpeedScale", adjustedMovespeed);
                player.Anim.SetTrigger("MoveForward");
            }
            else if (player.GetRight() == direction)
            {
                player.Anim.SetFloat("MoveSpeedScale", adjustedMovespeed);
                player.Anim.SetTrigger("MoveRight");
            }
            else if (player.GetLeft() == direction)
            {
                player.Anim.SetFloat("MoveSpeedScale", adjustedMovespeed);
                player.Anim.SetTrigger("MoveLeft");
            }
            else if (player.GetBackward() == direction)
            {
                player.Anim.SetFloat("MoveSpeedScale", adjustedMovespeed);
                player.Anim.SetTrigger("MoveBack");
            }
        }

        public void EnemyMove(Entity enemy, float adjustedMovespeed)
        {
            if (enemy.Anim == null)
                return;
            enemy.Anim.SetFloat("MoveSpeedScale", adjustedMovespeed);
            enemy.Anim.ResetTrigger("Walk");
            enemy.Anim.SetTrigger("Walk");
        }

        public void InterruptEnemy(Entity enemy)
        {
            enemy.Anim.SetTrigger("Interrupt");
        }

        public void EnemyTurn(Entity enemy, bool direction)
        {
            enemy.Anim = enemy.Instance.GetComponent<Animator>();

            if (enemy.Anim == null)
                return;

            enemy.Anim.ResetTrigger("TurnR");
            enemy.Anim.ResetTrigger("TurnL");

            if (direction)
                enemy.Anim.SetTrigger("TurnR");
            else
                enemy.Anim.SetTrigger("TurnL");
        }

        public void PlayerInterrupt()
        {
            player.Anim.SetTrigger("Interrupt");
        }

        //Starts a tween that activates the Animation Trigger at the proper time to sync animations.
        public void PlaySyncedPlayerAnimation(float castTime, float delay, float timing, Animator anim, string trigger)
        {
            Tween.Value(0, 1, (i) => { }, (castTime - timing), delay, completeCallback: () => anim.SetTrigger(trigger));
        }

        //Plays an enemy animation with a delay that makes the ability sync-up
        public void PlaySyncedEnemyAnimation(float castTime, float delay, float castPercent, Animator anim, string trigger)
        {
            Tween.Value(0, 1, (i) => { }, castTime, delay, completeCallback: () => anim.SetTrigger(trigger));
        }

        public void PlayPlayerVFX(AbilityObject abil)
        {
            Debug.Log("Attmpting to play particle effect...");
            if (abil.ParticleOrigin == null || abil.ParticleSystem == null)
            {
                Debug.LogError("Attempt Failed!");
                return;
            }


            Debug.Log("Playing " + abil.Name + " at Origin Point " + abil.ParticleOrigin.name);
            Instantiate(abil.ParticleSystem, abil.ParticleOrigin);
        }

        public void PlayEnemyVFX(AbilityObject abil, Entity caster)
        {
            if (abil.ParticleOrigin == null || abil.ParticleSystem == null || caster == null)
                return;

            Debug.Log("Playing " + abil.Name + " at Origin Point " + abil.ParticleOrigin + " for enemy " + caster.Name);
        }

        //Causes the enemy to flash the HitColor specified in the material for the target.
        public void HitColor(Entity hurtTarget)
        {
            if (hurtTarget == null)
                return;
            hurtTarget.Rend = hurtTarget.Instance.GetComponentInChildren<SkinnedMeshRenderer>();

            //Material mat = hurtTarget.Instance.GetComponentInChildren<SkinnedMeshRenderer>().material;
            Material mat = hurtTarget.Rend.material;

            if (mat == null)
                return;

            mat.SetFloat("_HurtScale", 1.0f);
            Tween.ShaderFloat(mat, "_HurtScale", 0.0f, 0.5f, 0.0f);
        }

        //Changed to use the animator of the the Entity, which is now found on instantiation.
        public void PlayHurtAnim(Entity hurtTarget)
        {
            Animator anim = hurtTarget.Anim;
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
            if (target == null || level == null)
                return;
          
            Material mat = target.Instance.GetComponentInChildren<SkinnedMeshRenderer>().material;
           
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

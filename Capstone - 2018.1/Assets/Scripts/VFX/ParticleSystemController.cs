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
    // -- (DONE) General hitspark method to display a hit spark when an enemy is hit.
    // -- Play player effects depending on a transform given.
    // -- Play enemy particle effects depending on more complex info given (Transform, cast time, activation, etc)
    // -- Handle Status effect particles to help the player know when something is affecting the target.
    // -- (DONE) Expose Enemy prefab components somewhere(probably in the Entity script) to make grabbing the pieces a lot faster and less taxing.
    // -- (DONE) Set the Level to properly track the player so I can grab it for effects and such. Gone on for FAR too long.

    public class ParticleSystemController : MonoBehaviour
    {

        private GameManager gameManager;
        private Player player;
        private List<TweenBase> tweens;

        [SerializeField]
        public ParticleSystem hitSpark;

        [SerializeField]
        public ParticleSystem coreEffect;

        [SerializeField]
        public AnimationCurve hurtCurve;

        [SerializeField]
        public float colorDecayTime = 0.5f;

        [Space(10)]
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
        public ParticleSystem Haste;

        [SerializeField]
        public ParticleSystem Heal;


        private void Start()
        {
            gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (gameManager == null || gameManager.LevelManager == null || gameManager.LevelManager.CurrentLevel == null || gameManager.LevelManager.CurrentLevel.Player == null)
                return;

            //Debug.Log("Scene Loaded");

            player = gameManager.LevelManager.CurrentLevel.Player;
            // player.Anim = player.Instance.GetComponent<Animator>();
            /*
            if (player == null)
                Debug.LogError("Player Not Found On Scene Load");
            else
                Debug.Log("Scene Loaded and found " + player.Name);

            if (player.Animator != null)
                Debug.Log(player.Name + " has an animator");
            else
                Debug.LogError("Cannot Find Animator on " + player.Name);
            */
        }

        //Used to display the hitspark effect
        //TODO: Get a point slightly closer to the player and make the effect happen there. Should help convey that the target was hit.
        public void PlayHitSpark(Entity hurtTarget)
        {
            //Debug.Log("Generating HitSpark for " + hurtTarget.Name + " at Cell: " + hurtTarget.Cell.X + "," + hurtTarget.Cell.Z);
            Transform targetTransform = hurtTarget.Instance.transform;
            Vector3 sparkVec = new Vector3(targetTransform.position.x, 0.5f, targetTransform.position.z);

            Instantiate(hitSpark, sparkVec, player.Instance.transform.localRotation);
        }

        //Instantiates a Core Effect Particle System when prompted.
        public void PlayCoreGather(Entity spawnTarget)
        {
            if (spawnTarget == null || spawnTarget.Instance == null)
                return;

            Transform targetTransform = spawnTarget.Instance.transform;
            //Debug.Log("Generating Core Gather Effect for " + spawnTarget.Name + "at Cell " + spawnTarget.Cell.X + "," + spawnTarget.Cell.Z + " with " + spawnTarget.Cores + " Cores. Spawning " + (spawnTarget.Cores / 5) + " particles." );
            Vector3 spawnVec = new Vector3(targetTransform.position.x, 0.5f, targetTransform.position.z);

            coreEffect.emission.SetBurst(0,
                new ParticleSystem.Burst(0.0f, (spawnTarget.Cores / 5))
                );

            Instantiate(coreEffect, spawnVec, player.Instance.transform.localRotation);

        }

        public void SetPlayerCastAnimation(float CTScale, AbilityObject abil, string trigger)
        {
            Animator playerAnim = player.Animator;
            if (playerAnim == null || trigger == "DefaultTrigger")
                return;

            playerAnim.ResetTrigger(trigger);
            playerAnim.ResetTrigger("CastActivate");
            playerAnim.SetFloat("CastTimeScale", CTScale);
            playerAnim.SetTrigger(trigger);
            PlaySyncedPlayerAnimation(player.GetAdjustedCastTime(abil.CastTime), abil.AnimDelay, abil.AnimTiming, playerAnim, "CastActivate");
        }

        public void PlayerMove(Direction direction, float adjustedMovespeed)
        {
            Animator playerAnim = player.Animator;
            playerAnim.SetFloat("MoveSpeedScale", adjustedMovespeed);

            if (player.Facing == direction)
            {
                // player.Anim.SetFloat("MoveSpeedScale", adjustedMovespeed);
                playerAnim.SetTrigger("MoveForward");
            }
            else if (player.GetRight() == direction)
            {
                // player.Anim.SetFloat("MoveSpeedScale", adjustedMovespeed);
                playerAnim.SetTrigger("MoveRight");
            }
            else if (player.GetLeft() == direction)
            {
                // player.Anim.SetFloat("MoveSpeedScale", adjustedMovespeed);
                playerAnim.SetTrigger("MoveLeft");
            }
            else if (player.GetBackward() == direction)
            {
                // player.Anim.SetFloat("MoveSpeedScale", adjustedMovespeed);
                playerAnim.SetTrigger("MoveBack");
            }
        }

        public void EnemyMove(Entity enemy, float adjustedMovespeed)
        {
            if (enemy.Animator == null)
                return;
            enemy.Animator.SetFloat("MoveSpeedScale", adjustedMovespeed);
            enemy.Animator.ResetTrigger("Walk");
            enemy.Animator.SetTrigger("Walk");
        }

        public void InterruptEnemy(Entity enemy)
        {
            if (enemy.Animator == null)
                return;
            enemy.Animator.SetTrigger("Interrupt");
            enemy.Animator.SetBool("Interrupted", true);
        }

        public void EnemyTurn(Entity enemy, bool direction)
        {
            Animator enemyAnim = enemy.Animator;
            if (enemyAnim == null)
                return;

            enemyAnim.ResetTrigger("TurnR");
            enemyAnim.ResetTrigger("TurnL");

            if (direction == true)
                enemyAnim.SetTrigger("TurnR");
            else
                enemyAnim.SetTrigger("TurnL");
        }

        public void PlayerInterrupt()
        {
            player.Animator.SetTrigger("Interrupt");
            player.Animator.SetBool("Interrupted", true);
        }

        //Starts a tween that activates the Animation Trigger at the proper time to sync animations.
        public void PlaySyncedPlayerAnimation(float castTime, float delay, float timing, Animator anim, string trigger)
        {
            Tween.Value(0, 1, (i) => { }, (castTime - timing), delay, completeCallback: () => CompleteSynchedAnimation(anim, trigger));
        }

        //Plays an enemy animation with a delay that makes the ability sync-up
        public void PlaySyncedEnemyAnimation(float castTime, float delay, float castPercent, Animator anim, string trigger)
        {
            Tween.Value(0, 1, (i) => { }, castTime, delay, completeCallback: () => CompleteSynchedAnimation(anim, trigger));
        }

        public void CompleteSynchedAnimation(Animator anim, string trigger)
        {
            if (anim.GetBool("Interrupted") == true)
            {
                anim.SetBool("Interrupted", false);
                return;
            }
            else
            {
                anim.SetTrigger(trigger);
            }
        }


        public void PlaySynchedVFX(float castTime, float timing, AbilityObject abil, Entity caster)
        {
            Debug.Log("Starting Synched VFX tween on " + caster.Name + " with ability " + abil.Name + " with timing " + timing + " and a cast time of " + castTime);
            Tween.Value(0, 1, (i) => { }, (castTime - timing), 0, completeCallback: () => PlayVFX(abil, caster));
        }


        //Callback method for the PlaySynchedVFX that actually handles the instantiation of the effect.
        public void PlayVFX(AbilityObject abil, Entity caster)
        {
            Debug.Log("Attempting to play VFX");
            if (abil.ParticleSystem == null || caster == null || caster.Animator.GetBool("Interrupted") == true)
            {
                Debug.LogError("Failed on Caster: " + caster.Name + " ability " + abil.Name + ". Particle is null?:" + abil.ParticleSystem + " Interrupted?: " + caster.Animator.GetBool("Interrupted"));
                return;
            }
            Debug.Log("Sucess");
            //Debug.Log("Playing " + abil.Name + " at Origin Point " + abil.ParticleOrigin + " for enemy " + caster.Name);
            Instantiate(abil.ParticleSystem, caster.Instance.transform);
        }

        //Same as the PlayVFX method, but for AOE abilities.
        public void PlayAOEVFX(AbilityObject abil, Cell cell, Entity caster)
        {
            if (abil == null || cell == null || caster.Animator.GetBool("Interrupted") == true || caster.Cell == cell)
                return;
            Instantiate(abil.ParticleSystem, gameManager.LevelManager.GetCellPosition(cell), caster.Instance.transform.localRotation, caster.Instance.transform);
        }

        //Causes the enemy to flash the HitColor specified in the material for the target.
        public void HitColor(Entity hurtTarget)
        {
            if (hurtTarget == null)
                return;

            Material mat = hurtTarget.Renderer.material;

            if (mat == null)
                return;

            mat.SetFloat("_HurtScale", 1.0f);
            Tween.ShaderFloat(mat, "_HurtScale", 0.0f, 0.5f, 0.0f);
        }

        //Changed to use the animator of the the Entity, which is now found on instantiation.
        public void PlayHurtAnim(Entity hurtTarget)
        {
            Animator anim = hurtTarget.Animator;
            if (anim == null)
                return;

            anim.SetTrigger("Hurt");
            //Tween.Value(1.0f, 0.0f,HandleHurtAnimChange, 0.5f,0.0f, hurtCurve);
        }

        public void DissolveEnemy(Entity target, Level level)
        {
            if (target == null || level == null)
                return;

            // Material mat = target.Instance.GetComponentInChildren<SkinnedMeshRenderer>().material;
            Material mat = target.Renderer.material;

            if (mat == null)
                return;

            Tween.ShaderFloat(mat, "_DissolveScale", 1.0f, 1.0f, 0.0f, completeCallback: () => GameObject.Destroy(target.Instance.gameObject));
        }

        //Used to apply a visual effect to a target during an ability.
        public void MaterialEffect(Entity target)
        {

        }

        //Blanket method that is called to display an effect for each ability effect currently affecting the player.
        //Will hookup to effect dictionary of the entity and play the particles/change the material to display that things are happening.
        public void ToggleStatusEffectVFX(Entity target, string effect)
        {

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParticleClasses;
using EntityClasses;
using AbilityClasses;
using AudioClasses;

namespace AnimationEvents
{
    public class PlaySound : StateMachineBehaviour
    {
        private GameManager gameManager;
        private ParticleSystemController particleManager;
        private AudioManager audioManager;

        public void OnEnable()
        {
            gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
            particleManager = gameManager.ParticleManager;
            audioManager = gameManager.AudioManager;
        }

        public void PlayAbilitySound(AbilityObject abil)
        {
            audioManager.PlaySoundEffect(abil.SoundEffect);
        }

        public void PlayParticleEffect()
        {
            particleManager.PlayPlayerVFX(gameManager.CurrentLevel.Player.Abilities[0]);
        }
        // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
        //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateExit is called before OnStateExit is called on any state inside this state machine
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateMove is called before OnStateMove is called on any state inside this state machine
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateIK is called before OnStateIK is called on any state inside this state machine
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateMachineEnter is called when entering a statemachine via its Entry Node
        //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
        //
        //}

        // OnStateMachineExit is called when exiting a statemachine via its Exit Node
        //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
        //
        //}
    }
}

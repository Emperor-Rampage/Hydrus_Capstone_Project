using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public class ParticleSystemController : MonoBehaviour
    {
        [SerializeField]
        public ParticleSystem hitSpark;
        [SerializeField]
        public Material hurtMat;

        private LevelManager level;

        [SerializeField]
        public float colorDecayTime = 0.5f;

        //Used to display the hitspark effect
        //TODO: Get a point slightly closer to the player and make the effect happen there. Should help convey that the target was hit.
        public void PlayHitSpark(Entity hurtTarget)
        {
            Debug.Log("Generating HitSpark for " + hurtTarget.Name + " at Cell: " + hurtTarget.Cell.X + "," + hurtTarget.Cell.Z);
            Vector3 sparkVec = new Vector3(hurtTarget.Instance.transform.position.x, 0.5f, hurtTarget.Instance.transform.position.z);
            
            Instantiate(hitSpark, sparkVec, GameObject.FindGameObjectWithTag("Player").transform.localRotation);
        }

        public void PlayPlayerVFX(AbilityObject abil)
        {
            Debug.Log("Playing " + abil.Name + " at Origin Point " + abil.ParticleOrigin);
        }

        public void PlayEnemyVFX(AbilityObject abil, Entity caster)
        {

        }

        //Going to change this to reference a shader function. Possibly a coroutine (may be overkill)
        public void HitColor(Entity hurtTarget)
        {
           
        }

        //Used to play the dissolve effect for enemy death. Shader effect.
        public void DestroyEnemy(Entity target)
        {

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

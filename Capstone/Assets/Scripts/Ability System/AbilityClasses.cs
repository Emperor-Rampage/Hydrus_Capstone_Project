using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilityClasses
{
    public enum AbilityType
    {
        None = -1,
        Self = 0,
        Melee = 1,
        Ranged = 2,
        AreaOfEffect = 3
    }
    public enum AbilityStatusEff
    {
        NoEffect = -1,
        CastTimeSlow = 0,
        CooldownSlow = 1,
        Stun = 2,
        MoveSlow = 3,
        Root = 4,
        Silence = 5,
        Heal = 6,
        Haste = 7,
        DamReduct = 8,
        DoT = 9
    }

    [System.Serializable]
    public class AbilityEffect                                                                                      //Structure for containing ability effects
    {
        public AbilityStatusEff effect;
        public float duration;
        public float value;   
    }

    [System.Serializable]
    public class EffectDictionary
    {
        public float cooldownScale = 1.0f;
        public float movementScale = 1.0f;
        public float castTimeScale = 1.0f;
        public float damageScale = 1.0f;
        public float healRate = 0.0f;
        public bool stunned = false;
        public bool rooted = false;
        public bool silenced = false;

        public Dictionary<AbilityStatusEff, List<AbilityEffect>> EffectLibrary; 
        public List<AbilityEffect> CurrentEffects; //Empty list for instantiation purposes

        public EffectDictionary() { }

        public void AddEffect(AbilityEffect AbilEffect)
        {
            if (EffectLibrary.ContainsKey(AbilEffect.effect))
            {
                List<AbilityEffect> effects = EffectLibrary[AbilEffect.effect]; //Indexer
                effects.Add(AbilEffect);
            }
            else
            {
                EffectLibrary.Add(AbilEffect.effect, CurrentEffects);
                EffectLibrary[AbilEffect.effect][0] = AbilEffect;
            }
        }

        public void ApplyEffects() //Method to apply all effect modifiers to the local values.
        {

        }

        public void RemoveEffect() //Method to remove an effect from the EffectLibrary
        {

        }
    }

    [System.Serializable]
    public abstract class AbilityObject : ScriptableObject
    {
        //TODO: Continue researching the scriptable objects ability system, and how to make it work for us.
        //This is the basic template for all abilites in the game. This container holds all the info we need to make the abilites actually activate.

        //Basic Ability Information
        public string abilityName = "Default Ability Name";                                                         //Ability Name
        public Sprite abilIcon;                                                                                     //Sprite Icon denoting the Ability
        [TextArea]
        public string toolTip = ": This is the Default Ability tooltip. Please change me.";                         //Tooltip that explaines what the ability does.
        public ParticleSystem abilityParticleSys;                                                                   //Container for the Particle Effect System of the Ability.
        public AudioClip soundEff;                                                                                  //Sound Effect to play on activation.
        public List<AbilityEffect> statusEffects;                                                                   //List of status effects, if any. Denoted by an Enumerable ID value that informs the ability what to affect the target entity with.
        public AbilityType type;                                                                                    //Denotes what kind of ability is being used, and who it affects. Uses an Enumerable ID value to inform the ability script.
        public Texture2D aoeSprite;                                                                               //Tilemap that denotes the effective range of the ability. Not used in Melee, or Self abilites.
        public float range;                                                                                         //For ranged abilities. Indicates the range of the ability.
        public float baseCooldown = 0.0f;                                                                           //The base Cooldown timer (in seconds) of the ability.
        public float baseCastTime = 0.0f;                                                                           //The base Cast Timer (in seconds) it takes for the ability to activate.
        public int initalDamage = 0;                                                                                //Initial damage value of the ability.
        public bool upgradable;                                                                                     //Boolean that denotes if the ability can be upgraded or not.

        //Upgrade System Info.
        public int abilityTier;                                                                                    //Integer used to denote the level of the ability in the upgrade tree.
        public AbilityObject previousTier;                                                                        //Reference the the previous ability level of the tree, if any. Used for validation.
        public List<AbilityObject> nextTier;                                                                      //Reference to the next ability level of the tree, if any. Used for validation.
        public float upgradeCost;                                                                                   //The Core cost to upgrade the ability, if any.

    }
}
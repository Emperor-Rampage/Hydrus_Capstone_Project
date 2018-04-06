﻿using System.Collections;
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
    public class AbilityEffect
    {
        public AbilityStatusEff effect;
        public float duration;
        public float value;
    }

    [System.Serializable]
    public abstract class AbilityObjBase : ScriptableObject
    {
        //To-Do: Continue researching the scriptable objects ability system, and how to make it work for us.
        //This is the basic template for all abilites in the game. This container holds all the info we need to make the abilites actually activate.

        //Basic Ability Information
        public static string abilityName = "Default Ability Name";                                                  //Ability Name
        public Sprite abilIcon;                                                                                     //Sprite Icon denoting the Ability
        [TextArea]
        public string toolTip = abilityName + ": This is the Default Ability tooltip. Please change me.";           //Tooltip that explaines what the ability does.
        public AudioClip soundEff;                                                                                  //Sound Effect to play on activation.
        public List<AbilityEffect> statusEffects;                                                                   //List of status effects, if any. Denoted by an Enumerable ID value that informs the ability what to affect the target entity with.
        public AbilityType type;                                                                                    //Denotes what kind of ability is being used, and who it affects. Uses an Enumerable ID value to inform the ability script.
        public Texture2D rangeSprite;                                                                               //Tilemap that denotes the effective range of the ability. Not used in Melee, or Self abilites.
        public float baseCooldown = 0.0f;                                                                           //The base Cooldown timer (in seconds) of the ability.
        public float baseCastTime = 0.0f;                                                                           //The base Cast Timer (in seconds) it takes for the ability to activate.
        public int initalDamage = 0;                                                                                //Initial damage value of the ability.
        public bool upgradable;                                                                                     //Boolean that denotes if the ability can be upgraded or not.

        //Upgrade System Info.
        public int abilityTier;                                                                                    //Integer used to denote the level of the ability in the upgrade tree.
        public AbilityObjBase previousTier;                                                                        //Reference the the previous ability level of the tree, if any. Used for validation.
        public List<AbilityObjBase> nextTier;                                                                      //Reference to the next ability level of the tree, if any. Used for validation.
        public float upgradeCost;                                                                                   //The Core cost to upgrade the ability, if any.
     
    }
}
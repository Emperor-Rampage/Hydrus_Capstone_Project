using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilityClasses
{
    [System.Serializable]
    public class AbilityObjBase : ScriptableObject
    {
        //Basic Ability Information
        public static string abilityName = "Default Name";
        public Texture2D abilIcon = null;
        public string toolTip = abilityName + ": This is the default tooltip.";
        public AbilityStatusEff status1;
        public AbilityStatusEff status2;
        public AbilityType type;
        public float duration = 0.0f;
        public float cooldown = 0.0f;
        public float castTime = 0.0f;

        //For upgrade system.
        public AbilityObjBase previousLevel;
        public AbilityObjBase nextLevel;
        public float upgradeCost = 0.0f;
    }
}
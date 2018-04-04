using System.Collections;
using UnityEngine;
namespace AbilityClasses
{
    // Plan: Class that informs logic which effect to activate using the enum provided by the AbilityObject
    public enum AbilityType
    {
        None = -1,
        Self = 0,
        Melee = 1,
        Ranged = 2,
        AreaOfEffect = 3,
        OverTime = 4,
        AOEOverTime = 5
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
        Hast = 7,
        DamReduct = 8,
        Rage = 9
    }
    public class AbilityEffect
    {

    }

}
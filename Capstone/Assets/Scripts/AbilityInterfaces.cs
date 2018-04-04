using System.Collections;
using UnityEngine;
public enum AbilityType
{
    Self = 0,
    Melee = 1,
    Ranged = 2,
    AreaOfEffect = 3,
    OverTime = 4,
    AOEOverTime = 5
}
public enum AbilityStatusEff
{
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
public interface IAbility
{

}

public interface IAbilityEffect
{

}
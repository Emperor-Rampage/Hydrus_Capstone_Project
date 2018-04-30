using System.Collections;
using System.Collections.Generic;
using AbilityClasses;
using EntityClasses;
using UnityEngine;

public class AbilityTree : MonoBehaviour
{
    Player player;
    List<List<AbilityObject>> tiers;
    [SerializeField] float numTiers;
    [SerializeField] float uiSpacing;
    [SerializeField] float uiCellWidth;
    [SerializeField] float uiCellHeight;

    public float TotalNumLeafs { get; private set; }

    public AbilityTree(Player player, List<AbilityObject> t1)
    {
        List<AbilityObject> prevTier;
        tiers = new List<List<AbilityObject>>();
        tiers.Add(t1);
        prevTier = t1;
        for (int t = 1; t < numTiers; t++)
        {
            var tier = new List<AbilityObject>();
            foreach (AbilityObject ability in prevTier)
            {
                tier.AddRange(ability.NextTier);
            }
            prevTier = tier;
        }

        TotalNumLeafs = tiers[tiers.Count].Count;
    }

    public bool IsAvailable(AbilityObject ability) {
        foreach (AbilityObject playerAbility in player.Abilities) {
            if (ability.PreviousTier.Contains(playerAbility))
                return true;
        }

        return false;
    }

    public bool CanUpgrade(AbilityObject ability) {
        if (IsAvailable(ability) && player.Cores >= ability.Cost)
            return true;

        return false;
    }

    public int GetAbilityLeafs(int tier1Index) {
        int numLeafs = 0;
        if (tier1Index < 0 || tier1Index >= tiers.Count) {
            var tier1 = tiers[0];
            var lastTier = tiers[tiers.Count];
            if (tier1 != null && lastTier != null) {
                var abilityTier1 = tier1[tier1Index];
                if (abilityTier1 != null) {
                    foreach (var abilityLastTier in lastTier) {
                        if (abilityLastTier.BaseAbility == abilityTier1) {
                            numLeafs++;
                        }
                    }
                }
            }
        }
        return numLeafs;
    }

    public bool IsOffsetTier(int tier)
    {
        return (tier % 2 == 0);
    }
}

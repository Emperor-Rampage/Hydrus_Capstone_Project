using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AbilityClasses;
using EntityClasses;
using UnityEngine;

[System.Serializable]
public class AbilityTree
{
    Player player;
    public Player Player { get { return player; } }
    List<List<AbilityObject>> tiers;
    [SerializeField] int numTiers;
    public int NumTiers { get { return numTiers; } }
    [SerializeField] int uiPadding;
    public int Padding { get { return uiPadding; } }
    [SerializeField] int uiSpacing;
    public int Spacing { get { return uiSpacing; } }
    [SerializeField] int uiCellWidth;
    public int CellWidth { get { return uiCellWidth; } }
    [SerializeField] int uiCellHeight;
    public int CellHeight { get { return uiCellHeight; } }

    public int Width { get { return (uiCellWidth * TotalNumLeafs) + ((uiSpacing - 1) * TotalNumLeafs) + (uiPadding * 2); } }
    public int Height { get { return (uiCellHeight * numTiers) + ((uiSpacing - 1) * numTiers) + (uiPadding * 2); } }
    public int TotalNumLeafs { get; private set; }

    public void Initialize(Player player)
    {
        if (player == null)
            Debug.Log("WARNING: Passed in null player referenece to Initialize in AbilityTree");
        this.player = player;
        List<AbilityObject> t1 = this.player.Class.BaseAbilities;
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
            tiers.Add(tier);
            prevTier = tier;
        }

        TotalNumLeafs = tiers[tiers.Count - 1].Count;
        Debug.Log("Number of total leafs: " + TotalNumLeafs);
    }

    public List<AbilityObject> GetAbilityTier(int tier1Index, int targetTierIndex)
    {
        if (targetTierIndex >= 0 && targetTierIndex < tiers.Count)
        {
            var tier1 = tiers[0];
            var targetTier = tiers[targetTierIndex];
            if (tier1 != null)
            {
                var abilityTier1 = tier1[tier1Index];
                if (abilityTier1 != null)
                {
                    return targetTier.Where((abil) => abil.BaseAbility == abilityTier1).ToList();
                }
            }
        }
        return null;
    }

    public int GetAbilityLeafs(int tier1Index)
    {
        int numLeafs = 0;
        var tier1 = tiers[0];
        var lastTier = tiers[tiers.Count - 1];
        if (tier1 != null && lastTier != null)
        {
            var abilityTier1 = tier1[tier1Index];
            if (abilityTier1 != null)
            {
                foreach (var abilityLastTier in lastTier)
                {
                    if (abilityLastTier.BaseAbility == abilityTier1)
                    {
                        numLeafs++;
                    }
                }
            }
        }
        return numLeafs;
    }

    public int GetNumberOfChildren(int tierIndex, int abilityIndex)
    {
        int numChildren = 0;
        var tier = tiers[tierIndex];
        // var lastTier = tiers[]
        return 0;
    }

    public bool IsAvailable(AbilityObject ability)
    {
        foreach (AbilityObject playerAbility in player.Abilities)
        {
            if (ability.PreviousTier.Contains(playerAbility))
                return true;
        }

        return false;
    }

    public bool CanUpgrade(AbilityObject ability)
    {
        if (IsAvailable(ability) && player.Cores >= ability.Cost)
            return true;

        return false;
    }

    public bool IsOffsetTier(int tier)
    {
        return (tier % 2 == 0);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AbilityClasses;
using EntityClasses;
using UnityEngine;

[System.Serializable]
public class AbilityTree
{
    // TODO: Move ability tree ui stuff to the UIManager
    // TODO: Switch over saving/loading of abilities to using the AbilityTree
    //       instead of a list in the GameManager. Should all be in one place.
    //       Save by tier and index in that tier.
    Player player;
    public Player Player { get { return player; } set { player = value; } }
    List<List<AbilityObject>> tiers;
    // public List<List<GameObject>> UITiers { get; private set; } = new List<List<GameObject>>();
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

    public int Width { get { return (uiCellWidth * TotalNumLeafs) + ((uiSpacing) * TotalNumLeafs) + (uiPadding * 2); } }
    public int Height { get { return (uiCellHeight * numTiers) + ((uiSpacing) * numTiers) + (uiPadding * 2); } }
    public int TotalNumLeafs { get; private set; }

    public void Initialize(PlayerClass playerClass)
    {
        List<AbilityObject> t1 = playerClass.BaseAbilities;

        List<AbilityObject> prevTier;
        tiers = new List<List<AbilityObject>>();
        tiers.Add(t1);
        // UITiers.Add(new List<GameObject>());
        prevTier = t1;
        for (int t = 1; t < numTiers; t++)
        {
            var tier = new HashSet<AbilityObject>();
            // var uiTier = new List<GameObject>();
            foreach (AbilityObject ability in prevTier)
            {
                foreach (AbilityObject nextAbility in ability.NextTier) {
                    tier.Add(nextAbility);
                }
                // tier.AddRange(ability.NextTier);
            }
            var tierList = tier.ToList();
            tiers.Add(tierList);
            // UITiers.Add(uiTier);
            prevTier = tierList;
        }

        TotalNumLeafs = tiers[tiers.Count - 1].Count;
    }

    public bool UpgradeAbility(AbilityObject ability) {
        if (ability != null) {
            if (CanUpgrade(ability)) {
                AbilityObject baseAbility = ability.BaseAbility;
                foreach (AbilityObject playerAbility in Player.Abilities) {
                    if (ability.PreviousTier.Contains(playerAbility)) {
                        int index = Player.Abilities.IndexOf(playerAbility);
                        Player.Abilities[index] = ability;

                        Player.Cores -= ability.Cost;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public AbilityObject GetTreeAbility(int tierIndex, int abilityIndex)
    {
        var tier = tiers[tierIndex];
        if (tier != null)
        {
            AbilityObject ability = tier.FirstOrDefault((abil) => abil.Index == abilityIndex);
            return ability;
            // return tier.SingleOrDefault((abil) => abil.Index == abilityIndex);
        }
        Debug.LogError("ERROR: Could not find ability object");
        return null;
    }

    public int GetTreeAbilityIndex(int tierIndex, AbilityObject ability)
    {
        var tier = tiers[tierIndex];
        if (tier != null)
        {
            return tier.IndexOf(ability);
        }
        Debug.LogError("ERROR: Could not find ability index.");
        return -1;
    }

    // public void AddAbilityUI(int tierIndex, int abilityIndex, GameObject abilityUI)
    // {
    //     // Debug.Log("Adding ability index " + abilityIndex + " for tier " + tierIndex);
    //     if (abilityIndex == -1)
    //     {
    //         return;
    //     }
    //     var tier = UITiers[tierIndex];
    //     tier.Insert(abilityIndex, abilityUI);
    // }

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

    public bool IsCurrentAbility(AbilityObject ability) {
        return player.Abilities.Contains(ability);
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
        return (IsAvailable(ability) && player.Cores >= ability.Cost);
    }

    public bool IsOffsetTier(int tier)
    {
        return (tier % 2 == 0);
    }
}

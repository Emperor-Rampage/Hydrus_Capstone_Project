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

    public float NumLeafs { get; private set; }

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

        NumLeafs = tiers[tiers.Count].Count;
    }

    public bool IsOffsetTier(int tier)
    {
        return (tier % 2 == 0);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilityClasses
{
    [Serializable]
    public class EffectIconsDictionary
    {
        [SerializeField] Sprite defaultIcon;
        public Sprite DefaultIcon { get { return defaultIcon; } }
        [SerializeField] List<AbilityStatusEff> effectsList;// = new List<AbilityStatusEff>();
        [SerializeField] List<Sprite> iconsList;// = new List<Sprite>();
        public Dictionary<AbilityStatusEff, Sprite> IconsDictionary { get; set; }

        // public EffectIconsDictionary()
        // {
        //     Initialize();
        // }

        public void Initialize()
        {
            IconsDictionary = new Dictionary<AbilityStatusEff, Sprite>();
            for (int e = 0; e < effectsList.Count; e++)
            {
                AbilityStatusEff effect = effectsList[e];
                Sprite icon = defaultIcon;
                if (e < iconsList.Count)
                {
                    icon = iconsList[e];
                }
                if (!IconsDictionary.ContainsKey(effect))
                    IconsDictionary.Add(effect, icon);
            }
            Debug.Log("Effect Icons Dictionary Initialized! Dictonary count " + IconsDictionary.Count);
        }
    }
}
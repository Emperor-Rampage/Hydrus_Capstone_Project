﻿using AbilityClasses;
using MapClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityClasses
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Enemy Spawn", menuName = "Game/Enemy Spawn")]
    public class EnemyObject : ScriptableObject
    {
        [SerializeField] Entity enemy;
        public Entity Enemy { get { return enemy; } private set { enemy = value; } }
    }
}
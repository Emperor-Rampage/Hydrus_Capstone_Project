using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AbilityClasses;
using MapClasses;
using AudioClasses;
using System;

namespace EntityClasses
{
    [Serializable]
    public class Enemy : Entity
    {
        public bool InCombat { get; set; }
        public Direction Target { get; set; }
        [SerializeField] float interval;
        public float Interval { get { return interval; } }
        [SerializeField] float variance;
        public float Variance { get { return variance; } }
        public float NextAmbientPlay { get; set; }
        [SerializeField] SoundEffect ambientSound;
        public SoundEffect AmbientSound { get { return ambientSound; } private set { ambientSound = value; } }
        public Enemy() : base() { }
        public Enemy(Enemy entity) : base(entity)
        {
            interval = entity.interval;
            variance = entity.variance;
            ambientSound = entity.ambientSound;
        }
    }
}
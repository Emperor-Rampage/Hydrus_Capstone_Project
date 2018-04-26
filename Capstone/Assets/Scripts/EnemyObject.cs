using AbilityClasses;
using MapClasses;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityClasses
{
    [CreateAssetMenu(fileName = "New Enemy Spawn", menuName = "Game/Enemy Spawn")]
    public class EnemyObject : ScriptableObject
    {
        [SerializeField] Entity enemy;
        public Entity Enemy { get { return enemy; } private set { enemy = value; } }
    }

    public class Enemy : Entity
    {
        public bool InCombat { get; set; }
        public Direction Target { get; set; }
        public Enemy() : base() { }
        public Enemy(Entity entity) : base(entity) { }
    }
}

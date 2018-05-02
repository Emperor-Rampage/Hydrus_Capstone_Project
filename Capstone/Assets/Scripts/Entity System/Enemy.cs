using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AbilityClasses;
using MapClasses;

namespace EntityClasses {
    public class Enemy : Entity
    {
        public bool InCombat { get; set; }
        public Direction Target { get; set; }
        public Enemy() : base() { }
        public Enemy(Entity entity) : base(entity) { }
    }
}
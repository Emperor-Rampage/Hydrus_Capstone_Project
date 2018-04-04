using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityClasses
{
    [System.Serializable]
    public class EnemySpawn {
        [SerializeField] int x;
        public int X { get { return x; } private set { x = value; } }
        [SerializeField] int z;
        public int Z { get { return z; } private set { z = value; } }
        [SerializeField] Enemy enemy;
        public Enemy Enemy { get { return enemy; } private set { enemy = value; } }
    }
}


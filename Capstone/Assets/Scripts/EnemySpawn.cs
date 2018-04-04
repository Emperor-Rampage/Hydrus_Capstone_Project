using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityClasses
{
    [System.Serializable]
    public class EnemySpawn {
        [SerializeField] int x;
        public int X { get; private set; }
        [SerializeField] int z;
        public int Z { get; private set; }
        [SerializeField] Enemy enemy;
        public Enemy Enemy { get; private set; }
    }
}


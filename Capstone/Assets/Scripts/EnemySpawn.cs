using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityClasses
{
    [System.Serializable]
    public class EnemySpawn
    {
        [SerializeField] int x;
        public int X { get { return x; } private set { x = value; } }
        [SerializeField] int z;
        public int Z { get { return z; } private set { z = value; } }
        [SerializeField] EnemyObject enemy;
        public EnemyObject EnemyObject { get { return enemy; } private set { enemy = value; } }
    }
}


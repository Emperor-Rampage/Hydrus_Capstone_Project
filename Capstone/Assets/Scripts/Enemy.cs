using AbilityClasses;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityClasses
{
    [CreateAssetMenu(fileName = "New Entity Spawn", menuName = "Entity/EntitySpawn")]
    public class Enemy : ScriptableObject
    {
        [SerializeField] Entity entity;
        public Entity Entity { get { return entity; } private set { entity = value; } }
        [SerializeField] List<AbilityObjBase> abilities;
        public List<AbilityObjBase> Abilities { get { return abilities; } private set { abilities = value; } }
    }
}

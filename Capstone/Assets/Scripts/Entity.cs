﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapClasses;
using AbilityClasses;

namespace EntityClasses
{
    // This is all horrible code. Terrible.SAd.

    public enum EntityState
    {
        Null = -1,
        Idle = 0,
        Moving = 1,
        Casting = 2,
    }

    public interface IEntity
    {
        GameObject Instance { get; set; }
        string Name { get; set; }
        Cell Cell { get; set; }
    }

    [System.Serializable]
    public class Entity : IEntity
    {
        [SerializeField] GameObject instance;
        public GameObject Instance { get { return instance; } set { instance = value; } }

        [SerializeField] string name;
        public string Name { get { return name; } set { name = value; } }

        [SerializeField] int cores;
        public int Cores { get { return cores; } set { cores = value; } }

        [SerializeField] int maxHealth;
        public int MaxHealth { get { return maxHealth; } set { maxHealth = value; } }

        public int CurrentHealth { get; set; }

        public Cell Cell { get; set; }

        public Direction Facing { get; set; }

        public EntityState State { get; set; }

        [SerializeField] List<AbilityObject> abilities;
        public List<AbilityObject> Abilities { get { return abilities; } private set { abilities = value; } }

        // Ability Variables
        // TODO: Create custom data structure for current effects.
        //       Should contain a dictionary of lists of effects (to support stacking effects), should use local "cooldown scale" (for example) variable, then multiply the value by the "value" field of each effect. For multiplicative stacking.
        public List<AbilityEffect> CurrentEffects { get; private set; } = new List<AbilityEffect>();

        public Entity() { }
        public Entity(Entity entity)
        {
            Instance = entity.Instance;
            Name = entity.Name;
            Cores = entity.Cores;
            MaxHealth = entity.MaxHealth;
            CurrentHealth = MaxHealth;
            Facing = Direction.Up;
            State = EntityState.Idle;
        }

        public void TurnLeft()
        {
            int facing = (int)Facing - 1;
            if (facing < 0)
            {
                facing += 4;
            }

            Facing = (Direction)facing;
        }
        public void TurnRight()
        {
            int facing = (int)Facing + 1;
            if (facing > 3)
            {
                facing -= 4;
            }

            Facing = (Direction)facing;
        }
        public Direction ToAbsoluteDirection(Direction direction)
        {
            // Facing is Left (3).
            // direction is Down (2).

            // absoluteDirection = 2 + 3 = 5
            // if 5 > 3
            //  5 -= 4 = 1
            // returns 1 (Right)

            int facing = (int)Facing;

            int absoluteDirection = (int)direction + facing;
            if (absoluteDirection > 3)
                absoluteDirection -= 4;

            return (Direction)absoluteDirection;
        }

        public Direction GetBackward()
        {
            int newDir = (int)Facing - 2;
            if (newDir < 0)
                newDir += 4;

            return (Direction)newDir;
        }

        public Direction GetLeft()
        {
            int newDir = (int)Facing - 1;
            if (newDir < 0)
                newDir += 4;

            return (Direction)newDir;
        }

        public Direction GetRight()
        {
            int newDir = (int)Facing + 1;
            if (newDir > 3)
                newDir -= 4;

            return (Direction)newDir;
        }
    }

    public class Player : Entity
    {
        public Player() : base() { }
        public Player(Entity entity) : base(entity) { }
    }

    public class Item : IEntity
    {
        public GameObject Instance { get; set; }
        public string Name { get; set; }
        public Cell Cell { get; set; }
    }
}

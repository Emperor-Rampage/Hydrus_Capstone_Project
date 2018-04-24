﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapClasses;
using AbilityClasses;
using Pixelplacement;

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
        public int Index { get; set; }

        [SerializeField] GameObject instance;
        public GameObject Instance { get { return instance; } set { instance = value; } }

        [SerializeField] string name;
        public string Name { get { return name; } set { name = value; } }

        [SerializeField] int cores;
        public int Cores { get { return cores; } set { cores = value; } }

        [SerializeField] float maxHealth;
        public float MaxHealth { get { return maxHealth; } set { maxHealth = value; } }

        public float CurrentHealth { get; set; }
        public int CurrentAbility { get; set; }

        public float CastProgress { get; set; }
        public float CurrentCastTime { get; set; }

        public Cell Cell { get; set; }

        public Direction Facing { get; set; }

        public EntityState State { get; set; }

        [SerializeField] List<AbilityObject> abilities;
        public List<AbilityObject> Abilities { get { return abilities; } private set { abilities = value; } }

        public List<Indicator> Indicators { get; set; } = new List<Indicator>();

        // Ability Variables
        // TODO: Create custom data structure for current effects.
        //       Should contain a dictionary of lists of effects (to support stacking effects), should use local "cooldown scale" (for example) variable, then multiply the value by the "value" field of each effect. For multiplicative stacking.

        public EffectDictionary StatusEffects = new EffectDictionary();

        List<Coroutine> coroutines = new List<Coroutine>();

        //        Dictionary<AbilityObject, float> cooldowns = new Dictionary<AbilityObject, float>();
        public Dictionary<AbilityObject, float> Cooldowns { get; private set; } = new Dictionary<AbilityObject, float>();
        public Dictionary<AbilityObject, float> CooldownsRemaining { get; private set; } = new Dictionary<AbilityObject, float>();

        public Entity() { Abilities = new List<AbilityObject>(); }
        public Entity(Entity entity)
        {
            Instance = entity.Instance;
            Name = entity.Name;
            Cores = entity.Cores;
            MaxHealth = entity.MaxHealth;
            CurrentHealth = MaxHealth;
            Facing = Direction.Up;
            State = EntityState.Idle;
            Abilities = new List<AbilityObject>(entity.Abilities);
        }

        public AbilityObject CastAbility(int index)
        {
            if (State != EntityState.Idle)
                return null;

            if (index < 0 || index >= Abilities.Count)
                return null;

            AbilityObject ability = Abilities[index];
            if (ability == null)
            {
                Debug.LogError("ERROR: Attempting to cast null ability.");
                return null;
            }

            if (CooldownsRemaining.ContainsKey(ability) && CooldownsRemaining[ability] > 0)
                return null;

            Debug.Log("Casting abilty " + ability.Name + " with cast time of " + ability.CastTime * StatusEffects.CastTimeScale + " at " + Cell.X + ", " + Cell.Z);
            CurrentAbility = index;
            coroutines.Add(GameManager.Instance.StartCoroutine(CastAbility_Coroutine(ability)));
            State = EntityState.Casting;
            return ability;
        }

        IEnumerator CastAbility_Coroutine(AbilityObject ability)
        {
            // Wait the cast time, update cast time progress.
            CurrentCastTime = ability.CastTime / (StatusEffects.CastTimeScale * StatusEffects.HasteScale);
            Tween.Value(0f, 1f, ((prog) => CastProgress = prog), CurrentCastTime, 0f, completeCallback: () => CastProgress = 0f);
            yield return new WaitForSeconds(CurrentCastTime);

            // Call method in GameManager instance to perform the ability actions.
            GameManager.Instance.PerformAbility(this, ability);
            float adjustedCooldown = ability.Cooldown / StatusEffects.CooldownScale;
            Cooldowns[ability] = adjustedCooldown;
            CooldownsRemaining[ability] = adjustedCooldown;
            Tween.Value(adjustedCooldown, 0f, (val) => CooldownsRemaining[ability] = val, adjustedCooldown, 0f);
            RemoveIndicators();
            CurrentAbility = -1;
            CastProgress = 0f;
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

        public float GetDirectionDegrees()
        {
            float degrees = 0f;

            if (Facing == Direction.Up)
            {
                degrees = 0f;
            }
            else if (Facing == Direction.Right)
            {
                degrees = 90f;
            }
            else if (Facing == Direction.Down)
            {
                degrees = 180f;
            }
            else if (Facing == Direction.Left)
            {
                degrees = 270f;
            }

            return degrees;
        }
        public Direction GetDirection(Direction direction)
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

        // Returns true if full health, false if not.
        public bool Heal(float heal)
        {
            CurrentHealth += heal;
            if (CurrentHealth >= MaxHealth)
            {
                CurrentHealth = MaxHealth;
                return true;
            }

            return false;
        }

        // Returns true if alive, false if dead.
        public bool Damage(float damage)
        {
            Debug.Log(Name + " currently has " + CurrentHealth + " health. Taking " + damage + " damage.");
            float adjustedDamage = damage * StatusEffects.DamageScale;
            if (CurrentHealth > adjustedDamage)
            {
                CurrentHealth -= adjustedDamage;
                Debug.Log(Name + " now has " + CurrentHealth);
                return true;
            }

            CurrentHealth = 0;
            Kill();
            return false;
        }

        public void Kill()
        {
            // Stop current coroutines.
            coroutines.ForEach((coroutine) => GameManager.Instance.StopCoroutine(coroutine));
            RemoveIndicators();
            if (State == EntityState.Casting)
                State = EntityState.Idle;
        }
        public void RemoveIndicators()
        {
            while (Indicators.Count > 0)
            {
                Indicators[0].RemoveIndicator();
            }
        }
    }

    public class Player : Entity
    {
        //        PlayerClass playerClass;
        //        public PlayerClass Class { get { return playerClass; } set { playerClass = value; } }
        public PlayerClass Class { get; set; }

        public Player() : base() { }
        public Player(Entity entity) : base(entity) { }

        public void SetupBaseAbilities()
        {
            Abilities.Clear();
            if (Class.BaseAbilities == null)
            {
                Debug.Log("Class object has no base abilities.");
            }
            foreach (AbilityObject ability in Class.BaseAbilities)
            {
                Abilities.Add(ability);
            }
        }
    }

    public class Item : IEntity
    {
        public GameObject Instance { get; set; }
        public string Name { get; set; }
        public Cell Cell { get; set; }
    }
}

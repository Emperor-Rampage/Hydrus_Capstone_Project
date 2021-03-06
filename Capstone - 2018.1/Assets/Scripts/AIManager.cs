﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EntityClasses;
using AbilityClasses;
using MapClasses;
using AStar;

namespace AIClasses
{
    public enum Movement
    {
        Null = -1,
        Nothing = 0,
        Forward = 1,
        TurnLeft = 2,
        TurnRight = 3,
    }
    public class EnemyAction
    {
        public int AbilityIndex { get; set; }
        public Movement Movement { get; set; }
    }

    public struct ActionValue : IHeapObject<ActionValue>
    {
        public int ActionIndex { get; set; }
        public int HeapIndex { get; set; }
        public float Value { get; set; }

        public int CompareTo(ActionValue action)
        {
            int result = Value.CompareTo(action.Value);
            //            Debug.Log("Comparing " + Value + " to " + action.Value + " .. result is " + result);
            return -result;
        }
    }

    public class AIManager : MonoBehaviour
    {
        // TODO: Implement AI System.
        /*

            Each tick (Frame? 1/10th of a second? 1/2 of a second?),
                Iterate through all enemy entities.
                If they are idle, analyze their available actions and assign each one a value.
                Choose the action with the highest value, and execute.
                Alternatively: Grab the top two actions, flip a coin, and choose between them randomly.

            Enemies will be in one of two modes:
                Out of Combat
                In Combat

            While out of combat, enemies will simply wander around occasionally.
            While in combat, enemies will actively pursue and attack the player.
                An enemy will go into combat mode when the player reaches a certain proximity to the enemy.
                Calculated as Proximity = Abs(player.X - enemy.X) + Abs(player.Z - enemy.Z)

            Actions possible:
                Do nothing
                Move forward
                Turn left
                Turn right
                Cast an ability

            Factors that will affect an action's value:
                Enemy's health
                Enemy's state
                Player's health
                Player's state (stunned, moving, etc)
                Player's proximity
                For each ability:
                    Cast Time
                    Cooldown
                    Damage
                    Effects
                    Type

			Actions will be compiled into a single list of IEnemyAction.
			EnemyAbility will contain a reference to the ability to be cast.
			EnemyMovement will contain a index that refers to the type of movement.
				0 = Do nothing
				1 = Move forward
				2 = Move backward
				3 = Turn left
				4 = Turn right
        */

        //        GameManager manager;
        AStarManager astar = new AStarManager();

        void Start()
        {
            //            manager = GameManager.Instance;
        }

        public EnemyAction ExecuteAIOnEnemy(Enemy enemy, Level level)
        {
            if (enemy == null || level == null)
            {
                Debug.LogError("ERROR: Passed in null enemy or level to AIManager.ExecuteAIOnEnemy");
                return null;
            }

            astar.Level = level;

            // Debug.Log("Executing AI on " + enemy.Name);
            EnemyAction bestAction = new EnemyAction { AbilityIndex = -1, Movement = Movement.Null };
            if (enemy.InCombat)
            {
                // Debug.Log("Enemy is in combat, pursuing the palyer.");
                // Pursue and attack the player.
                // For abilities, just calculate based on situation and distance
                // For movement, get the best target neighbor to move to and calculate each possible movement's value towards getting to that target.
                if (level.Player != null)
                {
                    // First, get a list of possible actions.
                    List<EnemyAction> actionList = new List<EnemyAction>();

                    // Iterate through abilities.

                    Cell enemyCell = enemy.Cell;
                    Cell playerCell = level.Player.Cell;
                    Path pathToPlayer = astar.GetBestPath(enemyCell, playerCell);
                    if (pathToPlayer == null)
                    {
                        Debug.Log("Could not find path to player.");
                        enemy.Target = Direction.Null;
                    }
                    else
                    {
                        Cell nextCell = pathToPlayer.Next() as Cell;
                        Direction closest = enemyCell.GetNeighborDirection(nextCell);
                        // Debug.Log("Found path to player. Current cell is " + enemyCell.X + ", " + enemyCell.Z +
                        // ". Next cell is " + nextCell.X + ", " + nextCell.Z + ".  Direction is " + closest);
                        enemy.Target = closest;
                    }

                    // Get best direction for target.
                    // if (enemy.Target == Direction.Null)
                    // {


                    // int closestDist = 1000;
                    // Direction closest = Direction.Null;
                    // //                    List<Direction> directions = new List<Direction>(); //level.GetNeighbors(enemy.Cell);
                    // foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                    // {
                    //     Cell neighbor = level.GetNeighbor(enemy.Cell, direction);
                    //     if (neighbor != null && !neighbor.Locked)
                    //     {
                    //         // Debug.Log("Distance between " + neighbor.X + ", " + neighbor.Z + " and " + level.Player.Cell.X + ", " + level.Player.Cell.Z + " is " + neighbor.GetDistance(level.Player.Cell) + " for direction " + direction);
                    //         int dist = neighbor.GetDistance(level.Player.Cell);
                    //         // If in combat, either go towards an empty cell or the player.
                    //         if (dist < closestDist && (neighbor.Occupant == null || neighbor.Occupant.IsPlayer))
                    //         {
                    //             closest = direction;
                    //             closestDist = dist;
                    //         }
                    //     }
                    //     //                            directions.Add(direction);
                    // }

                    // if (closest != Direction.Null)
                    //     enemy.Target = closest;

                    // }
                    // Debug.Log("Best movement determined to be " + enemy.Target);

                    // Add all movements.
                    foreach (Movement movement in Enum.GetValues(typeof(Movement)))
                    {
                        actionList.Add(new EnemyAction { AbilityIndex = -1, Movement = movement });
                    }
                    // Add all abilities.
                    for (int a = 0; a < enemy.Abilities.Count; a++)
                    {
                        actionList.Add(new EnemyAction { AbilityIndex = a, Movement = Movement.Null });
                    }

                    // Next, calculate each action's value.

                    // FIXME: Heap not returning correct value.
                    // Heap<ActionValue> actionHeap = new Heap<ActionValue>(actionList.Count);
                    List<ActionValue> actionHeap = new List<ActionValue>();

                    for (int a = 0; a < actionList.Count; a++)
                    {
                        EnemyAction action = actionList[a];
                        ActionValue actionValue = new ActionValue { ActionIndex = a, Value = CalculateAction(enemy, action, level) };

                        actionHeap.Add(actionValue);
                    }

                    // ActionValue best = actionHeap.RemoveFirst();
                    ActionValue best = actionHeap[0];
                    foreach (ActionValue v in actionHeap)
                    {
                        if (v.Value > best.Value)
                            best = v;
                    }

                    // Debug.Log("Heap returned " + best.ActionIndex + " with value " + best.Value);
                    // Debug.Log("Corresponding action is " + actionList[best.ActionIndex].Movement);

                    bestAction = actionList[best.ActionIndex];
                }
            }
            else
            {
                // Wander around occasionally.

                // Choose a random neighbor as a target, then turn towards it and move there.
                // If on the target, set target to null and,
                //		decide a new random target,
                //		or do nothing.

                // If target is not null,
                //		Get a list of possible movements.
                // 		Assess each movement

                //                Debug.Log("-- Enemy is not in combat.");
                // Get a new target cell if the target cell is null.
                if (enemy.Target == Direction.Null)
                {
                    float chance = UnityEngine.Random.Range(0f, 1f);
                    //                    Debug.Log("-- Getting new target, roll of die was " + chance);
                    if (chance < 0.02f)
                    {
                        List<Direction> directions = new List<Direction>(); //level.GetNeighbors(enemy.Cell);
                        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                        {
                            Cell neighbor = level.GetDestination(enemy, direction);
                            if (neighbor != null)
                                directions.Add(direction);
                        }

                        if (directions.Count == 0)
                            return bestAction;

                        Direction targetDirection = directions[UnityEngine.Random.Range(0, directions.Count)];
                        // Debug.Log("-- New target direction is " + targetDirection);
                        enemy.Target = targetDirection;
                    }
                }

                List<EnemyAction> actionList = new List<EnemyAction>();

                foreach (Movement movement in Enum.GetValues(typeof(Movement)))
                {
                    actionList.Add(new EnemyAction { AbilityIndex = -1, Movement = movement });
                }

                // FIXME: Heap not returning correct value.
                //                Heap<ActionValue> actionHeap = new Heap<ActionValue>(actionList.Count);
                List<ActionValue> actionHeap = new List<ActionValue>();

                for (int a = 0; a < actionList.Count; a++)
                {
                    EnemyAction action = actionList[a];
                    ActionValue actionValue = new ActionValue { ActionIndex = a, Value = CalculateAction(enemy, action, level) };

                    actionHeap.Add(actionValue);
                }

                //                ActionValue best = actionHeap.RemoveFirst();
                ActionValue best = actionHeap[0];
                foreach (ActionValue v in actionHeap)
                {
                    if (v.Value > best.Value)
                        best = v;
                }

                // Debug.Log("Heap returned " + best.ActionIndex + " with value " + best.Value);
                // Debug.Log("Corresponding action is " + actionList[best.ActionIndex].Movement);

                bestAction = actionList[best.ActionIndex];

                //                List<float> values;
            }
            // Debug.Log("-- Best action found for " + enemy.Name + " was " + bestAction.Movement.ToString() + " .. target was " + enemy.Target);
            return bestAction;
        }

        public float CalculateAction(Enemy enemy, EnemyAction action, Level level = null)
        {
            float value = 0f;
            EffectDictionary enemyEffectDictionary = enemy.StatusEffects;
            bool stunned = enemyEffectDictionary.GetEffectValue_Bool(AbilityStatusEff.Stun);
            bool silenced = enemyEffectDictionary.GetEffectValue_Bool(AbilityStatusEff.Silence);
            bool rooted = enemyEffectDictionary.GetEffectValue_Bool(AbilityStatusEff.Root);
            float slow = enemyEffectDictionary.GetEffectValue_Float(AbilityStatusEff.MoveSlow);
            float castSlow = enemyEffectDictionary.GetEffectValue_Float(AbilityStatusEff.CastTimeSlow);
            float castBuff = enemyEffectDictionary.GetEffectValue_Float(AbilityStatusEff.CastTimeBuff);

            // TODO: Implement calculation based on ability effects.
            if (action.AbilityIndex != -1)
            {
                // Abilities should be impossible if stunned or silenced. Less likely if cast time is slowed. More likely if move slowed.
                //           More likely when hasted.

                // If stunned, calculate ability value as 0.
                if (stunned || silenced)
                    return value;

                AbilityObject ability = enemy.Abilities[action.AbilityIndex];
                // Calculate ability value.

                // TODO: Edit to be better. Right now just attacks if it can.
                //       Takes into account: Damage, player health. Enemy's state and current effects.
                //       To be added: Player's state, cast time, cooldown, effects, type.

                // If it's on cooldown, can't use the ability.
                if (enemy.CooldownsRemaining.ContainsKey(ability) && enemy.CooldownsRemaining[ability] > 0f)
                    return value;

                if (level != null)
                {
                    Player player = level.Player;
                    var affected = level.GetAffectedCells_Highlight(enemy, ability);
                    // Debug.Log(affected.Count);
                    // If the ability would hit the player or the caster (only possible for self cast abilities)
                    if (affected.Contains(player.Cell) || ability.Type == AbilityType.Self)
                    {
                        value += 1f;
                    }
                    float modifier = 1f;
                    // Increase based on damage.
                    modifier += (ability.Damage / 100f);
                    // Increase depending on how much health the player has.
                    // modifier += (1 / (player.CurrentHealth / (float)player.MaxHealth));
                    // Increase depending on amount hasted.
                    modifier += (castBuff * 0.25f);
                    // Decrease depending on amount cast time slowed.
                    modifier -= (castSlow * 0.25f);
                    // Increase depending on amount move slowed.
                    modifier += (slow * 0.15f);

                    value *= modifier;

                    float effectsValue = 1f;

                    foreach (var effect in ability.StatusEffects)
                    {
                        switch (effect.Effect)
                        {
                            case (AbilityStatusEff.CastTimeSlow):
                            case (AbilityStatusEff.CooldownSlow):
                            case (AbilityStatusEff.DamReduct):
                            case (AbilityStatusEff.CastTimeBuff):
                            case (AbilityStatusEff.MoveSlow):
                                {
                                    effectsValue += (effect.Value * effect.Duration / 5f);
                                    break;
                                }
                            case (AbilityStatusEff.Root):
                            case (AbilityStatusEff.Silence):
                            case (AbilityStatusEff.Stun):
                                {
                                    effectsValue += (effect.Duration / 5f);
                                    break;
                                }
                            case (AbilityStatusEff.DoT):
                            case (AbilityStatusEff.Heal):
                                {
                                    effectsValue += ((effect.Value / effect.Duration) / 5f);
                                    break;
                                }
                        }
                    }

                    value *= effectsValue;

                    // value *= ((ability.Damage / 100f) + 1);


                    // value = value / (player.CurrentHealth / (float)player.MaxHealth);

                }
                else
                {
                    Debug.LogWarning("WARNING: Level is null in AIManager.");
                }
                // Debug.Log("Calculated " + action.AbilityIndex + " as " + value);
            }
            else if (action.Movement != Movement.Null)
            {
                // Movement should be impossible if rooted or stunned. Less likely when movement slowed.
                //          More likely when cast time slowed. More likely when silenced.

                // Calculate Nothing movement independently.
                if (action.Movement == Movement.Nothing)
                {
                    if (enemy.Target == Direction.Null)
                    {
                        value += 1;
                    }
                    else
                    {
                        // Just so that the ai will do nothing if everything is 0.
                        value = 0.1f;
                    }
                }

                if (rooted || stunned)
                    return value;

                // Calculate movement value.
                if (action.Movement == Movement.Forward)
                {
                    if (enemy.Facing == enemy.Target)
                    {
                        value += 1;
                    }
                }
                else if (action.Movement == Movement.TurnLeft)
                {
                    if (enemy.GetLeft() == enemy.Target)
                    {
                        value += 1;
                    }
                    else if (enemy.GetBackward() == enemy.Target)
                    {
                        value += 1;
                    }
                }
                else if (action.Movement == Movement.TurnRight)
                {
                    if (enemy.GetRight() == enemy.Target)
                    {
                        value += 1;
                    }
                }

                float modifier = 1f;
                modifier -= (slow * 0.25f);
                modifier += (castSlow * 0.15f);

                value *= modifier;
                // Debug.Log("Calculated " + action.Movement + " as " + value);
                // value = value / (enemy.CurrentHealth / (float)enemy.MaxHealth);
            }
            //            Debug.Log("Calculate " + action.Movement + " as " + value);
            return value;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AbilityClasses;

[System.Serializable]
[CreateAssetMenu(fileName = "New Player Class", menuName = "Game/Player Class")]
public class PlayerClass : ScriptableObject
{
    [SerializeField] string className;
    public string Name { get { return className; } }

    [TextArea]
    [SerializeField]
    string description;
    public string Description { get { return description; } }
    [SerializeField] List<AbilityObject> baseAbilities;
    public List<AbilityObject> BaseAbilities { get { return baseAbilities; } }
}



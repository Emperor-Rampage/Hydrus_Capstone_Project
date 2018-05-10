using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AbilityClasses;

[System.Serializable]
[CreateAssetMenu(fileName = "New Player Class", menuName = "Game/Player Class")]
public class PlayerClass : ScriptableObject
{
    //Classname
    [SerializeField] string className;
    public string Name { get { return className; } }

    //Class Description
    [TextArea]
    [SerializeField]
    string description;
    public string Description { get { return description; } }
    [SerializeField] int health;
    public int Health { get { return health; } }

    //List of the Base abilities offered by the class.
    [SerializeField] List<AbilityObject> baseAbilities;
    public List<AbilityObject> BaseAbilities { get { return baseAbilities; } }

    //Adding in the link to the player prefab that will contain the Camera, Arms, and Animator/tions to run the player. -Conner
    [SerializeField] GameObject classCamera;
    public GameObject ClassCamera { get { return classCamera; } }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AbilityClasses;
using AudioClasses;

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
    [SerializeField] EntityContainer classCamera;
    public EntityContainer ClassCamera { get { return classCamera; } }
    [SerializeField] SoundEffect walkingSound;
    public SoundEffect WalkingSound { get { return walkingSound; } private set { walkingSound = value; } }
    [SerializeField] SoundEffect hitSound;
    public SoundEffect HitSound { get { return hitSound; } private set { hitSound = value; } }
    [SerializeField] SoundEffect hurtSound;
    public SoundEffect HurtSound { get { return hurtSound; } private set { hurtSound = value; } }
    [SerializeField] SoundEffect deathSound;
    public SoundEffect DeathSound { get { return deathSound; } private set { deathSound = value; } }
}



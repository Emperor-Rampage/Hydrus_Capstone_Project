using System.Collections;
using System.Collections.Generic;
using AudioClasses;
using EntityClasses;
using UnityEngine;
using Pixelplacement;

namespace AbilityClasses
{
    public enum AbilityType
    {
        None = -1,
        Self = 0,
        Melee = 1,
        Ranged = 2,
        AreaOfEffect = 3
    }

    public enum AbilityStatusEff
    {
        NoEffect = -1,
        CastTimeSlow = 0,
        CooldownSlow = 1,
        Stun = 2,
        MoveSlow = 3,
        Root = 4,
        Silence = 5,
        Heal = 6,
        Haste = 7,
        DamReduct = 8,
        DoT = 9
    }

   
    [System.Serializable]

    public class AbilityEffect                                                                                    //Structure for containing ability effects
    {
        // public Entity Owner { get; private set; }
        // Entity owner;
        public int OwnerIndex { get; set; }
        
        [SerializeField]
        AbilityStatusEff effect;
        public AbilityStatusEff Effect { get { return effect; } private set { effect = value; } }

        [SerializeField]
        float duration;
        public float Duration { get { return duration; } private set { duration = value; } }

        [SerializeField]
        float value;
        public float Value { get { return value; } private set { this.value = value; } }

        public AbilityEffect(int ownerIndex, AbilityStatusEff eff, float dur, float val)
        {
            OwnerIndex = ownerIndex;
            Effect = eff;
            Duration = dur;
            Value = val;
        }

        public void StartTween()
        {

        }
    }

    [System.Serializable]
    public class EffectDictionary
    {

        //To Do: RemoveEffect method, Tween/timer method
        private float cooldownScale = 1.0f;
        private float movementScale = 1.0f;
        private float castTimeScale = 1.0f;
        private float damageScale = 1.0f;
        private float healRate = 0.0f;
        private float damageRate = 0.0f;
        private bool stunned = false;
        private bool rooted = false;
        private bool silenced = false;

        public Dictionary<AbilityStatusEff, List<AbilityEffect>> EffectLibrary;
        public List<AbilityEffect> CurrentEffects; //Empty list for instantiation purposes

        public EffectDictionary() { }

        public void AddEffect(AbilityEffect AbilEffect)
        {
            if (EffectLibrary.ContainsKey(AbilEffect.Effect))
            {
                // NOTE: I moved this to one line because we already have too many darn lines.
                EffectLibrary[AbilEffect.Effect].Add(AbilEffect);
                CalcEffects(AbilEffect.Effect);

            }
            else if(AbilEffect.Effect == AbilityStatusEff.NoEffect) //Error handling for no value
            {
                return;
            }
            else
            {
                // CurrentEffects is never instantiated, and if it was it'd just keep referring to the same list.
                // Should just create a new one.
                EffectLibrary.Add(AbilEffect.Effect, new List<AbilityEffect>());
                EffectLibrary[AbilEffect.Effect].Add(AbilEffect);
                CalcEffects(AbilEffect.Effect);
            }
        }


        //At the end of the ability effect Tween, remove the AbilityEffect from the list of current effects.
        //If the list is empty, remove the key entirely.
        //Also recalculate the current effects list on every remove.
        public void RemoveEffect()
        {

        }

        public List<AbilityEffect> GetEffectList(AbilityStatusEff targetType)
        {
            List<AbilityEffect> result = null;

            if (EffectLibrary.TryGetValue(targetType, out result))
            {
                Debug.Log("Found key, returning list.");
            }

            return result;
        }

        public void CalcEffects(AbilityStatusEff type)
        {
            if(type != AbilityStatusEff.NoEffect)
            {
                switch(type)
                {
                    case AbilityStatusEff.CastTimeSlow:
                        {
                            List<AbilityEffect> index = EffectLibrary[AbilityStatusEff.CastTimeSlow];

                            if (index != null && index.Count != 0) //validation check.
                            {
                                for (int i = 0; i <= index.Count;)
                                {
                                    castTimeScale *= index[i].Value;
                                }
                            }
                            else if (index != null && index.Count == 0)
                            {
                                castTimeScale = 1.0f;
                            }
                            else if (index == null)
                            {
                                castTimeScale = 1.0f;
                            }
                            break;
                        }

                    case AbilityStatusEff.CooldownSlow:
                        {
                            List<AbilityEffect> index = EffectLibrary[AbilityStatusEff.CooldownSlow];

                            if (index != null && index.Count != 0) //validation check.
                            {
                                for (int i = 0; i <= index.Count;)
                                {
                                    cooldownScale *= index[i].Value;
                                }
                            }
                            else if (index != null && index.Count == 0)
                            {
                                cooldownScale = 1.0f;
                            }
                            else if (index == null)
                            {
                                cooldownScale = 1.0f;
                            }
                            break;
                        }

                    case AbilityStatusEff.Stun:
                        {
                            List<AbilityEffect> index = EffectLibrary[AbilityStatusEff.Stun];
                            if (index != null && index.Count != 0)
                            {
                                stunned = true;
                            }
                            else
                            {
                                stunned = false;
                            }
                            break;
                        }

                    case AbilityStatusEff.MoveSlow:
                        {
                            List<AbilityEffect> index = EffectLibrary[AbilityStatusEff.MoveSlow];

                            if ( index != null && index.Count != 0) //validation check.
                            {
                                for (int i = 0; i <= index.Count;)
                                {
                                    movementScale *= index[i].Value;
                                }
                            }
                            else if(index != null && index.Count == 0)
                            {
                                movementScale = 1.0f;
                            }
                            else if (index == null)
                            {
                                movementScale = 1.0f;
                            }
                            break;
                        }

                    case AbilityStatusEff.Root:
                        {
                            List<AbilityEffect> index = EffectLibrary[AbilityStatusEff.Root];
                            if (index != null && index.Count != 0)
                            {
                                rooted = true;
                            }
                            else
                            {
                                rooted = false;
                            }
                            break;
                        }

                    case AbilityStatusEff.Silence:
                        {
                            List<AbilityEffect> index = EffectLibrary[AbilityStatusEff.Silence];
                            if (index != null && index.Count != 0)
                            {
                                silenced = true;
                            }
                            else
                            {
                                silenced = false;
                            }
                            break;
                        }

                    case AbilityStatusEff.Heal:
                        {
                            List<AbilityEffect> index = EffectLibrary[AbilityStatusEff.Heal];

                            if (index != null && index.Count != 0) //validation check.
                            {
                                for (int i = 0; i <= index.Count;)
                                {
                                    healRate += index[i].Value;
                                }
                            }
                            else if (index != null && index.Count == 0)
                            {
                                healRate = 0.0f;
                            }
                            else if (index == null)
                            {
                                healRate = 0.0f;
                            }
                            break;
                        }

                    case AbilityStatusEff.Haste:
                        {
                            List<AbilityEffect> index = EffectLibrary[AbilityStatusEff.Haste];

                            if (index != null && index.Count != 0) //validation check.
                            {
                                for (int i = 0; i <= index.Count;)
                                {
                                    castTimeScale *= index[i].Value;
                                }
                            }
                            else if (index != null && index.Count == 0)
                            {
                                movementScale = 1.0f;
                            }
                            else if (index == null)
                            {
                                movementScale = 1.0f;
                            }
                            break;
                        }

                    case AbilityStatusEff.DamReduct:
                        {
                            List<AbilityEffect> index = EffectLibrary[AbilityStatusEff.DamReduct];

                            if (index != null && index.Count != 0) //validation check.
                            {
                                for (int i = 0; i <= index.Count;)
                                {
                                    damageRate *= index[i].Value;
                                }
                            }
                            else if (index != null && index.Count == 0)
                            {
                               damageRate = 1.0f;
                            }
                            else if (index == null)
                            {
                                damageRate = 1.0f;
                            }
                            break;
                        }

                    case AbilityStatusEff.DoT:
                        {
                            List<AbilityEffect> index = EffectLibrary[AbilityStatusEff.DoT];

                            if (index != null && index.Count != 0) //validation check.
                            {
                                for (int i = 0; i <= index.Count;)
                                {
                                    damageRate += index[i].Value;
                                }
                            }
                            else if (index != null && index.Count == 0)
                            {
                                damageRate = 0.0f;
                            }
                            else if (index == null)
                            {
                                damageRate = 0.0f;
                            }
                            break;
                        }
                }
            }
        }

        public float GetEffectValue_float(AbilityStatusEff type)
        {
            if (EffectLibrary.ContainsKey(type) && type != AbilityStatusEff.NoEffect)
            {
                switch (type)
                {
                    case AbilityStatusEff.CastTimeSlow:
                        {
                            return castTimeScale;
                        }

                    case AbilityStatusEff.CooldownSlow:
                        {
                            return cooldownScale;
                        }

                    case AbilityStatusEff.MoveSlow:
                        {
                            return movementScale;
                        }

                    case AbilityStatusEff.Heal:
                        {
                            return healRate;
                        }

                    case AbilityStatusEff.Haste:
                        {
                            return castTimeScale;
                        }

                    case AbilityStatusEff.DamReduct:
                        {
                            return damageScale;
                        }

                    case AbilityStatusEff.DoT:
                        {
                            return damageRate;
                        }
                }
            }
            return 1.0f;
        }

        public bool GetEffectValue(AbilityStatusEff type)
        {
            if (EffectLibrary.ContainsKey(type) && type != AbilityStatusEff.NoEffect)
            {
                switch (type)
                {

                    case AbilityStatusEff.Stun:
                        {
                            return stunned;
                        }

                    case AbilityStatusEff.Root:
                        {
                            return rooted;
                        }

                    case AbilityStatusEff.Silence:
                        {
                            return silenced;
                        }
                    default:
                        {
                            return false;
                        }
                }
            }
            return false;
        }

    }

    [System.Serializable]
    public abstract class AbilityObject : ScriptableObject
    {
        //TODO: Continue researching the scriptable objects ability system, and how to make it work for us.
        //This is the basic template for all abilites in the game. This container holds all the info we need to make the abilites actually activate.

        //Basic Ability Information
        [SerializeField] string abilityName = "Default Ability Name";                                                         //Ability Name
        public string Name { get { return abilityName; } }

        [SerializeField] Sprite icon;                                                                                     //Sprite Icon denoting the Ability
        public Sprite Icon { get { return icon; } }

        [TextArea]
        [SerializeField]
        string toolTip = ": This is the Default Ability tooltip. Please change me.";                         //Tooltip that explaines what the ability does.
        public string ToolTip { get { return toolTip; } }

        [SerializeField] ParticleSystem particleSystem;                                                                   //Container for the Particle Effect System of the Ability.
        public ParticleSystem ParticleSystem { get { return particleSystem; } }

        [SerializeField] SoundEffect soundEffect;                                                                                  //Sound Effect to play on activation.
        public SoundEffect SoundEffect { get { return soundEffect; } }

        [SerializeField] List<AbilityEffect> statusEffects;                                                                   //List of status effects, if any. Denoted by an Enumerable ID value that informs the ability what to affect the target entity with.
        public List<AbilityEffect> StatusEffects { get { return statusEffects; } }

        [SerializeField] AbilityType type;                                                                                    //Denotes what kind of ability is being used, and who it affects. Uses an Enumerable ID value to inform the ability script.
        public AbilityType Type { get { return type; } }

        [SerializeField] Texture2D aoeSprite;                                                                               //Tilemap that denotes the effective range of the ability. Not used in Melee, or Self abilites.
        public Texture2D AOESprite { get { return aoeSprite; } }

        [SerializeField] float range;                                                                                         //For ranged abilities. Indicates the range of the ability.
        public float Range { get { return range; } }

        [SerializeField] float cooldown = 0.0f;                                                                           //The base Cooldown timer (in seconds) of the ability.
        public float Cooldown { get { return cooldown; } }

        [SerializeField] float castTime = 0.0f;                                                                           //The base Cast Timer (in seconds) it takes for the ability to activate.
        public float CastTime { get { return castTime; } }

        [SerializeField] int damage = 0;                                                                                //Initial damage value of the ability.
        public int Damage { get { return damage; } }

        [SerializeField] bool upgradeable;                                                                                     //Boolean that denotes if the ability can be upgraded or not.
        public bool Upgradeable { get { return upgradeable; } }


        //Upgrade System Info.
        [SerializeField] int tier;                                                                                    //Integer used to denote the level of the ability in the upgrade tree.
        public int Tier { get { return tier; } }

        [SerializeField] AbilityObject previousTier;                                                                        //Reference the the previous ability level of the tree, if any. Used for validation.
        public AbilityObject PreviousTier { get { return previousTier; } }

        [SerializeField] List<AbilityObject> nextTier;                                                                      //Reference to the next ability level of the tree, if any. Used for validation.
        public List<AbilityObject> NextTier { get { return nextTier; } }

        [SerializeField] float cost;                                                                                   //The Core cost to upgrade the ability, if any.
        public float Cost { get { return cost; } }
    }
}
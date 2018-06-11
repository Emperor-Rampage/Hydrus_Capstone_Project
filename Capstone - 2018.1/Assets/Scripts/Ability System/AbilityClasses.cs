using System.Collections;
using System.Collections.Generic;
using AudioClasses;
using EntityClasses;
using UnityEngine;
using Pixelplacement;
using MapClasses;
using Pixelplacement.TweenSystem;

namespace AbilityClasses
{
    public enum AbilityType
    {
        None = -1,
        Self = 0,
        Melee = 1,
        Ranged = 2,
        AreaOfEffect = 3,
        Zone = 4
    }

    public enum AbilityStatusEff
    {
        NoEffect = -1,
        CastTimeSlow = 0,
        CastTimeBuff = 1,
        CooldownSlow = 2,
        Stun = 3,
        MoveSlow = 4,
        MoveBuff = 5,
        Root = 6,
        Silence = 7,
        Heal = 8,
        DamReduct = 10,
        DoT = 11
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

        public float Remaining { get; set; }

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
    }

    [System.Serializable]
    public class EffectDictionary
    {

        //To Do: RemoveEffect method, Tween/timer method
        //        private float cooldownScale = 1.0f;
        public float CooldownScale { get; private set; } = 1f;
        //        private float movementScale = 1.0f;
        public float MovementScale { get; private set; } = 1f;
        public float MovementBuff { get; private set; } = 1f;
        //        private float castTimeScale = 1.0f;
        public float CastTimeScale { get; private set; } = 1f;
        //        private float hasteScale = 1.0f;
        public float CastTimeBuff { get; private set; } = 1f;
        // private float damageScale = 1.0f;
        public float DamageScale { get; private set; } = 1f;
        // private float healRate = 0.0f;
        public float HealRate { get; private set; } = 0f;
        // private float damageRate = 0.0f;
        public float DamageRate { get; private set; } = 0f;
        // private bool stunned = false;
        public bool Stunned { get; private set; } = false;
        // private bool rooted = false;
        public bool Rooted { get; private set; } = false;
        // private bool silenced = false;
        public bool Silenced { get; private set; } = false;

        public Dictionary<AbilityStatusEff, List<AbilityEffect>> EffectLibrary;

        List<TweenBase> tweens;

        public EffectDictionary()
        {
            EffectLibrary = new Dictionary<AbilityStatusEff, List<AbilityEffect>>();
            tweens = new List<TweenBase>();
        }

        public void AddEffect(AbilityEffect AbilEffect)
        {
            if (EffectLibrary.ContainsKey(AbilEffect.Effect))
            {
                if (EffectLibrary[AbilEffect.Effect].Exists(e => e.GetHashCode() == AbilEffect.GetHashCode()))
                    return;

                EffectLibrary[AbilEffect.Effect].Add(AbilEffect);
                StartTween(AbilEffect);
                CalcEffects(AbilEffect.Effect);
            }
            else if (AbilEffect.Effect == AbilityStatusEff.NoEffect) //Error handling for no value
            {
                return;
            }
            else
            {
                EffectLibrary.Add(AbilEffect.Effect, new List<AbilityEffect>());
                EffectLibrary[AbilEffect.Effect].Add(AbilEffect);
                StartTween(AbilEffect);
                CalcEffects(AbilEffect.Effect);
            }
        }

        void StartTween(AbilityEffect eff)
        {
            eff.Remaining = eff.Duration;
            TweenBase effectTween = Tween.Value(eff.Duration, 0f, (value) => eff.Remaining = value, eff.Duration, 0.0f, completeCallback: () => RemoveEffect(eff));
            tweens.Add(effectTween);
        }

        //At the end of the ability effect Tween, remove the AbilityEffect from the list of current effects.
        //Also recalculate the current effects list on every remove.
        void RemoveEffect(AbilityEffect AbilEffect)
        {
            //Validating that the list exists and it isn't empty. Because finding an item via Value is slow as heck.
            if (GetEffectList(AbilEffect.Effect) != null)
            {
                EffectLibrary[AbilEffect.Effect].Remove(AbilEffect);
                CalcEffects(AbilEffect.Effect);
            }
            else
            {
                throw new KeyNotFoundException(AbilEffect.Effect + " is not in the Dictionary of effects.");
            }
        }

        public List<AbilityEffect> GetEffectList(AbilityStatusEff targetType)
        {
            List<AbilityEffect> result = null;

            if (EffectLibrary.TryGetValue(targetType, out result))
            {
                //Debug.Log("Found key, returning list.");
            }

            return result;
        }

        // NOTE: Refactored just to make it more compact. Isn't really more efficient at all, just fewer lines.
        // NOTE: Removed the commented code, save for one, just in case I need to refer back to it! -Conner P.S. Pretty sure we won't.

        void CalcEffects(AbilityStatusEff type)
        {
            if (type != AbilityStatusEff.NoEffect)
            {
                List<AbilityEffect> typeList = GetEffectList(type);
                switch (type)
                {
                    case AbilityStatusEff.CastTimeSlow:
                        {
                            CastTimeScale = 1f;
                            if (typeList != null)
                            {
                                foreach (AbilityEffect effect in typeList)
                                {
                                    CastTimeScale *= (1f - effect.Value);
                                }
                            }
                            CastTimeScale = Mathf.Clamp(CastTimeScale, 0f, 1f);

                            break;
                        }

                    case AbilityStatusEff.CastTimeBuff:
                        {
                            CastTimeBuff = 1f;
                            if (typeList != null)
                            {
                                foreach (AbilityEffect effect in typeList)
                                {
                                    CastTimeBuff *= (1f + effect.Value);
                                }
                            }
                            CastTimeBuff = Mathf.Clamp(CastTimeBuff, 1f, 2f);

                            break;
                        }

                    case AbilityStatusEff.CooldownSlow:
                        {
                            CooldownScale = 1f;
                            if (typeList != null)
                            {
                                foreach (AbilityEffect effect in typeList)
                                {
                                    CooldownScale *= (1f - effect.Value);
                                }
                            }
                            CooldownScale = Mathf.Clamp(CooldownScale, 0f, 1f);

                            break;
                        }

                    case AbilityStatusEff.Stun:
                        {
                            Stunned = false;
                            if (typeList != null && typeList.Count != 0)
                            {
                                Stunned = true;
                            }

                            break;
                        }

                    case AbilityStatusEff.MoveSlow:
                        {
                            MovementScale = 1f;
                            if (typeList != null)
                            {
                                foreach (AbilityEffect effect in typeList)
                                {
                                    MovementScale *= (1f - effect.Value);
                                }
                            }
                            MovementScale = Mathf.Clamp(MovementScale, 0f, 1f);

                            break;
                        }

                    case AbilityStatusEff.MoveBuff:
                        {
                            MovementBuff = 1f;
                            if (typeList != null)
                            {
                                foreach (AbilityEffect effect in typeList)
                                {
                                    MovementBuff *= (1f + effect.Value);
                                }
                            }
                            MovementBuff = Mathf.Clamp(MovementBuff, 1f, 2f);

                            break;
                        }

                    case AbilityStatusEff.Root:
                        {
                            Rooted = false;
                            if (typeList != null && typeList.Count != 0)
                            {
                                Rooted = true;
                            }

                            break;
                        }

                    case AbilityStatusEff.Silence:
                        {
                            Silenced = false;
                            if (typeList != null && typeList.Count != 0)
                            {
                                Silenced = true;
                            }

                            break;
                        }

                    case AbilityStatusEff.Heal:
                        {
                            HealRate = 0f;
                            if (typeList != null)
                            {
                                foreach (AbilityEffect effect in typeList)
                                {
                                    HealRate += (effect.Value / effect.Duration);
                                }
                            }
                            HealRate = Mathf.Clamp(HealRate, 0f, 1f);

                            break;
                        }

                    case AbilityStatusEff.DamReduct:
                        {
                            DamageScale = 1f;
                            if (typeList != null)
                            {
                                foreach (AbilityEffect effect in typeList)
                                {
                                    DamageScale *= (1f - effect.Value);
                                }
                            }
                            DamageScale = Mathf.Clamp(DamageScale, 0f, 1f);
                            /*
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
                            */
                            break;
                        }

                    case AbilityStatusEff.DoT:
                        {
                            DamageRate = 0f;
                            if (typeList != null)
                            {
                                foreach (AbilityEffect effect in typeList)
                                {
                                    DamageRate += (effect.Value / effect.Duration);
                                }
                            }
                            DamageRate = Mathf.Clamp(DamageRate, 0f, 100f);
                            break;
                        }
                }
            }
        }

        public float GetEffectValue_Float(AbilityStatusEff type)
        {
            if (EffectLibrary.ContainsKey(type) && type != AbilityStatusEff.NoEffect)
            {
                switch (type)
                {
                    case AbilityStatusEff.CastTimeSlow:
                        {
                            return CastTimeScale;
                        }

                    case AbilityStatusEff.CastTimeBuff:
                        {
                            return CastTimeBuff;
                        }

                    case AbilityStatusEff.CooldownSlow:
                        {
                            return CooldownScale;
                        }

                    case AbilityStatusEff.MoveSlow:
                        {
                            return MovementScale;
                        }

                    case AbilityStatusEff.MoveBuff:
                        {
                            return MovementBuff;
                        }

                    case AbilityStatusEff.Heal:
                        {
                            return HealRate;
                        }

                    case AbilityStatusEff.DamReduct:
                        {
                            return DamageScale;
                        }

                    case AbilityStatusEff.DoT:
                        {
                            return DamageRate;
                        }
                }
            }
            return 1.0f;
        }

        public bool GetEffectValue_Bool(AbilityStatusEff type)
        {
            if (EffectLibrary.ContainsKey(type) && type != AbilityStatusEff.NoEffect)
            {
                switch (type)
                {

                    case AbilityStatusEff.Stun:
                        {
                            return Stunned;
                        }

                    case AbilityStatusEff.Root:
                        {
                            return Rooted;
                        }

                    case AbilityStatusEff.Silence:
                        {
                            return Silenced;
                        }
                    default:
                        {
                            return false;
                        }
                }
            }
            return false;
        }

        public void ClearEffects()
        {
            EffectLibrary.Clear();
            foreach (TweenBase tween in tweens)
            {
                tween.Cancel();
            }
            tweens.Clear();
            foreach (AbilityStatusEff type in typeof(AbilityStatusEff).GetEnumValues())
            {
                CalcEffects(type);
            }
        }

    }

    [System.Serializable]
    public abstract class AbilityObject : ScriptableObject
    {
        //This is the basic template for all abilites in the game. This container holds all the info we need to make the abilites actually activate.

        //Basic Ability Information
        [SerializeField] int index;
        public int Index { get { return index; } }
        [SerializeField] string abilityName = "Default Ability Name";                                        //Ability Name
        public string Name { get { return abilityName; } }

        [SerializeField] Sprite icon;                                                                        //Sprite Icon denoting the Ability
        public Sprite Icon { get { return icon; } }

        [TextArea]
        [SerializeField]
        string toolTip = ": This is the Default Ability tooltip. Please change me.";                         //Tooltip that explaines what the ability does.
        public string ToolTip { get { return toolTip; } }

        [Space(10)]
        [Header("Animation")]
        [SerializeField]
        string animTrigger = "DefaultTrigger";
        public string AnimTrigger { get { return animTrigger; } }                                            //Container for Animation Trigger string to activate the ability.

        [SerializeField]
        float animDelay = 0.0f;                                                                              //Container (in seconds) for how long to delay the animation trigger for a cast.
        public float AnimDelay { get { return animDelay; } }

        [SerializeField]
        float animTiming = 0.0f;                                                                             //Container (in seconds) for when to time the animation.
        public float AnimTiming { get { return animTiming; } }

        [Space(10)]
        [Header("VFX/SFX")]
        [SerializeField] ParticleSystem particleSystem;                                                      //Container for the Particle Effect System of the Ability.
        public ParticleSystem ParticleSystem { get { return particleSystem; } }

        [SerializeField] bool perCell;                                                                       //Boolean that determines if the particle is to be spawned per affected cell or just once.
        public bool PerCellInstantiation { get { return perCell; } }

        [SerializeField] Transform particleOrigin;                                                           //Container for the origin point of Particle Effect for Ability.
        public Transform ParticleOrigin { get { return particleOrigin; } }

        [SerializeField]
        float particleTiming = 0.0f;                                                                         //Container (in seconds) for when to time the vfx.
        public float ParticleTiming { get { return particleTiming; } }

        [SerializeField] SoundEffect soundEffect;                                                            //Sound Effect to play on activation.
        public SoundEffect SoundEffect { get { return soundEffect; } }

        [Space(10)]
        [SerializeField] List<AbilityEffect> statusEffects;                                                  //List of status effects, if any. Denoted by an Enumerable ID value that informs the ability what to affect the target entity with.
        public List<AbilityEffect> StatusEffects { get { return statusEffects; } }

        [SerializeField] AbilityType type;                                                                   //Denotes what kind of ability is being used, and who it affects. Uses an Enumerable ID value to inform the ability script.
        public AbilityType Type { get { return type; } }

        [SerializeField] Texture2D aoeSprite;                                                                //Tilemap that denotes the effective range of the ability. Not used in Melee, or Self abilites.
        public Texture2D AOESprite { get { return aoeSprite; } }

        [SerializeField] float range;                                                                        //For ranged abilities. Indicates the range of the ability.
        public float Range { get { return range; } }
        [SerializeField] float cooldown = 0.0f;                                                              //The base Cooldown timer (in seconds) of the ability.
        public float Cooldown { get { return cooldown; } }

        [SerializeField] float castTime = 0.0f;                                                              //The base Cast Timer (in seconds) it takes for the ability to activate.
        public float CastTime { get { return castTime; } }

        [SerializeField] int damage = 0;                                                                     //Initial damage value of the ability.
        public int Damage { get { return damage; } }

        [SerializeField] bool upgradeable;                                                                   //Boolean that denotes if the ability can be upgraded or not.
        public bool Upgradeable { get { return upgradeable; } }


        //Upgrade System Info.
        [SerializeField] AbilityObject baseAbility;
        public AbilityObject BaseAbility { get { return baseAbility; } }
        [SerializeField] int tier;                                                                           //Integer used to denote the level of the ability in the upgrade tree.
        public int Tier { get { return tier; } }

        [SerializeField] List<AbilityObject> previousTier;                                                         //Reference the the previous ability level of the tree, if any. Used for validation.
        public List<AbilityObject> PreviousTier { get { return previousTier; } }

        [SerializeField] List<AbilityObject> nextTier;                                                       //Reference to the next ability level of the tree, if any. Used for validation.
        public List<AbilityObject> NextTier { get { return nextTier; } }

        [SerializeField] int cost;                                                                         //The Core cost to upgrade the ability, if any.
        public int Cost { get { return cost; } }
    }

    public class Indicator
    {
        const int maxPoolSize = 100;
        static Stack<GameObject> enemyIndicatorInstancePool = new Stack<GameObject>();
        static Stack<GameObject> playerIndicatorInstancePool = new Stack<GameObject>();
        public GameObject Instance { get; set; }
        public Entity Entity { get; set; }
        public Cell Cell { get; set; }

        public static GameObject GetIndicatorFromEnemyPool()
        {
            GameObject poppedIndicator = (enemyIndicatorInstancePool.Count > 0) ? enemyIndicatorInstancePool.Pop() : null;
            if (poppedIndicator != null)
                poppedIndicator.SetActive(true);

            return poppedIndicator;
        }

        public static GameObject GetIndicatorFromPlayerPool()
        {
            GameObject poppedIndicator = (playerIndicatorInstancePool.Count > 0) ? playerIndicatorInstancePool.Pop() : null;
            if (poppedIndicator != null)
                poppedIndicator.SetActive(true);

            return poppedIndicator;
        }

        public void AddIndicator()
        {
            Entity.Indicators.Add(this);
            Cell.Indicators.Add(this);
        }

        public void RemoveIndicator()
        {
            if (Entity.IsPlayer)
            {
                if (playerIndicatorInstancePool.Count < maxPoolSize)
                {
                    playerIndicatorInstancePool.Push(Instance);
                    Instance.SetActive(false);
                }
                else
                {
                    GameObject.Destroy(Instance);
                }
            }
            else
            {
                if (enemyIndicatorInstancePool.Count < maxPoolSize)
                {
                    enemyIndicatorInstancePool.Push(Instance);
                    Instance.SetActive(false);
                }
                else
                {
                    GameObject.Destroy(Instance);
                }
            }
            Instance = null;
            Entity.Indicators.Remove(this);
            Cell.Indicators.Remove(this);
        }
    }
}
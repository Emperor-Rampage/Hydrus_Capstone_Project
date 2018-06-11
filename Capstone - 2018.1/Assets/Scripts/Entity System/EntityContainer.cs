using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityContainer : MonoBehaviour
{
    [SerializeField] Animator animator;
    public Animator Animator { get { return animator; } }
    [SerializeField] new SkinnedMeshRenderer renderer;
    public SkinnedMeshRenderer Renderer { get { return renderer; } }
    // [SerializeField] Material[] materials;
    // public Material[] Materials { get { return materials; } }

    [SerializeField] ParticleSystem stun;
    public ParticleSystem Stun { get { return stun; } }

    [SerializeField] ParticleSystem mslow;
    public ParticleSystem MSlow { get { return mslow; } }

    [SerializeField] ParticleSystem cslow;
    public ParticleSystem CSlow { get { return cslow; } }

    [SerializeField] ParticleSystem chaste;
    public ParticleSystem CHaste { get { return chaste; } }

    [SerializeField] ParticleSystem mhaste;
    public ParticleSystem MHaste { get { return mhaste; } }

    [SerializeField] ParticleSystem silence;
    public ParticleSystem Silence { get { return silence; } }

    [SerializeField] ParticleSystem nani;
    public ParticleSystem Nani { get { return nani; } }

    [SerializeField] ParticleSystem dot;
    public ParticleSystem DOT { get { return dot; } }

    [SerializeField] ParticleSystem root;
    public ParticleSystem Root { get { return root; } }

    [SerializeField] ParticleSystem heal;
    public ParticleSystem Heal { get { return heal; } }
}

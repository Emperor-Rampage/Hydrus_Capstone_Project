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
}

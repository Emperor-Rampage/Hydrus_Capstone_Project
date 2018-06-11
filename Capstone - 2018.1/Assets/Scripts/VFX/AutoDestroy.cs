using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    //Ultra small script that auto destroys the particle system on completion, for one shot particle effects.
    private ParticleSystem ps;

    public bool Interrupt = false;

    private Animator anim;

    public void DestroyThis()
    {
        ps.Stop();
        Destroy(gameObject);
       
    }

    public void Start()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null)
            DestroyThis();

        anim = GetComponentInParent<Animator>();
        if (anim == null)
            DestroyThis();

        gameObject.transform.parent = null;
    }

    public void Update()
    {
        if(anim != null)
            Interrupt = anim.GetBool("Interrupted");

        if (ps)
        {
            if (!ps.IsAlive())
            {
                DestroyThis();
            }
        }
        else if (Interrupt == true)
        {
            DestroyThis();
        }
        else
        {
            DestroyThis();
        }
    }
}


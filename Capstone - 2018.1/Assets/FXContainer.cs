using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityClasses;
using AudioClasses;
using ParticleClasses;
using AbilityClasses;

public class FXContainer : MonoBehaviour
{

    public ParticleSystem ps;

    public void PlayPS()
    {
        Instantiate(ps, this.gameObject.transform);
    }
    	
}

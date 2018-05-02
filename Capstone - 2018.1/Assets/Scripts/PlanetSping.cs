using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using UnityEngine;

public class PlanetSping : MonoBehaviour {

    public Transform planetTrans;

    private void Awake()
    {
        Tween.LocalRotation(planetTrans, new Vector3(0, 90, 0), 200, 0, loop: Tween.LoopType.Loop);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveHandler : MonoBehaviour {

    public Transform dissolveStart;
    public Transform dissolveEnd;

    private Material monsterMat;

	// Use this for initialization
	void Start ()
    {
        monsterMat = GetComponent<MeshRenderer>().material;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //monsterMat.SetVector("_DissolveStart", new Vector4(dissolveStart.position.x, dissolveStart.position.y, dissolveStart.position.z, 1));
        //monsterMat.SetVector("_DissolveEnd", new Vector4(dissolveEnd.position.x, dissolveEnd.position.y, dissolveEnd.position.z, 1));
    }
}

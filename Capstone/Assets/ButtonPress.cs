using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPress : MonoBehaviour {

    public ShakeTransform st;
    public CameraShakeObject data;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space))
        {
            //Camera.main.GetComponentInParent<ShakeTransform>().AddShakeEvent(data);
            st.AddShakeEvent(data);
            Debug.Log("Adding Shake Event");
        }
	}
}

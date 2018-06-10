using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accessor : MonoBehaviour, ISerializationCallbackReceiver
{

    public List<string> _keys = new List<string>();
    public List<Transform> _tran = new List<Transform>();

    public Dictionary<string, Transform> AccessDict = new Dictionary<string, Transform>();

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {

    }
    // Use this for initialization
    void Start ()
    {
        
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}

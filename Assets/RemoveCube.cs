using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveCube : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("collider");
        Destroy(other.gameObject);
    }

    // Update is called once per frame
    void Update () {
		
	}
}

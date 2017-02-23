using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadManager : MonoBehaviour {

    public Camera camera;
    GameObject[] allquads; 
	// Use this for initialization
	void Start () {
        allquads = GameObject.FindGameObjectsWithTag("billboard");
	}
	
	// Update is called once per frame
	void Update ()
    {
		foreach(GameObject qd in allquads)
        {
            qd.transform.LookAt(qd.transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
        }
	}
}

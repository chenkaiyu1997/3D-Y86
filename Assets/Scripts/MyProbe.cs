using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyProbe: MonoBehaviour {

    public Camera mycamera;
    public GameObject plane;
	// Update is called once per frame
	void Update () {
        transform.position = mycamera.transform.position + new Vector3(0, -2 * mycamera.transform.position.y , 0);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateLineManager : MonoBehaviour {

    TrailManager[] tms;
    public int stateorder;
    // Use this for initialization
    void Start () {
        tms = GetComponentsInChildren<TrailManager>();
        foreach (TrailManager i in tms)
        {
            i.stateColor = (i.myColor + stateorder) % 5;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

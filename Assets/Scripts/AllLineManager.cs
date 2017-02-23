using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllLineManager : MonoBehaviour {

    TrailManager[] tms;
	// Use this for initialization
	void Start () {
        tms = GetComponentsInChildren<TrailManager>();
    }
	// Update is called once per frame
	public void triggerLines () {
        Debug.Log("Triggerlines");
            Config.state = (Config.state + 1) % 5;
            foreach (TrailManager i in tms)
            {
                i.isactive = true;
            }
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delayer : MonoBehaviour {
    // Use this for initialization
    public Values manav;
    float ClockTime;
    float timer = 0;
    int clock_cnt;
    void Start () {
        ClockTime = Config.ClockTime * 1 / 5;
        timer = 1000;
	}
	public void delayUpdate(int tmp)
    {
        clock_cnt = tmp;
        ClockTime = Config.ClockTime * 1 / 5;
        timer = 0;
    }
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;

        if (timer > ClockTime + 0.05f && timer < ClockTime + 0.1f)
        {
            manav.allUpdate(clock_cnt);
        }
        if (timer > ClockTime + 0.1f && timer < ClockTime + 1)
        {
            int tmp = (int) ((timer - ClockTime - 0.1f) / 0.05f);
            Debug.Log("update" + tmp.ToString());
            manav.triggerUpdate(tmp);
        }
	}
}

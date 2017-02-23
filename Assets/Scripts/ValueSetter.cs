using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueSetter : MonoBehaviour {
    Text[] texts;
    public string key;
    public string value;
    public bool isBroken = false;
    public Image repair;
    public Values vals;
	// Use this for initialization
	void Start () {
        texts = GetComponentsInChildren<Text>();
        isBroken = false;
        repair.enabled = false;
    }
    public void myupdate()
    {
        if (isBroken == false)
            repair.enabled = false;
        else
            repair.enabled = true;
        Debug.Log(key);

        value = vals.value[key];
        //System.Reflection.PropertyInfo tmp = vals.GetType().GetProperty(key);
        //value = (string)tmp.GetValue(vals, null);
        Debug.Log(value);
        foreach (Text i in texts)
            i.text = key + "\n" + value;
    }
}

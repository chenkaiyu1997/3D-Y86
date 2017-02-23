using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System.Runtime.InteropServices;
using System;

public class Config : MonoBehaviour {
    public static float LineTime = 1f;
    public static float ClockTime = 5f;
    public static int state = 0;
    public static bool going = false;
    public static int clock_cnt = 0;
    public AllLineManager manal;
    public Delayer delayer;
    public Values manav;
    public Pipe pipe;

    // Use this for initialization

    public void Start()
    {
        ClockTime = 5f;
        LineTime = ClockTime / 5;
        state = 0;
        going = false;
        clock_cnt = 0;
        coolTimer = 0;
        currentTimer = 0;
    }
    public void mystart()
    {
        currentTimer = ClockTime;
        manav.allUpdate(0);
        for(int i = 0; i <= 10; i++)
            manav.triggerUpdate(i);
    }
    float currentTimer;
    float coolTimer;

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("goiing" + going.ToString());
        if (Input.GetButton("Openfile"))
        {
            OpenFileName ofn = new OpenFileName();
            ofn.structSize = Marshal.SizeOf(ofn);
            ofn.filter = "All Files\0*.*\0\0";
            ofn.file = new string(new char[256]);
            ofn.maxFile = ofn.file.Length;
            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = UnityEngine.Application.dataPath;
            ofn.title = "Open Project";
            ofn.defExt = "JPG"; 
            ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
            if (DllTest.GetOpenFileName(ofn))
                Debug.Log("Selected file with full path: {0}" + ofn.file);
            if(ofn.file.Length > 1)
                pipe.run(ofn.file);
        }
        float h = Input.GetAxisRaw("Clockspeed");
        if(h != 0f)
        {
            ClockTime += h / 10;
            LineTime = ClockTime / 5;
        }

        if (Input.GetButton("Pause"))
        {
            going = false;
            currentTimer = 0;
        }
        if(Input.GetButton("Go"))
        {
            going = true;
            currentTimer = 0;
        }
        if (Input.GetButton("Reload")){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if (going)
        {
            currentTimer += Time.deltaTime;
        }
        coolTimer += Time.deltaTime;
        if(currentTimer >= ClockTime || (coolTimer > ClockTime && Input.GetButton("Step")))
        {
            Debug.Log("call delay");
            if (clock_cnt > pipe.cycle_cnt) return;
            clock_cnt++;
            currentTimer = 0;
            manal.triggerLines();
            delayer.delayUpdate(clock_cnt);
        }
    }
}

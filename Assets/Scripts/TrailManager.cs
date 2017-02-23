using UnityEngine;
using System.Collections;

public class TrailManager : MonoBehaviour
{
    public Vector3[] points;
    public bool isactive;
    public float totTime;
    public int myColor;
    public int stateColor;
    int enabledColor;
    float v;
    float totDistance;
    float timer = 0;
    int stage;
    float[] times;
    float[] dis;
    float state;
    int len;
    int st;
    int et;
    float timer2;
    Vector3 movement;

    TrailRenderer tri;
    MeshRenderer mr;

    public void init(int tst, int tet)
    {
        st = tst;et = tet;
        totTime = Config.LineTime * (et - st);
        if (totTime == 0)
        {
            Debug.LogError("!!");
        }
        tri = GetComponentInChildren<TrailRenderer>();
        mr = GetComponentInChildren<MeshRenderer>();

        int pts = points.Length;
        len = pts - 1;
        dis = new float[pts];
        times = new float[pts];
        totDistance = 0;
        for (int i = 1; i < pts; i++)
        {
            dis[i] = Vector3.Distance(points[i], points[i - 1]);
            totDistance += dis[i];
        }
        v = totDistance / totTime;
        for (int i = 1; i <= len; i++)
            times[i] = dis[i] / v;
        stage = 0;
        transform.position = points[0];
        timer = 0;
        timer2 = 0;
    }
    void FixedUpdate()
    {
        enabledColor = Config.state;
        if (isactive == true)
        {
            timer2 += Time.deltaTime;
            if (timer2 < st * Config.LineTime) return;
            if(stateColor != enabledColor)
            {
                isactive = false;
                return;
            }
            tri.time = Config.LineTime;
            tri.enabled = true;
            mr.enabled = true;
            if (stage == 0) stage = 1;
            if (timer > times[stage] - Time.deltaTime / 2)
            {
                stage++;
                timer = 0;
            }
            if (stage > len)
            {
                isactive = false;
                return;
            }

            movement = points[stage] - transform.position;

            movement = movement.normalized * v * Time.deltaTime;
            transform.Translate(movement);

            timer += Time.deltaTime;
        }
        else
        {
            tri.time = 0;
            tri.enabled = false;
            mr.enabled = false;
            transform.position = points[0];
            stage = 0;
            timer = 0;
            timer2 = 0;
        }
    }
}
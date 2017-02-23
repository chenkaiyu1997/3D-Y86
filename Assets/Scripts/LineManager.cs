using UnityEngine;
using System.Collections;

public class LineManager : MonoBehaviour
{
    public int st;
    public int et;
    TrailManager[] tms;
    LineRenderer lr;
    ArrayList p = new ArrayList();
    void Start()
    {
        Debug.Log("LINE Checking!");
        tms = GetComponentsInChildren<TrailManager>();
        lr = GetComponentInChildren<LineRenderer>();
        p.Clear();
        Transform[] ptrans = GetComponentsInChildren<Transform>();
        foreach (Transform child in ptrans)
            if (child.CompareTag("Pos"))
                p.Add(child.position);
        foreach (TrailManager tm in tms)
        {
            tm.points = (Vector3[])p.ToArray(typeof(Vector3));
            lr.SetPositions(tm.points);
            tm.init(st, et);
        }
    }
}
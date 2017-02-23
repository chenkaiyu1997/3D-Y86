using UnityEngine;
using System.Collections;

public class UIStaticManager : MonoBehaviour
{


    public GameObject robot;
    public Camera camera;
    private Vector3 roboPos;
    private RectTransform rt;
    private Vector3 roboScreenPos;

    // Use this for initialization
    void Start()
    {
        roboPos = robot.transform.position;

        rt = GetComponent<RectTransform>();
        roboScreenPos = camera.WorldToViewportPoint(robot.transform.TransformPoint(roboPos));
        rt.anchorMax = roboScreenPos;
        rt.anchorMin = roboScreenPos;
    }

    // Update is called once per frame
    void Update()
    {
        roboPos = robot.transform.position;
        roboScreenPos = camera.WorldToViewportPoint(robot.transform.TransformPoint(roboPos));
        rt.anchorMax = roboScreenPos;
        rt.anchorMin = roboScreenPos;
    }
}
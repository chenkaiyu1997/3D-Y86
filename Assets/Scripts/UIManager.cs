using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{


    public GameObject robot;
    public Camera camera;
    private Vector3 roboPos;
    private RectTransform rt;
    private Vector3 roboScreenPos;
    float timer = 0;

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

        timer += 1f;
        rt.transform.rotation = Quaternion.Euler(0, 0, timer);
    }
}
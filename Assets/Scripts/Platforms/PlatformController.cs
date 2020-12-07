using System;
using System.Collections;
using System.Collections.Generic;
using BaseAI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlatformController : MonoBehaviour
{
    public Tuple<Vector3, Vector3> GlobalZones;

    public List<GameObject> Zones;

    public float AcrossTime;
    
    [SerializeField] private bool moving;
    [FormerlySerializedAs("rotationSpeed")] [SerializeField] public float Speed = 1.0f;
    [SerializeField] private Text timeText;
    
    private int radius = 10;

    private Vector3 rotationCenter;
    private Vector3 startPosition;

    public float TimeAllCircle = 1;

    private bool isCalculatingTime = false;
    void Awake()
    {
        rotationCenter = transform.position + 10 * Vector3.left;
        if (Zones.Count >= 2)
            GlobalZones = new Tuple<Vector3, Vector3>(Zones[0].transform.position, Zones[1].transform.position);
    }

    private void Start()
    {
        //timeText.text = Math.Round(((2 * Math.PI * radius) / rotationSpeed), 2).ToString();
        var v = Math.Round(((2 * Math.PI * radius) / Speed), 2);
        var w = v / radius;
        timeText.text = Math.Round(w, 2).ToString();

        startPosition = transform.position;
        StartCoroutine(SetFlagInTime(1));
    }

    private IEnumerator SetFlagInTime(int time)
    {
        yield return new WaitForSeconds(time);
        isCalculatingTime = true;
        TimeAllCircle = 1;
    }

    void Update()
    {
        if (!moving) 
            return;
        var lastpos = transform.position;
        transform.RotateAround(rotationCenter, Vector3.up, Time.deltaTime*Speed);
        if (Round(transform.position) != Round(startPosition) && isCalculatingTime)
            TimeAllCircle += Time.deltaTime;
        else
            isCalculatingTime = false;
    }
    
    //This returns the angle in radians
    public static float AngleInRad(Vector3 vec1, Vector3 vec2) {
        return Mathf.Atan2(vec2.y - vec1.y, vec2.x - vec1.x);
    }
 
    //This returns the angle in degrees
    public static float AngleInDeg(Vector3 vec1, Vector3 vec2) {
        return AngleInRad(vec1, vec2) * 180 / Mathf.PI;
    }
    
    private Vector3 Round(Vector3 transformPosition) => new Vector3(Mathf.Round(transformPosition.x),
        Mathf.Round(transformPosition.y),  Mathf.Round(transformPosition.z));
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            other.transform.SetParent(transform);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            other.transform.SetParent(null);
    }

    public PathNode GetRotatedPoint(PathNode node, float timeDelta)
    {
        var rotationSpeed = Speed;
        Vector3 dir = node.Position - rotationCenter;
        return new PathNode()
        {
            Position = rotationCenter + Quaternion.AngleAxis(-rotationSpeed * timeDelta, Vector3.up) * dir,
            Direction = Quaternion.AngleAxis(-rotationSpeed * timeDelta, Vector3.up) * node.Direction
        };
    }
}

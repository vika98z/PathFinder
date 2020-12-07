using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorProperties : MonoBehaviour
{
    public List<Transform> Neighbours = new List<Transform>();
    public List<Vector3> Vector3Neighbours = new List<Vector3>();
    
    public Transform Enter;
    public Transform Exit;
    
    public int Index;

    public bool ContainsFinish;
    public Transform Finish;
    private void Awake()
    {
        foreach (var neighbour in Neighbours)
        {
            Vector3Neighbours.Add(neighbour.position);
        }
    }
    
    public void SwapEnterExit()
    {
        var temp = Enter;
        Enter = Exit;
        Exit = temp;
    }

    public float AcrossTime()
    {
        var dist = Vector3.Distance(Enter.position, Exit.position);
        return dist / BotMovement.Speed;
    }
}

using System;
using System.Collections.Generic;
using BaseAI;
using UnityEngine;
using Priority_Queue;

public class LocalPathPlaner: MonoBehaviour
{
    //список зон, которые надо пройти до финиша по порядку (PathZones[0] - та, в которой стартуем)
    public List<GameObject> PathZones = new List<GameObject>();
    public GameObject Bot;
    
    [SerializeField] private Transform finishTransform;
    
    private float step = 1f;
    private bool isInit = false;
    private List<Vector3> checkedPositions = new List<Vector3>();
    private int _countOfChildPoints = 16;
    private int depth = 30;
    
    public List<PathNode> FinalList = new List<PathNode>();
    private GameObject localFinishPosition;
    public bool IsWalking = false;
    private bool isFindingExit = true;

    private GameObject LastZone;

    private bool isFinish = false;
    
    public void Initialize(List<PathNode> finalList, List<GameObject> zones, Transform botTrnsm)
    {
        finalList.Reverse();

        for (var i = 0; i < finalList.Count; i++)
        {
            var node = finalList[i];
            foreach (var zone in zones)
            {
                if (zone.transform.position == node.Position)
                {
                    PathZones.Add(zone);
                    break;
                }
            }
        }

        isInit = true;
        LastZone = PathZones[0];
        //CalculatePath();
    }
    
    private void Update()
    {
        if (isInit)
        {
            if (PathZones.Count > 0)
            {
                if (!IsWalking)
                {
                    if (isFindingExit)
                    {
                        IsWalking = true;
                        isFindingExit = false;
                        if (LastZone.GetComponent<SectorProperties>().ContainsFinish)
                        {
                            localFinishPosition = PathZones[0].GetComponent<SectorProperties>().Finish.gameObject;
                            CalculatePath();
                            isFinish = true;

                            PathZones.Clear();
                        }
                        else
                        {
                            localFinishPosition = PathZones[0].GetComponent<SectorProperties>().Exit.gameObject;
                            CalculatePath();
                            LastZone = PathZones[0];
                            PathZones.RemoveAt(0);
                        }
                    }
                    else
                    {
                        if (PathZones.Count > 0)
                        {
                            IsWalking = true;
                            isFindingExit = true;

                            if (LastZone.GetComponent<SectorProperties>().ContainsFinish)
                            {
                                localFinishPosition = PathZones[0].GetComponent<SectorProperties>().Finish.gameObject;
                                CalculatePath();
                                isFinish = true;
                                PathZones.Clear();
                            }
                            else
                            {
                                localFinishPosition = PathZones[0].GetComponent<SectorProperties>().Enter.gameObject;
                                LastZone = PathZones[0];
                                CalculatePath();
                            }
                        }
                    }

                    
                }
            }
            else if (PathZones.Count == 0 && !isFinish && !IsWalking)
            {
                localFinishPosition = finishTransform.gameObject;
                isFinish = true;
                CalculatePath();
                isFinish = true;
            }
        }
    }
    
    private List<PathNode> GetNeighbours(PathNode current)
    {
        List<PathNode> nodes = new List<PathNode>();
        var incr = -(_countOfChildPoints / 2);
        for (int i = 0; i < _countOfChildPoints; i++)
        {
            for (int z = -3; z < 4; z++)
            {
                Vector3 childPoint = Round(new Vector3(current.Position.x + incr, current.Position.y, current.Position.z + z));
                if (!checkedPositions.Contains(childPoint))
                {
                    var node = new PathNode(childPoint);
                    nodes.Add(node);
                    checkedPositions.Add(childPoint);
                }
            }

            incr++;
        }
        return nodes;
    }
    
    private void CheckWalkableNode(PathNode node)
    {
        node.IsWalkable = true;

        var cols = Physics.OverlapSphere(node.Position, 1);
        foreach (var col in cols)
        {
            if (col.tag != "Sector" && col.tag != "Player" && col.tag != "Portal" && col.tag != "Floor")
            {
                node.IsWalkable = false;
            }
        }
    }

    public void CalculatePath()
    {
        FinalList.Clear();
        checkedPositions.Clear();

        if (!localFinishPosition)
            return;
        IsWalking = true;

        var start = new PathNode(Round(Bot.transform.position));
        start.Dist = 0;

        FastPriorityQueue<PathNode> nodes = new FastPriorityQueue<PathNode>(3000);
        PriorityQueue<PathNode> finalNodes = new PriorityQueue<PathNode>();
        nodes.Enqueue(start, 0);

        var FinishNode = new PathNode(Round(localFinishPosition.transform.position));

        int curdepth = 0;
        while (nodes.Count != 0)
        {
            PathNode current = nodes.Dequeue();
            var neighbours = GetNeighbours(current);

            var flcur = Round(current.Position);
            var flfin = Round(FinishNode.Position);
                
            if (Round(current.Position) == Round(FinishNode.Position))
                break;
            
            foreach (var node in neighbours)
            {
                node.IsChecked = true;
                CheckWalkableNode(node);
                if (!node.IsWalkable)
                {
                    if (checkedPositions.Contains(node.Position))
                        checkedPositions.Remove(node.Position);
                    continue;
                }

                float cost = current.FlooredDistance(node);
                cost += Heuristic(node, FinishNode);
                
                if (cost < node.Dist)
                {
                    node.Dist = cost;
                    node.Parent = current;
                    nodes.Enqueue(node, cost);
                    finalNodes.Enqueue(node, cost);
                }
            }
            
            curdepth++;
        }

        var pathElem = finalNodes.Peek();
        FinalList = new List<PathNode>();
        while (pathElem != null)
        {
            FinalList.Add(pathElem);
            pathElem = pathElem.Parent;
        }

        bool isSecondCheck = false;
        int beginRemove = 0, endRemove = 0;
        for (int i = 0; i < FinalList.Count - 1; i++)
        {
            if (!isSecondCheck)
            {
                if (FinalList[i].IsAboveTheGround() && !FinalList[i + 1].IsAboveTheGround())
                {
                    beginRemove = i + 1;
                    isSecondCheck = true;
                }
            }
            else
            {
                if (!FinalList[i].IsAboveTheGround() && FinalList[i + 1].IsAboveTheGround())
                {
                    endRemove = i;
                    break;
                }
            }
        }

        for (int i = beginRemove + 1; i < endRemove; i++)
            FinalList.RemoveAt(beginRemove + 1);
        FinalList.Reverse();

        Bot.GetComponent<BotMovement>().SetLocalPath(FinalList, this); 
        //Debug.DrawLine(node.Position, next.Position, Color.blue, 10f);
        for (int i = 0; i < FinalList.Count - 1; i++)
        {
            Debug.DrawLine(FinalList[i].Position, FinalList[i + 1].Position, Color.green, 10f);
        }
    }

    private Vector3 Round(Vector3 transformPosition) => new Vector3(Mathf.Round(transformPosition.x),
        Mathf.Round(transformPosition.y),  Mathf.Round(transformPosition.z));

    private void OnDrawGizmos()
    {
        foreach (var p in checkedPositions)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(p, 0.3f);
        }
    }

    public float Heuristic(PathNode a, PathNode b)
    {
        Vector3 indA = Round(a.Position), indB = Round(b.Position);
        
        return Vector3.Distance(indA, indB);
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using BaseAI;
using Priority_Queue;

public class GlobalPathPlaner : MonoBehaviour
{
    [SerializeField] private List<GameObject> wayPoints;
    [SerializeField] private Transform botTransform;
    [SerializeField] private List<GameObject> sectors;
    [SerializeField] private List<PlatformController> platforms;
    [SerializeField] private Transform finishTransform;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private GameObject startSector;
    [SerializeField] private LocalPathPlaner localPlaner;

    public FastPriorityQueue<PathNode> wayPointsQueue = new FastPriorityQueue<PathNode>(3000);
    public FastPriorityQueue<PathNode> sectorsQueue = new FastPriorityQueue<PathNode>(3000);
    
    private PathNode startNode = null;
    private List<Vector3> checkedPositions = new List<Vector3>();
    
    void Start()
    {
        foreach (var item in wayPoints)
        {
            var node = new PathNode(item.transform.position);
            wayPointsQueue.Enqueue(node, node.Distance(botTransform.position));
        }
        
        startNode = new PathNode(startSector.transform.position);
        CalculatePath();
    }

    private List<PathNode> GetNeighboursSectors(PathNode current)
    {
        List<PathNode> nodes = new List<PathNode>();
        var tempCur = current;
        foreach (var node in sectors)
        {
            if (checkedPositions.Contains(node.transform.position))
                continue;
            
            var sectorProp = node.GetComponent<SectorProperties>();
            if (sectorProp)
            {
                if (sectorProp.Vector3Neighbours.Contains(current.Position))
                    nodes.Add(new PathNode(node.transform.position));
                else
                {
                    foreach (var platform in platforms)
                    {
                        if ((platform.GlobalZones.Item1.Equals(current.Position) &&
                             platform.GlobalZones.Item2.Equals(node.transform.position))
                            || (platform.GlobalZones.Item2.Equals(current.Position) &&
                                platform.GlobalZones.Item1.Equals(node.transform.position)))
                        {
                            nodes.Add(new PathNode(node.transform.position));
                        }
                    }
                }
            }
        }

        return nodes;
    }

    public void CalculatePath()
    {
        PathNode start = startNode;

        start.Dist = 0;

        FastPriorityQueue<PathNode> nodes = new FastPriorityQueue<PathNode>(3000);
        PriorityQueue<PathNode> finalNodes = new PriorityQueue<PathNode>();
        nodes.Enqueue(start, 0);

        var FinishNode = new PathNode(finishTransform.position);

        while (nodes.Count != 0)
        {
            PathNode current = nodes.Dequeue();
            
            var neighbours = GetNeighboursSectors(current);

            foreach (var node in neighbours)
            {
                node.IsChecked = true;

                //float cost = Heuristic(node, FinishNode);
                
                //float cost = current.FlooredDistance(node);
                float cost = Heuristic(node, current);

                if (cost < node.Dist)
                {
                    checkedPositions.Add(node.Position);
                    node.Dist = cost;
                    node.Parent = current;
                    nodes.Enqueue(node, cost);
                    finalNodes.Enqueue(node, cost);
                }
            }
        }

        var pathElem = finalNodes.Peek();
        var finalList = new List<PathNode>();
        while (pathElem != null)
        {
            finalList.Add(pathElem);
            
            pathElem = pathElem.Parent;
        }
        
        //SwapEnterExit
        if (Round(finalList[finalList.Count - 2].Position) != Round(sectors[3].transform.position))
        {
            foreach (var sector in sectors)
            {
                sector.GetComponent<SectorProperties>().SwapEnterExit();
            }
        }

        for (int i = 0; i < sectors.Count - 1; i++)
        {
            var n1 = new PathNode(sectors[i].transform.position);
            var n2 = new PathNode(sectors[i+1].transform.position);
            var h = Heuristic(n1, n2);
            print(sectors[i].name + " + " + sectors[i+1].name + " - heurist = " + h);
        }
        
        var nn1 = new PathNode(sectors[0].transform.position);
        var nn2 = new PathNode(sectors[sectors.Count-1].transform.position);
        var hh = Heuristic(nn1, nn2);
        print(sectors[0].name + " + " + sectors[sectors.Count-1].name + " - heurist = " + hh);

        localPlaner.Initialize(finalList, sectors, botTransform);
    }
    
    private Vector3 Round(Vector3 transformPosition) => new Vector3(Mathf.Round(transformPosition.x),
        Mathf.Round(transformPosition.y),  Mathf.Round(transformPosition.z));

    /*public float Heuristic(PathNode a, PathNode b)
    {
        int indA = 0, indB = 0;
        foreach (var sector in sectors)
        {
            if (sector.transform.position == a.Position)
                indA = sector.GetComponent<SectorProperties>().Index;
            if (sector.transform.position == b.Position)
                indB = sector.GetComponent<SectorProperties>().Index;
        }
        
        return Mathf.Abs(indA - indB);
    }*/
    
    public float Heuristic(PathNode a, PathNode b)
    {
        float indA = 0, indB = 0;
        GameObject aSector = sectors[0];
        GameObject bSector = sectors[0];
        foreach (var sector in sectors)
        {
            if (sector.transform.position == a.Position)
            {
                indA = sector.GetComponent<SectorProperties>().AcrossTime();
                aSector = sector;
            }

            if (sector.transform.position == b.Position)
            {
                indB = sector.GetComponent<SectorProperties>().AcrossTime();
                bSector = sector;
            }
        }
        
        var res = indA + indB;
        foreach (var platform in platforms)
        {
            if (platform.GlobalZones.Item1.Equals(a.Position) &&
                 platform.GlobalZones.Item2.Equals(b.Position))
            {
                float time = Vector3.Distance(a.Position, aSector.GetComponent<SectorProperties>().Exit.position) / BotMovement.Speed;
                var distTime = platform.AcrossTime / 2f + time;
                res += distTime;
            }

            if (platform.GlobalZones.Item2.Equals(a.Position) &&
                platform.GlobalZones.Item1.Equals(b.Position))
            {
                float time = Vector3.Distance(b.Position, aSector.GetComponent<SectorProperties>().Exit.position) / BotMovement.Speed;
                var distTime = platform.AcrossTime / 2f + time;
                res += distTime;
            }
        }

        return res;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enable KDTree usage
using DataStructures.ViliWonka.KDTree;

// Enable Helpers
using Entity.Utilities;

public class newDiffGrow : MonoBehaviour
{
    // Editor Input
    [SerializeField] float growthRate = 2f, desiredDistance = 0.8f, maxDistance = 1, kdSearchRadius = 0.8f;
    [SerializeField] [Range(0f, 1)] float attractionForce = 0.5f, repulsionForce = 0.5f, alignmentForce = 0.5f, forceAmount = 1f;
    [SerializeField] [Range(0, 6)] int steps = 2;
    [SerializeField] [Range(0, 1f)] float stepDiv = 0;
    [SerializeField][Range(0f, 0.01f)] float debugScale = 0.001f;
    [SerializeField] bool loop = false, customGrowthPattern, debug = false;
    [SerializeField] bool stopGrowth;

    // Output
    public List<int> nodeID;
    public KDTree nodes;

    // Variables
    int canvasResolution = 4096;
    private float stepBarrier;
    private KDQuery query;
    bool growthRunning;
    private int stopQ;
    int initialSteps;

    // Start is called before the first frame update
    void Start()
    {
        query = new KDQuery();
        InitKDTree(InitStartCircle(400, 32));
    }

    // Update is called once per frame
    void Update()
    {
        // Node manangement loop
        for (int i = 0; i < nodes.Count; i++)
        {
            Vector3 dispSum = AttractionForceOnPoint(i, desiredDistance, attractionForce);
            dispSum += RepulsionForceOnPoint(i, repulsionForce);
            dispSum += AlignmentForceOnPoint(i, alignmentForce);
            nodes.Points[i] = Vector3.Lerp(nodes.Points[i], nodes.Points[i] + dispSum, forceAmount);
            SplitEdges(i);
            
        }
        nodes.Rebuild();
        
        // Debug Area //
        if ((debug == true) && (Input.GetKeyDown("space")))
        {
            Debug.Log("KDTree //nodes contains " + nodes.Count + " (" + nodes.Points.Length + ") node(s)."  );
        }

        if (debug == true)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Vector3 centerOnTex = new Vector3 (canvasResolution / 2, canvasResolution /2, 0); // bring in front of texture
                Debug.DrawLine((nodes.Points[i] - centerOnTex) * debugScale, (nodes.Points[(i + 1) % nodes.Count] - centerOnTex) * debugScale, Color.white);
            }
        }
    }

    Vector3[] InitStartCircle(float radius, int verts)
    {
        // Settings
        Vector3[] shape = new Vector3[verts];
        Vector3 origin = new Vector3(canvasResolution / 2, canvasResolution / 2, 0);

        // Init
        float a = 2 * Mathf.PI;
        float angleInc = a / verts;
        Vector3 point;

        for (int i = 0; i < verts; i++)
        {
            point.x = origin.x + Mathf.Cos(a) * radius;
            point.y = origin.y + Mathf.Sin(a) * radius;
            point.z = origin.z;
            shape[i] = point;
            a = a + angleInc;
        }
        return shape;
    }

    void InitKDTree(Vector3[] firstShape)
    {
        nodes = new KDTree(firstShape, 32); // KDTree Default 32

        // INIT NODE ID MANAGEMENT
        nodeID = new List<int>();
        for (int i = 0; i < firstShape.Length; i++)
        {
            nodeID.Insert(i, i);
        }
        /////////////////////

        // Init coroutines
        StartCoroutine(RandomGrowth(growthRate));
    }
    
    // Node Injection / Growth
    IEnumerator RandomGrowth (float growthRate)
    {
        growthRunning = true;
        initialSteps = steps;
        while(growthRunning == true)
        {
            for (int i = 0; i < steps; i++)
            {
                Subdivide(EdgeStartingAt(Random.Range(0,nodes.Count)));
            }
            stepBarrier += stepDiv;
            
            if (stepBarrier > 1 && steps > stopQ)
            {
                steps -= 1;
                stepBarrier = 0;
            }
            if (steps == 0) growthRunning = false;
            yield return new WaitForSeconds(growthRate);
        }
        if (debug == true) print("growth ended");
    }

    Vector3 AttractionForceOnPoint(int index, float desiredDistance, float force)
    {
        float distance2next;
        float distance2prev;
        Vector3 currentNode = nodes.Points[index];
        Vector3 diff2next = Vector3.zero;
        Vector3 diff2prev = Vector3.zero;

        if (loop == true)
        {
            distance2next = Vector3.Distance(currentNode, nodes.Points[(index + 1) % nodes.Count]);
            if (distance2next > desiredDistance)
            {
                Vector3 newPoint = Vector2.Lerp(currentNode, nodes.Points[(index + 1) % nodes.Count], force);
                diff2next = newPoint - currentNode;
            }
            distance2prev = Vector3.Distance(currentNode, nodes.Points[Utils.mod((index - 1), nodes.Count)]);
            if (distance2prev > desiredDistance)
            {
                Vector3 newPoint = Vector2.Lerp(currentNode, nodes.Points[Utils.mod((index - 1), nodes.Count)], force);
                diff2prev = newPoint - currentNode;
            }
        }
        else if (index + 1 < nodes.Count)
        {
            distance2next = Vector3.Distance(currentNode, nodes.Points[index + 1]);
            if (distance2next > desiredDistance)
            {
                Vector3 newPoint = Vector2.Lerp(currentNode, nodes.Points[index + 1], force);
                diff2next = newPoint - currentNode;
            }
        }
        else if (index - 1 >= 0)
        {
            distance2prev = Vector3.Distance(currentNode, nodes.Points[index - 1]);
            if (distance2prev > desiredDistance)
            {
                Vector3 newPoint = Vector2.Lerp(currentNode, nodes.Points[index - 1], force);
                diff2prev = newPoint - currentNode;
            }
        }
        Vector3 displacement = diff2next + diff2prev;
        
        return displacement;
    }

    Vector3 RepulsionForceOnPoint(int index, float force)
    {
        /*
        Vector3 newPoint = nodes.Points[index];

        for (int i = 0; i < resultIndices.Count; i++)
        {
            newPoint = Vector2.LerpUnclamped(newPoint, nodes.Points[resultIndices[i]], force);
        }
        Vector3 displacement = newPoint - nodes.Points[index];
        */

        force = -force; // repulsion through negative lerp factor -> unclamped
        var resultIndices = findInRadiusKDTree(index, kdSearchRadius);

        Vector3 displacement = Vector3.zero;
        
        for (int i = 0; i < resultIndices.Count; i++)
        {
            displacement += nodes.Points[resultIndices[i]] - nodes.Points[index];
        }
        if (debug)
        {
            Vector3 centerOnTex = new Vector3 (canvasResolution / 2, canvasResolution /2, 0);
            Debug.DrawLine((nodes.Points[index] - centerOnTex) * debugScale, (displacement * force + nodes.Points[index] - centerOnTex) * debugScale, Color.cyan);
        }
        return displacement * force;
    }

    Vector3 AlignmentForceOnPoint(int index, float force)
    {
        Vector3 displacement = Vector3.zero;
        Vector3 currentNode = nodes.Points[index];

        if (loop == true)
        {
            Vector3 midPoint = (nodes.Points[Utils.mod((index - 1),nodes.Count)] + nodes.Points[(index + 1) % nodes.Count]) / 2;
            if (debug)
            {
                Vector3 centerOnTex = new Vector3 (canvasResolution / 2, canvasResolution /2, 0);
                Debug.DrawLine((currentNode - centerOnTex) * debugScale, (midPoint - centerOnTex) * debugScale, Color.magenta);
            }
            Vector3 newPoint = Vector2.Lerp(currentNode, midPoint, force);
            displacement = newPoint - currentNode;
        }
        else if (index - 1 >= 0 && index + 1 < nodes.Count )
        {
            Vector3 midPoint = (nodes.Points[index - 1] + nodes.Points[index + 1]) / 2;
            if (debug)
            {
                Vector3 centerOnTex = new Vector3 (canvasResolution / 2, canvasResolution /2, 0);
                Debug.DrawLine((currentNode - centerOnTex) * debugScale, (midPoint - centerOnTex) * debugScale, Color.magenta);
            }
            Vector3 newPoint = Vector2.Lerp(currentNode, midPoint, force);
            displacement = newPoint - currentNode;
        }

        return displacement;
    }

    void SplitEdges(int index)
    {
        if (loop == true)
        {
            float distance = Vector3.Distance(nodes.Points[index], nodes.Points[Utils.mod((index + 1),nodes.Count)]);
            if (distance > maxDistance)
            {
                Subdivide(EdgeStartingAt(index));
            }
        }
        else if (index - 1 >= 0)
        {
            float distance = Vector3.Distance(nodes.Points[index], nodes.Points[index + 1]);
            if (distance > maxDistance)
            {
                Subdivide(EdgeStartingAt(index));
            }
        }
    }

    edge EdgeStartingAt(int index)
    {
        int end = Utils.mod((index + 1), nodes.Count);
        edge thisedge = new edge (index, end);
        return thisedge;
    }

    struct edge
    {
        public int start;
        public int end;
        public edge (int start, int end)
        {
            this.start = start;
            this.end = end;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // KD TREE / ID - MANAGEMENT //////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////
    List<int> findInRadiusKDTree(int index, float radius)
    {
        var resultIndices = new List<int>();
        query.Radius(nodes, nodes.Points[index], radius, resultIndices);
        return resultIndices;
    }

    void Subdivide(edge edge)
    {
        Vector3 midPoint = (nodes.Points[edge.start] + nodes.Points[edge.end]) / 2;
        InjectNodeToKDTree(midPoint, edge.end); // Should this be start or end?
    }

    void InjectNodeToKDTree(Vector3 point, int index)
    {
        int count = nodes.Count;
        int newCount = count + 1;

        Vector3[] shiftBuffer = new Vector3[newCount - index];
        int [] nodeIDShiftbuffer = new int[newCount - index];  // NODE ID MANAGEMENT -> Mirroring Shiftbuffer
        
        // Load new point into buffer
        shiftBuffer[0] = point;
        nodeIDShiftbuffer[0] = index; // NODE ID MANAGEMENT -> Store Index of "point"

        // Load shifted points from KD Tree behind new points into buffer
        for (int i = 1, j = index; j < count; i++, j++)
        {
            shiftBuffer[i] = nodes.Points[j];
            nodeIDShiftbuffer[i] = nodeID[j]; // NODE ID MANAGEMENT -> Fill shiftbuffer
        }

        nodes.SetCount(newCount);
        nodeID.RemoveRange(index, count - index); // NODE ID MANAGEMENT -> Clear overwritten area

        // Write buffer into KD Tree
        for (int i = index, j = 0; j < shiftBuffer.Length; i++, j++)
        {
            nodes.Points[i] = shiftBuffer[j];
            nodeID.Add(nodeIDShiftbuffer[j]); // NODE ID MANAGEMENT -> update from shiftbuffer storage
        }
    }
}

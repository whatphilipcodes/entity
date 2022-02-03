using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enable KDTree usage
using DataStructures.ViliWonka.KDTree;

// Enable Helpers
using Seed.Utilities;

public class differentialGrowth_old : MonoBehaviour
{
    ////////////////////////////////////////
    // Personal Notes //
    ////////////////////////////////////////
    
    // gameObject = current object, GameObject = class name

    // wrap loop indices exceeding array length (tested on for) using modulo [%] operator ! See wrappedIndexTest.cs
    // use cutom Utils.mod() method to handle negative [-i] indices ! See helperFunctions.cs

    // The Debug class is very useful when developing complex vector calculations
    // see: https://docs.unity3d.com/ScriptReference/Debug.html

    // lerp can be used in negative direction (Jason Webb -> https://github.com/jasonwebb/2d-differential-growth-experiments)

    ///////////////////////////////////////

    // Editor Input
    [SerializeField]
    int circleStartVerts = 8;

    [SerializeField]
    float circleRadius = 2, growthRate = 2f, desiredDistance = 0.8f, maxDistance = 1, attractionForce = 1,
    minDistance = 0.8f, repulsionForce = 1, kdSearchRadius = 0.8f, debugScale = 100;

    [SerializeField]
    bool skipNeighbor = true, includeZ = false, debug = false;

    // KDTree Setting; Default is 32
    int maxPointsPerLeafNode = 32;

    // Public Vars
    public static KDTree nodes;

    // Private Vars
    private KDQuery query;
    private LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        //Init main
        InitKDTree(InitStartCircle(0,0,0,circleRadius,circleStartVerts));
        line = gameObject.GetComponent<LineRenderer>();
        query = new KDQuery();

        //Check Resources
        if (line == null)
        {
            Debug.LogError("Please add Line Renderer to GameObject in Editor");
        }

        //Init coroutines
        StartCoroutine(Growth(growthRate));
    }

    // Node injection/growth
    IEnumerator Growth (float growthRate)
    {
        while(true)
        {
            //print("coroutine iteration started");
            SubdivideTarget (Random.Range(0,nodes.Count));
            yield return new WaitForSeconds(growthRate);
        }
    }

    // Update is called once per frame
    void Update()
    {
        RenderLine();

        // Node manangement loop
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes.Points[i] += RepulsionForceOnPoint(i, minDistance, repulsionForce, skipNeighbor);
            nodes.Points[i] += AttractionForceOnPoint(i, desiredDistance, attractionForce);
        }
        nodes.Rebuild();

        /*
        // New node creation
        if (Input.GetMouseButton(0))
        {
            SubdivideTarget (Random.Range(0,nodes.Count));
        }
        */

        // Debug area
        if ((debug == true) && (Input.GetKeyDown("space")))
        {
            Debug.Log("KDTree //nodes contains " + nodes.Count + " (" + nodes.Points.Length + ") node(s)."  );
        }
    }

    Vector3[] InitStartCircle(float x, float y, float z, float radius, int verts)
    {
        //Settings
        Vector3[] shape = new Vector3[verts];
        Vector3 origin = new Vector3(x,y,z);

        //Init
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
        nodes = new KDTree(firstShape, maxPointsPerLeafNode);

        if (debug == true)
        {
            Debug.Log("KDTree //nodes now contains " + nodes.Count + " node(s)");
        }
    }

    void RenderLine()
    {
        line.positionCount = nodes.Points.Length;
        for (int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, nodes.Points[i]);
        }
    }

    Vector3 AttractionForceOnPoint(int index, float desiredDistance, float scaleFactor)
    {
        //if (debug == true) Debug.DrawLine(nodes.Points[index], nodes.Points[(index + 1) % nodes.Count], Color.green);

        //Vector3 currentToNext = nodes.Points[(index + 1) % nodes.Count] - nodes.Points[index];
        float distance = Vector3.Distance(nodes.Points[(index + 1) % nodes.Count],nodes.Points[index]);

        float amount = (distance - desiredDistance) * scaleFactor;
        if (distance > desiredDistance)
        {
            if (distance > maxDistance)
            {
            //Vector3 newPoint = nodes.Points[index] + currentToNext / 2;
            //InjectNodeToKDTree(newPoint, index);
            SubdivideTarget(index);
            }
            Vector3 currentToNext = nodes.Points[(index + 1) % nodes.Count] - nodes.Points[index];
            return currentToNext.normalized * amount;
        } else {
            return new Vector3();
        }
    }

    Vector3 RepulsionForceOnPoint(int index, float minDistance, float scaleFactor , bool skipNeighbor)
    {
        var resultIndices = findInRadiusKDTree(index, kdSearchRadius);
        float forceSum = 0f;
        Vector3 directionSum = new Vector3();

        for (int i = 0; i < resultIndices.Count; i++)
        {
             Vector3 currentDirection = new Vector3();
            if (skipNeighbor == true && resultIndices[i] != Utils.mod((index - 1), nodes.Count) && resultIndices[i] != (index + 1) % nodes.Count)
            {
                if (debug == true) Debug.DrawLine(nodes.Points[index], nodes.Points[resultIndices[i]], Color.cyan);
                currentDirection = -(nodes.Points[resultIndices[i]] - nodes.Points[index]);
                float currentDistance = currentDirection.magnitude;
                if (currentDistance < minDistance)
                {
                    if (includeZ == true) directionSum = Vector3.Slerp(directionSum, currentDirection, 0.5f);
                    else directionSum = Vector2.Lerp(directionSum, currentDirection, 0.5f);
                    forceSum += minDistance - currentDistance;
                }
            } else if (skipNeighbor == false) {
                if (debug == true) Debug.DrawLine(nodes.Points[index], nodes.Points[resultIndices[i]], Color.cyan);
                currentDirection = -(nodes.Points[resultIndices[i]] - nodes.Points[index]);
                float currentDistance = currentDirection.magnitude;
                if (currentDistance < minDistance)
                {
                    if (includeZ == true) directionSum = Vector3.Slerp(directionSum, currentDirection, 0.5f);
                    else directionSum = Vector2.Lerp(directionSum, currentDirection, 0.5f);
                    forceSum += minDistance - currentDistance;
                }
            }
        }
        forceSum -= minDistance;
        Vector3 resultForce = new Vector3();
        resultForce = directionSum.normalized * (forceSum * scaleFactor);
        if (debug == true) Debug.DrawRay(nodes.Points[index], resultForce * debugScale, Color.red);
        return resultForce;
    }

    List<int> findInRadiusKDTree(int index, float radius)
    {
        var resultIndices = new List<int>();
        query.Radius(nodes, nodes.Points[index], radius, resultIndices);
        return resultIndices;
    }

    void SubdivideTarget(int splitIndex)
    {
        int nextIndex = (splitIndex + 1) % nodes.Count; // catches values greater than nodes.Count and restarts at index 0
        //print(splitIndex + " / " + nextIndex);
        //Vector3 currentToNext = nodes.Points[nextIndex] - nodes.Points[splitIndex];
        //Vector3 midPoint = nodes.Points[splitIndex] + currentToNext.normalized * (currentToNext.magnitude/2);
        Vector3 midPoint = (nodes.Points[splitIndex] + nodes.Points[nextIndex]) / 2;
        if (debug == true) Debug.DrawLine(new Vector3(0,0,0), midPoint, Color.magenta, 1f);
        InjectNodeToKDTree(midPoint, nextIndex);
    }

    void InjectNodeToKDTree(Vector3 point, int nextIndex)
    {
        int oldCount = nodes.Count;
        Vector3[] shiftBuffer = new Vector3[oldCount + 1 - nextIndex];
        nodes.SetCount(oldCount + 1);

        // Load new point into buffer
        shiftBuffer[0] = point;
        // Load shifted points from KD Tree behind new points into buffer
        for (int i = 1, j = nextIndex; i < shiftBuffer.Length; i++, j++)
        {
            shiftBuffer[i] = nodes.Points[j];
        }
        // Write buffer into KD Tree
        for (int i = nextIndex, j = 0; i < nodes.Count; i++, j++)
        {
            nodes.Points[i] = shiftBuffer[j];
        }
        nodes.Rebuild();

        if (debug == true)
        {
            Debug.Log("KDTree //nodes now contains a new node " + point + " at index " + nextIndex + ". There are " + nodes.Count + " node(s) in total.");
        }
    }
}
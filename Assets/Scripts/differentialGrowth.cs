using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enable KDTree usage
using DataStructures.ViliWonka.KDTree;

// Enable Helpers
using Seed.Utilities;

public class differentialGrowth : MonoBehaviour
{
    ////////////////////////////////////////
    // Notes on general stuff //
    ////////////////////////////////////////
    
    // gameObject = current object, GameObject = class name

    // wrap loop indices exceeding array length (tested on for) using modulo [%] operator ! See wrappedIndexTest.cs
    // use cutom Utils.mod() method to handle negative [-i] indices ! See helperFunctions.cs

    // The Debug class is very useful when developing complex vector calculations
    // see: https://docs.unity3d.com/ScriptReference/Debug.html

    ///////////////////////////////////////

    // Editor Input
    public int circleStartVerts = 8;
    public float circleRadius = 2;
    public float maximumDistance = 0.8f;
    public float minimumDistance = 0.8f;
    public float searchRadius = 0.8f;
    public int maxPointsPerLeafNode = 32; // KDTree Balance; Default is 32
    public bool debug;

    // Global Vars
    KDTree nodes;
    KDQuery query;

    LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        //Init
        initKDTree(InitStartCircle(0,0,0,circleRadius,circleStartVerts));
        line = gameObject.GetComponent<LineRenderer>();
        query = new KDQuery();

        //Check Resources
        if (line == null)
        {
            Debug.LogError("Please add Line Renderer to GameObject in Editor");
        }
    }

    // Update is called once per frame
    void Update()
    {
        AttractiveNodes(0.005f, maximumDistance);
        RenderLine();

        for (int i = 0; i < nodes.Count; i++)
        {
            Vector3 force = RepulsionForceOnPoint(i, 0.01f);
            nodes.Points[i]+=force;
        }
        nodes.Rebuild();

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 force = RepulsionForceOnPoint(0, 0.01f);
            nodes.Points[0]+=force;
        }

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

    void initKDTree(Vector3[] firstShape)
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

    void AttractiveNodes(float scaleFactor, float desiredDisance)
    {
        Vector3 currentToNext;
        float distance;
        float amount;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (i == nodes.Count - 1)
            {
                currentToNext = nodes.Points[0] - nodes.Points[i];
            } else {
                currentToNext = nodes.Points[i + 1] - nodes.Points[i];
            }
            distance = currentToNext.magnitude;
            amount = (distance - maximumDistance) * scaleFactor;
            if (distance != maximumDistance)
            {
                nodes.Points[i] = nodes.Points[i] + currentToNext.normalized * amount;
            }
            if (debug == true) Debug.DrawRay(nodes.Points[i], currentToNext, Color.red, .1f);
        }
        nodes.Rebuild();
    }

    Vector3 RepulsionForceOnPoint(int index, float scaleFactor)
    {
        var resultIndices = findInRadiusKDTree(index, searchRadius);
        //foreach ( var x in resultIndices) Debug.Log( x.ToString());

        float forceSum = 0f;
        Vector3 directionSum = new Vector3();

        for (int i = 0; i < resultIndices.Count; i++)
        {
            Vector3 currentDirection = -(nodes.Points[resultIndices[i]] - nodes.Points[index]);
            float currentDistance = currentDirection.magnitude;
            if (currentDistance < minimumDistance)
            {
                directionSum += currentDirection;
                forceSum += minimumDistance - currentDistance;
            }
            //Debug.DrawRay(nodes.Points[index], -currentDirection, Color.blue)
        }
        //Debug.DrawRay(nodes.Points[index], forceSum * scaleFactor, Color.green, 5f);
        forceSum -= minimumDistance;
        //print(forceSum);
        Vector3 resultForce = new Vector3();
        resultForce = (nodes.Points[index] + directionSum).normalized * (forceSum * scaleFactor);
        //Debug.DrawRay(nodes.Points[index], resultForce, Color.cyan, 1f);
        return resultForce;
    }

    List<int> findInRadiusKDTree(int index, float radius)
    {
        var resultIndices = new List<int>();
        query.Radius(nodes, nodes.Points[index], radius, resultIndices);
        return resultIndices;
    }

    void SubdivideTarget(int splitIndex) // must work for 0 - nodes.Count as splitIndex
    {
        int nextIndex = (splitIndex + 1) % nodes.Count; // catches values equal to nodes.Count
        print(splitIndex + " / " + nextIndex);
        Vector3 currentToNext = nodes.Points[nextIndex] - nodes.Points[splitIndex];
        Vector3 midPoint = nodes.Points[splitIndex] + currentToNext.normalized * (currentToNext.magnitude/2);
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

    void addNodesToKDTree(Vector3[] points)
    {
        int oldCount = nodes.Count;
        nodes.SetCount(oldCount + points.Length);
        for (int i = oldCount, j = 0; i < nodes.Points.Length; i++, j++)
        {
            nodes.Points[i] = points[j];
        }
        nodes.Rebuild();

        if (debug == true)
        {
            Debug.Log("KDTree //nodes now contains new nodes at index " + oldCount + ". There are " + nodes.Count + " node(s) in total.");
        }
    }

    /*
    void InjectNodesToKDTree(Vector3 point, int splitIndex)
    {
        int oldCount = nodes.Count;
        Vector3[] shiftBuffer = new Vector3[oldCount + points.Length - splitIndex];
        nodes.SetCount(oldCount + points.Length);

        // Load new points into buffer
        for (int i = 0; i < points.Length; i++)
        {
            shiftBuffer[i] = points[i];
        }
        // Load shifted points from KD Tree after new points into buffer
        for (int i = points.Length, j = splitIndex; i < shiftBuffer.Length; i++, j++)
        {
            shiftBuffer[i] = nodes.Points[j];
        }
        // Write buffer into KD Tree
        for (int i = splitIndex, j = 0; i < nodes.Count; i++, j++)
        {
            nodes.Points[i] = shiftBuffer[j];
        }
        nodes.Rebuild();

        if (debug == true)
        {
            Debug.Log("KDTree //nodes now contains " + points.Length + " new node(s) at index " + splitIndex + ". There are " + nodes.Count + " node(s) in total.");
        }
    }

    void testRadiusFinder(int index)
    {
            var test = findInRadiusKDTree(index, searchRadius);
            foreach ( var x in test)
            {
                Debug.Log( x.ToString());
            }
    }
    
        void RepulsiveNodes(float scaleFactor, float maximumDistance, float queryRadius)
    {
        float distance;
        Vector3 currentToNext;

        Vector3[] distanceCheckPoints = null;
        for (int i = 0; i < nodes.Count; i++)
        {
            var resultIndices = new List<int>();
            query.Radius(nodes, nodes.Points[0], queryRadius, resultIndices); // In KDTree: Search for points within given radius
            if (resultIndices == null) return; // Skip if there are no points in radius proximity
            distanceCheckPoints = new Vector3[resultIndices.Count]; // Create array to write results into
            for (int j = 0; j < resultIndices.Count; j++)
            {
                distanceCheckPoints[j] = nodes.Points[resultIndices[j]]; // Write points into array
                currentToNext = distanceCheckPoints[(j + 1) % resultIndices.Count] - nodes.Points[i];
                distance = currentToNext.magnitude;
                if (distance < maximumDistance)
                {
                    nodes.Points[i] = nodes.Points[i] + (-currentToNext.normalized) * (maximumDistance - distance);
                    Debug.DrawRay(nodes.Points[i], currentToNext, Color.red, 1f);
                }
            }
        }
        nodes.Rebuild();
    }
    */
}
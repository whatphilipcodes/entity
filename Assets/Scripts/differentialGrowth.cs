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
    // use cutom Utils.mod() method for negative [-i] indices ! See helperFunctions

    ///////////////////////////////////////

    // Editor Input
    public int circleStartVerts = 8;
    public float desiredDistance= 0.8f;
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
        line = gameObject.GetComponent<LineRenderer>();

        //Check Resources
        if (line == null)
        {
            Debug.LogError("Please add Line Renderer to GameObject in Editor");
        }
        initKDTree(InitStartCircle(0,0,0,1,circleStartVerts));
    }

    // Update is called once per frame
    void Update()
    {
        AttractiveNodes(0.005f);
        RenderLine();
        if (Input.GetMouseButtonDown(0))
        {
            SubdivideTarget(Random.Range(0,nodes.Count));
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

    void AttractiveNodes(float scaleFactor)
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
            amount = (distance - desiredDistance) * scaleFactor;
            if (distance != desiredDistance)
            {
                nodes.Points[i] = nodes.Points[i] + currentToNext.normalized * amount;
            }
        }
        nodes.Rebuild();
    }

    void RepulsiveNodes(float scl)
    {

    }

    List<int> findInRadiusKDTree(Vector3 point, float radius)
    {
        var resultIndices = new List<int>();
        query.Radius(nodes, point, radius, resultIndices);
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
    } */
}
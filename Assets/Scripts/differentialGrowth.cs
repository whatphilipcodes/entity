using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enables KDTree usage
using DataStructures.ViliWonka.KDTree;

public class differentialGrowth : MonoBehaviour
{
    ////////////////////////////////////////
    // Notes on general stuff //
    ////////////////////////////////////////
    
    // gameObject = aktuelles Objekt, GameObject = Name der Klasse
    //
    //
    //

    ///////////////////////////////////////

    // Editor Input
    public int circleStartVerts = 8;
    public float dDesired = 0.8f;
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
        //AttractiveNodes(0.001f);
        RenderLine();
        if (Input.GetMouseButtonDown(0))
        {
            //SubdivideTarget(1);
            Vector3[] test = new Vector3 [1];
            test[0] = new Vector3(0,0,0);
            injectNodesToKDTree(test, 2);
        }

        // Debug area
        if ((debug == true) && (Input.GetKeyDown("space")))
        {
            Debug.Log("KDTree //nodes contains " + nodes.Count + " (" + nodes.Points.Length + ") nodes."  );
        }
    }
    Vector3[] InitStartCircle(float x, float y, float z, float radius, int verts)
    {
        //Settings
        Vector3[] shape = new Vector3[verts];
        Vector3 origin = new Vector3(x,y,z);

        //Init
        float a = 2*Mathf.PI;
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

    void AttractiveNodes(float scl)
    {
        Vector3 ND;
        float d;
        float amt;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (i == nodes.Count-1)
            {
                ND = nodes.Points[0]-nodes.Points[i];
            } else {
                ND = nodes.Points[i+1]-nodes.Points[i];
            }
            d = ND.magnitude;
            amt = (d - dDesired) * scl;
            if (d != dDesired)
            {
                nodes.Points[i] = nodes.Points[i] + ND.normalized * amt;
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

    void SubdivideTarget(int sIndex)
    {
        Vector3[] tmp = new Vector3[1];
        Vector3 PtoQ = nodes.Points[sIndex + 1] - nodes.Points[sIndex];
        tmp[0] = nodes.Points[sIndex] + PtoQ.normalized * (PtoQ.magnitude/2);
        injectNodesToKDTree(tmp, sIndex);
    }

    void injectNodesToKDTree(Vector3[] points, int splitIndex)
    {
        int oldCount = nodes.Count;
        Vector3[] shiftBuffer = new Vector3[oldCount + points.Length - splitIndex];
        print("shiftBuffer.Length = " + shiftBuffer.Length);
        nodes.SetCount(oldCount + points.Length);
        print("new Nodes.Count = " + nodes.Count + " (old Nodes.Count = " + oldCount);

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
            Debug.Log("KDTree //nodes now contains new nodes at index " + splitIndex + ". There are " + nodes.Count + " node(s) in total.");
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
}
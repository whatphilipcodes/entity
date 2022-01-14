using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DataStructures.ViliWonka.KDTree;

public class learnKD : MonoBehaviour
{
    public int maxPointsPerLeafNode = 32; // KDTree Balance; Default is 32
    public bool debug;
    KDTree nodes;
    KDQuery queryDistance;

    // Testing only variables
    int p;
    List<int> idx;

    // Start is called before the first frame update
    void Start()
    {
        initKDTree(createRect());
        queryDistance = new KDQuery();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            p = addNodeToKDTree(createRandomPoint());
        }
        if (Input.GetKey("space"))
        {
            idx = findInRadiusKDTree(nodes.Points[p], 1);
            print(idx.Count + " point(s) in radius.");
            for(int i = 0; i < idx.Count; i++)
            {
                 print(idx[i]);
            }
        }
    }

    Vector3[] createRect()
    {
        Vector3[] shape = new Vector3[4];
        shape[0] = new Vector3(-1, 1, 0);
        shape[1] = new Vector3( 1, 1, 0);
        shape[2] = new Vector3( 1,-1, 0);
        shape[3] = new Vector3(-1,-1, 0);
        return shape;
    }
    Vector3 createRandomPoint()
    {
        return new Vector3(Random.Range(-1f, 1), Random.Range(-1f, 1), 0);
    }

    Vector3 createTestPoint()
    {
        return new Vector3(1,2,3);
    }

    void initKDTree(Vector3[] firstShape)
    {
        nodes = new KDTree(firstShape, maxPointsPerLeafNode);

        if (debug == true)
        {
            Debug.Log("KDTree //nodes now contains " + nodes.Count + " point(s)");
        }
    }

    int addNodeToKDTree(Vector3 point)
    {
        int oldCount = nodes.Count;
        nodes.SetCount(oldCount + 1); // Creates space for new Point by increasing the count by one
        nodes.Points[oldCount] = point; // Places point into newly created space (array counting starts from 0 but count skips 0)
        nodes.Rebuild();

        if (debug == true)
        {
            Debug.Log("KDTree //nodes now contains node " + nodes.Points[oldCount] + " at index " + oldCount + ". There are " + nodes.Count + " node(s) in total.");
        }

        return oldCount;
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

    List<int> findInRadiusKDTree(Vector3 point, float radius)
    {
        var resultIndices = new List<int>();
        queryDistance.Radius(nodes, point, radius, resultIndices);
        return resultIndices;
    }
}

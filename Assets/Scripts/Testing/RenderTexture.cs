using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTexture : MonoBehaviour
{

    Vector3[] points;
    // Start is called before the first frame update
    void Start()
    {
        points = new Vector3 [10];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector3(Random.Range(-2,2),Random.Range(-2,2),0);
            };
            print("points now contains " + points.Length + " point(s).");
        }
    }
}

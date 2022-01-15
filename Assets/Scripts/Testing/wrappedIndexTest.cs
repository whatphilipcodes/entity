using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wrappedIndexTest : MonoBehaviour
{
    int[] output = new int[24];
    int[] loop = new int[3];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = 0;
        }

        loop[0] = 9;
        loop[1] = 4;
        loop[2] = 5;
     }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int x = 1; // startIndex of loop
            // wrapped index test
            for (int i = 0, j; i < output.Length; i++)
            {
                j = (i + x) % loop.Length;
                output[i] = loop[j];
            }

            // output
            foreach (int output in output)
            {
                Debug.Log(output);
            }
        }
    }
}

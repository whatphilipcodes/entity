using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class readTree : MonoBehaviour
{
    [SerializeField] differentialGrowth diffGrow;
    Vector2[] testArray;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        testArray = new Vector2 [diffGrow.nodes.Count];
        for (int i = 0; i < testArray.Length; i++)
        {
            testArray[i] = diffGrow.nodes.Points[i];
        }
    }
}

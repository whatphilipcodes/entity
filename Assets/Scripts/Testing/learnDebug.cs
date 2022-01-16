using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class learnDebug : MonoBehaviour
{
    Vector3 origin;
    Vector3 direction;
    // Start is called before the first frame update
    void Start()
    {
        origin = new Vector3(0,0,0);
        direction = new Vector3(1,1,1);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(origin, direction, Color.red, .5f);
        direction.x = direction.x + 0.01f;
    }
}

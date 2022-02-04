using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coordinateTranslator : MonoBehaviour
{
    private float uv_width, uv_height, uv_triangle_side, uv_triangle_height;
    private Vector2 fixed_origin, direction_down, direction_up;

    [SerializeField]
    private bool debug = false;

    [SerializeField]
    private Vector2 origin = new Vector2(0f,0f);

    [SerializeField]
    private int diagonal_divisions = 1200; //step size for alternative coordinate system, should translate into aprox 1 pixel on texture (1365 with height of 4096)

    // Start is called before the first frame update
    void Start()
    {
        uv_height = uv_width = 1f;
        uv_triangle_side = 2f / 11f;
        uv_triangle_height = Mathf.Sqrt(3f) / 11f;
        if(debug == true) print("width " + uv_width + " | height " + uv_height + " | tri_side " + uv_triangle_side + " | tri_height " + uv_triangle_height);

        fixed_origin = new Vector2 (origin.x, origin.y + 2f * uv_triangle_side);
        direction_up = (new Vector2 (uv_triangle_side / 2f, uv_triangle_height)).normalized * (uv_triangle_side / diagonal_divisions);
        direction_down = (new Vector2 (uv_triangle_side / 2f, - uv_triangle_height)).normalized * (uv_triangle_side / diagonal_divisions);

        if(debug == true)
        {
            Debug.DrawLine(fixed_origin, fixed_origin +  direction_up, Color.yellow, 100f);
            Debug.DrawLine(fixed_origin, fixed_origin + direction_down, Color.magenta, 100f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

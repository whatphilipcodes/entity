using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class runCRT : MonoBehaviour
{
    [SerializeField] CustomRenderTexture _texture;

    //Mouse Draw Test Vars
    [SerializeField] Material _updateMaterial;
    public Camera _camera;
    private RaycastHit _hit;

    // Start is called before the first frame update
    void Start()
    {;

        // CRT
        _texture.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        // Mouse Draw Test
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out _hit))
            {
                _updateMaterial.SetVector("_Coordinate", new Vector4(_hit.textureCoord.x, _hit.textureCoord.y, 0, 0));
                //print(_hit.textureCoord);
            }
        }

        // Updates the CRT
        _texture.Update();
    }
}

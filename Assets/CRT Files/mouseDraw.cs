using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseDraw : MonoBehaviour
{
    public Camera _camera;
    public Shader _drawShader;

    //private RenderTexture _splatmap;
    private RaycastHit _hit;
    private Material /*_surfaceMaterial,*/ _drawMaterial;
    // Start is called before the first frame update
    void Start()
    {
        _drawMaterial = new Material (_drawShader);
        _drawMaterial.SetColor("_Color", Color.red);

        //_surfaceMaterial = GetComponent<MeshRenderer>().material;
        //_splatmap = new RenderTexture(1024,1024,0,RenderTextureFormat.ARGBFloat);
        //_surfaceMaterial.SetTexture("_Splat", _splatmap);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out _hit))
            {
                _drawMaterial.SetVector("_Coordinate", new Vector4(_hit.textureCoord.x, _hit.textureCoord.y, 0, 0));
                /*
                RenderTexture temp = RenderTexture.GetTemporary(_splatmap.width, _splatmap.height, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(_splatmap, temp);
                Graphics.Blit(temp, _splatmap, _drawMaterial);
                RenderTexture.ReleaseTemporary(temp);
                */
            }
        }
    }
    /*
    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0,0,256,256), _splatmap, ScaleMode.ScaleToFit, false, 1);
    }
    */
}

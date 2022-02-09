using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drawTest : MonoBehaviour
{
    public Camera _camera;
    public CustomRenderTexture _splatmap;

    public Material _drawMaterial;

    private RaycastHit _hit;

    [Range(1, 100)]
    public float _brushSize = 1f;
    [Range(0, 10)]
    public float _brushStrength = 1f;

    private readonly float m_GUIsize = 512;
    private readonly int m_RenderTexSize = 1024;

    void Start()
    {
        _splatmap = new CustomRenderTexture(m_RenderTexSize, m_RenderTexSize, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
        {
            name = "splatmap_CRT_generated",
            initializationColor = Color.blue,
            initializationSource = CustomRenderTextureInitializationSource.Material,
            initializationMaterial = _drawMaterial,
            material = _drawMaterial,
            doubleBuffered = true,
            updateMode = CustomRenderTextureUpdateMode.OnDemand
        };

        _drawMaterial.SetVector("_DrawColor", Color.red);
        _drawMaterial.SetTexture("_SplatMap", _splatmap);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out _hit, 100f))
            {
                _drawMaterial.SetVector("_InputPoint", new Vector4(_hit.textureCoord.x, _hit.textureCoord.y, 0, 0));
                _drawMaterial.SetFloat("_BrushStrength", _brushStrength);
                _drawMaterial.SetFloat("_BrushSize", _brushSize);
            }
        }
    }


    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, m_GUIsize, m_GUIsize), _splatmap, ScaleMode.StretchToFill, false, 1);
    }
}

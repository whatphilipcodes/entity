using UnityEngine;
using System.Collections;

public class runCompute : MonoBehaviour
{
    // UserInput
    [SerializeField] int texResolution = 1024;
    [SerializeField] ComputeShader shader;
    [SerializeField] Material target;
    [SerializeField] Color lineColor = new Color(1,1,1,1);
    [SerializeField] Vector2 pointA = new Vector2 (0.2f, 0.2f);
    [SerializeField] Vector2 pointB = new Vector2 (0.8f, 0.8f);
    [SerializeField] float thickness = 0.1f;
    [SerializeField] float crispness = 0f;

    // ShaderData
    
    int dispatchSet;
    int renderHandle;
    RenderTexture outputTexture;

    // BufferData
    ComputeBuffer buffer;
    Vector2[] testData;
    
    // Use this for initialization
    void Start()
    {
        testData = new Vector2[4];
        testData[0] = new Vector2 (0.2f, 0.2f);
        testData[1] = new Vector2 (0.2f, 0.8f);
        testData[2] = new Vector2 (0.8f, 0.8f);
        testData[3] = new Vector2 (0.8f, 0.2f);

        outputTexture = new RenderTexture(texResolution, texResolution, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.filterMode = FilterMode.Point;
        outputTexture.Create();

        InitData();
        InitShader();
    }

    void Update()
    {
        DispatchKernel(dispatchSet);
    }

    private void OnDestroy()
    {
        buffer.Dispose();
    }

    private void InitData()
    {   // Gets GroupSize for dispatch call
        renderHandle = shader.FindKernel("renderCanvas");

        uint threadGroupSizeX;
        shader.GetKernelThreadGroupSizes(renderHandle, out threadGroupSizeX, out _, out _);

        dispatchSet = (int) (texResolution / threadGroupSizeX);
        //print(dispatchSet);
    }

    private void InitShader()
    {
        shader.SetVector( "_lcol", lineColor );
        shader.SetFloat( "_thick", thickness);
        shader.SetFloat( "_crisp", crispness );
        shader.SetVector( "_a", pointA );
        shader.SetVector( "_b", pointB );
        shader.SetInt( "texResolution", texResolution );

        int stride = 2 * 4; //2 floats position vector - 4 bytes per float
        buffer = new ComputeBuffer(testData.Length, stride);
        buffer.SetData(testData);
        shader.SetBuffer(renderHandle, "positionsBuffer", buffer);

        shader.SetTexture( renderHandle, "Result", outputTexture );
        target.SetTexture("_MainTex", outputTexture); // sets output render texture into target material
    }

    private void DispatchKernel(int set)
    {
        shader.Dispatch(renderHandle, set, set, 1);
    }
}
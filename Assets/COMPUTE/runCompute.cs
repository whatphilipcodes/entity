using UnityEngine;
using System.Collections;

public class runCompute : MonoBehaviour
{
    // UserInput
    [SerializeField] int texResolution = 1024;
    [SerializeField] ComputeShader shader;
    [SerializeField] Material target;
    [SerializeField] Color pointColor = new Color(1,1,1,1);

    // ShaderData
    int dispatchSet;
    int renderHandle;
    RenderTexture outputTexture;
    ComputeBuffer buffer;
    Vector2[] pointsData;


    // Use this for initialization
    void Start()
    {
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

        pointsData = new Vector2[10000];

        //test setup
        for (int i = 0; i < pointsData.Length; i++)
        {
            pointsData[i] = new Vector2(Random.Range(0,texResolution), Random.Range(0,texResolution));
        }
        print(pointsData[9999]);
    }

    private void InitShader()
    {
        shader.SetVector( "_pcol", pointColor );
        shader.SetInt( "texResolution", texResolution );

        int stride = (2) * 4; //2 floats in each vector - 4 bytes per float
        buffer = new ComputeBuffer(pointsData.Length, stride);
        buffer.SetData(pointsData);
        shader.SetBuffer(renderHandle, "pointsBuffer", buffer);

        shader.SetTexture( renderHandle, "Result", outputTexture );
        target.SetTexture("_MainTex", outputTexture); // sets output render texture into target material
    }

    private void DispatchKernel(int set)
    {
        shader.Dispatch(renderHandle, set, set, 1);
    }
}
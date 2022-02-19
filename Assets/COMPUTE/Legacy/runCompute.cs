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

    public differentialGrowth diffGrowth;


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

        pointsData = new Vector2[6400];

        //print(diffGrowth.nodes.Points[0]);
        //test setup
        for (int i = 0; i < 6400; i++)
        {
            pointsData[i] = new Vector2(Random.Range(0,texResolution), Random.Range(0,texResolution));
            //pointsData[i] = new Vector2(diffGrowth.nodes.Points[i].x,diffGrowth.nodes.Points[i].y);
        }
        //print(pointsData[6399]);
    }

    private void InitShader()
    {
        shader.SetVector( "_pcol", pointColor );
        shader.SetInt( "texResolution", texResolution );

        int stride = (3) * 4; //2 floats in each vector - 4 bytes per float
        buffer = new ComputeBuffer(4096, stride);
        //buffer.SetData(null);
        shader.SetBuffer(renderHandle, "pointsBuffer", buffer);

        shader.SetTexture( renderHandle, "Result", outputTexture );
        target.SetTexture("_MainTex", outputTexture); // sets output render texture into target material
    }

    private void DispatchKernel(int set)
    {   /*
        for (int i = 0; i < diffGrowth.nodes.Count; i++)
        {
            //pointsData[i] = new Vector2(Random.Range(0,texResolution), Random.Range(0,texResolution));
            pointsData[i] = new Vector2(diffGrowth.nodes.Points[i].x,diffGrowth.nodes.Points[i].y);
        } */
        buffer.SetData(diffGrowth.nodes.Points);
        shader.Dispatch(renderHandle, set, set, 1);
    }
}
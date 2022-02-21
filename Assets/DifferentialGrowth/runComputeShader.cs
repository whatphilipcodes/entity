using UnityEngine;
using System.Collections;

public class runComputeShader : MonoBehaviour
{
    // UserInput
    [SerializeField] Color pointColor = new Color(1,1,1,1);
    [SerializeField] int texResolution = 1024;
    [SerializeField] ComputeShader shader;
    [SerializeField] Material target;

    [SerializeField] RenderTexture settingRef;

    // TrailTesting
    [Range(0f, 1f)] public float decay = 0.00122f;

    // ShaderData
    int pointsHandle;
    int trailsHandle;
    RenderTexture outputTexture;
    ComputeBuffer pointsBuffer;
    ComputeBuffer colorsBuffer;

    // Script References
    public differentialGrowth diffGrowth;
    public getColors getCol;


    // Use this for initialization
    void Start()
    {
        // Create Texture
        /*
        outputTexture = new RenderTexture(texResolution, texResolution, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.filterMode = FilterMode.Trilinear;
        */
        outputTexture = new RenderTexture(settingRef);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        // INIT

        // set handles for ease of use
        pointsHandle = shader.FindKernel("DrawPoints");
        trailsHandle = shader.FindKernel("DrawTrails");

        // send variables into shader
        shader.SetVector( "_pcol", pointColor );
        shader.SetInt("_texres", texResolution);
        shader.SetInt("_colres", getCol.colorAmount);
        shader.SetFloat("_decay", decay);

        // buffer setup
        int stride = (3) * 4; // every component as a float (3) * 4 bytes per float
        pointsBuffer = new ComputeBuffer(8192, stride);

        stride = (4) * 4;
        colorsBuffer = new ComputeBuffer(getCol.colorAmount, stride);

        // 
        shader.SetTexture( pointsHandle, "Result", outputTexture ); // inputs texture to shader
        shader.SetTexture( trailsHandle, "Result", outputTexture );
        target.SetTexture("_MainTex", outputTexture); // sets output render texture into target material
    }

    void Update()
    {
        /*
        // update compute shader
        pointsBuffer.SetData(diffGrowth.nodes.Points);
        InitColors();
        shader.SetBuffer(pointsHandle, "pointsBuffer", pointsBuffer);
        shader.Dispatch(pointsHandle, 128, 1, 1);
        shader.Dispatch(trailsHandle, 256, 256, 1);
        */
    }

    private void OnDestroy()
    {
        if (pointsBuffer != null) pointsBuffer.Dispose();
        if (colorsBuffer != null) colorsBuffer.Dispose();
    }

    void InitColors()
    {
        colorsBuffer.SetData(getCol.results);
        shader.SetBuffer(pointsHandle, "colorsBuffer", colorsBuffer);
    }
}

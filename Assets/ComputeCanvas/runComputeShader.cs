using UnityEngine;
using System.Collections;

public class runComputeShader : MonoBehaviour
{
    // UserInput
    [SerializeField] int texResolution = 1024;
    [SerializeField] ComputeShader shader;
    [SerializeField] Material target;

    [SerializeField] RenderTexture settingRef;

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
        outputTexture = new RenderTexture(settingRef);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        // INIT

        // set handles for ease of use
        pointsHandle = shader.FindKernel("DrawPoints");
        trailsHandle = shader.FindKernel("DrawTrails");

        // send variables into shader
        shader.SetInt("_texres", texResolution);
        shader.SetInt("_colres", getCol.colorAmount);

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
        // update compute shader
        pointsBuffer.SetData(diffGrowth.nodes.Points);
        InitColors();
        shader.SetBuffer(pointsHandle, "pointsBuffer", pointsBuffer);
        shader.Dispatch(pointsHandle, 128, 1, 1);
        shader.Dispatch(trailsHandle, 256, 256, 1);
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


// using UnityEngine;
// using System.Collections;

// public class runComputeShader : MonoBehaviour
// {
//     // UserInput
//     [SerializeField] int texResolution = 4096;
//     [SerializeField] ComputeShader shader;
//     [SerializeField] Material target;

//     [SerializeField] RenderTexture settingRef;

//     // ShaderData
//     int pointsHandle;
//     int trailsHandle;
//     int clearHandle;
//     RenderTexture outputTexture;
//     ComputeBuffer pointsBuffer;
//     ComputeBuffer colorsBuffer;

//     // Script References
//     public differentialGrowth diffGrowth;
//     //public getColors getCol;
//     public int colorAmount = 4;


//     // Use this for initialization
//     void Start()
//     {
//         // Create Texture
//         /*
//         outputTexture = new RenderTexture(texResolution, texResolution, 0);
//         outputTexture.enableRandomWrite = true;
//         outputTexture.filterMode = FilterMode.Trilinear;
//         */
//         outputTexture = new RenderTexture(settingRef);
//         outputTexture.enableRandomWrite = true;
//         outputTexture.Create();

//         // INIT

//         // set handles for ease of use
//         pointsHandle = shader.FindKernel("DrawPoints");
//         trailsHandle = shader.FindKernel("DrawTrails");
//         clearHandle = shader.FindKernel("SetTexture");

//         // send variables into shader
//         shader.SetInt("_texres", texResolution);
//         shader.SetInt("_colres", colorAmount);

//         // buffer setup
//         int stride = (3) * 4; // every component as a float (3) * 4 bytes per float
//         pointsBuffer = new ComputeBuffer(8192, stride);

//         stride = (4) * 4;
//         colorsBuffer = new ComputeBuffer(colorAmount, stride);

//         // 
//         shader.SetTexture( clearHandle, "Result", outputTexture );
//         shader.SetTexture( pointsHandle, "Result", outputTexture ); // inputs texture to shader
//         shader.SetTexture( trailsHandle, "Result", outputTexture );

//         shader.Dispatch(clearHandle, 256, 256, 1); // put black into the texture

//         target.SetTexture("_MainTex", outputTexture); // sets output render texture into target material

//         Color[] testColors = new Color[colorAmount];
//         testColors[0] = new Color(1, 0, 0, 1); //red _ yes
//         testColors[1] = new Color(0, 1, 0, 1); //green
//         testColors[2] = new Color(0, 0, 1, 1); //blue
//         testColors[3] = new Color(1, 1, 0, 1); //yellow _yes
//         testColors[4] = new Color(0, 1, 1, 1); //cyan
//         testColors[5] = new Color(1, 0, 1, 1); //magenta
//         testColors[6] = new Color(0.5f, 0, 1, 0); //mix6 _yes
//         testColors[7] = new Color(1, 0.5f, 1, 0); //mix7
//         testColors[8] = new Color(0.5f, 0, 1, 0); //mix8
//         testColors[9] = new Color(0.5f, 0.5f, 1, 0); //mix9 _yes
//         testColors[10] = new Color(0.5f, 0.5f, 1, 0); //mix10
//         testColors[11] = new Color(0.5f, 0.5f, 1, 0); //mix11

//         colorsBuffer.SetData(testColors);
//     }

//     void Update()
//     {
//         // update compute shader
//         pointsBuffer.SetData(diffGrowth.nodes.Points);
//         shader.SetBuffer(pointsHandle, "colorsBuffer", colorsBuffer);
//         shader.SetBuffer(pointsHandle, "pointsBuffer", pointsBuffer);
//         shader.Dispatch(pointsHandle, 128, 1, 1);
//         shader.Dispatch(trailsHandle, 256, 256, 1);
//     }

//     private void OnDestroy()
//     {
//         if (pointsBuffer != null) pointsBuffer.Dispose();
//         if (colorsBuffer != null) colorsBuffer.Dispose();
//     }
// }
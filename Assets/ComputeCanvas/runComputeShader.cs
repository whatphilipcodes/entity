using UnityEngine;
using System.Collections;

public class runComputeShader : MonoBehaviour
{
    // Editor Input
    [SerializeField] static public int canvasResolution = 4096;
    [SerializeField] ComputeShader shader;
    [SerializeField] Material target;
    [SerializeField] RenderTexture settingRef;
    [SerializeField] bool debug = false;
    public AnimationCurve colorWeights;

    // ShaderData
    int pointsHandle;
    int trailsHandle;
    int clearHandle;
    RenderTexture outputTexture;
    ComputeBuffer pointsBuffer;
    ComputeBuffer colorsBuffer;

    // Input
    public differentialGrowth diffGrowth;
    public analyzeInput analyzeIn;

    // Variables
    float[] absoluteWeights;


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
        clearHandle = shader.FindKernel("SetTexture");

        // sends variables into shader
        shader.SetInt("_texres", canvasResolution);
        shader.SetInt("_colres", analyzeIn.colorsLimit);

        // buffer setup
        int stride = (3) * 4; // every component as a float (3) * 4 bytes per float
        pointsBuffer = new ComputeBuffer(8192, stride);

        stride = (4) * 4;
        colorsBuffer = new ComputeBuffer(8192, stride);

        // sends textures to kernels
        shader.SetTexture( clearHandle, "Result", outputTexture );
        shader.SetTexture( pointsHandle, "Result", outputTexture ); // inputs texture to shader
        shader.SetTexture( trailsHandle, "Result", outputTexture );
        target.SetTexture("_MainTex", outputTexture); // sets output render texture into target material

        shader.Dispatch(clearHandle, 256, 256, 1); // put black into the texture

        // WEIGHING COLORS
        int colorLength = analyzeIn.identifiedColors.Length;
        absoluteWeights = new float[colorLength];
        float sum = 0;

        for (int n = 0; n < colorLength; n++)
        {
            absoluteWeights[n] = colorWeights.Evaluate( (float)n / (float)colorLength );
            sum += absoluteWeights[n];
        }

        float test = 0;
        for (int o = 0; o < colorLength; o++)
        {
            absoluteWeights[o] = absoluteWeights[o] / sum;
            test += absoluteWeights[o];
            if (debug == true) print ("absolutWeight[" + o + "] = " + absoluteWeights[o]);
        }
        if (debug == true) print("one test: " + test);
    }

    void Update()
    {   // prepare buffer arrays
        int length = diffGrowth.nodes.Points.Length;

        Vector3[] mergePointsID = new Vector3[length];
        for (int i = 0; i < length; i++)
        {
            mergePointsID[i].x = diffGrowth.nodes.Points[i].x;
            mergePointsID[i].y = diffGrowth.nodes.Points[i].y;
            mergePointsID[i].z = diffGrowth.nodeID[i];
        }

        Color[] weightedColors = new Color[length];

        for (int k = 0; k < absoluteWeights.Length; k++)
        {
            for (int n = 0; n < absoluteWeights[k] * length; n++)
            {
                weightedColors[diffGrowth.nodeID[n]] = analyzeIn.identifiedColors[k];
            }
        }

        /*
        for (int i = 0; i < length; i++)
        {
            weightedColors[i] = analyzeIn.identifiedColors[ i % analyzeIn.identifiedColors.Length];
        }
        */
        
        // update compute shader
        pointsBuffer.SetData(mergePointsID);
        colorsBuffer.SetData(weightedColors);
        shader.SetBuffer(pointsHandle, "colorsBuffer", colorsBuffer);
        shader.SetBuffer(pointsHandle, "pointsBuffer", pointsBuffer);
        shader.Dispatch(pointsHandle, 128, 1, 1);
        shader.Dispatch(trailsHandle, 256, 256, 1);


    }

    private void OnDestroy()
    {
        if (pointsBuffer != null) pointsBuffer.Dispose();
        if (colorsBuffer != null) colorsBuffer.Dispose();
    }

    //Courtesy of Kiwasi -> https://forum.unity.com/threads/random-numbers-with-a-weighted-chance.442190/#post-2859657
    public int GetRandomItem (int noOfItems)
    {
        return noOfItems * (int)colorWeights.Evaluate(Random.value);
    }
}
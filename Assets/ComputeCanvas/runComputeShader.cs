using UnityEngine;
using System.Collections;

public class runComputeShader : MonoBehaviour
{
    // Editor Input
    [SerializeField] static public int canvasResolution = 4096;
    [SerializeField] ComputeShader shader;
    [SerializeField] Material target;
    [SerializeField] RenderTexture settingRef;
    [SerializeField] [Range(0, 0.1f)] float fadeAmount;
    [SerializeField] bool debug = false;
    public AnimationCurve colorWeights;

    // ShaderData
    int pointsHandle;
    int trailsHandle;
    //int fadeHandle;
    int clearHandle;
    RenderTexture outputTexture;
    ComputeBuffer pointsBuffer;
    ComputeBuffer colorsBuffer;

    // Input
    public differentialGrowth diffGrowth;
    public analyzeInput analyzeIn;

    // Variables
    float[] absoluteWeights;
    bool faded;


    // Use this for initialization
    void Start()
    {
        InitializeShader();
        faded = true;
    }

    void Update()
    {
        if (analyzeInput.startSim == true)
        {
            InitializeShader();
            ColorWeighing();
        }

        if (differentialGrowth.simRunning == true)
        {
            // prepare buffer arrays
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
            
            // update compute shader
            pointsBuffer.SetData(mergePointsID);
            colorsBuffer.SetData(weightedColors);
            shader.SetBuffer(pointsHandle, "colorsBuffer", colorsBuffer);
            shader.SetBuffer(pointsHandle, "pointsBuffer", pointsBuffer);
            shader.Dispatch(pointsHandle, 128, 1, 1);
            shader.Dispatch(trailsHandle, 256, 256, 1);
        }

        if (watchForInput.scanStarted == true && faded == false)
        {
            faded = true;
            StartCoroutine(FadeToBlack(0.01f));
        }

        if (analyzeInput.startSim == true) analyzeInput.startSim = false;
    }

    private void InitializeShader()
    {
        //fadingRunning = false;
        faded = false;
        target.SetFloat("_Opacity", 1.0f);

        if (pointsBuffer != null) pointsBuffer.Dispose();
        if (colorsBuffer != null) colorsBuffer.Dispose();

        outputTexture = new RenderTexture(settingRef);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        // INIT

        // set handles for ease of use
        pointsHandle = shader.FindKernel("DrawPoints");
        trailsHandle = shader.FindKernel("DrawTrails");
        //fadeHandle = shader.FindKernel("FadeBlack");
        clearHandle = shader.FindKernel("SetTexture");

        // sends variables into shader
        shader.SetInt("_texres", canvasResolution);
        shader.SetInt("_colres", analyzeIn.colorsLimit);
        shader.SetFloat("_fadeAmount", fadeAmount);

        // buffer setup
        int stride = (3) * 4; // every component as a float (3) * 4 bytes per float
        pointsBuffer = new ComputeBuffer(8192, stride);

        stride = (4) * 4;
        colorsBuffer = new ComputeBuffer(8192, stride);

        // sends textures to kernels
        shader.SetTexture( clearHandle, "Result", outputTexture );
        //shader.SetTexture( fadeHandle, "Result", outputTexture );
        shader.SetTexture( pointsHandle, "Result", outputTexture ); // inputs texture to shader
        shader.SetTexture( trailsHandle, "Result", outputTexture );
        target.SetTexture("_MainTex", outputTexture); // sets output render texture into target material

        shader.Dispatch(clearHandle, 256, 256, 1); // put black into the texture
    }

    private void ColorWeighing()
    {
        // WEIGHING COLORS
        int colorLength = analyzeIn.identifiedColors.Length;
        absoluteWeights = new float[colorLength];
        float sum = 0;

        for (int n = 0; n < colorLength; n++)
        {
            absoluteWeights[n] = colorWeights.Evaluate( (float)n / (float)colorLength );
            sum += absoluteWeights[n];
        }
    }

    public IEnumerator FadeToBlack(float fadeAmount)
    {
        if (debug == true) print("shader fade out started");

        //INIT
        float intensity = 1.0f;
        bool active = true;

        while (active == true)
        {
            intensity -= fadeAmount;
            //Mathf.Clamp(intensity, 0, 1);
            target.SetFloat("_Opacity", intensity);
            if (intensity < 0)
            {
                active = false;
                target.SetFloat("_Opacity", 0f);
                if (debug == true) print("shader fade out finished");
            }
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (pointsBuffer != null) pointsBuffer.Dispose();
        if (colorsBuffer != null) colorsBuffer.Dispose();
    }
}
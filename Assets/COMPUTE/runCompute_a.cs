using UnityEngine;
using System.Collections;

public class runCompute_a : MonoBehaviour
{
    // UserInput
    [SerializeField] Color pointColor = new Color(1,1,1,1);
    [SerializeField] int texResolution = 1024;
    [SerializeField] ComputeShader shader;
    [SerializeField] Material target;

    // TrailTesting
    [Range(0f, 1f)] public float decay = 0.00122f;

    // ShaderData
    int pointsHandle;
    int trailsHandle;
    RenderTexture outputTexture;
    ComputeBuffer buffer;

    // Reference for Points of Growth Simulation
    public differentialGrowth diffGrowth;


    // Use this for initialization
    void Start()
    {
        // Create Texture
        outputTexture = new RenderTexture(texResolution, texResolution, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.filterMode = FilterMode.Trilinear;
        outputTexture.Create();

        // INIT

        // set handles for ease of use
        pointsHandle = shader.FindKernel("DrawPoints");
        trailsHandle = shader.FindKernel("DrawTrails");

        // send variables into shader
        shader.SetVector( "_pcol", pointColor );
        shader.SetInt("_texres", texResolution);
        shader.SetFloat("_decay", decay);

        // buffer setup
        int stride = (3) * 4; // every component as a float (3) * 4 bytes per float
        buffer = new ComputeBuffer(8192, stride);

        // 
        shader.SetTexture( pointsHandle, "Result", outputTexture ); // inputs texture to shader
        shader.SetTexture( trailsHandle, "Result", outputTexture );
        target.SetTexture("_MainTex", outputTexture); // sets output render texture into target material
    }

    void Update()
    {
        // update compute shader
        buffer.SetData(diffGrowth.nodes.Points);
        shader.SetBuffer(pointsHandle, "pointsBuffer", buffer);
        shader.Dispatch(pointsHandle, 128, 1, 1);
        shader.Dispatch(trailsHandle, 256, 256, 1);
    }

    private void OnDestroy()
    {
        if (buffer != null) buffer.Dispose();
    }
}

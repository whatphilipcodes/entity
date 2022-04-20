using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enable KDTree usage
using DataStructures.ViliWonka.KDTree; // found on: https://github.com/viliwonka/KDTree

// Enable Helpers
using Entity.Utilities;

public class differentialGrowth : MonoBehaviour
{
    // Editor Input
    public analyzeInput analyzeIn;
    [SerializeField] int setCanvasResolution = 4096;
    [SerializeField] float growthRate = 2f, desiredDistance = 0.8f, maxDistance = 1, kdSearchRadius = 0.8f;
    [SerializeField] [Range(0f, 1)] float attractionForce = 0.5f, repulsionForce = 0.5f, alignmentForce = 0.5f;
    [SerializeField] [Range(1,40)] float maxForcePerFrame = 20;
    [SerializeField] [Range(0, 6)] int steps = 1;
    [SerializeField] [Range(0, 1f)] float stepDiv = 0;
    [SerializeField] bool stopGrowth;
    private float stepBarrier;
    private int stopQ;
    [SerializeField] bool loop = false, customGrowthPattern, debug = false, testRun = false;
    [SerializeField][Range(0f, 0.01f)] float debugScale = 0.001f;

    // KDTree Setting: Default is 32
    int maxPointsPerLeafNode = 32;

    // Output
    public KDTree nodes;
    public List<int> nodeID;

    // Variables
    private KDQuery query;
    bool growthRunning;
    public static bool simRunning;
    int initialSteps;
    public static int canvasResolution;
    int growCounter;

    // Structs
    struct ColorAndID
    {
        public Color col;
        public int id;

        public ColorAndID (Color col, int id)
        {
            this.col = col;
            this.id = id;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (stopGrowth == true)
        {
            stopQ = 0;
        } else {
            stopQ = 1;
        }

        query = new KDQuery();
        initialSteps = steps;
        simRunning = false;

        canvasResolution = setCanvasResolution;

        if (testRun == true)
        {
            simRunning = true;
            InitKDTree(InitStartCircle(400, 32));
        }
    }

    // Node injection/growth
    IEnumerator RandomGrowth (float growthRate)
    {
        growthRunning = true;
        steps = initialSteps;

        while(growthRunning == true)
        {
            //print("coroutine iteration started");
            for (int i = 0; i < steps; i++)
            {
                SubdivideTarget (Random.Range(0,nodes.Count));
            }
            stepBarrier += stepDiv;
            
            if (stepBarrier > 1 && steps > stopQ)
            {
                steps -= 1;
                stepBarrier = 0;
            }
            if (steps == 0) growthRunning = false;
            yield return new WaitForSeconds(growthRate);
        }
        if (debug == true) print("growth ended");
    }

    IEnumerator CustomGrowth (float growthRate)
    {
        growthRunning = true;
        steps = initialSteps;

        while(growthRunning == true)
        {
            //print("coroutine iteration started");
            for (int i = 0; i < steps; i++)
            {
                if (runComputeShader.weightedColors != null) SubdivideTarget(ColorLookup());
                growCounter++;
            }
            stepBarrier += stepDiv;
            
            if (stepBarrier > 1 && steps > stopQ)
            {
                steps -= 1;
                stepBarrier = 0;
            }
            if (steps == 0) growthRunning = false;
            yield return new WaitForSeconds(growthRate);
        }
        //print("growth ended");
    }

    // Update is called once per frame
    void Update()
    {
        if (analyzeInput.startSim == true)
        {
            InitKDTree(analyzeIn.initPoints);
        }

        if (simRunning == true)
        {
            // Node manangement loop
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes.Points[i] += AttractionForceOnPoint(i, desiredDistance, attractionForce);
                nodes.Points[i] += RepulsionForceOnPoint(i, repulsionForce);
                nodes.Points[i] += AlignmentForceOnPoint(i, alignmentForce);
                SplitEdges(i);
                //if (pruneNodes == true) PruneNodes(i);
                
            }
            nodes.Rebuild();
            
            // Debug area
            if ((debug == true) && (Input.GetKeyDown("space")))
            {
                Debug.Log("KDTree //nodes contains " + nodes.Count + " (" + nodes.Points.Length + ") node(s)."  );
            }

            if (debug == true)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    Vector3 centerOnTex = new Vector3 (canvasResolution / 2, canvasResolution /2, 0); // bring in front of texture
                    Debug.DrawLine((nodes.Points[i] - centerOnTex) * debugScale, (nodes.Points[(i + 1) % nodes.Count] - centerOnTex) * debugScale, Color.magenta);
                }
            }
        }
    }

    
    Vector3[] InitStartCircle(float radius, int verts)
    {
        //Settings
        Vector3[] shape = new Vector3[verts];
        Vector3 origin = new Vector3(canvasResolution / 2, canvasResolution / 2, 0);

        //Init
        float a = 2 * Mathf.PI;
        float angleInc = a / verts;
        Vector3 point;

        for (int i = 0; i < verts; i++)
        {
            point.x = origin.x + Mathf.Cos(a) * radius;
            point.y = origin.y + Mathf.Sin(a) * radius;
            point.z = origin.z;
            shape[i] = point;
            a = a + angleInc;
        }
        return shape;
    }
    

    void InitKDTree(Vector3[] firstShape)
    {
        growCounter = 0;
        simRunning = true;
        nodes = new KDTree(firstShape, maxPointsPerLeafNode);

        // INIT NODE ID MANAGEMENT
        nodeID = new List<int>();
        for (int i = 0; i < firstShape.Length; i++)
        {
            nodeID.Insert(i, i);
        }
        /////////////////////

        //Init coroutines
        StopCoroutine(RandomGrowth(growthRate));
        if ( customGrowthPattern == true)
        {
            StartCoroutine(CustomGrowth(growthRate));
        } else {
            StartCoroutine(RandomGrowth(growthRate));
        }

        if (debug == true)
        {
            for (int i = 0; i < firstShape.Length; i++)
            {
                Vector3 centerOnTex = new Vector3 (canvasResolution / 2, canvasResolution /2, 0);
                Debug.DrawLine((firstShape[i] - centerOnTex) * debugScale, (firstShape[( i + 1 ) % firstShape.Length] - centerOnTex) * debugScale, Color.green, 100f);
            }
            //print("NodeID contains " + nodeID.Count + " entries");
        }
    }

    Vector3 AttractionForceOnPoint(int index, float desiredDistance, float force)
    {
        force *= 0.5f;
        float distance2next;
        float distance2prev;
        Vector3 currentNode = nodes.Points[index];
        Vector3 diff2next = Vector3.zero;
        Vector3 diff2prev = Vector3.zero;

        if (loop == true)
        {
            distance2next = Vector3.Distance(currentNode, nodes.Points[(index + 1) % nodes.Count]);
            if (distance2next > desiredDistance)
            {
                Vector3 newPoint = Vector2.Lerp(currentNode, nodes.Points[(index + 1) % nodes.Count], force);
                diff2next = newPoint - currentNode;
            }
            distance2prev = Vector3.Distance(currentNode, nodes.Points[Utils.mod((index - 1), nodes.Count)]);
            if (distance2prev > desiredDistance)
            {
                Vector3 newPoint = Vector2.Lerp(currentNode, nodes.Points[Utils.mod((index - 1), nodes.Count)], force);
                diff2prev = newPoint - currentNode;
            }
        }
        else if (index + 1 < nodes.Count)
        {
            distance2next = Vector3.Distance(currentNode, nodes.Points[index + 1]);
            if (distance2next > desiredDistance)
            {
                Vector3 newPoint = Vector2.Lerp(currentNode, nodes.Points[index + 1], force);
                diff2next = newPoint - currentNode;
            }
        }
        else if (index - 1 >= 0)
        {
            distance2prev = Vector3.Distance(currentNode, nodes.Points[index - 1]);
            if (distance2prev > desiredDistance)
            {
                Vector3 newPoint = Vector2.Lerp(currentNode, nodes.Points[index - 1], force);
                diff2prev = newPoint - currentNode;
            }
        }
        Vector3 displacement = diff2next + diff2prev;

        if (displacement.magnitude > maxForcePerFrame)
        {
            return displacement.normalized * maxForcePerFrame;
        } else {
            return displacement;
        }
    }

    Vector3 RepulsionForceOnPoint(int index, float force)
    {
        force *= -0.5f; // repulsion through negative lerp factor -> unclamped
        var resultIndices = findInRadiusKDTree(index, kdSearchRadius);
        Vector3 newPoint = nodes.Points[index];

        for (int i = 0; i < resultIndices.Count; i++)
        {
            newPoint = Vector2.LerpUnclamped(newPoint, nodes.Points[resultIndices[i]], force);
        }
        Vector3 displacement = newPoint - nodes.Points[index];
        
        if (displacement.magnitude > maxForcePerFrame)
        {
            return displacement.normalized * maxForcePerFrame;
        } else {
            return displacement;
        }
    }

    Vector3 AlignmentForceOnPoint(int index, float force)
    {
        force *= 0.5f;
        Vector3 displacement = Vector3.zero;
        Vector3 currentNode = nodes.Points[index];

        if (loop == true)
        {
            Vector3 midPoint = (nodes.Points[Utils.mod((index - 1),nodes.Count)] + nodes.Points[(index + 1) % nodes.Count]) / 2;
            Vector3 newPoint = Vector2.Lerp(currentNode, midPoint, force);
            displacement = newPoint - currentNode;
        }
        else if (index - 1 >= 0 && index + 1 < nodes.Count )
        {
            Vector3 midPoint = (nodes.Points[index - 1] + nodes.Points[index + 1]) / 2;
            Vector3 newPoint = Vector2.Lerp(currentNode, midPoint, force);
            displacement = newPoint - currentNode;
        }

        if (displacement.magnitude > maxForcePerFrame)
        {
            return displacement.normalized * maxForcePerFrame;
        } else {
            return displacement;
        }
    }

    void SplitEdges(int index)
    {
        if (loop == true)
        {
            float distance = Vector3.Distance(nodes.Points[index], nodes.Points[Utils.mod((index - 1),nodes.Count)]);
            if (distance > maxDistance)
            {
                SubdivideTarget(index);
            }
        }
        else if (index - 1 >= 0)
        {
            float distance = Vector3.Distance(nodes.Points[index], nodes.Points[index - 1]);
            if (distance > maxDistance)
            {
                SubdivideTarget(index);
            }
        }
    }

    List<int> findInRadiusKDTree(int index, float radius)
    {
        var resultIndices = new List<int>();
        query.Radius(nodes, nodes.Points[index], radius, resultIndices);
        return resultIndices;
    }

    void SubdivideTarget(int splitIndex)
    {
        int nextIndex = (splitIndex + 1) % nodes.Count; // catches values greater than nodes.Count and restarts at index 0
        Vector3 midPoint = (nodes.Points[splitIndex] + nodes.Points[nextIndex]) / 2;
        //if (debug == true) Debug.DrawLine(new Vector3(0,0,0), midPoint, Color.magenta, 1f);
        InjectNodeToKDTree(midPoint, nextIndex);
    }

    void InjectNodeToKDTree(Vector3 point, int Index)
    {
        int count = nodes.Count;
        int newCount = count + 1;

        Vector3[] shiftBuffer = new Vector3[newCount - Index];
        int [] nodeIDShiftbuffer = new int[newCount - Index];  // NODE ID MANAGEMENT -> Mirroring Shiftbuffer
        
        // Load new point into buffer
        shiftBuffer[0] = point;
        nodeIDShiftbuffer[0] = Index; // NODE ID MANAGEMENT -> Store Index of "point"

        // Load shifted points from KD Tree behind new points into buffer
        for (int i = 1, j = Index; j < count; i++, j++)
        {
            shiftBuffer[i] = nodes.Points[j];
            nodeIDShiftbuffer[i] = nodeID[j]; // NODE ID MANAGEMENT -> Fill shiftbuffer
        }

        nodes.SetCount(newCount);
        nodeID.RemoveRange(Index, count - Index); // NODE ID MANAGEMENT -> Clear overwritten area

        // Write buffer into KD Tree
        for (int i = Index, j = 0; i < nodes.Count; i++, j++)
        {
            nodes.Points[i] = shiftBuffer[j];
            nodeID.Add(nodeIDShiftbuffer[j]); // NODE ID MANAGEMENT -> update from shiftbuffer storage
        }
    }

    int ColorLookup()
    {
        List<ColorAndID> colID = new List<ColorAndID>();

        for (int i = 0; i < runComputeShader.weightedColors.Length; i++)
        {
            colID.Add(new ColorAndID(runComputeShader.weightedColors[i], nodeID[i]));
        }
        colID.Sort(BySaturation);
        return colID[growCounter].id;
    }

    private int BySaturation (ColorAndID a, ColorAndID b)
    {
        float a_sat;
        float b_sat;
        Color.RGBToHSV(a.col, out _, out a_sat, out _);
        Color.RGBToHSV(b.col, out _, out b_sat, out _);
        if (a_sat > b_sat)
        {
            return - 1;
        } else if (a_sat < b_sat) {
            return 1;
        } else {
            return 0;
        }
    }
}

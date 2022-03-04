using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enable Extra Libraries
using Seed.Utilities;
using FindDominantColour;

public class analyzeInput : MonoBehaviour
{
    // Editor Input
    //[SerializeField] watchForInput input;
    //[SerializeField] GameObject diffGrow;
    [SerializeField] public int colorsLimit = 512;
    [SerializeField] bool useColorLimit = true, discardDarkCol = true,fillEveryThird = true /*, randomColors = true*/;
    [SerializeField] colorSortingMode sortingMode = new colorSortingMode();
    [SerializeField] [Range(0,1)] float brightThresh = 0.2f, satThresh = 0.2f, pixelSaturationThresh = 0.2f, pixelBrightThresh = 0.2f;
    [SerializeField] bool useStaticThreshold = false; 
    [SerializeField] public int pointsAmount = 128;
    [SerializeField] bool debug = false, smoothing = true;
    [SerializeField] [Range(0,100)] int widthThresh = 40;
    [SerializeField] [Range(0,100)] int minPointDistance = 40;
    [SerializeField] int iterationsPerFrame = 200;

    // Output
    public Color[] identifiedColors;
    public Vector3[] initPoints;

    // Structs & Enums
    struct anglePoint
    {
        public Vector2 point;
        public float angle;

        public anglePoint (Vector2 point, float angle)
        {
            this.point = point;
            this.angle = angle;
        }
    }

    enum colorSortingMode
     {
         Random,
         BySaturation, 
         ByBrightness,
         ByDominance
     };

    // Uduino
    [Range(0,255)] public/*private*/int intensity;

    // Variables
    public static bool startSim;
    private float initBrightThresh;

    // Start is called before the first frame update
    void Start()
    {
        initBrightThresh = brightThresh;
    }

    // Update is called once per frame
    void Update()
    {
        if (watchForInput.newInput == true)
        {
            StartCoroutine(AnalyzeScan(watchForInput.scan));
            watchForInput.newInput = false;
        }
    }

    // Functions
    IEnumerator AnalyzeScan (Texture2D scan)
    {
        if (debug == true) print("Analysis started");
        List<Color> colors = new List<Color>();
        List<Vector2> points = new List<Vector2>();
        Color[] allColors = scan.GetPixels();

        float mediumBright = 0;

        // Calculate Treshhold Bias
        for (int i = 0; i < allColors.Length; i++)
        {
            float currentBright;
            Color.RGBToHSV(allColors[i], out _, out _, out currentBright);
            mediumBright += currentBright;
        }

        mediumBright /= allColors.Length;
        if (useStaticThreshold == false) brightThresh = mediumBright + mediumBright * initBrightThresh;

        if (debug == true)
        {
            print("calculated brightness median: " + mediumBright + " | therefore current brightness Threshhold: " + brightThresh);
        }
        
        bool inObject = false;
        bool startEndObject = false;
        bool checkWidth = false;

        int currentIndex = 0;
        int lastIndex= 0;

        int iterationCounter = 0;

        for (int i = 0; i < allColors.Length; i++)
        {
            Vector2 currentPixel = new Vector2 (i % scan.width, i / scan.width);

            float bright;
            float sat;

            Color.RGBToHSV(allColors[i], out _, out sat, out bright);

            if ( bright > brightThresh && sat > satThresh)
            {
                inObject = true;
            } else if (inObject == true) {
                inObject = false;
            }

            if (inObject == true && startEndObject == false)
            {
                lastIndex = points.Count;
                points.Add(currentPixel);

                if (sat > pixelSaturationThresh && bright > pixelBrightThresh) colors.Add(allColors[i]);

                startEndObject = true;
            } else if (inObject == false && startEndObject == true) {
                currentIndex = points.Count;
                points.Add(currentPixel);

                if (sat > pixelSaturationThresh && bright > pixelBrightThresh) colors.Add(allColors[i]);

                startEndObject = false;
                checkWidth = true;
            }
            
            if (points.Count > 1 && checkWidth == true)
            {
                int width = (int) Vector2.Distance(points[ currentIndex ], points[ lastIndex ]);
                if (width < widthThresh)
                {
                    points.RemoveAt( currentIndex );
                    points.RemoveAt( lastIndex );
                } else if (points[ currentIndex ].y != points[ lastIndex ].y) {
                    points.RemoveAt( currentIndex );
                    points.RemoveAt( lastIndex );
                }
                checkWidth = false;
            }
            iterationCounter++;
            if (iterationCounter > iterationsPerFrame)
            {
                iterationCounter = 0;
                yield return null;
            }
        }

        if (sortingMode != colorSortingMode.ByDominance)
        {
            FillColorsArray(colors);
        } else {

            //DOMINANT COLORS////////

            int reducedResolution = 200;
            Texture2D scaledInput;
            iterationCounter = 0;

            ////////////////////////////////////////////////

            if (scan.width > scan.height)
            {
                scaledInput = Utils.ScaleTexture(scan, reducedResolution, (int) Math.Floor((scan.height / (scan.width * 1.0f)) * reducedResolution));
            } else {
                scaledInput = Utils.ScaleTexture(scan, (int) Math.Floor((scan.width / (scan.width * 1.0f)) * reducedResolution),reducedResolution);
            }
            
            List<System.Drawing.Color> dCandidates = new List<System.Drawing.Color>(scaledInput.width * scaledInput.height);
            for (int x = 0; x < scaledInput.width; x++) {
                for (int y = 0; y < scaledInput.height; y++) {
                    
                    float bright;

                    UnityEngine.Color ColIn = scaledInput.GetPixel(x, y);
                    UnityEngine.Color.RGBToHSV(ColIn, out _, out _, out bright);
                    

                    float r = ColIn.r * 255;
                    float g = ColIn.g * 255;
                    float b = ColIn.b * 255;

                    //if (debug == true) print("Added: R" + r + " G" + g + " B" + b);

                    if (discardDarkCol == false )
                    {
                        dCandidates.Add(System.Drawing.Color.FromArgb((int) r, (int) g, (int) b));
                    } else if (bright > brightThresh) {
                        dCandidates.Add(System.Drawing.Color.FromArgb((int) r, (int) g, (int) b));
                    }
                    iterationCounter++;
                    if (iterationCounter > iterationsPerFrame)
                    {
                        iterationCounter = 0;
                        yield return null;
                    }
                }
            }
            ////////////////////////////////////////////////

            FillColorsArrayDom(dCandidates);

        }

        FillPointsArray(points);

        if (debug == true) print("Analysis complete");
        //diffGrow.SetActive(true);
        startSim = true;
    }

    void FillColorsArray (List<Color> colorList)
    {
        if (sortingMode != colorSortingMode.Random && sortingMode == colorSortingMode.ByBrightness)
        {
            colorList.Sort(ByBrightness);
        } else if (sortingMode == colorSortingMode.BySaturation) {
            colorList.Sort(BySaturation);
        }

        int listCounter = 0;
        int ArrCounter = 1;
        if (fillEveryThird == true) ArrCounter = 3;
        if (useColorLimit == true)
        {
            identifiedColors = new Color[colorsLimit];
        } else {
            identifiedColors = new Color[colorList.Count];
        }
        
        for (int i = 0; i < colorsLimit; i+=ArrCounter) // BUG: Only every third color is displayed in compute shader
        {
            if (sortingMode == colorSortingMode.Random)
            {
                identifiedColors[i] = colorList[UnityEngine.Random.Range(0,colorList.Count)];
            } else {
                identifiedColors[i] = colorList[listCounter];
            }

            if (useColorLimit == true)
            {
                listCounter += colorList.Count / colorsLimit;
            } else {
                listCounter++;
            }
        }
    }

    void FillColorsArrayDom (List<System.Drawing.Color> dCandidates)
    {
        KMeansClusteringCalculator clustering = new KMeansClusteringCalculator();
            IList<System.Drawing.Color> dominantColours = clustering.Calculate(colorsLimit, dCandidates, 5.0d);

            List<UnityEngine.Color> dColorResults = new List<UnityEngine.Color>();

            foreach (System.Drawing.Color color in dominantColours) {
                UnityEngine.Color result = new UnityEngine.Color();

                result.r = (float) color.R / 255;
                result.g = (float) color.G / 255;
                result.b = (float) color.B / 255;

                dColorResults.Add(result);
            }

        identifiedColors = new Color[colorsLimit];

        for (int i = 0; i < dColorResults.Count; i++)
        {
            identifiedColors[i] = dColorResults[i];
        }
    }

    void FillPointsArray (List<Vector2> pointsList)
    {
        if (pointsList.Count > pointsAmount)
        {
            // Unordered Points
            Vector2[] tempPoints = new Vector2[pointsAmount];
            List<anglePoint> anglePoints = new List<anglePoint>();

            // Sorted points will go in here
            initPoints = new Vector3[pointsAmount];

            for (int i = 0, j = 0; i < pointsAmount; i++)
            {
                tempPoints[i] = pointsList[j];
                j += (int) pointsList.Count / pointsAmount;
            }
            
            Vector2 sum = new Vector2();
            for (int i = 0; i < pointsAmount; i++)
            {
                sum += tempPoints[i];
            }

            Vector2 centroid = sum / pointsAmount;
            Vector2 zeroid = new Vector2 (0 , -1.0f); // "zero" angle reference pointing straight downwards

            for (int i = 0; i < pointsAmount; i++)
            {
                Vector2 current = tempPoints[i] - centroid;
                float theta = Vector2.SignedAngle(zeroid, current);
                if (theta < 0) theta = 360 + theta;
                anglePoints.Add(new anglePoint (tempPoints[i], theta));
            }

            anglePoints.Sort(ByAngle);

            if (smoothing == true)
            {
                for (int i = 0; i < pointsAmount; i++)
                {
                    if (Vector2.Distance ( anglePoints[i].point, anglePoints[(i + 1) % pointsAmount].point) < minPointDistance)
                    {
                        anglePoint midPoint01 = new anglePoint (Vector2.Lerp(anglePoints[i].point, anglePoints[Utils.mod(i + 1, pointsAmount)].point, 0.33f), 0f);
                        anglePoint midPoint02 = new anglePoint (Vector2.Lerp(anglePoints[i].point, anglePoints[Utils.mod(i + 1, pointsAmount)].point, 0.66f), 0f);

                        anglePoints.RemoveAt(Utils.mod(i + 1, anglePoints.Count));
                        anglePoints.RemoveAt(Utils.mod(i, anglePoints.Count));

                        anglePoints.Insert(Utils.mod(i, anglePoints.Count), midPoint01);
                        anglePoints.Insert(Utils.mod(i + 1, anglePoints.Count), midPoint02);
                    }
                }
            }

            Vector2 centreTexture = new Vector2 ((int) runComputeShader.canvasResolution * 0.5f, (int) runComputeShader.canvasResolution * 0.5f);
            Vector2 transform = centreTexture - centroid;

            for (int i = 0; i < pointsAmount; i++)
            {
                initPoints[i] = anglePoints[i].point + transform;
            }
        }
    }
 
    private int ByAngle (anglePoint a, anglePoint b)
    {
        if (a.angle < b.angle)
        {
            return - 1;
        } else if (a.angle > b.angle) {
            return 1;
        } else {
            return 0;
        }
    }

    private int ByBrightness (Color a, Color b)
    {
        float a_bright;
        float b_bright;
        Color.RGBToHSV(a, out _, out _, out a_bright);
        Color.RGBToHSV(b, out _, out _, out b_bright);
        if (a_bright > b_bright)
        {
            return - 1;
        } else if (a_bright < b_bright) {
            return 1;
        } else {
            return 0;
        }
    }

    private int BySaturation (Color a, Color b)
    {
        float a_sat;
        float b_sat;
        Color.RGBToHSV(a, out _, out a_sat, out _);
        Color.RGBToHSV(b, out _, out b_sat, out _);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enable Uduino
using Uduino;

public class Larduino : MonoBehaviour
{
    // Settings (Visible)
    [SerializeField] AnimationCurve FadeCurve;
    [SerializeField] [Range(1,10000)] int TouchThreshold = 4000;
    [SerializeField] bool Debug;


    // Settings (Hidden)
    private static int fadeAmount = 1;
    private static float fadeFrequency = 0.01f;
    private static AnimationCurve fadeCurve;
    private static bool debug;
    private static int touchThreshold = 4000;

    // Output Properties
    public static bool scanTrigger;
    //public static bool saveFrameTrigger;

    // Internal Properties
    private static int intensity;
    private static bool init;

    // Start is called before the first frame update
    void Awake()
    {
        // Adjust settings from inspector
        touchThreshold = TouchThreshold;
        fadeCurve = FadeCurve;
        debug = Debug;
    }

    void Start()
    {
        // Init
        init = true;
        intensity = 0;
        //UduinoManager.Instance.pinMode(9, PinMode.PWM);
        UduinoManager.Instance.OnDataReceived += OnDataReceived;
    }

    void Update()
    {
        if (init)
        {
            UduinoManager.Instance.sendCommand("t", touchThreshold);
            if (Time.timeSinceLevelLoad > 1f) init = false;
        }
    }

    public static IEnumerator FadeInLED()
    {
        if (debug == true) print("led fade in started");

        // Init
        bool active = true;
        if (intensity != 0) yield return null;
        UduinoManager.Instance.pinMode(9, PinMode.PWM);

        while (active)
        {
            intensity += fadeAmount;
            intensity = Mathf.Clamp(intensity, 0, 255);

            if (intensity == 255)
            {
                active = false;
            }
            yield return new WaitForSeconds(fadeFrequency);
            UduinoManager.Instance.analogWrite(9, intensity);
        }

        if (debug == true) print("led fade in finished");
        MissionControl.state = MissionControl.states.awaitingInput;
    }

    public static IEnumerator FadeOutLED()
    {
        if (debug == true) print("led fade out started");

        // Init
        bool active = true;
        if (intensity != 255) yield return null;
        UduinoManager.Instance.pinMode(9, PinMode.PWM);

        while (active)
        {
            int factor = (int) fadeCurve.Evaluate(intensity / 255f);
            intensity -=  fadeAmount * factor;
            intensity = Mathf.Clamp(intensity, 0, 255);

            yield return new WaitForSeconds(fadeFrequency);
            UduinoManager.Instance.analogWrite(9, intensity);

            if (intensity == 0)
            {
                active = false;
            }
        }

        // Force LOW
        yield return new WaitForSeconds(0.4f);
        UduinoManager.Instance.pinMode(9, PinMode.Input);

        if (debug == true) print("fade out finished");
    }

    public static void ExitLeds()
    {
        UduinoManager.Instance.pinMode(9, PinMode.Input);
    }

    // Touch Sensor
    public static void OnDataReceived(string data, UduinoDevice uduinoBoard)
    {
        if (data.Contains("1")
        && !scanTrigger
        && MissionControl.state == MissionControl.states.awaitingInput)
        {
            if (debug == true) print("touch detected");
            scanTrigger = true;
        }
    }
}

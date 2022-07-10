using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enable Uduino
using Uduino;

public class Larduino : MonoBehaviour
{
    // Editor Input
    [SerializeField] int IterationTime = 3;
    [SerializeField][Range(0,5)] int fadeAmount = 1;
    [SerializeField][Range(0,0.5f)] float fadeFrequency = 0.01f;
    [SerializeField] AnimationCurve fadeCurve;
    [SerializeField] bool debug = false;
    [SerializeField] [Range(1,10000)] public int touchThreshold = 1000;

    // Variables
    private int intensity;
    public static bool saveFrameTrigger;
    public static bool triggerScan;
    bool init;

    // Start is called before the first frame update
    void Start()
    {
        // INIT
        init = true;
        intensity = 0;
        UduinoManager.Instance.pinMode(9, PinMode.PWM);
        UduinoManager.Instance.OnDataReceived += OnDataReceived;
        
        IterationTime *= 60;
    }

    void Update()
    {
        if (init)
        {
            UduinoManager.Instance.sendCommand("t", touchThreshold);
            if (Time.timeSinceLevelLoad > 0.1f) init = false;
        }
    }

    public IEnumerator FadeInLED()
    {
        if (debug == true) print("fade in started");

        //INIT
        bool active = true;
        if (intensity != 0) yield return null;

        while (active == true)
        {
            intensity += fadeAmount;
            Mathf.Clamp(intensity, 0, 255);

            if (intensity > 254)
            {
                intensity = 255;
                active = false;
                if (debug == true) print("fade in finished");
            }
            yield return new WaitForSeconds(fadeFrequency);
            UduinoManager.Instance.analogWrite(9, intensity);
        }
        watchForInput.ready = true;
    }

    public IEnumerator FadeOutLED()
    {
        if (debug == true) print("fade out started");

        //INIT
        bool active = true;
        if (intensity != 255) yield return null;

        while (active == true)
        {
            int factor = (int) fadeCurve.Evaluate(intensity / 255f);
            //print(factor);
            intensity -=  fadeAmount * factor;
            Mathf.Clamp(intensity, 0, 255);

            if (intensity < 1)
            {
                intensity = 0;
                active = false;
                if (debug == true) print("fade out finished");
            }
            yield return new WaitForSeconds(fadeFrequency);
            UduinoManager.Instance.analogWrite(9, intensity);
        }
    }

    public IEnumerator WaitIteration()
    {
        if (debug == true) print("waiting period started");
        yield return new WaitForSeconds(IterationTime);
        saveFrameTrigger = true;
        StartCoroutine(FadeInLED());
        if (debug == true) print("ready for next input");
    }

    // Touch Sensor Stuff
    void OnDataReceived(string data, UduinoDevice uduinoBoard)
    {
        //print("received data: " + data);
        bool isTouch = data.Contains("1");
        if (isTouch && watchForInput.ready)
        {
            if (debug == true) print("touch detected");
            triggerScan = true;
            StartCoroutine(FadeOutLED());
        }
    }
    
}

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
    [SerializeField] bool debug = false;

    // Variables
    private int intensity;

    // Start is called before the first frame update
    void Start()
    {
        // INIT
        intensity = 0;
        UduinoManager.Instance.pinMode(9, PinMode.PWM);
        IterationTime *= 60;
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
            intensity -=  fadeAmount;
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
        StartCoroutine(FadeInLED());
        if (debug == true) print("ready for next input");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Custom
using UnityEngine.Networking;
using System.Diagnostics;
using System;

using System.IO;
using System.Linq;

public class MissionControl : MonoBehaviour
{
    // State Display
    public enum states {
        init,
        awaitingInput,
        scanning,
        loading,
        analyzing,
        presenting
    }
    public static states state;
    public states displayState;

    // Editor Input
    [SerializeField] float scanTime = 30;
    [SerializeField] float presentationTime = 30*60;
    [SerializeField] public string rootpath;
    [SerializeField] bool debug = false;

    // Output
    public static Texture2D scan;
    public static string resultPath;
    public static bool captureImage;

    // State bools
    bool loading;
    bool analyzing;
    bool presenting;

    // Start is called before the first frame update
    void Start()
    {
        // create states
        state = new states();

        // disable cursor
        Cursor.visible = false;

        Init();

        // init public static
        resultPath = rootpath + "/DATA/RESULTS/";
    }

    void Init()
    {
        StartCoroutine(Larduino.FadeInLED());
    }

    // Update is called once per frame
    void Update()
    {
        // for display in inspector only
        displayState = state;

        switch (state)
        {
            case states.awaitingInput:
                if (Larduino.scanTrigger)
                {
                    StartScanner();
                    state = states.scanning;
                    Larduino.scanTrigger = false;

                    presenting = false;
                }
                break;

            case states.scanning:
                break;

            case states.loading:
                if (!loading)
                {
                    StartCoroutine(LoadScan());

                    loading = true;
                }
                break;

            case states.analyzing:
                if (!analyzing)
                {
                    StartCoroutine(Analyzer.AnalyzeScan(scan));

                    loading = false;
                    analyzing = true;
                }
                break;

            case states.presenting:
                if (!presenting)
                {
                    StartCoroutine(Presentation(presentationTime));

                    analyzing = false;
                    presenting = true;
                }
                break;
        }

    }

    // scanning
    void StartScanner()
    {
        if (debug) print("scanning object");
        Process.Start(rootpath + "/beginScan.app");
        StartCoroutine(Larduino.FadeOutLED());
        StartCoroutine(Scan(scanTime));
    }

    IEnumerator Scan(float scanTime)
    {
        yield return new WaitForSeconds(scanTime);
        if (debug) print("scan finished");
        state = states.loading;
    }

    // loading
    IEnumerator LoadScan()
    {
        if (debug) print("loading scan");

        var directory = new DirectoryInfo(rootpath + "/DATA/SCANS/");
        var path = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First(); // using System.linq this returns the file that was modified last

        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
        yield return www.SendWebRequest();
        if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            UnityEngine.Debug.Log(www.error);
        }
        else
        {
            scan = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }

        if (debug) print("scan loaded");
        state = states.analyzing;
    }

    // presententing
    public IEnumerator Presentation(float presentationTime)
    {
        if (debug) print("presentation starting");
        yield return new WaitForSeconds(presentationTime);

        captureImage = true;
        StartCoroutine(Larduino.FadeInLED());
        if (debug) print("ready for next input");
    }

    // various
    void OnApplicationQuit()
    {
        Larduino.ExitLeds();
    }
}
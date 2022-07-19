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

    // TO BE REMOVED
    public static bool ready;
    public static bool newInput;
    public static bool scanStarted;
    bool setupComplete;
    public static string fileID;

    // Start is called before the first frame update
    void Start()
    {
        // create states
        state = new states();

        // disable cursor
        Cursor.visible = false;

        Init();
    }

    void Init()
    {
        state = states.awaitingInput;
        StartCoroutine(Larduino.FadeInLED());
    }

    // Update is called once per frame
    void Update()
    {
        // update state display
        displayState = state;

        switch (state)
        {
            case states.awaitingInput:
                if (Larduino.scanTrigger)
                {
                    StartScanner();
                    state = states.scanning;
                    Larduino.scanTrigger = false;
                }
                break;
            case states.analyzing:
                StartCoroutine(LoadScan());
                break;
        }
    }

    // Functions
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
            StartCoroutine(Presentation(presentationTime));
        }
    }

    public IEnumerator Presentation(float presentationTime)
    {
        if (debug) print("presentation starting");
        state = states.presenting;
        Analyzer.AnalyzeScan(scan);
        yield return new WaitForSeconds(presentationTime);

        //saveFrameTrigger = true;

        if (debug) print("ready for next input");
        state = states.awaitingInput;
        StartCoroutine(Larduino.FadeInLED());
    }

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
        state = states.analyzing;
    }
}
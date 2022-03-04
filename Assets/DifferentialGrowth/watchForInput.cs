using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Custom
using UnityEngine.Networking;
using System.Diagnostics;
using SimpleFileBrowser;
using System.IO;
using System;

public class watchForInput : MonoBehaviour
{
    // Editor Input
    [SerializeField] Larduino larduino;
    [SerializeField] float WaitbeforeMoving = 30;
    [SerializeField] bool debug = false;

    // Variables
    System.IO.DirectoryInfo scanDirectory;
    string path;
    bool setupComplete;
    public static string folderpath;
    public static string fileID;

    // Output
    public static Texture2D scan;
    public static bool ready;
    public static bool newInput;
    public static bool scanStarted;

    // Start is called before the first frame update
    void Start()
    {
        //if (useBrowser == true) folderpath = "";
        StartCoroutine(ShowLoadDialogCoroutine());
        setupComplete = false;
        scanStarted = false;
    }

    void Init()
    {
        Directory.CreateDirectory(folderpath + "/history/");
        Directory.CreateDirectory(folderpath + "/results/");
        scanDirectory = new System.IO.DirectoryInfo(folderpath);
        Cursor.visible = false;
        StartCoroutine(larduino.FadeInLED());
    }

    // Update is called once per frame
    void Update()
    {
        if (setupComplete == true)
        {
            if(scanDirectory.GetFiles().Length > 1 && newInput == false && scanStarted == false)
            {
                if (debug == true) print("started HandleInput coroutine");
                StartCoroutine(HandleInput());
                scanStarted = true;
            }

            if (Input.GetMouseButtonDown(1)/*Input.GetKeyDown("space")*/ && ready == true)
            {
                StartScanner();
                ready = false;
            }
        }
    }

    // Functions
    IEnumerator HandleInput()
    {
        // Wait until Scan is complete
        yield return new WaitForSeconds(WaitbeforeMoving);

        if (debug == true) print("file moving process started");
        // At [0] there is "history" folder
        path = Directory.GetFiles(folderpath)[1];

        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
        yield return www.SendWebRequest();
        if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            UnityEngine.Debug.Log(www.error);
        }
        else
        {
            scan = ((DownloadHandlerTexture)www.downloadHandler).texture;
            newInput = true;
        }

        string sourceFile = path;
        // !!! FILE NAMING CONVENTION ALLOWS FOR MAX ONE SCAN PER SECOND ONLY otherwise files will be overwritten !!!
        fileID = DateTime.Now.ToString("yyyyMMddHHmmss");
        string destinationFile = (folderpath + "/history/" + "/visitor_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg");
        System.IO.File.Move(sourceFile, destinationFile);

        scanStarted = false;
        StartCoroutine(larduino.WaitIteration());
        if (debug == true) print("HandleInput coroutine finished");
    }
    IEnumerator ShowLoadDialogCoroutine()
	{
		yield return FileBrowser.WaitForLoadDialog( FileBrowser.PickMode.Folders, false, null, null, "Select folder that contains the app", "Load" );

		if (debug == true) print( FileBrowser.Success );

		if( FileBrowser.Success )
		{
			// Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
			for( int i = 0; i < FileBrowser.Result.Length; i++ )
            {
				if (debug == true) print( FileBrowser.Result[i] );
                folderpath = FileBrowser.Result[i] + "/DATA";
            }
            Init();
            setupComplete = true;
		} else {
            Application.Quit();
        }
	}

    void StartScanner()
    {
        StartCoroutine(larduino.FadeOutLED());
        Process.Start(folderpath + "/beginScan.app");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Custom
using UnityEngine.Networking;
using System.IO;
using System;

public class watchForInput : MonoBehaviour
{
    // Editor Input
    [SerializeField] string folderpath;

    // Variables
    System.IO.DirectoryInfo scanDirectory;
    string path;

    // Output
    public Texture2D scan;
    public bool newInput;

    // Start is called before the first frame update
    void Start()
    {
        Directory.CreateDirectory(folderpath + "/history/");
        scanDirectory = new System.IO.DirectoryInfo(folderpath);
    }

    // Update is called once per frame
    void Update()
    {
        if(scanDirectory.GetFiles().Length > 1 && newInput == false)
        {
            // At [0] there is "history" folder
            path = Directory.GetFiles(folderpath)[1];
            StartCoroutine(GetTexture());

            string sourceFile = path;
            // !!! FILE NAMING CONVENTION ALLOWS FOR MAX ONE SCAN PER SECOND ONLY otherwise files will be overwritten !!!
            string destinationFile = (folderpath + "/history/" + "/visitor_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg");
            System.IO.File.Move(sourceFile, destinationFile);
        }
    }

    // Functions
    IEnumerator GetTexture()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
        yield return www.SendWebRequest();
        if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            scan = ((DownloadHandlerTexture)www.downloadHandler).texture;
            newInput = true;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System;

public class FileManager : MonoBehaviour
{
    [SerializeField]
    bool alphaIsTransparency, isReadable;
    [SerializeField]
    TextureImporterNPOTScale npotScale;
    [SerializeField]
    TextureWrapMode wrapMode;
    [SerializeField]
    FilterMode filterMode;
    string path;
    [SerializeField]
    RawImage rawImage;
    int y = 1;
    string[] pathname = null;

    [SerializeField]
    string folderpath;
    //[SerializeField]
    //string firstFilePath;
    
    
    void Start()
    {
        Directory.CreateDirectory(folderpath + "/Benutzt/");
    }

    void Update() 
    {
        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(folderpath);
        
        int count = dir.GetFiles().Length;
        //Debug.Log(count);
        
        if(y != count)
        {
            pathname = Directory.GetFiles(folderpath);
            path = pathname[1];
            Debug.Log(path);
            StartCoroutine(GetTexture());

            string sourceFile = path;
            string destinationFile = (folderpath + "/Benutzt/" + "/Visitor" + DateTime.Now.ToFileTime() + ".jpg");
            // To move a file or folder to a new location:
            System.IO.File.Move(sourceFile, destinationFile);
            count = dir.GetFiles().Length;
            
            y = count;
        }
    }

    IEnumerator GetTexture()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);

        yield return www.SendWebRequest();

        TextureSettings();

        if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            rawImage.texture = myTexture;
        }
    }
    void TextureSettings() // in Filemanager
    {
        alphaIsTransparency = true;
        npotScale = TextureImporterNPOTScale.None;
        isReadable = true;
        wrapMode = TextureWrapMode.Clamp;
        filterMode = FilterMode.Point;
    }
}
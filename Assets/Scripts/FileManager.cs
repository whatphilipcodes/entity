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
    string path;
    [SerializeField]
    RawImage rawImage;
    int y = 1;
    string[] pathname = null;
        
    Texture2D t2D;
    

    void Start()
    {
        //rawImage = GetComponent<RawImage> ();
        //t2D = rawImage.texture as Texture2D;
        //t2D = rawImage.texture as Texture2D;
        //t2D = (Texture2D) rawImage.texture;
        //OpenFileExplorer();
        
    }

    void Update() 
    {
        StartCoroutine(FileWatching(path, rawImage, y, pathname, t2D));
    }

    /*public void OpenFileExplorer()
    {
        path = EditorUtility.OpenFilePanel("VisitorScans (.png)", "/Users/laraketzenberg/Desktop/VisitorScans", "png"); // EXCHANGE "FILENAME" AND "FILEPATH"
        StartCoroutine(GetTexture());
    }*/

    /*
    IEnumerator GetTexture()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + path);

        yield return www.SendWebRequest();

        if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            rawImage.texture = myTexture;
        }
    }*/

    IEnumerator FileWatching(string path, RawImage rawImage, int y, string[] pathname, Texture2D t2D)
    {
        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo("/Users/laraketzenberg/Desktop/VisitorScans");
        int count = dir.GetFiles().Length;
        //Debug.Log(count);
        
        if(y != count)
        {
            pathname = Directory.GetFiles("/Users/laraketzenberg/Desktop/VisitorScans");
            //Debug.Log(pathname[0]); //.DS_Store??
            path = pathname[1]; //Xchange to 0 if ".DS_Store" is not in Folder
            Debug.Log(path);
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + path);

            yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(www.error);
                }
            else
                {
                    Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    t2D = myTexture;
                    rawImage.texture = myTexture;
                }

            string sourceFile = path;
            string destinationFile = "/Users/laraketzenberg/Desktop/VisitorScans/Benutzt/";
            // To move a file or folder to a new location:
            System.IO.File.Move(sourceFile, destinationFile + Path.GetFileName(path));
            count = dir.GetFiles().Length;
            ColorPicker(); // nicht vor System.IO.File.Move setzen
            y = count;
        }
    }

    void ColorPicker()
    {
        //t2D = rawImage.texture.GetPixels();
        var pixelData = t2D.GetPixels ();
        print("Total pixels" + pixelData.Length);
        var colorIndex = new List<Color> ();
        var total = pixelData.Length;
        for(var i = 0; i < total; i++)
        {
            var color = pixelData [i];
            if(colorIndex.IndexOf(color) == -1)
            {
                colorIndex.Add (color);
            }
        }

        print("Indexed colors" + colorIndex.Count);
        foreach(var color in colorIndex)
        {
            print(color); // –– RGB Farbausgabe
            //print("#" + ColorUtility.ToHtmlStringRGB(color)); // –– Hex Farbausgabe
        }
    }
}
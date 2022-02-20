using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System;

public class getColor : MonoBehaviour
{   
    string path;
    [SerializeField] GameObject diffGrower;
    [SerializeField] Texture2D tex;
    [SerializeField] public Color result;
    [SerializeField] [Range(0,1)] float thresh;
    int y = 1;
    string[] pathname = null;

    [SerializeField] string folderpath;
    public bool newEntity = false;
    bool newTexture = false;
    bool init = true;
    
    
    void Start()
    {
        Directory.CreateDirectory(folderpath + "/Benutzt/");
    }

    void Update() 
    {
        if (newTexture == true)
        {
            result = AverageColor(tex);
            newEntity = true;
            //print(result);

            newTexture = false;
        }

        if (newEntity == true && init == true)
        {
            diffGrower.SetActive(true);
            init = false;
        }

        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(folderpath);
        
        int count = dir.GetFiles().Length;
        
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

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }

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
            tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
            newTexture = true;
        }
    }

    Color AverageColor (Texture2D tex)
    {
        var texColors = tex.GetPixels();
        var total = 0;
        float r = 0, g = 0, b = 0;
        for (var i = 0; i < texColors.Length; i++)
        {
            float bright;
            Color.RGBToHSV(texColors[i], out _, out _, out bright);
            if ( bright > thresh )
            {
                //print(bright);
                r += texColors[i].r;
                g += texColors[i].g;
                b += texColors[i].b;
                total++;
            }
        }
        return new Color (r/total, g/total, b/total, 0);
    }
}
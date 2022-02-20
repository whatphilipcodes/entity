using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System;

public class getColors : MonoBehaviour
{   
    string path;
    [SerializeField] GameObject diffGrower;
    [SerializeField] Texture2D tex;
    [SerializeField] [Range(0,1)] float thresh;
    int y = 1;
    string[] pathname = null;

    [SerializeField] string folderpath;
    [SerializeField] public int colorAmount = 512;
    public bool newEntity = false;
    bool newTexture = false;
    bool init = true;


    public Color[] results;
    
    
    void Start()
    {
        Directory.CreateDirectory(folderpath + "/Benutzt/");
    }

    void Update() 
    {
        if (newTexture == true)
        {
            int counter = 0;
            var colorList = GetColorList(tex);
            results = new Color[colorAmount];
            for (int i = 0; i < colorAmount; i++)
            {
                results[i] = colorList[counter];
                counter += colorList.Count / colorAmount;
            }
            newTexture = false;
            newEntity = true;
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

    List<Color> GetColorList (Texture2D tex)
    {
        List<Color> colors = new List<Color>();
        var texColors = tex.GetPixels();
        for (var i = 0; i < texColors.Length; i++)
        {
            float bright;
            Color.RGBToHSV(texColors[i], out _, out _, out bright);
            if ( bright > thresh )
            {
                colors.Add(texColors[i]);
            }
        }
        return colors;
    }
}

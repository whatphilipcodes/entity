using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Texture2DEditor : MonoBehaviour
{
    public Texture2D t2D;
    Vector2 mousePos = new Vector2();
    RectTransform rect;
    int width = 1072;
    int height = 1072;

    void Start()
    {   
        RawImage rawImage = GetComponent<RawImage> ();
        t2D = rawImage.texture as Texture2D;
        //InitGetMousePosition(rawImage);
        //ColorPicker();
        ClearTexture();
    }

    void Update()
    {
        //GetMousePosition();
        if (Input.GetMouseButtonDown(0))
        {
            DrawLine();
        }
        t2D.Apply();
    }

    void ClearTexture()
    {
        for(int yStep = 0; yStep < height; yStep++)
        {
            for(int xStep = 0; xStep < width; xStep++)
            {
                t2D.SetPixel(xStep,yStep,new Color(0,0,0));
            }
        }
    }

    void DrawLine()
    {
        int ax = UnityEngine.Random.Range(0,1072); 
        int bx = UnityEngine.Random.Range(0,1072); 
        int ay = UnityEngine.Random.Range(0,1072); 
        int by = UnityEngine.Random.Range(0,1072); 

        //t2D.SetPixel(ax,ay,new Color(255,255,255)); // –– Einzelne Punkte setzen

        int m = 0;
            
        if(bx != ax)
        {
            m = (by - ay) / (bx - ax); 
        }

        int q = ax;
        int s = ay;

        for(int i = 0; i < Math.Abs(ax-bx); i++)
        {
            s = s + m;
            q = q + 1;
            t2D.SetPixel(q,s,new Color(255,255,255));
        }
    }

    void ColorPicker()
    {
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

    void InitGetMousePosition(RawImage rawImage)
    {
        rect = rawImage.GetComponent<RectTransform>();
        width = (int) rect.rect.width;
        height = (int) rect.rect.height;
    }

    void GetMousePosition()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, Camera.main, out mousePos);
        mousePos.x = width - (width / 2 - mousePos.x);
        mousePos.y = Mathf.Abs((height / 2 - mousePos.y) - height);
        print("Mouse" + mousePos.x + "," + mousePos.y);
    }
    

}

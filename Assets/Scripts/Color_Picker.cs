using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Color_Picker : MonoBehaviour
{
    [SerializeField]
    Texture2D t2D;
    Vector2 mousePos = new Vector2();
    RectTransform rect;
    int width = 0;
    int height = 0;

    void Start()
    {  
        RawImage rawImage = GetComponent<RawImage> ();
        rect = rawImage.GetComponent<RectTransform>();
        width = (int) rect.rect.width;
        height = (int) rect.rect.height;
        t2D = rawImage.texture as UnityEngine.Texture2D;




        //var pixelData = t2D.GetPixel(-224,103);
        //var colorIndex = new List<Color> ();
        //var color = pixelData;
        //print(color); // –– RGB Farbausgabe
        //print("#" + ColorUtility.ToHtmlStringRGB(color)); // –– Hex Farbausgabe

        var pixelData = t2D.GetPixels((int)mousePos.x,(int)mousePos.y,1,1);
        //print("Total pixels" + pixelData.Length);
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

        //print("Indexed colors" + colorIndex.Count);
        foreach(var color in colorIndex)
        {
            //print(color); // –– RGB Farbausgabe
            print("#" + ColorUtility.ToHtmlStringRGB(color)); // –– Hex Farbausgabe
        }
    }
    void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, Camera.main, out mousePos);
        mousePos.x = width - (width / 2 - mousePos.x);
        if(mousePos.x > width || mousePos.x < 0)
        {
            mousePos.x = -1;
        }
        mousePos.y = Mathf.Abs((height / 2 - mousePos.y) - height);
        if(mousePos.y > height || mousePos.y < 0)
        {
            mousePos.y = -1;
        }
        //print("Mouse" + mousePos.x + "," + mousePos.y);

        if (Input.GetMouseButtonDown(0))
        {
            if(mousePos.x > -1 && mousePos.y > -1)
            {
                var color = t2D.GetPixel((int)mousePos.x, (int)mousePos.y);
                print("Sampled #" + ColorUtility.ToHtmlStringRGB(color)); // –– Hex Farbausgabe
            }
        }
    }  
}
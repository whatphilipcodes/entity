using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Color_Picker : MonoBehaviour
{
    [SerializeField]
    Image colorPreview;
    [SerializeField]
    Texture2D t2D;
    Vector2 mousePos = new Vector2();
    RectTransform rect;
    int width = 0;
    int height = 0;
    
    //Color backColor = new Color(0,1,0,1);

    void Start()
    {  
        var rawImage = GetComponent<RawImage> ();
        rect = rawImage.GetComponent<RectTransform>();
        width = (int) rect.rect.width;
        height = (int) rect.rect.height;
        t2D = rawImage.texture as Texture2D;

        /*
        Color backgroundColor = new Color(0,0,0,0);

        if(color(0,0,0,1) == true)
        {
            t2D.SetPixel(new Color(0,0,0,0));
        }
        */
                

        //var pixelData = t2D.GetPixel(-224,103);
        //var colorIndex = new List<Color> ();
        //var color = pixelData;
        //print(color); // –– RGB Farbausgabe
        //print("#" + ColorUtility.ToHtmlStringRGB(color)); // –– Hex Farbausgabe
        
        //–––––––––––––––––––––––––––––––––––––––––– alle farben auslesen
        /*var pixelData = t2D.GetPixels();
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
            //print(color); // –– RGB Farbausgabe
            print("#" + ColorUtility.ToHtmlStringRGB(color)); // –– Hex Farbausgabe
        }
        */
       /*for (int y = 0; y < t2D.height; y++)
        {
            for (int x = 0; x < t2D.width; x++)
            {
                if(ColorUtility.ToHtmlStringRGB(t2D.GetPixel(x,y)) == ColorUtility.ToHtmlStringRGB(backColor))
                {
                // Change the pixel to another color
                t2D.SetPixel(x, y, new Color(0,0,0,0));              
                }
            }
        }
        t2D.Apply();*/   
    }
    void Update()
    {
        var rawImage = GetComponent<RawImage> ();
        rect = rawImage.GetComponent<RectTransform>();
        width = (int) rect.rect.width;
        height = (int) rect.rect.height;
        t2D = rawImage.texture as Texture2D;

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

                //print(color); // –– RGB Farbausgabe
                colorPreview.color = new Color(color.r, color.g, color.b);
                //print(backColor);


                //print("Sampled #" + ColorUtility.ToHtmlStringRGB(backColor));

                
                //var color2 = color;
                //print(color2);
                /*
                if(ColorUtility.ToHtmlStringRGB(color) == ColorUtility.ToHtmlStringRGB(backColor))
                {
                    print("jey");
                    t2D.SetPixel(new Color(0,1,0,1));
                }
                */
            }
        }
    }  
}
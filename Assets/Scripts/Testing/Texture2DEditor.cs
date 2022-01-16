using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Texture2DEditor : MonoBehaviour
{
    Texture2D t2D;
    Vector2 mousePos = new Vector2();
    RectTransform rect;
    int width = 0;
    int height = 0;

    // Start is called before the first frame update
    void Start()
    {
        var rawImage = GetComponent<RawImage> ();
        rect = rawImage.GetComponent<RectTransform>();

        width = (int) rect.rect.width;
        height = (int) rect.rect.height;

        t2D = rawImage.texture as Texture2D;
        var pixelData = t2D.GetPixels ();
        print("Total pixels" + pixelData.Length);

        //Color Picker
        var colorIndex = new List<Color> ();
        var total = pixelData.Length;
        for(var i = 0; i < total; i++){
            var color = pixelData [i];
            if(colorIndex.IndexOf(color) == -1){
                colorIndex.Add (color);
            }
        }

        print("Indexed colors" + colorIndex.Count);
        foreach(var color in colorIndex){
            //print(color); –– RGB Farbausgabe
            print("#" + ColorUtility.ToHtmlStringRGB(color)); // –– Hex Farbausgabe
        }
    }

    // Update is called once per frame
    void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, Camera.main, out mousePos);
        mousePos.x = width - (width / 2 - mousePos.x);
        mousePos.y = Mathf.Abs((height / 2 - mousePos.y) - height);
        print("Mouse" + mousePos.x + "," + mousePos.y);
        for (int i = 0; i <= 1072; i++){
            t2D.SetPixel(i,i,new Color(255,255,255));
        
        }
        t2D.Apply();
    }
}

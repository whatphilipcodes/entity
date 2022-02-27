using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seed.Utilities {
    public static class Utils
    {
        // Custom modulo operation dealing with negative indices
        // see: https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain/1082938
        public static int mod (int k, int n)
        {
            return ((k %= n) < 0) ? k+n : k;
        }

        public static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            Color[] rpixels = result.GetPixels(0);

            float incX = (1.0f / (float) targetWidth);
            float incY = (1.0f / (float) targetHeight);

            for(int px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float) px % targetWidth), incY * ((float) Mathf.Floor(px / targetWidth)));
            }
            result.SetPixels(rpixels, 0);
            result.Apply();

            return result;
        }
    }
}

// USEFUL CODE

/*

///////////////////////////
if (Input.GetKey("escape"))
{
    Application.Quit();
}
///////////////////////////

*/

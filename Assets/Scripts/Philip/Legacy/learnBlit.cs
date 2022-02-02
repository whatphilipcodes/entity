using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class learnBlit : MonoBehaviour
{ // Copies aTexture to rTex and displays it in all cameras.

    [SerializeField]
    Texture aTexture;
    [SerializeField]
    RenderTexture rTex;

    void Start()
    {
        if (!aTexture || !rTex)
        {
            Debug.LogError("A texture or a render texture are missing, assign them.");
        }
    }

    void Update()
    {
        Graphics.Blit(aTexture, rTex);
    }
}

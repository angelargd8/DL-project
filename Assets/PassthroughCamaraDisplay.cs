using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PassthroughCameraSamples;

public class PassthroughCamaraDisplay : MonoBehaviour
{
    public WebCamTextureManager webCamManager;
    public Renderer quadRenderer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (webCamManager.WebCamTexture != null)
        {
            quadRenderer.material.mainTexture = webCamManager.WebCamTexture;
        }
    }
}

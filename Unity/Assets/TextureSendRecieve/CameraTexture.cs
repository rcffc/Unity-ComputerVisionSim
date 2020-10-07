using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class CameraTexture : MonoBehaviour
{
    RenderTexture rt;
    Camera camera;

    void Start()
    {
        rt = new RenderTexture(512, 512, 24, GraphicsFormat.R8G8B8A8_UNorm);
        camera = GetComponent<Camera>();
        camera.targetTexture = rt;
    }
}

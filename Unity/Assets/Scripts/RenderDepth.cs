using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace TextureSendReceive
{
    [RequireComponent(typeof(TextureSender))]
    [ExecuteInEditMode]
    public class RenderDepth : MonoBehaviour
    {
        [Range(0f, 3f)]
        public float depthLevel = 0.5f;

        private Shader _shader;
        private Shader shader
        {
            get { return _shader != null ? _shader : (_shader = Shader.Find("Custom/RenderDepth")); }
        }

        Camera camera;

        TextureSender sender;
        Texture2D sendTexture;

        public RawImage image;

        private Material _material;
        private Material material
        {
            get
            {
                if (_material == null)
                {
                    _material = new Material(shader);
                    _material.hideFlags = HideFlags.HideAndDontSave;
                }
                return _material;
            }
        }

        private void Start()
        {
            if (!SystemInfo.supportsImageEffects)
            {
                print("System doesn't support image effects");
                enabled = false;
                return;
            }
            if (shader == null || !shader.isSupported)
            {
                enabled = false;
                print("Shader " + shader.name + " is not supported");
                return;
            }

            camera = GetComponent<Camera>();
            // turn on depth rendering for the camera so that the shader can access it via _CameraDepthTexture
            camera.depthTextureMode = DepthTextureMode.Depth;

            sender = GetComponent<TextureSender>();

            sendTexture = new Texture2D((int)camera.targetTexture.width, (int)camera.targetTexture.height);

            sender.SetSourceTexture(sendTexture);
        }

        private void OnDisable()
        {
            if (_material != null)
                DestroyImmediate(_material);
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (shader != null)
            {
                material.SetFloat("_DepthLevel", depthLevel);
                Graphics.Blit(src, dest, material);
            }
            else
            {
                Graphics.Blit(src, dest);
            }
        }

        void Update()
        {
            RenderTexture.active = camera.targetTexture;
            sendTexture.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0, false);

            // Set preview image target
            image.texture = camera.targetTexture;

            if (Input.GetKeyDown("space"))
            {
                long timestamp = DateTime.Now.ToFileTime();
                string path = "D:/projects/Sending/Unity/Obj";
                SaveMesh(timestamp, path);
                SavePicture(timestamp, path);
            }
        }

        void SaveMesh(long timestamp, string path)
        {
            int width = camera.targetTexture.width;
            int height = camera.targetTexture.height;

            string[] output = new string[width * height];
            Color[] pixelInfo = sendTexture.GetPixels();

            string fileName = timestamp + ".xyz";
            using (StreamWriter file = new StreamWriter(@Path.Combine(path, fileName)))
            {
                for (int i = 0; i < pixelInfo.Length; i++)
                {
                    float r, g, b;
                    double x, y, z, fl, range;
                    double realX, realY, realZ;

                    // Obtain X and Y Pixel Coordinates
                    double pixelX = i % width;
                    double pixelY = i / height;

                    // r = g = b because we are getting the value from the depth grayscale image
                    range = pixelInfo[i].r;

                    x = (pixelX / width) - 0.5;
                    y = (-(pixelY - height) / height) - 0.5;
                    fl = -0.5 / (Math.Tan((camera.fieldOfView / 2) * Math.PI / 180));
                    z = fl;

                    double vecLength = Math.Sqrt((x * x) + (y * y) + (z * z));

                    r = (int)(pixelInfo[i].r * 255);
                    g = (int)(pixelInfo[i].g * 255);
                    b = (int)(pixelInfo[i].b * 255);
                    // unitize the vector
                    x /= vecLength;
                    y /= vecLength;
                    z /= vecLength;
                    // multiply vector components by range to obtain real x, y, z
                    realX = x * range;
                    realY = y * range * -1;
                    realZ = z * range;

                    file.WriteLine(realX + " " + realY + " " + realZ);
                }
            }
        }

        void SavePicture(long timestamp, string path)
        {
            sendTexture.Apply();
            var imageData = sendTexture.EncodeToPNG();
            string fileName = timestamp + ".png";
            File.WriteAllBytes(Path.Combine(path, fileName), imageData);
        }
    }
}
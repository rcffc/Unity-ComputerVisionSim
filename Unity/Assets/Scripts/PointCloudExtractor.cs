using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace TextureSendReceive
{
    [RequireComponent(typeof(TextureSender))]

    public class PointCloudExtractor : MonoBehaviour
    {

        Camera camera;
        TextureSender sender;
        Texture2D sendTexture;

        public RawImage image;

        // Start is called before the first frame update
        void Start()
        {
            camera = GetComponent<Camera>();
            sender = GetComponent<TextureSender>();

            sendTexture = new Texture2D((int)camera.targetTexture.width, (int)camera.targetTexture.height);

            sender.SetSourceTexture(sendTexture);
        }

        // Update is called once per frame
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
                    //fl = -0.5 / (Math.Tan((camera.fieldOfView / 2) * Math.PI / 180));
                    //z = fl;
                    z = range;
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
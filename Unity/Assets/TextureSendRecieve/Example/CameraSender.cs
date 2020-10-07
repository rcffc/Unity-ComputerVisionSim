﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace TextureSendReceive {
	[RequireComponent(typeof(TextureSender))]
	public class CameraSender : MonoBehaviour {
		public Camera camera;
		TextureSender sender;
		Texture2D sendTexture;

		public RawImage image;

		// Use this for initialization
		void Start () {
			// camera = GetComponent<Camera>();
			sender = GetComponent<TextureSender>();

			sendTexture = new Texture2D((int)camera.targetTexture.width, (int)camera.targetTexture.height);
			
			// Set send texture
			sender.SetSourceTexture(sendTexture);
		}
		
		// Update is called once per frame
		void Update () {
			RenderTexture.active = camera.targetTexture;
			sendTexture.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0, false);
				
			// Set preview image target
			image.texture = camera.targetTexture;
		}
	}
}
﻿using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class BufferVideoAR : MonoBehaviour
{
    // VARIABLES
	private static BufferVideoAR instance;
	public static BufferVideoAR Instance
	{
		get
		{
			if (instance == null)
				instance = new GameObject("BufferVideoAR").AddComponent<BufferVideoAR>();

			return instance;
		}
	}
	private bool cleanVideosBuffer;
	public bool CleanVideosBuffer
	{
		get
		{
			return cleanVideosBuffer;
		}
		set
		{
			cleanVideosBuffer = value;
			CleanVideoOnBuffer();
		}
	}

	private const string nameFolder = "VideosBuffer";
	public Action<bool, string> OnGetVideo;

    // METHODS_PUBLIC

    public void GetVideoForBuffer(string uri)
	{
		StartCoroutine(GetVideo_Coroutine(uri));
	}

    // METHODS_PRIVATE

    private void CleanVideoOnBuffer()
	{
		if (cleanVideosBuffer)
		{
			string[] videos = Directory.GetFiles(GetPathVideos());

			foreach (string video in videos)
			{
				File.Delete(video);
			}

			Debug.Log("CleanVideoOnBuffer");
		}
	}

	private string GetPathVideos()
	{
		return Path.Combine(Application.persistentDataPath, nameFolder);
	}

	private bool IsVideoOnBuffer(string pathVideo)
	{
		return File.Exists(pathVideo);
	}

    // COROUTINES

    private IEnumerator GetVideo_Coroutine(string urlVideo)
    {
        string nameVideo = urlVideo.Split('/')[urlVideo.Split('/').Length - 1];
        string pathVideo = Path.Combine(GetPathVideos(), nameVideo);

        if (IsVideoOnBuffer(pathVideo))
        {
            Debug.Log("Video on buffer");
            OnGetVideo?.Invoke(false, pathVideo);
        }
        else
        {
            Debug.Log("Start get video:: " + urlVideo);
            using (UnityWebRequest webRequest = UnityWebRequest.Get(urlVideo))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    //Debug.LogError("GetVideo_Coroutine:: " + webRequest.error);
                    OnGetVideo?.Invoke(true, webRequest.error);
                }
                else
                {
                    Debug.Log("GetVideo_Coroutine");
                    byte[] bytesVideo = webRequest.downloadHandler.data;

                    if (!Directory.Exists(GetPathVideos()))
                        Directory.CreateDirectory(GetPathVideos());

                    File.WriteAllBytes(pathVideo, bytesVideo);
                    OnGetVideo?.Invoke(false, pathVideo);
                }
            }
        }
    }
}
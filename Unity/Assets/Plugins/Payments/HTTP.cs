using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Networking;

namespace Payments
{
    public class HTTP : MonoBehaviour
    {
        public static void SendPost(string URL, string Data, Action<bool, string> Response)
        {
            var restObject = new GameObject("HTTP");
            restObject.AddComponent<HTTP>().StartCoroutine (_Post(URL, Data, (isSuccess, Body) => {
                Response?.Invoke(isSuccess, Body);
                Destroy(restObject);
            }));
        }

        public static void SendGet(string URL, Action<bool, string> Response)
        {
            var restObject = new GameObject("HTTP");
            restObject.AddComponent<HTTP>().StartCoroutine(_Get(URL, (isSuccess, Body) => {
                Response?.Invoke(isSuccess, Body);
                Destroy(restObject);
            }));
        }

        public static void SendGetImage(string URL, Action<bool, Texture2D> Response)
        {
            var restObject = new GameObject("HTTP");
            restObject.AddComponent<HTTP>().StartCoroutine(_GetImage(URL, (isSuccess, T2D) => {
                Response?.Invoke(isSuccess, T2D);
                Destroy(restObject);
            }));
        }

        public static IEnumerator _Post(string URL, string Data, Action<bool, string> Response)
        {
            Debug.Log("[HTTP] Post => " + URL);
            UnityWebRequest send = UnityWebRequest.Post(URL, Data);

            UploadHandler uploader = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(Data));
            uploader.contentType = "application/json";
            send.uploadHandler = uploader;
            send.SetRequestHeader("Content-Type", "application/json");

            yield return send.SendWebRequest();

            if (!!!string.IsNullOrEmpty(send.error))
            {
                Debug.Log("[HTTP] Post failed. Network error => " + send.error);
                Response?.Invoke(false, null);
                yield break;
            }

            Debug.Log("[HTTP] Post success => " + send.downloadHandler.text);
            Response?.Invoke(true, send.downloadHandler.text);
        }

        public static IEnumerator _Get(string URL, Action<bool, string> Response)
        {
            Debug.Log("[HTTP] Get => " + URL);

            UnityWebRequest send = UnityWebRequest.Get(URL);
            yield return send.SendWebRequest();

            if (!string.IsNullOrEmpty(send.error))
            {
                Debug.Log("[HTTP] Get failed. Network error => " + send.error);
                Response?.Invoke(false, null);
                yield break;
            }

            Response?.Invoke(true, send.downloadHandler.text);
        }

        public static IEnumerator _GetImage (string URL, Action<bool, Texture2D> Response)
        {
            Debug.Log("[HTTP] GetData => " + URL);

            UnityWebRequest send = UnityWebRequestTexture.GetTexture(URL);
            yield return send.SendWebRequest();

            if (!string.IsNullOrEmpty(send.error))
            {
                Debug.Log("[HTTP] Get failed. Network error => " + send.error);
                Response?.Invoke(false, null);
                yield break;
            }

            Response?.Invoke(true, ((DownloadHandlerTexture)send.downloadHandler).texture);
        }
    }
}

using System.Collections;
using UnityEngine;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class SVMClient : MonoBehaviour
{
    public string url;
    private int prediction;
    [System.Serializable]
    public class JsonResponse
    {
        public float[] predictions;
    }


    public IEnumerator GetPrediction(List<Vector3> sequence, System.Action<int> callback)
    {
        // Convert the sequence to a 2D float array
        float[][] data = sequence.Select(v => new float[] { v.x, v.y, v.z }).ToArray();

        // Flatten the data into a 1D array
        float[] flatData = data.SelectMany(x => x).ToArray();

        UnityEngine.Debug.Log("Flattened Length" + flatData.Length);
        // Convert the data to a JSON string
        string dataJson = ToJson(flatData);
        UnityEngine.Debug.Log("Sending JSON data: " + dataJson);

        // Send a POST request to the Flask API
        yield return StartCoroutine(HttpPost(url, dataJson, prediction =>
        {
            // Call the callback function with the predicted label
            callback(prediction);
        }));
    }

    private IEnumerator HttpPost(string url, string data, System.Action<int> callback)
    {
        var encoding = new System.Text.UTF8Encoding();
        var jsonBytes = encoding.GetBytes(data);

        var request = new UnityEngine.Networking.UnityWebRequest(url, "POST");
        request.uploadHandler = (UnityEngine.Networking.UploadHandler)new UnityEngine.Networking.UploadHandlerRaw(jsonBytes);
        request.downloadHandler = (UnityEngine.Networking.DownloadHandler)new UnityEngine.Networking.DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError || request.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError)
        {
            UnityEngine.Debug.Log(request.error);
            yield break;
        }

        // Get the predicted label from the response
        string jsonResponseText = request.downloadHandler.text;
        UnityEngine.Debug.Log("JSON Response: " + jsonResponseText);

        JsonResponse jsonResponse = JsonUtility.FromJson<JsonResponse>("{\"predictions\":" + jsonResponseText + "}");
        int prediction = Mathf.RoundToInt(jsonResponse.predictions[0]);

        // Call the callback function with the predicted label
        callback(prediction);
    }

    public static string ToJson<T>(T obj)
    {
        return JsonUtility.ToJson(obj);
    }

    public static T FromJson<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.items = array;
        return JsonUtility.ToJson(wrapper);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}

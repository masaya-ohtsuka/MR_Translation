using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class Translator : MonoBehaviour {

public static Translator instance;

private string translationTokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
private string translationTextEndpoint = "https://api.microsofttranslator.com/v2/http.svc/Translate?";

private const string ocpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";

//Substitute the value of authorizationKey with your own Key
private const string authorizationKey = "cb6f35c426954555924c562e807f752a";

private string authorizationToken;

// languages set below are:
// English
// French
// Italian
// Japanese
// Korean
public enum Languages { en, fr, it, ja, ko };
public Languages from = Languages.en;
public Languages to = Languages.it;

private void Awake()
{
    // Set this class to behave similar to singleton
    instance = this;
}

// Use this for initialization
void Start()
{
    // When the application starts, request an auth token 
    StartCoroutine("GetTokenCoroutine", authorizationKey);
}

/// <summary>
/// Request a Token from Azure Translation Service by providing the access key.
/// Debugging result is delivered to the Results class.
/// </summary>
private IEnumerator GetTokenCoroutine(string key)
{
    if (string.IsNullOrEmpty(key))
    {
        throw new InvalidOperationException("Authorization key not set.");
    }

    WWWForm webForm = new WWWForm();

    using (UnityWebRequest unityWebRequest =
    UnityWebRequest.Post(translationTokenEndpoint, webForm))
    {
        unityWebRequest.SetRequestHeader("Ocp-Apim-Subscription-Key", key);

        // The download handler is responsible for bringing back the token after the request
        unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return unityWebRequest.SendWebRequest();

        authorizationToken = unityWebRequest.downloadHandler.text;

        if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
        {
            Results.instance.azureResponseText.text = unityWebRequest.error;
        }

        long responseCode = unityWebRequest.responseCode;

        // Update the UI with the response code
        Results.instance.SetAzureResponse(responseCode.ToString());
    }

    // After receiving the token, begin capturing Audio with the MicrophoneManager Class
    MicrophoneManager.instance.StartCapturingAudio();
    StopCoroutine("GetTokenCoroutine");
    yield return null;
}

/// <summary>
/// Request a translation from Azure Translation Service by providing a string. 
/// Debugging result is delivered to the Results class.
/// </summary>
public IEnumerator TranslateWithUnityNetworking(string text)
{
    WWWForm webForm = new WWWForm();
    string result;
    string queryString;

    // This query string will contain the parameters for the translation
    queryString = string.Concat("text=", Uri.EscapeDataString(text), "&from=", from, "&to=", to);

    using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(translationTextEndpoint + queryString))
    {
        unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
        unityWebRequest.SetRequestHeader("Authorization", "Bearer " + authorizationToken);
        unityWebRequest.SetRequestHeader("Accept", "application/xml");

        yield return unityWebRequest.SendWebRequest();

        string deliveredString = unityWebRequest.downloadHandler.text;

        // The response will be in Json format
        // Therefore we need to deserialise it
        DataContractSerializer serializer;
        serializer = new DataContractSerializer(typeof(string));

        using (Stream stream = GenerateStreamFromString(deliveredString))
        {
            // Set the UI with the translation
            Results.instance.SetTranslatedResult((string)serializer.ReadObject(stream));
        }

        if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
        {
            Debug.Log(unityWebRequest.error);
        }


        StopCoroutine("TranslateWithUnityNetworking");

    }
}

public static Stream GenerateStreamFromString(string incomingString)
{
    MemoryStream stream = new MemoryStream();
    StreamWriter writer = new StreamWriter(stream);
    writer.Write(incomingString);
    writer.Flush();
    stream.Position = 0;
    return stream;
}

}
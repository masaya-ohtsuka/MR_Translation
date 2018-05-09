using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;



public class MicrophoneManager : MonoBehaviour
{
    public static MicrophoneManager instance; //help to access instance of this object
    private int frequency = 44100;      //recording frequency of mic
    private AudioSource audioSource;        //AudioSource component, provides access to mic
    private bool microphoneDetected;        //flag indicating mic detection
    private bool isCapturingAudio;      //flag indicating audio capture status
    private DictationRecognizer dictationRecognizer;  //Component converting speech to text
    

    private void Awake()
    {
        // Set this class to behave similar to singleton
        instance = this;
    }

    void Start()
    {
        //Use Unity Microphone class to detect devices and setup Audiosource
        if (Microphone.devices.Length > 0)
        {
            Results.instance.SetMicrophoneStatus("Initialising...");
            audioSource = GetComponent<AudioSource>();
            microphoneDetected = true;
        }
        else
        {
            Results.instance.SetMicrophoneStatus("No Microphone detected");
        }
    }

    /// <summary>
    /// Start microphone capture. Debugging message is delivered to the Results class.
    /// </summary>
    public void StartCapturingAudio()
    {
        if (microphoneDetected)
        {
            // Start microphone capture
            isCapturingAudio = true;

            // Start dictation
            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
            dictationRecognizer.Start();

            // Update UI with mic status
            Results.instance.SetMicrophoneStatus("Capturing...");
        }
    }

    /// <summary>
    /// Stop microphone capture. Debugging message is delivered to the Results class.
    /// </summary>
    public void StopCapturingAudio()
    {
        Results.instance.SetMicrophoneStatus("Mic sleeping");
        isCapturingAudio = false;
        Microphone.End(null);
        dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
        dictationRecognizer.Dispose();
    }


    /// <summary>
    /// Start microphone capture. Debugging message is delivered to the Results class.
    /// </summary>
    //public void StartCapturingAudio()
    //{
    //    if (microphoneDetected)
    //    {
    //        // Start microphone capture
    //        isCapturingAudio = true;

    //        // Start dictation
    //        dictationRecognizer = new DictationRecognizer();
    //        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
    //        dictationRecognizer.Start();

    //        // Update UI with mic status
    //        Results.instance.SetMicrophoneStatus("Capturing...");
    //    }
    //}

    /// <summary>
    /// Stop microphone capture. Debugging message is delivered to the Results class.
    /// </summary>


    //public void StopCapturingAudio()
    //{
    //    Results.instance.SetMicrophoneStatus("Mic sleeping");
    //    isCapturingAudio = false;
    //    Microphone.End(null);
    //    dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
    //    dictationRecognizer.Dispose();
    //}

    /// <summary>
    /// This handler is called every time the Dictation detects a pause in the speech. 
    /// Debugging message is delivered to the Results class.
    /// </summary>
    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        // Update UI with dictation captured
        Results.instance.SetDictationResult(text);

        // Start the coroutine that process the dictation through Azure 
        StartCoroutine(Translator.instance.TranslateWithUnityNetworking(text));
    }

}
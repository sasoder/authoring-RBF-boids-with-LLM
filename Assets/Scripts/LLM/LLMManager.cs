using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using System;
using TMPro;
using System.Text;
using p_bois_steering_behaviors.Scripts;

public class LLMManager : MonoBehaviour
{
    // --- Event for other scripts to listen to ---
    public static event Action<LLMGenerationOutput> OnLLMResponseReceived;

    // --- UI Elements ---
    public TMP_InputField promptInputField;
    public Button voiceButton;
    public Button sendButton;
    public TextMeshProUGUI voiceButtonText;

    // --- Microphone Settings ---
    private const int RECORD_DURATION_SECONDS = 20;
    private const int SAMPLE_RATE = 16000;
    private AudioClip recordingClip;
    private string microphoneDevice;
    private bool isRecording = false;

    // --- Server Settings ---
    private const string SERVER_URL = "http://localhost:8000";
    private const string LLM_ENDPOINT = "/generate";
    private const string TRANSCRIBE_ENDPOINT = "/transcribe";

    void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("No microphone found!");
            voiceButton.interactable = false;
        }
        else
        {
            microphoneDevice = Microphone.devices[0]; // Use the default microphone
        }

        // Add listeners for UI elements
        promptInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        promptInputField.onSubmit.AddListener((_) => ProcessSendClick()); // Allow Enter key to send
        voiceButton.onClick.AddListener(ToggleRecording); // This button just Records/Stops
        sendButton.onClick.AddListener(ProcessSendClick); // The main send action
        UpdateVoiceButtonText();
        UpdateSendButtonState();
    }

    void UpdateVoiceButtonText()
    {
        voiceButtonText.text = isRecording ? "Stop Recording" : "Use Voice";
    }

    void OnInputFieldValueChanged(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            // If user starts typing, clear any previously recorded clip
            if (recordingClip != null)
            {
                Destroy(recordingClip);
                recordingClip = null;
            }
        }
        UpdateSendButtonState();
    }

    void UpdateSendButtonState()
    {
        bool hasText = !string.IsNullOrWhiteSpace(promptInputField.text);
        sendButton.interactable = hasText && !isRecording;
    }

    void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    void StartRecording()
    {
        if (string.IsNullOrEmpty(microphoneDevice)) {
            Debug.LogError("Microphone device name is null or empty!");
        }

        if (Microphone.IsRecording(microphoneDevice))
        {
            Debug.LogWarning($"Microphone '{microphoneDevice}' was already recording. Stopping it first.");
            Microphone.End(microphoneDevice);
        }

        // Clear text field and any old clip when starting recording
        promptInputField.text = "";
        if (recordingClip != null)
        {
            Destroy(recordingClip);
            recordingClip = null;
        }

        recordingClip = Microphone.Start(microphoneDevice, false, RECORD_DURATION_SECONDS, SAMPLE_RATE);

        // --- More Debugging ---
        if (recordingClip == null)
        {
             Debug.LogError("Microphone.Start failed! Returned null AudioClip. Check microphone permissions and device name.");
             isRecording = false; // Ensure state is correct
             UpdateVoiceButtonText();
             UpdateSendButtonState();
             return; // Exit if failed
        }
        // --- End Debugging Additions ---


        isRecording = true;
        UpdateVoiceButtonText();
        UpdateSendButtonState(); // Disable Send button while recording
        // Start the timeout coroutine
        StartCoroutine(StopRecordingAfterTimeout());
    }

     IEnumerator StopRecordingAfterTimeout()
     {
         yield return new WaitForSeconds(RECORD_DURATION_SECONDS);
         if (isRecording)
         {
             StopRecording(); // This will now handle processing
         }
     }

    void StopRecording()
    {
        if (!isRecording || !Microphone.IsRecording(microphoneDevice)) return;

        Microphone.End(microphoneDevice);
        isRecording = false;
        UpdateVoiceButtonText();

        if (recordingClip == null)
        {
            Debug.LogError("Recording clip was null when stopping. Cannot process audio.");
            UpdateSendButtonState(); // Update UI state even if clip is null
            return; // Exit early
        }

        // --- Automatically process the recorded audio ---
        byte[] wavData = WavUtility.FromAudioClip(recordingClip);
        if (wavData != null && wavData.Length > WavUtility.HEADER_SIZE) // Basic check for valid WAV data
        {
            StartCoroutine(SendAudioToServer(wavData));
        }
        else
        {
            Debug.LogError("Failed to convert AudioClip to valid WAV data or clip was empty.");
        }

        // Clean up the clip immediately after getting data
        Destroy(recordingClip);
        recordingClip = null;
        // --- End automatic processing ---

        UpdateSendButtonState();
    }

    void ProcessSendClick()
    {
        if (isRecording) return; // Don't send while recording

        string currentInputText = promptInputField.text;

        if (!string.IsNullOrWhiteSpace(currentInputText))
        {
            LLMGenerateRequestPayload payload = CreatePayloadFromPrompt(currentInputText);
            StartCoroutine(SendPromptToServer(payload));
            promptInputField.text = ""; // Clear text after sending
            if (recordingClip != null)
            {
                Destroy(recordingClip);
                recordingClip = null;
            }
        }
        else
        {
            Debug.LogWarning("Send button clicked but text input is empty. No action taken.");
        }
        UpdateSendButtonState();
    }

    LLMGenerateRequestPayload CreatePayloadFromPrompt(string promptText)
    {
        // Get all objects tagged as "LLMPrompt"
        GameObject[] sceneObjects = GameObject.FindGameObjectsWithTag("LLMPrompt");
        List<LLMGameObject> gameObjects = new List<LLMGameObject>();

        foreach (GameObject obj in sceneObjects)
        {
            gameObjects.Add(new LLMGameObject 
            { 
                name = obj.name,
                origin = new LLMCoordinate 
                { 
                    x = obj.transform.position.x,
                    y = obj.transform.position.y,
                    z = obj.transform.position.z
                },
                bounds = null // We could add bounds if needed
            });
        }

        Debug.Log($"Found {gameObjects.Count} objects tagged as LLMPrompt");

        // Find the GridRenderer directly
        GridRenderer gridRenderer = FindFirstObjectByType<GridRenderer>();
        if (gridRenderer == null)
        {
            Debug.LogError("Could not find GridRenderer in the scene!");
            return null;
        }

        // Get the current bounds from GridRenderer
        var (minPoint, maxPoint) = gridRenderer.GetGridBounds();
        Debug.Log($"Using GridRenderer bounds: Min({minPoint}), Max({maxPoint})");

        GameObject flock = GameObject.Find("FlockOfBirds");
        if (flock == null)
        {
            Debug.LogError("Could not find BoidFlock in the scene!");
            return null;
        }
        var randomBoid_position = flock.transform.GetChild(0).gameObject.transform.position;

        LLMGenerateRequestPayload payload = new LLMGenerateRequestPayload
        {
            prompt = promptText,
            flock_position = new LLMCoordinate { x = randomBoid_position.x, y = randomBoid_position.y, z = randomBoid_position.z },
            scene_graph = new LLMSceneGraph
            {
                world_bounds = new LLMWorldBoundsType
                {
                    min = new LLMCoordinate { x = minPoint.x, y = minPoint.y, z = minPoint.z}, 
                    max = new LLMCoordinate { x = maxPoint.x, y = maxPoint.y, z = maxPoint.z }
                },
                game_objects = gameObjects
            },
            available_styles = BoidUIManager.AvailableStyles
        };

        // Log the payload we're sending to the server
        string payloadJson = JsonUtility.ToJson(payload, true); // true for pretty printing
        Debug.Log("=== Payload being sent to server ===");
        Debug.Log(payloadJson);
        Debug.Log("===================================");

        return payload;
    }

    IEnumerator SendAudioToServer(byte[] audioData)
    {
        string url = SERVER_URL + TRANSCRIBE_ENDPOINT;

        // --- Use Multipart Form for File Upload ---
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        // "audio_file" must match the parameter name in the FastAPI endpoint
        formData.Add(new MultipartFormFileSection("audio_file", audioData, "recording.wav", "audio/wav"));

        UnityWebRequest request = UnityWebRequest.Post(url, formData);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error sending audio: {request.error}");
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            try
            {
                TranscriptionResponse transcription = JsonUtility.FromJson<TranscriptionResponse>(jsonResponse);
                if (transcription != null && !string.IsNullOrWhiteSpace(transcription.transcript))
                {
                    // --- Prepare and send the full request payload for /generate ---
                    LLMGenerateRequestPayload payload = CreatePayloadFromPrompt(transcription.transcript);
                    StartCoroutine(SendPromptToServer(payload)); // Send transcript to LLM as part of full payload
                }
                else
                {
                    Debug.LogWarning("Received empty or invalid transcription response.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse transcription response: {e.Message} Response: {jsonResponse}");
            }
        }
        request.Dispose();
    }

    IEnumerator SendPromptToServer(LLMGenerateRequestPayload payload)
    {
        string url = SERVER_URL + LLM_ENDPOINT;
        string jsonBody = JsonUtility.ToJson(payload);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.uploadHandler.contentType = "application/json";
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error sending prompt / receiving stream: {request.error} Response Code: {request.responseCode} Body: {request.downloadHandler.text}");
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                try
                {
                    LLMGenerationOutput llmOutput = JsonUtility.FromJson<LLMGenerationOutput>(jsonResponse);
                    if (llmOutput != null && llmOutput.vectors != null)
                    {
                        // Create a formatted string of the response
                        string responseLog = $"LLM Response:\nStyle: {llmOutput.style}\nVectors ({llmOutput.vectors.Count}):\n";
                        for (int i = 0; i < llmOutput.vectors.Count; i++)
                        {
                            var vector = llmOutput.vectors[i];
                            responseLog += $"Vector {i + 1}:\n";
                            responseLog += $"  Start: ({vector.s.x}, {vector.s.y}, {vector.s.z})\n";
                            responseLog += $"  End: ({vector.e.x}, {vector.e.y}, {vector.e.z})\n";
                        }
                        Debug.Log(responseLog);

                        OnLLMResponseReceived?.Invoke(llmOutput);
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to parse LLM response or output was incomplete. JSON: {jsonResponse}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing LLM JSON response: {e.Message} JSON: {jsonResponse}");
                }
            }
            else
            {
                Debug.LogWarning("Received empty response from server.");
            }
        }
        request.Dispose();
    }

    // --- Helper Class for JSON Parsing (Transcription) ---
    [System.Serializable]
    private class TranscriptionResponse // This class is fine as it's for a different endpoint
    {
        public string transcript;
    }
}

// --- WavUtility ---
// Helper class to convert AudioClip to WAV byte array
public static class WavUtility
{
    public const int HEADER_SIZE = 44;

    public static byte[] FromAudioClip(AudioClip clip)
    {
        if (clip == null) return null;

        MemoryStream stream = new MemoryStream();
        try
        {
            WriteHeader(stream, clip);
            ConvertAndWrite(stream, clip);
            stream.Seek(0, SeekOrigin.Begin); // Ensure stream position is at the beginning
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }
        finally 
        { // Ensure stream is always disposed
           if(stream != null) stream.Dispose();
        }
    }

    static void ConvertAndWrite(MemoryStream stream, AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        short[] intData = new short[samples.Length];
        // converting in 2 steps : float[] to Int16[], // Int16[] to Byte[]

        byte[] bytesData = new byte[samples.Length * 2];
        // bytesData array is twice the size of
        // dataSource array because a float converted in Int16 is 2 bytes.

        int rescaleFactor = 32767; //to convert float to Int16

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = System.BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }
        stream.Write(bytesData, 0, bytesData.Length);
    }

    static void WriteHeader(MemoryStream stream, AudioClip clip)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        stream.Seek(0, SeekOrigin.Begin);

        // RIFF chunk descriptor
        WriteStr(stream, "RIFF");
        stream.Write(System.BitConverter.GetBytes(HEADER_SIZE + samples * channels * 2 - 8), 0, 4); // ChunkSize
        WriteStr(stream, "WAVE");

        // fmt sub-chunk
        WriteStr(stream, "fmt ");
        stream.Write(System.BitConverter.GetBytes(16), 0, 4); // Subchunk1Size (16 for PCM)
        stream.Write(System.BitConverter.GetBytes((ushort)1), 0, 2); // AudioFormat (1 for PCM)
        stream.Write(System.BitConverter.GetBytes((ushort)channels), 0, 2); // NumChannels
        stream.Write(System.BitConverter.GetBytes(hz), 0, 4); // SampleRate
        stream.Write(System.BitConverter.GetBytes(hz * channels * 2), 0, 4); // ByteRate
        stream.Write(System.BitConverter.GetBytes((ushort)(channels * 2)), 0, 2); // BlockAlign
        stream.Write(System.BitConverter.GetBytes((ushort)16), 0, 2); // BitsPerSample (16 for PCM)

        // data sub-chunk
        WriteStr(stream, "data");
        stream.Write(System.BitConverter.GetBytes(samples * channels * 2), 0, 4); // Subchunk2Size
    }

     static void WriteStr(MemoryStream stream, string s)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
        stream.Write(bytes, 0, bytes.Length);
    }
}

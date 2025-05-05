using UnityEngine;
using System;
using TMPro;

public class LLMResponseListener : MonoBehaviour
{
    // Assign this in the Inspector to the TextMeshPro object you want to update
    public TextMeshProUGUI debugTextOutput;

    // reference to BoidManager if needed to pass data
    // public BoidManager boidManager;

    void OnEnable()
    {
        // subscribe to the event when this component is enabled
        LLMManager.OnLLMResponseReceived += HandleLLMResponse;
        Debug.Log("LLMResponseListener subscribed.");
    }

    void OnDisable()
    {
        // unsubscribe when the component is disabled to prevent memory leaks
        LLMManager.OnLLMResponseReceived -= HandleLLMResponse;
        Debug.Log("LLMResponseListener unsubscribed.");
    }

    void HandleLLMResponse(string response)
    {
        Debug.Log($"LLMResponseListener received: {response}");

        // Update the assigned TextMeshPro component
        if (debugTextOutput != null)
        {
            debugTextOutput.text = response;
        }

        // --- TODO: Use the response string here ---
        // For example, parse the response and tell BoidManager what to do
        // if (boidManager != null) {
        //    boidManager.ProcessLLMCommand(response); // Hypothetical method
        // }
    }
}

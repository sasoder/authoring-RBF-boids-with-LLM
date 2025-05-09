using UnityEngine;
using System;
using TMPro;

public class LLMResponseListener : MonoBehaviour
{
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

    void HandleLLMResponse(LLMGenerationOutput response)
    {
        Debug.Log($"LLMResponseListener received: {response}");

        // Update the assigned TextMeshPro component
        if (debugTextOutput != null)
        {
            debugTextOutput.text = response.style;
        }

        // something like this @marcus
        // if (boidManager != null) {
        //    boidManager.ProcessLLMCommand(response); 
        // }
    }
}

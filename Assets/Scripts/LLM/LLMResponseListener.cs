using UnityEngine;
using System;
using TMPro;
using p_bois_steering_behaviors.Scripts;
using System.Collections.Generic;

[System.Serializable]
public class BoidStylePreset
{
    public string styleName;
    public float cohesionForceFactor;
    public float cohesionRadius;
    public float separationForceFactor;
    public float separationRadius;
    public float alignmentForceFactor;
    public float alignmentRadius;
    public float maxSpeed;
    public float minSpeed;
    public float drag;
    public bool hasFlow;
    public bool hasVectorField;
}

public static class BoidStylePresets
{
    public static BoidStylePreset GetPreset(string styleName)
    {
        switch (styleName.ToLower())
        {
            case "calm":
                return new BoidStylePreset
                {
                    styleName = "calm",
                    cohesionForceFactor = 0.8f,
                    cohesionRadius = 4f,
                    separationForceFactor = 0.5f,
                    separationRadius = 2f,
                    alignmentForceFactor = 0.7f,
                    alignmentRadius = 3f,
                    maxSpeed = 5f,
                    minSpeed = 2f,
                    drag = 0.2f,
                    hasFlow = false,
                    hasVectorField = false
                };

            case "aggressive":
                return new BoidStylePreset
                {
                    styleName = "aggressive",
                    cohesionForceFactor = 1.2f,
                    cohesionRadius = 3f,
                    separationForceFactor = 1.5f,
                    separationRadius = 2.5f,
                    alignmentForceFactor = 1.3f,
                    alignmentRadius = 2.5f,
                    maxSpeed = 12f,
                    minSpeed = 6f,
                    drag = 0.1f,
                    hasFlow = false,
                    hasVectorField = false
                };

            case "exploratory":
                return new BoidStylePreset
                {
                    styleName = "exploratory",
                    cohesionForceFactor = 0.4f,
                    cohesionRadius = 5f,
                    separationForceFactor = 0.8f,
                    separationRadius = 2f,
                    alignmentForceFactor = 0.3f,
                    alignmentRadius = 4f,
                    maxSpeed = 8f,
                    minSpeed = 4f,
                    drag = 0.15f,
                    hasFlow = false,
                    hasVectorField = true
                };

            case "formation":
                return new BoidStylePreset
                {
                    styleName = "formation",
                    cohesionForceFactor = 1.5f,
                    cohesionRadius = 3f,
                    separationForceFactor = 1.2f,
                    separationRadius = 1.5f,
                    alignmentForceFactor = 2f,
                    alignmentRadius = 4f,
                    maxSpeed = 6f,
                    minSpeed = 4f,
                    drag = 0.1f,
                    hasFlow = false,
                    hasVectorField = false
                };

            case "social":
                return new BoidStylePreset
                {
                    styleName = "social",
                    cohesionForceFactor = 1.4f,
                    cohesionRadius = 3.5f,
                    separationForceFactor = 0.8f,
                    separationRadius = 1.8f,
                    alignmentForceFactor = 1.2f,
                    alignmentRadius = 4f,
                    maxSpeed = 6f,
                    minSpeed = 3f,
                    drag = 0.15f,
                    hasFlow = false,
                    hasVectorField = false
                };

            case "independent":
                return new BoidStylePreset
                {
                    styleName = "independent",
                    cohesionForceFactor = 0.2f,
                    cohesionRadius = 5f,
                    separationForceFactor = 1.5f,
                    separationRadius = 3f,
                    alignmentForceFactor = 0.3f,
                    alignmentRadius = 4f,
                    maxSpeed = 7f,
                    minSpeed = 4f,
                    drag = 0.1f,
                    hasFlow = false,
                    hasVectorField = false
                };

            case "chaotic":
                return new BoidStylePreset
                {
                    styleName = "chaotic",
                    cohesionForceFactor = 0.4f,
                    cohesionRadius = 4f,
                    separationForceFactor = 1.2f,
                    separationRadius = 2.5f,
                    alignmentForceFactor = 0.3f,
                    alignmentRadius = 3.5f,
                    maxSpeed = 11f,
                    minSpeed = 5f,
                    drag = 0.05f,
                    hasFlow = true,
                    hasVectorField = true
                };

            default:
                Debug.LogWarning($"Unknown style preset: {styleName}. Returning calm preset.");
                return GetPreset("calm");
        }
    }
}

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
        // Log the full response
        // Debug.Log($"Received LLM Response:");
        // Debug.Log($"Style: {response.style}");
        // Debug.Log($"Number of vectors: {response.vectors?.Count ?? 0}");

        // Update the assigned TextMeshPro component
        if (debugTextOutput != null)
        {
            debugTextOutput.text = response.style;
        }

        // Apply the style preset using BoidUIManager
        ApplyStylePreset(response.style);

        // Handle vector replacement
        if (response.vectors != null && response.vectors.Count > 0)
        {
            SourceVectorContainer container = FindObjectOfType<SourceVectorContainer>();
            if (container != null)
            {
                container.ReplaceSourceVectors(response.vectors);
                Debug.Log($"Replaced source vectors with {response.vectors.Count} new vectors");
            }
            else
            {
                Debug.LogError("Could not find SourceVectorContainer in scene");
            }
        }
    }

    void ApplyStylePreset(string styleName)
    {
        // Find and update UI elements
        BoidUIManager uiManager = FindObjectOfType<BoidUIManager>();
        if (uiManager != null)
        {
            uiManager.ApplyStyleByName(styleName);
            Debug.Log($"Applied {styleName} preset to flock and UI");
        }
        else
        {
            Debug.LogWarning("No BoidUIManager found in scene!");
        }
    }
}

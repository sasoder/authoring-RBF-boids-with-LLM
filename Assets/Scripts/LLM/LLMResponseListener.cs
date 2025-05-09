using UnityEngine;
using System;
using TMPro;
using p_bois_steering_behaviors.Scripts;

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
    public Flock targetFlock;

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

        // Apply the style preset to the flock
        if (targetFlock != null)
        {
            ApplyStylePreset(response.style);
        }
        else
        {
            Debug.LogWarning("No target flock assigned to LLMResponseListener!");
        }

        // something like this @marcus
        // if (boidManager != null) {
        //    boidManager.ProcessLLMCommand(response); 
        // }
    }

    void ApplyStylePreset(string styleName)
    {
        BoidStylePreset preset = BoidStylePresets.GetPreset(styleName);
        
        // Apply all parameters from the preset to the flock
        targetFlock.CohesionForceFactor = preset.cohesionForceFactor;
        targetFlock.CohesionRadius = preset.cohesionRadius;
        targetFlock.SeparationForceFactor = preset.separationForceFactor;
        targetFlock.SeparationRadius = preset.separationRadius;
        targetFlock.AlignmentForceFactor = preset.alignmentForceFactor;
        targetFlock.AlignmentRadius = preset.alignmentRadius;
        targetFlock.MaxSpeed = preset.maxSpeed;
        targetFlock.MinSpeed = preset.minSpeed;
        targetFlock.Drag = preset.drag;
        targetFlock.hasFlow = preset.hasFlow;
        targetFlock.hasVectorField = preset.hasVectorField;

        Debug.Log($"Applied {styleName} preset to flock");
    }
}

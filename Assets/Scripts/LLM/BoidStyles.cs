using UnityEngine;
using System.Collections.Generic;

namespace p_bois_steering_behaviors.Scripts
{
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

    public static class BoidStyles
    {
        public static readonly List<string> AvailableStyles = new List<string>
        {
            "calm",
            "aggressive",
            "exploratory",
            "formation",
            "social",
            "independent",
            "chaotic"
        };

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
                        hasFlow = true,
                        hasVectorField = true
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
                        hasFlow = true,
                        hasVectorField = true
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
                        hasFlow = true,
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
                        hasFlow = true,
                        hasVectorField = true
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
                        hasFlow = true,
                        hasVectorField = true
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
                        hasFlow = true,
                        hasVectorField = true
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
} 
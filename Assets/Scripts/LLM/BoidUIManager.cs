using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BoidUIManager : MonoBehaviour
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

    [Header("References")]
    [SerializeField] private Flock targetFlock;

    [Header("Behavior Selection")]
    [SerializeField] private TMP_Dropdown behaviorDropdown;
    [SerializeField] private TextMeshProUGUI behaviorLabel;

    [Header("Cohesion Controls")]
    [SerializeField] private Slider cohesionForceSlider;
    [SerializeField] private Slider cohesionRadiusSlider;
    [SerializeField] private TextMeshProUGUI cohesionForceText;
    [SerializeField] private TextMeshProUGUI cohesionRadiusText;

    [Header("Separation Controls")]
    [SerializeField] private Slider separationForceSlider;
    [SerializeField] private Slider separationRadiusSlider;
    [SerializeField] private TextMeshProUGUI separationForceText;
    [SerializeField] private TextMeshProUGUI separationRadiusText;

    [Header("Alignment Controls")]
    [SerializeField] private Slider alignmentForceSlider;
    [SerializeField] private Slider alignmentRadiusSlider;
    [SerializeField] private TextMeshProUGUI alignmentForceText;
    [SerializeField] private TextMeshProUGUI alignmentRadiusText;

    [Header("Movement Controls")]
    [SerializeField] private Slider maxSpeedSlider;
    [SerializeField] private Slider minSpeedSlider;
    [SerializeField] private Slider dragSlider;
    [SerializeField] private TextMeshProUGUI maxSpeedText;
    [SerializeField] private TextMeshProUGUI minSpeedText;
    [SerializeField] private TextMeshProUGUI dragText;

    [Header("Flow Controls")]
    [SerializeField] private Toggle flowToggle;

    [Header("Vector Field Controls")]
    [SerializeField] private Toggle vectorFieldToggle;

    private void Start()
    {
        if (targetFlock == null)
        {
            targetFlock = FindFirstObjectByType<Flock>();
            if (targetFlock == null)
            {
                Debug.LogError("No Flock found in scene!");
                return;
            }
        }

        InitializeBehaviorDropdown();
        InitializeSliders();
        InitializeToggles();
        UpdateUIValues();
    }

    private void InitializeBehaviorDropdown()
    {
        if (behaviorDropdown != null)
        {
            // Clear existing options
            behaviorDropdown.ClearOptions();

            // Add all available styles as options
            behaviorDropdown.AddOptions(AvailableStyles);

            // Add listener for when the dropdown value changes
            behaviorDropdown.onValueChanged.AddListener(OnBehaviorSelected);

            // Set initial value
            behaviorDropdown.value = 0;
            OnBehaviorSelected(0);
        }
    }

    private void InitializeSliders()
    {
        // Cohesion
        if (cohesionForceSlider != null)
        {
            cohesionForceSlider.minValue = 0f;
            cohesionForceSlider.maxValue = 5f;
            cohesionForceSlider.value = targetFlock.CohesionForceFactor;
            cohesionForceSlider.onValueChanged.AddListener(OnCohesionForceChanged);
        }

        if (cohesionRadiusSlider != null)
        {
            cohesionRadiusSlider.minValue = 0f;
            cohesionRadiusSlider.maxValue = 10f;
            cohesionRadiusSlider.value = targetFlock.CohesionRadius;
            cohesionRadiusSlider.onValueChanged.AddListener(OnCohesionRadiusChanged);
        }

        // Separation
        if (separationForceSlider != null)
        {
            separationForceSlider.minValue = 0f;
            separationForceSlider.maxValue = 5f;
            separationForceSlider.value = targetFlock.SeparationForceFactor;
            separationForceSlider.onValueChanged.AddListener(OnSeparationForceChanged);
        }

        if (separationRadiusSlider != null)
        {
            separationRadiusSlider.minValue = 0f;
            separationRadiusSlider.maxValue = 10f;
            separationRadiusSlider.value = targetFlock.SeparationRadius;
            separationRadiusSlider.onValueChanged.AddListener(OnSeparationRadiusChanged);
        }

        // Alignment
        if (alignmentForceSlider != null)
        {
            alignmentForceSlider.minValue = 0f;
            alignmentForceSlider.maxValue = 5f;
            alignmentForceSlider.value = targetFlock.AlignmentForceFactor;
            alignmentForceSlider.onValueChanged.AddListener(OnAlignmentForceChanged);
        }

        if (alignmentRadiusSlider != null)
        {
            alignmentRadiusSlider.minValue = 0f;
            alignmentRadiusSlider.maxValue = 10f;
            alignmentRadiusSlider.value = targetFlock.AlignmentRadius;
            alignmentRadiusSlider.onValueChanged.AddListener(OnAlignmentRadiusChanged);
        }

        // Movement
        if (maxSpeedSlider != null)
        {
            maxSpeedSlider.minValue = 0f;
            maxSpeedSlider.maxValue = 20f;
            maxSpeedSlider.value = targetFlock.MaxSpeed;
            maxSpeedSlider.onValueChanged.AddListener(OnMaxSpeedChanged);
        }

        if (minSpeedSlider != null)
        {
            minSpeedSlider.minValue = 0f;
            minSpeedSlider.maxValue = 10f;
            minSpeedSlider.value = targetFlock.MinSpeed;
            minSpeedSlider.onValueChanged.AddListener(OnMinSpeedChanged);
        }

        if (dragSlider != null)
        {
            dragSlider.minValue = 0f;
            dragSlider.maxValue = 1f;
            dragSlider.value = targetFlock.Drag;
            dragSlider.onValueChanged.AddListener(OnDragChanged);
        }
    }

    private void InitializeToggles()
    {
        if (flowToggle != null)
        {
            flowToggle.isOn = targetFlock.GetFlow();
            flowToggle.onValueChanged.AddListener(OnFlowToggled);
        }

        if (vectorFieldToggle != null)
        {
            vectorFieldToggle.isOn = targetFlock.HasVectorField();
            vectorFieldToggle.onValueChanged.AddListener(OnVectorFieldToggled);
        }
    }

    private void UpdateUIValues()
    {
        // Update all text displays
        if (cohesionForceText != null) cohesionForceText.text = $"{targetFlock.CohesionForceFactor:F1}";
        if (cohesionRadiusText != null) cohesionRadiusText.text = $"{targetFlock.CohesionRadius:F1}";
        if (separationForceText != null) separationForceText.text = $"{targetFlock.SeparationForceFactor:F1}";
        if (separationRadiusText != null) separationRadiusText.text = $"{targetFlock.SeparationRadius:F1}";
        if (alignmentForceText != null) alignmentForceText.text = $"{targetFlock.AlignmentForceFactor:F1}";
        if (alignmentRadiusText != null) alignmentRadiusText.text = $"{targetFlock.AlignmentRadius:F1}";
        if (maxSpeedText != null) maxSpeedText.text = $"{targetFlock.MaxSpeed:F1}";
        if (minSpeedText != null) minSpeedText.text = $"{targetFlock.MinSpeed:F1}";
        if (dragText != null) dragText.text = $"{targetFlock.Drag:F1}";
    }

    // Event Handlers
    private void OnCohesionForceChanged(float value)
    {
        targetFlock.CohesionForceFactor = value;
        if (cohesionForceText != null) cohesionForceText.text = $"{value:F1}";
    }

    private void OnCohesionRadiusChanged(float value)
    {
        targetFlock.CohesionRadius = value;
        if (cohesionRadiusText != null) cohesionRadiusText.text = $"{value:F1}";
    }

    private void OnSeparationForceChanged(float value)
    {
        targetFlock.SeparationForceFactor = value;
        if (separationForceText != null) separationForceText.text = $"{value:F1}";
    }

    private void OnSeparationRadiusChanged(float value)
    {
        targetFlock.SeparationRadius = value;
        if (separationRadiusText != null) separationRadiusText.text = $"{value:F1}";
    }

    private void OnAlignmentForceChanged(float value)
    {
        targetFlock.AlignmentForceFactor = value;
        if (alignmentForceText != null) alignmentForceText.text = $"{value:F1}";
    }

    private void OnAlignmentRadiusChanged(float value)
    {
        targetFlock.AlignmentRadius = value;
        if (alignmentRadiusText != null) alignmentRadiusText.text = $"{value:F1}";
    }

    private void OnMaxSpeedChanged(float value)
    {
        targetFlock.MaxSpeed = value;
        if (maxSpeedText != null) maxSpeedText.text = $"{value:F1}";
    }

    private void OnMinSpeedChanged(float value)
    {
        targetFlock.MinSpeed = value;
        if (minSpeedText != null) minSpeedText.text = $"{value:F1}";
    }

    private void OnDragChanged(float value)
    {
        targetFlock.Drag = value;
        if (dragText != null) dragText.text = $"{value:F1}";
    }

    private void OnFlowToggled(bool value)
    {
        targetFlock.hasFlow = value;
    }

    private void OnVectorFieldToggled(bool value)
    {
        // Reset boids to safe positions/velocities before enabling vector field
        if (value)
        {
            foreach (Boid boid in targetFlock.GetComponentsInChildren<Boid>())
            {
                // Ensure boid has valid position and velocity
                if (float.IsNaN(boid.Position.magnitude) || float.IsInfinity(boid.Position.magnitude))
                {
                    boid.Position = boid.transform.position; // Reset to current transform position
                }
                if (float.IsNaN(boid.Velocity.magnitude) || float.IsInfinity(boid.Velocity.magnitude))
                {
                    boid.Velocity = Vector3.forward * targetFlock.MinSpeed; // Reset to minimum speed
                }
            }
        }
        
        targetFlock.hasVectorField = value;
    }

    private void OnBehaviorSelected(int index)
    {
        if (index < 0 || index >= AvailableStyles.Count) return;

        string selectedStyle = AvailableStyles[index];
        BoidStylePreset preset = BoidStylePresets.GetPreset(selectedStyle);

        // Update all sliders to match the preset
        if (cohesionForceSlider != null) cohesionForceSlider.value = preset.cohesionForceFactor;
        if (cohesionRadiusSlider != null) cohesionRadiusSlider.value = preset.cohesionRadius;
        if (separationForceSlider != null) separationForceSlider.value = preset.separationForceFactor;
        if (separationRadiusSlider != null) separationRadiusSlider.value = preset.separationRadius;
        if (alignmentForceSlider != null) alignmentForceSlider.value = preset.alignmentForceFactor;
        if (alignmentRadiusSlider != null) alignmentRadiusSlider.value = preset.alignmentRadius;
        if (maxSpeedSlider != null) maxSpeedSlider.value = preset.maxSpeed;
        if (minSpeedSlider != null) minSpeedSlider.value = preset.minSpeed;
        if (dragSlider != null) dragSlider.value = preset.drag;

        // Update toggles
        if (flowToggle != null) flowToggle.isOn = preset.hasFlow;
        if (vectorFieldToggle != null) vectorFieldToggle.isOn = preset.hasVectorField;

        // Update the flock with the new values
        UpdateFlockWithPreset(preset);
    }

    private void UpdateFlockWithPreset(BoidStylePreset preset)
    {
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
    }

    // Public method to apply a style by name
    public void ApplyStyleByName(string styleName)
    {
        int styleIndex = AvailableStyles.IndexOf(styleName.ToLower());
        if (styleIndex != -1)
        {
            // Update the dropdown's selected value
            if (behaviorDropdown != null)
            {
                behaviorDropdown.value = styleIndex;
            }
            OnBehaviorSelected(styleIndex);
        }
        else
        {
            Debug.LogWarning($"Style {styleName} not found in available styles!");
        }
    }
} 
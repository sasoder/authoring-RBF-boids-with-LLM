using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoidUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Flock targetFlock;

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

        InitializeSliders();
        InitializeToggles();
        UpdateUIValues();
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
        targetFlock.hasVectorField = value;
    }
} 
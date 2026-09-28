using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Movement playerMovement;
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private Image boostIndicator;
    [SerializeField] private Image fuelFillImage; // Reference to the Slider's Fill image for color changes

    [SerializeField] private Color fullFuelColor = new Color(0f, 1f, 0f, 1f); // Green (100% fuel)
    [SerializeField] private Color halfFuelColor = new Color(1f, 1f, 0f, 1f); // Yellow (50% fuel)
    [SerializeField] private Color quarterFuelColor = new Color(1f, 0.5f, 0f, 1f); // Orange (25% fuel)
    [SerializeField] private Color emptyFuelColor = new Color(1f, 0f, 0f, 1f); // Red (0% fuel)
    [SerializeField] private float colorTransitionSpeed = 2f; // Speed of color interpolation
    [SerializeField] private float uiFadeSpeed = 2f; // Speed of UI fade-in/out (adjust for smoothness)

    private Color targetColor;
    private Color currentColor;
    private float sliderAlpha = 1f; // Start visible by default
    private float indicatorAlpha = 1f; // Start visible by default
    private bool isShowingUI = true; // Start with UI visible

    void Start()
    {
        // Validate assignments with detailed errors
        if (playerMovement == null)
            Debug.LogError("PlayerMovement not assigned in GameUI! Drag the rocket GameObject with Movement.");
        if (fuelSlider == null)
            Debug.LogError("FuelSlider not assigned in GameUI! Drag the Slider GameObject.");
        if (boostIndicator == null)
            Debug.LogError("BoostIndicator not assigned in GameUI! Drag the Boost Indicator Image.");
        if (fuelFillImage == null)
            Debug.LogError("FuelFillImage not assigned in GameUI! Drag the Slider’s Fill Image (e.g., FuelSlider/Fill Area/Fill).");

        // Initialize colors and alpha
        currentColor = fullFuelColor;
        targetColor = currentColor;

        // Initialize UI elements with full alpha (visible)
        SetUIAlpha(1f);

        // Apply initial color
        if (fuelFillImage != null)
            fuelFillImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, sliderAlpha);
        else
            Debug.LogError("FuelFillImage is null at Start!");
    }

    void Update()
    {
        if (playerMovement != null && fuelFillImage != null && fuelSlider != null)
        {
            // Get and validate fuel percentage
            float fuelPercentage = playerMovement.GetFuelPercentage();
            fuelPercentage = Mathf.Clamp01(fuelPercentage);

            // Detect input for showing/hiding the fuel UI
            bool shouldShowUI = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift);

            // Smoothly transition UI visibility
            if (shouldShowUI && !isShowingUI)
            {
                isShowingUI = true;
            }
            else if (!shouldShowUI && isShowingUI)
            {
                isShowingUI = false;
            }

            // Interpolate alpha for smooth fade
            float targetAlpha = isShowingUI ? 1f : 0f;
            sliderAlpha = Mathf.Lerp(sliderAlpha, targetAlpha, Time.deltaTime * uiFadeSpeed);
            indicatorAlpha = Mathf.Lerp(indicatorAlpha, targetAlpha, Time.deltaTime * uiFadeSpeed);

            // Apply alpha to UI elements
            SetUIAlpha(Mathf.Clamp01(sliderAlpha));

            // Update UI if visible
            if (isShowingUI)
            {
                // Prevent fuel slider from reaching exactly 0 (UI issue fix)
                fuelSlider.value = Mathf.Max(fuelPercentage, 0.01f);

                // Ensure the fill image is always active but respects alpha
                fuelFillImage.gameObject.SetActive(true);

                // Smoothly transition fuel fill color
                UpdateFuelColor(fuelPercentage);
                currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed);

                // Ensure alpha stays fully opaque for fill color but respects parent alpha
                Color displayColor = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
                fuelFillImage.color = new Color(displayColor.r, displayColor.g, displayColor.b, sliderAlpha);

                // Update boost indicator visibility and alpha
                boostIndicator.enabled = fuelPercentage * playerMovement.MaxFuel >= 25f;
                Color indicatorColor = boostIndicator.color;
                boostIndicator.color = new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, indicatorAlpha);
            }
            else
            {
                // Hide boost indicator if UI is fading out
                boostIndicator.enabled = false;
            }

            // Debug log to monitor behavior
            Debug.Log($"Fuel: {fuelPercentage:F2}, Slider Alpha: {sliderAlpha:F2}, Indicator Alpha: {indicatorAlpha:F2}, Boost Active: {boostIndicator.enabled}");
        }
        else
        {
            if (playerMovement == null)
                Debug.LogError("PlayerMovement is null in GameUI during Update!");
            if (fuelFillImage == null)
                Debug.LogError("FuelFillImage is null in GameUI during Update!");
            if (fuelSlider == null)
                Debug.LogError("FuelSlider is null in GameUI during Update!");
        }
    }

    private void UpdateFuelColor(float fuelPercentage)
    {
        // Ensure fuelPercentage is within valid range
        fuelPercentage = Mathf.Clamp01(fuelPercentage);

        if (fuelPercentage >= 0.5f)
        {
            // Transition between full (green) and half (yellow)
            float t = Mathf.InverseLerp(1f, 0.5f, fuelPercentage);
            targetColor = Color.Lerp(fullFuelColor, halfFuelColor, t);
        }
        else if (fuelPercentage >= 0.25f)
        {
            // Transition between half (yellow) and quarter (orange)
            float t = Mathf.InverseLerp(0.5f, 0.25f, fuelPercentage);
            targetColor = Color.Lerp(halfFuelColor, quarterFuelColor, t);
        }
        else
        {
            // Transition between quarter (orange) and empty (red)
            float t = Mathf.InverseLerp(0.25f, 0f, fuelPercentage);
            targetColor = Color.Lerp(quarterFuelColor, emptyFuelColor, t);
        }

        // Ensure targetColor has alpha = 1
        targetColor.a = 1f;
    }

    private void SetUIAlpha(float alpha)
    {
        if (fuelSlider != null)
        {
            // Apply alpha to all relevant components of the slider
            CanvasGroup sliderCanvasGroup = fuelSlider.GetComponent<CanvasGroup>();
            if (sliderCanvasGroup == null)
            {
                sliderCanvasGroup = fuelSlider.gameObject.AddComponent<CanvasGroup>();
            }
            sliderCanvasGroup.alpha = alpha;

            // Ensure fill image respects the alpha
            if (fuelFillImage != null)
            {
                Color fillColor = fuelFillImage.color;
                fuelFillImage.color = new Color(fillColor.r, fillColor.g, fillColor.b, alpha);
            }
        }

        if (boostIndicator != null)
        {
            CanvasGroup indicatorCanvasGroup = boostIndicator.GetComponent<CanvasGroup>();
            if (indicatorCanvasGroup == null)
            {
                indicatorCanvasGroup = boostIndicator.gameObject.AddComponent<CanvasGroup>();
            }
            indicatorCanvasGroup.alpha = alpha;
        }
    }
}
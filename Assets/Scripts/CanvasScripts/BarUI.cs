using UnityEngine;
using UnityEngine.UI;


public class BarUI : MonoBehaviour
{
    [Header("Wiring")]
    public Slider slider;      // assign your Slider
    public Image fill;         // assign the Fill image of the slider
    public Gradient gradient;  // set colors from empty->full

    [Header("Auto-Fit (optional)")]
    [Tooltip("If > 0, bar will shrink to fit this max width (in pixels) so it stays inside its border.")]
    public float maxWidth = 0f; // e.g., 260. Leave 0 to disable.
    public RectTransform barRoot; // RectTransform to scale (usually the slider's RectTransform)
    public float padding = 0f;    // extra padding inside the border

    private int _max = 1;

    void Reset()
    {
        slider = GetComponentInChildren<Slider>();
        if (slider != null)
        {
            // Try to auto-find the fill image
            var fillTransform = slider.fillRect;
            if (fillTransform) fill = fillTransform.GetComponent<Image>();
            if (!barRoot) barRoot = slider.GetComponent<RectTransform>();
        }
    }

    public void SetMax(int maxValue)
    {
        _max = Mathf.Max(1, maxValue);
        slider.maxValue = _max;
        slider.minValue = 0;
        slider.value = _max; // start full

        UpdateColor();
        AutoFit();
    }

    public void SetValue(int current)
    {
        slider.value = Mathf.Clamp(current, 0, _max);
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (fill && _max > 0)
            fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    private void AutoFit()
    {
        if (barRoot == null || maxWidth <= 0f) return;

        // Measure the slider's current preferred width (Layout) and clamp it visually
        float currentWidth = barRoot.rect.width;
        if (currentWidth > (maxWidth - padding))
        {
            float scale = (maxWidth - padding) / currentWidth;
            barRoot.localScale = new Vector3(scale, barRoot.localScale.y, 1f);
        }
        else
        {
            // ensure scale is 1 if under the limit (optional)
            barRoot.localScale = new Vector3(1f, barRoot.localScale.y, 1f);
        }
    }
    
}

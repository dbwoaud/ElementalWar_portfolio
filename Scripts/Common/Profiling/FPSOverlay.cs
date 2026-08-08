using UnityEngine;

public class FPSOverlay : MonoBehaviour
{
    [SerializeField] private int fontSize = 28;
    [SerializeField] private string variantLabel = "";
    private float smoothedMs = 16f;


    private void Update()
    {
        smoothedMs = Mathf.Lerp(smoothedMs, Time.unscaledDeltaTime * 1000f, 0.1f);
    }

    private void OnGUI()
    {
        var style = new GUIStyle
        {
            fontSize = fontSize,
            normal = { textColor = Color.yellow }
        };

        string label = $"{variantLabel}\n" +
                       $"{smoothedMs:F1} ms  ({1000f / smoothedMs:F0} FPS)\n" +
                       $"Units: {UnitRegistry.ActiveUnits.Count}";
        GUI.Label(new Rect(20, 20, 600, 160), label, style);
    }
}
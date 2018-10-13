using UnityEngine;

public class OrbColor : MonoBehaviour
{
    public float AnimationSpeed = 2.0f;

    public Color SurfaceColor = Color.white;
    public Color AccentColor = Color.white;
    public Color BaseColor = Color.white;

    Color startSurfaceColor;
    Color startAccentColor;
    Color startBaseColor;

    Material material;

    void Awake ()
    {
        material = GetComponent<MeshRenderer>().sharedMaterial;

        startSurfaceColor = material.GetColor(OrbVariable.SURFACE_COLOR);
        startAccentColor = material.GetColor(OrbVariable.ACCENT_COLOR);
        startBaseColor = material.GetColor(OrbVariable.BASE_COLOR);
    }

    void OnDestroy()
    {
        SetColors(startSurfaceColor, startAccentColor, startBaseColor);
    }

    void Update()
    {
        Color currentSurfaceColor = material.GetColor(OrbVariable.SURFACE_COLOR);
        Color currentAccentColor = material.GetColor(OrbVariable.ACCENT_COLOR);
        Color currentBaseColor = material.GetColor(OrbVariable.BASE_COLOR);
        float rate = Time.deltaTime * AnimationSpeed;
        SetColors(Color.Lerp(currentSurfaceColor, SurfaceColor, rate), Color.Lerp(currentAccentColor, AccentColor, rate), Color.Lerp(currentBaseColor, BaseColor, rate));
    }

    void SetColors(Color surfaceColor, Color accentColor, Color baseColor)
    {
        material.SetColor(OrbVariable.SURFACE_COLOR, surfaceColor);
        material.SetColor(OrbVariable.ACCENT_COLOR, accentColor);
        material.SetColor(OrbVariable.BASE_COLOR, baseColor);
    }

    public void GetFromMaterial()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("GetFromMaterial() should be used only in edit mode. Nothing will happen.");
            return;
        }

        material = GetComponent<MeshRenderer>().sharedMaterial;

        SurfaceColor = material.GetColor(OrbVariable.SURFACE_COLOR);
        AccentColor = material.GetColor(OrbVariable.ACCENT_COLOR);
        BaseColor = material.GetColor(OrbVariable.BASE_COLOR);
    }

    public void ApplyToMaterial()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("ApplyToMaterial() should be used only in edit mode. Nothing will happen.");
            return;
        }

        material = GetComponent<MeshRenderer>().sharedMaterial;

        SetColors(SurfaceColor, AccentColor, BaseColor);
    }
}

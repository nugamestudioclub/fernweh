using UnityEngine;

public class GlobalSnowCover : MonoBehaviour
{
    [Header("Timing")]
    public float secondsToFullSnow = 20f;

    [Header("Strength")]
    [Range(0f, 1f)] public float targetWhiteness = 1f; 
    public float targetSmoothness = 0.1f;

    [Header("Optional filtering")]
    public LayerMask affectLayers = ~0; 

    float snowAmount;

    void Update()
    {
        snowAmount = Mathf.Clamp01(snowAmount + (Time.deltaTime / Mathf.Max(0.1f, secondsToFullSnow)) * 0.2f);

        ApplyToAllRenderers();
    }

    void ApplyToAllRenderers()
    {
        var meshRenderers = FindObjectsOfType<MeshRenderer>(true);
        foreach (var r in meshRenderers)
            ApplyToRenderer(r);

        var skinned = FindObjectsOfType<SkinnedMeshRenderer>(true);
        foreach (var r in skinned)
            ApplyToRenderer(r);
    }

    void ApplyToRenderer(Renderer r)
    {
        if (((1 << r.gameObject.layer) & affectLayers.value) == 0) return;

        var mats = r.materials; 

        for (int i = 0; i < mats.Length; i++)
        {
            var m = mats[i];
            if (m == null) continue;

            if (m.HasProperty("_BaseColor"))
            {
                Color current = m.GetColor("_BaseColor");
                Color whiteTarget = Color.Lerp(current, Color.white, targetWhiteness);
                m.SetColor("_BaseColor", Color.Lerp(current, whiteTarget, snowAmount));
            }

            if (m.HasProperty("_Color"))
            {
                Color current = m.GetColor("_Color");
                Color whiteTarget = Color.Lerp(current, Color.white, targetWhiteness);
                m.SetColor("_Color", Color.Lerp(current, whiteTarget, snowAmount));
            }

            if (m.HasProperty("_Smoothness"))
            {
                float s = m.GetFloat("_Smoothness");
                m.SetFloat("_Smoothness", Mathf.Lerp(s, targetSmoothness, snowAmount));
            }
        }
    }
}

using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class HorizontalTextGlitch : MonoBehaviour
{
    [Header("Horizontal Glitch Settings")]
    [MinMax(0.1f, 2f)] public Vector2 glitchDuration = new Vector2(0.2f, 0.8f);
    [Range(0f, 1f)] public float horizontalIntensity = 0.5f;
    [Range(0f, 0.3f)] public float tearDistance = 0.1f;
    [MinMax(0.1f, 3f)] public Vector2 glitchInterval = new Vector2(0.5f, 2f);

    [Header("Advanced Control")]
    public bool useHorizontalUVScroll = true;
    public float uvScrollSpeed = 2f;
    public bool enableAutoGlitch = true;

    private TMP_Text tmpText;
    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector2[] originalUVs;
    private Coroutine glitchRoutine;
    private float currentGlitchTime;
    private bool isGlitching;

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
        CacheOriginalMesh();
    }

    void OnEnable()
    {
        if (enableAutoGlitch)
            glitchRoutine = StartCoroutine(AutoGlitchCycle());
    }

    void OnDisable() => SafeStopCoroutine(ref glitchRoutine);

    void Update()
    {
        if (isGlitching)
        {
            ApplyHorizontalGlitch();
            currentGlitchTime -= Time.deltaTime;
            if (currentGlitchTime <= 0) ResetGlitch();
        }
    }

    IEnumerator AutoGlitchCycle()
    {
        while (enableAutoGlitch)
        {
            yield return new WaitForSeconds(Random.Range(glitchInterval.x, glitchInterval.y));
            TriggerHorizontalGlitch();
        }
    }

    public void TriggerHorizontalGlitch(float duration = -1)
    {
        duration = duration < 0 ? Random.Range(glitchDuration.x, glitchDuration.y) : duration;
        isGlitching = true;
        currentGlitchTime = duration;
        tmpText.ForceMeshUpdate();
    }

    void CacheOriginalMesh()
    {
        tmpText.ForceMeshUpdate();
        mesh = tmpText.mesh;
        originalVertices = mesh.vertices.Clone() as Vector3[];
        originalUVs = mesh.uv.Clone() as Vector2[];
    }

    void ApplyHorizontalGlitch()
    {
        mesh = tmpText.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;

        int charCount = tmpText.textInfo.characterCount;
        float intensity = horizontalIntensity * (currentGlitchTime / glitchDuration.y);

        for (int i = 0; i < charCount; i++)
        {
            TMP_CharacterInfo charInfo = tmpText.textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            ApplyHorizontalDistortion(vertexIndex, ref vertices, ref uvs, intensity);
        }

        UpdateMesh(vertices, uvs);
    }

    void ApplyHorizontalDistortion(int vertexIndex, ref Vector3[] vertices, ref Vector2[] uvs, float intensity)
    {
        for (int j = 0; j < 4; j++)
        {
            int idx = vertexIndex + j;

            // X축으로만 이동하는 노이즈
            float xNoise = (Mathf.PerlinNoise(Time.time * 10 + idx, 0) - 0.5f) * intensity;
            Vector3 offset = new Vector3(xNoise, 0, 0) * tearDistance;

            // 수평 UV 왜곡
            Vector2 uvOffset = useHorizontalUVScroll ?
                new Vector2(
                    Mathf.Sin(Time.time * uvScrollSpeed + idx) * tearDistance,
                    0
                ) : Vector2.zero;

            vertices[idx] = originalVertices[idx] + offset;
            uvs[idx] = originalUVs[idx] + uvOffset;
        }
    }

    void UpdateMesh(Vector3[] vertices, Vector2[] uvs)
    {
        mesh.vertices = vertices;
        mesh.uv = uvs;
        tmpText.canvasRenderer.SetMesh(mesh);
    }

    void ResetGlitch()
    {
        isGlitching = false;
        mesh.vertices = originalVertices;
        mesh.uv = originalUVs;
        tmpText.canvasRenderer.SetMesh(mesh);
    }

    void SafeStopCoroutine(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
}

[System.Serializable]
public class MinMaxAttribute : PropertyAttribute
{
    public float Min;
    public float Max;

    public MinMaxAttribute(float min, float max)
    {
        Min = min;
        Max = max;
    }
}
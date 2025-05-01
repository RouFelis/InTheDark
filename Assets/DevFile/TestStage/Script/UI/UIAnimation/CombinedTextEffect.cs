using TMPro;
using UnityEngine;
using System.Collections;

public class CombinedTextEffect : UIAnimation
{
    [Header("Trail Effect Settings")]
    public float maxMovementDistance = 10f;
    public float trailDuration = 1f;

    [Header("Glitch Effect Settings")]
    public float glitchDuration = 1f;
    public float verticalIntensity = 0.5f;
    public float tearDistance = 0.1f;
    public float uvScrollSpeed = 2f;

    [SerializeField] private TMP_Text textComponent;
    private Vector3[][] originalVertices;
    private Vector4[][] originalUVs;
    private Vector2[] trailDirections;
    private Coroutine activeAnimation;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (activeAnimation != null)
        {
            StopCoroutine(activeAnimation);
            ResetEffect();
        }
        SaveOriginalData();
        activeAnimation = StartCoroutine(RunAnimation());
    }

    void SaveOriginalData()
    {
        textComponent.gameObject.SetActive(true);
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        // Backup vertices
        originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalVertices[i] = (Vector3[])textInfo.meshInfo[i].vertices.Clone();
        }

        // Backup UVs
        originalUVs = new Vector4[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalUVs[i] = (Vector4[])textInfo.meshInfo[i].uvs0.Clone();
        }

        // Initialize trail directions
        trailDirections = new Vector2[textInfo.characterCount];
        for (int i = 0; i < trailDirections.Length; i++)
        {
            trailDirections[i] = Random.insideUnitCircle.normalized;
        }
    }

    IEnumerator RunAnimation()
    {
        float totalDuration = Mathf.Max(trailDuration, glitchDuration);

        // === 1단계: 퍼졌다가 모이기 ===
        float elapsedTime = 0f;
        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;

            float trailProgress = Mathf.Clamp01(elapsedTime / trailDuration);
            float currentMovement = Mathf.Lerp(maxMovementDistance, 0f, trailProgress);

            float glitchProgress = Mathf.Clamp01(elapsedTime / glitchDuration);
            float glitchStrength = Mathf.Lerp(1f, 0f, glitchProgress);

            ApplyTrail(currentMovement);
            ApplyGlitch(glitchStrength);

            textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            yield return null;
        }

        yield return new WaitForSeconds(3f);

        // === 2단계: 모인 상태에서 다시 퍼지기 ===
        elapsedTime = 0f;

        // 방향을 다시 설정하고 싶으면 여기서 trailDirections 초기화 가능
        for (int i = 0; i < trailDirections.Length; i++)
        {
            trailDirections[i] = Random.insideUnitCircle.normalized;
        }

        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;

            float trailProgress = Mathf.Clamp01(elapsedTime / trailDuration);
            float currentMovement = Mathf.Lerp(0f, maxMovementDistance, trailProgress); // 역방향

            float glitchProgress = Mathf.Clamp01(elapsedTime / glitchDuration);
            float glitchStrength = Mathf.Lerp(1f, 0f, glitchProgress);

            ApplyTrail(currentMovement);
            ApplyGlitch(glitchStrength);

            textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            yield return null;
        }

        ResetEffect();
        activeAnimation = null;
        textComponent.gameObject.SetActive(false);
    }

    void ApplyTrail(float movement)
    {
        TMP_TextInfo textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int meshIndex = charInfo.materialReferenceIndex;
            int vertexStartIndex = charInfo.vertexIndex;

            for (int j = 0; j < 4; j++)
            {
                textInfo.meshInfo[meshIndex].vertices[vertexStartIndex + j] =
                    originalVertices[meshIndex][vertexStartIndex + j] +
                    (Vector3)trailDirections[i] * movement;
            }
        }
    }

    void ApplyGlitch(float strength)
    {
        TMP_TextInfo textInfo = textComponent.textInfo;
        float timeNow = Time.time;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int meshIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            // 랜덤 좌우 찢김 오프셋
            float noise = Mathf.PerlinNoise(timeNow * 20 + i * 0.5f, 0);
            float xOffset = (noise - 0.5f) * tearDistance * 50f * strength;  // *50f로 강하게

            for (int j = 0; j < 4; j++)
            {
                textInfo.meshInfo[meshIndex].vertices[vertexIndex + j].x += xOffset;
            }
        }

        // UV 효과는 유지하거나 강화 가능
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = textInfo.meshInfo[i];

            for (int j = 0; j < meshInfo.uvs0.Length; j++)
            {
                Vector4 uv = originalUVs[i][j];
                uv.x += Mathf.Sin(timeNow * uvScrollSpeed + j * 0.3f) * tearDistance * strength * 0.2f;
                meshInfo.uvs0[j] = uv;
            }
        }
    }

    void ResetEffect()
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];

            Vector3[] vertices = meshInfo.vertices;
            Vector4[] uvs4 = meshInfo.uvs0;

            for (int j = 0; j < vertices.Length; j++)
            {
                vertices[j] = originalVertices[i][j];
                uvs4[j] = originalUVs[i][j];
            }

            // 복구된 버텍스와 UV를 실제 메쉬에 반영
            meshInfo.mesh.vertices = vertices;

            // Vector4 → Vector2 변환
            Vector2[] uvs2 = new Vector2[uvs4.Length];
            for (int j = 0; j < uvs2.Length; j++)
            {
                uvs2[j] = new Vector2(uvs4[j].x, uvs4[j].y);
            }
            meshInfo.mesh.uv = uvs2;
        }

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }
}

using TMPro;
using UnityEngine;
using System.Collections;

public class TextTrailEffect : MonoBehaviour
{
    public TMP_Text textComponent;
    public float maxOffset = 10f;  // 최대 이동 거리
    public float animationSpeed = 1f;  // 애니메이션 속도

    private Vector3[][] originalVertices;  // 원본 정점 위치 저장
    private Vector2[] directions;  // 각 문자의 이동 방향

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        CacheOriginalVertices(); // 초기 정점 위치 캐싱
        StartCoroutine(AnimateTrail());
    }

    // 정점 위치 및 방향 초기화
    void CacheOriginalVertices()
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;
        originalVertices = new Vector3[textInfo.characterCount][];
        directions = new Vector2[textInfo.characterCount];

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            // 각 문자의 정점 위치 저장
            originalVertices[i] = new Vector3[4];
            for (int j = 0; j < 4; j++)
            {
                originalVertices[i][j] = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices[charInfo.vertexIndex + j];
            }

            // 랜덤 방향 설정 (예: 4방향)
            directions[i] = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }
    }

    // 애니메이션 코루틴
    IEnumerator AnimateTrail()
    {
        while (true)
        {
            float time = Mathf.PingPong(Time.time * animationSpeed, 1f); // 0 → 1 → 0 반복
            UpdateVertexPositions(time * maxOffset);
            yield return null;
        }
    }

    // 정점 위치 업데이트
    void UpdateVertexPositions(float displacement)
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            // 정점에 변위 적용
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            for (int j = 0; j < 4; j++)
            {
                vertices[charInfo.vertexIndex + j] = originalVertices[i][j] + (Vector3)directions[i] * displacement;
            }
        }

        // 메시 업데이트
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}
using TMPro;
using UnityEngine;
using System.Collections;

public class TextTrailEffect : MonoBehaviour
{
    public TMP_Text textComponent;
    public float maxOffset = 10f;  // �ִ� �̵� �Ÿ�
    public float animationSpeed = 1f;  // �ִϸ��̼� �ӵ�

    private Vector3[][] originalVertices;  // ���� ���� ��ġ ����
    private Vector2[] directions;  // �� ������ �̵� ����

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        CacheOriginalVertices(); // �ʱ� ���� ��ġ ĳ��
        StartCoroutine(AnimateTrail());
    }

    // ���� ��ġ �� ���� �ʱ�ȭ
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

            // �� ������ ���� ��ġ ����
            originalVertices[i] = new Vector3[4];
            for (int j = 0; j < 4; j++)
            {
                originalVertices[i][j] = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices[charInfo.vertexIndex + j];
            }

            // ���� ���� ���� (��: 4����)
            directions[i] = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }
    }

    // �ִϸ��̼� �ڷ�ƾ
    IEnumerator AnimateTrail()
    {
        while (true)
        {
            float time = Mathf.PingPong(Time.time * animationSpeed, 1f); // 0 �� 1 �� 0 �ݺ�
            UpdateVertexPositions(time * maxOffset);
            yield return null;
        }
    }

    // ���� ��ġ ������Ʈ
    void UpdateVertexPositions(float displacement)
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            // ������ ���� ����
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            for (int j = 0; j < 4; j++)
            {
                vertices[charInfo.vertexIndex + j] = originalVertices[i][j] + (Vector3)directions[i] * displacement;
            }
        }

        // �޽� ������Ʈ
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}
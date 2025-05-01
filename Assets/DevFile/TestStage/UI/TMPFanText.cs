using UnityEngine;
using TMPro;

//[RequireComponent(typeof(TextMeshPro))]
public class TMPFanText : MonoBehaviour
{
    public float curveStrength = 0.1f; // 양수면 위로, 음수면 아래로 휘어짐

    private TMP_Text textMesh;
    private Mesh mesh;
    private Vector3[] vertices;

    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
        textMesh.ForceMeshUpdate(); // Mesh 강제 갱신
        mesh = textMesh.mesh;
        vertices = mesh.vertices;

        int characterCount = textMesh.textInfo.characterCount;
        for (int i = 0; i < characterCount; i++)
        {
            var charInfo = textMesh.textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            // 각 문자 vertex의 인덱스
            int vertexIndex = charInfo.vertexIndex;

            // 문자의 중심 계산
            Vector3 charMidBaseline = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;

            for (int j = 0; j < 4; j++)
            {
                Vector3 offset = vertices[vertexIndex + j] - charMidBaseline;
                float x = offset.x;
                float angle = x * curveStrength;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                // 회전 변형 적용
                vertices[vertexIndex + j] = new Vector3(
                    charMidBaseline.x + (cos * offset.x - sin * offset.y),
                    charMidBaseline.y + (sin * offset.x + cos * offset.y),
                    vertices[vertexIndex + j].z
                );
            }
        }

        // 변경된 버텍스를 mesh에 적용
        mesh.vertices = vertices;
        textMesh.canvasRenderer.SetMesh(mesh);
    }
}

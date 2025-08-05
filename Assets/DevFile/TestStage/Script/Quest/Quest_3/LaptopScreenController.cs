using UnityEngine;

public class LaptopScreenController : MonoBehaviour
{
    [SerializeField] private MeshRenderer screenRenderer;   // LaptopScreen
    [SerializeField] private Camera uiCamera;               // UICamera
    [SerializeField] private int textureSize_X = 720;
    [SerializeField] private int textureSize_Y = 405;
    [SerializeField] private Material originalMaterial;     // ������ ���� ��Ƽ����

    private void Start()
    {
        SetupRenderTexture();
    }

    private void SetupRenderTexture()
    {
        // 1. RenderTexture ����
        RenderTexture renderTex = new RenderTexture(textureSize_X, textureSize_Y, 16);
        renderTex.Create();

        // 2. ī�޶� RenderTexture ����
        uiCamera.targetTexture = renderTex;

        // 3. ȭ�鿡 ������ ��Ƽ���� ���� �� �Ҵ�
        Material mat = new Material(originalMaterial);
        mat.mainTexture = renderTex;

        screenRenderer.material = mat;
    }
}

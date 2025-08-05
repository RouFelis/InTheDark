using UnityEngine;

public class LaptopScreenController : MonoBehaviour
{
    [SerializeField] private MeshRenderer screenRenderer;   // LaptopScreen
    [SerializeField] private Camera uiCamera;               // UICamera
    [SerializeField] private int textureSize_X = 720;
    [SerializeField] private int textureSize_Y = 405;
    [SerializeField] private Material originalMaterial;     // 복사할 원본 머티리얼

    private void Start()
    {
        SetupRenderTexture();
    }

    private void SetupRenderTexture()
    {
        // 1. RenderTexture 생성
        RenderTexture renderTex = new RenderTexture(textureSize_X, textureSize_Y, 16);
        renderTex.Create();

        // 2. 카메라에 RenderTexture 연결
        uiCamera.targetTexture = renderTex;

        // 3. 화면에 적용할 머티리얼 생성 및 할당
        Material mat = new Material(originalMaterial);
        mat.mainTexture = renderTex;

        screenRenderer.material = mat;
    }
}

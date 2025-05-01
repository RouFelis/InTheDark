using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
	[Header("Laser Settings")]
	public float maxDistance = 100f;
	public float startWidth = 0.1f;
	public float endWidth = 0.1f;
	public Color laserColor = Color.red;

	[Header("Effects")]
	public float scrollSpeed = 1f;
	public float brightness = 2f;
	public Material laserMaterial;

	private LineRenderer lineRenderer;
	private Vector3[] positions = new Vector3[2];

	void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		//lineRenderer.material = laserMaterial;
		//lineRenderer.startWidth = startWidth;
		//lineRenderer.endWidth = endWidth;

		UpdateMaterialProperties();
	}

	void Update()
	{
		// 레이저 방향 업데이트
		positions[0] = transform.position;
		positions[1] = transform.position + transform.forward * maxDistance;

		// 충돌 체크
		if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxDistance))
		{
			positions[1] = hit.point;
		}

		lineRenderer.SetPositions(positions);
	}

	void UpdateMaterialProperties()
	{
		if (laserMaterial != null)
		{
			lineRenderer.material = laserMaterial;
			lineRenderer.startWidth = startWidth;
			lineRenderer.endWidth = endWidth;

			laserMaterial.SetColor("_Color", laserColor);
			laserMaterial.SetFloat("_ScrollSpeed", scrollSpeed);
			laserMaterial.SetFloat("_Brightness", brightness);
		}
	}

	//// 에디터에서 값 변경 시 자동 업데이트
	//void OnValidate()
	//{
	//	UpdateMaterialProperties();
	//}
}
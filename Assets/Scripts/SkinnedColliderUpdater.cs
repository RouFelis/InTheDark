using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class SkinnedColliderUpdater : MonoBehaviour
{
	[SerializeField]
	private float[] _frame = new float[2];

	[SerializeField]
	private SkinnedMeshRenderer _renderer;

	[SerializeField]
	private MeshCollider _collider;

	[SerializeField]
	private Mesh _baked;

	private void Awake()
	{
		_baked = new();
		_baked.name = $"{gameObject.name}_Baked";
	}

	private void LateUpdate()
	{
		_frame[0]++;

		if (_frame[0] >= _frame[1])
		{
			// 1) 애니메이션이 모두 적용된 후에 베이킹
			_renderer.BakeMesh(_baked);

			// 2) 베이킹된 메쉬를 콜라이더에 연결
			_collider.sharedMesh = null;
			_collider.sharedMesh = _baked;

			_frame[0] -= _frame[1];
		}
	}
}

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
			// 1) �ִϸ��̼��� ��� ����� �Ŀ� ����ŷ
			_renderer.BakeMesh(_baked);

			// 2) ����ŷ�� �޽��� �ݶ��̴��� ����
			_collider.sharedMesh = null;
			_collider.sharedMesh = _baked;

			_frame[0] -= _frame[1];
		}
	}
}

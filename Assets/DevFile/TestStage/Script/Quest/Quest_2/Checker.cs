using UnityEngine;
using Unity.Netcode;


public class Checker : InteractableObject
{
	public NetworkVariable<bool> isComplete = new NetworkVariable<bool>(false);
	[SerializeField] bool isStart = false;
	[SerializeField] Quest2 quest;
	Transform startEndTransform;
	public int connectionOrder;
	public Color matColor;

	public override void Start()
	{
		base.Start();

		quest = this.GetComponentInParent<Quest2>();
		startEndTransform = GetComponentInChildren<Transform>();
		SetColor();
	}

	public void SetColor()
	{
		// value를 0~15로 제한
		int value = Mathf.Clamp(connectionOrder, 0, 15);

		// HSV 색상 계산 (Hue를 0~1 사이로 분배)
		float hue = value / 15f; // 0부터 1까지 나눔
		matColor = Color.HSVToRGB(hue, 1f, 1f); // 채도(S)와 명도(V)를 최대값으로 설정

		this.GetComponent<MeshRenderer>().material.color = matColor;
	}

	[ServerRpc(RequireOwnership = false)]
	public void CompleteBoolChangeServerRpc(bool value)
	{
		isComplete.Value = value;
	}

	public override bool Interact(ulong uerID, Transform interactingObjectTransform)
	{
		if (!base.Interact(uerID, interactingObjectTransform))
			return false;

		if (isComplete.Value)
		{
			return false;
		}

		if (isStart)
		{
			quest.WireStar(startEndTransform, this, this.connectionOrder, uerID);
		}
		else
		{
			quest.WireEnd(startEndTransform, this);
		}

		return true;
	}
}

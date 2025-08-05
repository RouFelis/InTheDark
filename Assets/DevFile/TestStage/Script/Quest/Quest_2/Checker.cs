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
		// value�� 0~15�� ����
		int value = Mathf.Clamp(connectionOrder, 0, 15);

		// HSV ���� ��� (Hue�� 0~1 ���̷� �й�)
		float hue = value / 15f; // 0���� 1���� ����
		matColor = Color.HSVToRGB(hue, 1f, 1f); // ä��(S)�� ��(V)�� �ִ밪���� ����

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

using UnityEngine;
using Dissonance;

[System.Serializable]
public struct MicStruct
{
	public VoiceReceiptTrigger receipt;
	public VoiceBroadcastTrigger broadcast;

	public void SetState(bool receiptState, bool broadcastState)
	{
		if (receipt != null)
        {
            receipt.enabled = receiptState;
/*			if (receiptState)
				receipt.AddToken("DieToken");
			else
                receipt.RemoveToken("DieToken");*/
        }
		if (broadcast != null)
        {
            broadcast.enabled = broadcastState;

        }
	}
}


public class PlayerMicController : MonoBehaviour
{
	[SerializeField] MicStruct broadcastVoice;
	[SerializeField] MicStruct dieVoice;

	private void Start()
	{
		SetAliveMicState();
	}

	public void Die()
	{
		// ���� ���� ����ũ ����
		broadcastVoice.SetState(true, false); // ��� O, ���ϱ� X
		dieVoice.SetState(true, true);        // ��� O, ���ϱ� O
	}

	public void Revive()
	{
		// ����ִ� ���� ����ũ ����
		broadcastVoice.SetState(true, true);  // ��� O, ���ϱ� O
		dieVoice.SetState(false, false);      // ��� X, ���ϱ� X
	}

	// �ʱ�ȭ �ÿ��� ȣ���� �� �ֵ��� ���� �޼��� �и�
	private void SetAliveMicState()
	{
		Revive();
	}
}

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
		// 죽은 상태 마이크 설정
		broadcastVoice.SetState(true, false); // 듣기 O, 말하기 X
		dieVoice.SetState(true, true);        // 듣기 O, 말하기 O
	}

	public void Revive()
	{
		// 살아있는 상태 마이크 설정
		broadcastVoice.SetState(true, true);  // 듣기 O, 말하기 O
		dieVoice.SetState(false, false);      // 듣기 X, 말하기 X
	}

	// 초기화 시에도 호출할 수 있도록 따로 메서드 분리
	private void SetAliveMicState()
	{
		Revive();
	}
}

using UnityEngine;

public class Compass : MonoBehaviour
{
	bool InDungeun = false;


	[HideInInspector] public EnterStage enterStage;
	[HideInInspector] public OutStage outStage;

	public Vector3 northDirection;
	public Transform player;
	public Quaternion missionDirection;

	public RectTransform missionLayer;
	public Transform missionPlace;


	private void Start()
	{
		enterStage = FindAnyObjectByType<EnterStage>();
		enterStage.enterDoorAction += setDungeunBool;
	}

	private void OnDisable()
	{
		enterStage.enterDoorAction -= setDungeunBool;
		enterStage.enterDoorAction -= FindOutDoor;
		outStage.outDoorAction -= setDungeunBool;
	}

	void Update()
	{
		ChangeMissionDirection();
	}

	public void setDungeunBool(bool value)
	{
		InDungeun = value;
	}

	private void FindOutDoor(bool value)
	{
		if (missionPlace == null)
		{
			missionPlace = GameObject.Find("OutDoor_1(Clone)").GetComponent<Transform>(); ;
		}
	}

	public void ChangeMissionDirection()
	{
		if (!InDungeun)
		{

			return;
		}

		//  ��ǥ ������Ʈ�� �÷��̾� ������ ���� ����
		Vector3 directionToTarget = missionPlace.position - player.position;
		directionToTarget.y = 0; // Y�� ���� (���� ���⸸ ���)

		//  �÷��̾ �ٶ󺸴� ���� (���� ����)
		Vector3 playerForward = player.forward;
		playerForward.y = 0; // Y�� ����

		//  ��ũź��Ʈ2 (Atan2)�� ���� ���
		float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
		float playerAngle = Mathf.Atan2(playerForward.x, playerForward.z) * Mathf.Rad2Deg;

		//  ��ǥ ����� �÷��̾� ������ ���̸� ���
		float angleDifference = targetAngle - playerAngle;

		//  ��ħ�� �ٴ� ȸ�� (Z�� ����)
		missionLayer.rotation = Quaternion.Euler(0, 0, -angleDifference);
	}


}

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

		//  목표 오브젝트와 플레이어 사이의 방향 벡터
		Vector3 directionToTarget = missionPlace.position - player.position;
		directionToTarget.y = 0; // Y축 무시 (수평 방향만 고려)

		//  플레이어가 바라보는 방향 (수평 기준)
		Vector3 playerForward = player.forward;
		playerForward.y = 0; // Y축 무시

		//  아크탄젠트2 (Atan2)로 각도 계산
		float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
		float playerAngle = Mathf.Atan2(playerForward.x, playerForward.z) * Mathf.Rad2Deg;

		//  목표 방향과 플레이어 방향의 차이를 계산
		float angleDifference = targetAngle - playerAngle;

		//  나침반 바늘 회전 (Z축 기준)
		missionLayer.rotation = Quaternion.Euler(0, 0, -angleDifference);
	}


}

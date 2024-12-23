using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.Netcode;

public class Quest2 : QuestBase
{
    [Header("플러그 소리")]
    public AudioClip startSound;
    public AudioClip endSound;
    [SerializeField] private AudioSource audioSource;
    public NetworkVariable<bool> isUsed = new NetworkVariable<bool>(false);

    LineDrawer lineDrawer;
    [SerializeField]Checker startChecker;    
    [Header("플러그 넣기")]
    [SerializeField] List<Checker> checkerList;
    [SerializeField] List<Checker> startCheckerList;

	protected override void Start()
	{
        base.Start();

        lineDrawer = gameObject.GetComponent<LineDrawer>();
        CheckListinit();
    }


    public void CheckListinit()
	{
        System.Random random = new System.Random(SharedData.Instance.networkSeed.Value);

        startCheckerList = startCheckerList.OrderBy(x => random.Next()).ToList();
        int counter = 0;

		foreach (var start in startCheckerList)
		{
            start.connectionOrder = counter;
            start.SetColor();
            counter++;
        }
    }


	#region 라인 만들기
	public void WireStar(Transform startPoint, Checker startChecker , int matColor ,ulong uerID)
	{
		if (!isUsed.Value)
		{
            /*wireObject = new GameObject("WireObject");
            wireObject.transform.SetParent(this.transform);
            wireObject.transform.localPosition = Vector3.zero;
            wireObject.transform.localRotation = Quaternion.identity;
            lineDrawer = wireObject.AddComponent<LineDrawer>();*/

            this.startChecker = startChecker;
            // 초기화
            lineDrawer.InitDrawer(startPoint, matColor , uerID);            

            PlaySound(startSound);
            UsedBoolChangeServerRpc(true);
        }
	}

    [ServerRpc(RequireOwnership = false)]
    public void UsedBoolChangeServerRpc(bool value)
	{
        isUsed.Value = value;
    }

    public void WireEnd(Transform endPoint, Checker endChecker)
    {
        if (endChecker.connectionOrder == startChecker.connectionOrder)
        {
            lineDrawer.EndDraw(endPoint);
            endChecker.CompleteBoolChangeServerRpc(true);
            startChecker.CompleteBoolChangeServerRpc(true);
            UsedBoolChangeServerRpc(false);
            PlaySound(endSound);
            WireSetNull();
            if (CheckerCheck())
            {
                CompleteBoolChangeServerRpc(true);
            }
        }
        else
        {
            lineDrawer.MissDraw();
            WireSetNull();
            UsedBoolChangeServerRpc(false);
        }
    }

    #endregion
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    public bool CheckerCheck()
    {
        int boolCount = 0;
        foreach (var checker in checkerList)
        {
            if (checker.isComplete.Value)
            {
                boolCount++;
            }
        }

        if (checkerList.Count == boolCount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void WireSetNull()
    {
        startChecker = null;
    }
    public override void QuestComplete(bool oldValue, bool newValue)
	{
		base.QuestComplete(oldValue, newValue);
	}
}

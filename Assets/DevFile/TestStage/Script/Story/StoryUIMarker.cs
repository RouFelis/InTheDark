using UnityEngine;
using UnityEngine.UI;

public class StoryUIMarker : MonoBehaviour
{
	[Header("ReadChecker")]
	[SerializeField] private GameObject reddot;
	[SerializeField] private Button storyButton;

	[HideInInspector] public int storyID; // ◀◀◀ 이 변수를 추가해주세요!
	[HideInInspector] private bool isRead = true;
	[HideInInspector] private string storyText = "";


	public bool IsRead { get { return isRead; } set { isRead = value; } }
	public int StoryID { get { return storyID; } set { storyID = value; } }
	public string StoryText { get { return storyText; } set { storyText = value; } }


	private void Start()
	{
		storyButton.onClick.AddListener(StroyReadButton);
	}

	public void SetReadState(bool value)
	{
		isRead = value;
		reddot.SetActive(false);
	}

	private void StroyReadButton()
	{
		reddot.SetActive(false);
		StoryManaager.Inst.storyText.text = storyText;
		StoryManaager.Inst.memoWindow.SetActive(true);
	}

}

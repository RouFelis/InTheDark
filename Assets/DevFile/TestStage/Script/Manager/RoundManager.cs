using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections;

public class RoundManager : NetworkBehaviour
{
	[Header("TV")]
	[SerializeField] private TMP_Text areaTMP;
	[SerializeField] private TMP_Text missionQuotaTMP;
	[SerializeField] private TMP_Text moneyQuotaTMP;

	[Header("결과")]
	[SerializeField] private TMP_Text roundClearResultTMP;
	[SerializeField] private TMP_Text roundClearContentsTMP;
	[SerializeField] private TMP_Text roundClearPSTMP;
	[SerializeField] private Canvas summaryCanvs;


	public LocalizedString localizedString;

	void Start()
	{
		SharedData.Instance.area.OnValueChanged += SetAreaTMP;
		SharedData.Instance.questQuota.OnValueChanged += SetquestQuotaTMP;
		SharedData.Instance.moneyQuota.OnValueChanged += SetmoneyQuotaTMP;
		SharedData.Instance.Money.OnValueChanged += SetmoneyQuotaTMP;

		QuestManager.inst.nowClearedQuestTotal.OnValueChanged += SetquestQuotaTMP;

		LocalizationSettings.SelectedLocaleChanged += SetLanguage;

		InitResult();
	}

	private void OnDisable()
	{
		LocalizationSettings.SelectedLocaleChanged -= SetLanguage;
	}

	private void InitResult()
	{
		// 캔버스와 이미지 초기화
		summaryCanvs.gameObject.SetActive(false);

		// 텍스트 초기화
		/*		roundClearResultTMP.text = "";
				roundClearContentsTMP.text = "";
				roundClearPSTMP.text = "";*/
		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Area"; // 사용하고자 하는 키
		areaTMP.text = $"{localizedString.GetLocalizedString()} : " + SharedData.Instance.area.Value;

		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Mission Quota"; // 사용하고자 하는 키
		missionQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + QuestManager.inst.nowClearedQuestTotal.Value + " / " + SharedData.Instance.questQuota.Value;

		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Money Quota"; // 사용하고자 하는 키
		moneyQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + SharedData.Instance.Money.Value + " / " + SharedData.Instance.moneyQuota.Value;
	}

	private void SetLanguage(Locale newLocale)
	{
		Debug.Log($"언어가 {newLocale.LocaleName}로 변경되었습니다.");

		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Area"; // 사용하고자 하는 키
		areaTMP.text = $"{localizedString.GetLocalizedString()} : " + SharedData.Instance.area.Value;

		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Mission Quota"; // 사용하고자 하는 키
		missionQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + QuestManager.inst.nowClearedQuestTotal.Value + " / " + SharedData.Instance.questQuota.Value;

		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Money Quota"; // 사용하고자 하는 키
		moneyQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + SharedData.Instance.Money.Value + " / " + SharedData.Instance.moneyQuota.Value;
	}

	private void SetAreaTMP(int oldValue, int newValue)
	{
		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Area"; // 사용하고자 하는 키
		areaTMP.text = $"{localizedString.GetLocalizedString()} : " + newValue.ToString();
	}
	private void SetquestQuotaTMP(int oldValue, int newValue)
	{
		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Mission Quota"; // 사용하고자 하는 키
		missionQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + QuestManager.inst.nowClearedQuestTotal + " / " + SharedData.Instance.questQuota.Value;
	}
	private void SetmoneyQuotaTMP(int oldValue, int newValue)
	{
		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Money Quota"; // 사용하고자 하는 키
		moneyQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + SharedData.Instance.Money.Value + " / " + SharedData.Instance.moneyQuota.Value;
	}


	#region 게임 클리어 관련
	[ServerRpc]
	public void GameClearCheckServerRpc()
	{
		bool missionQuotaCleared = QuestManager.inst.nowClearedQuestTotal.Value >= SharedData.Instance.questQuota.Value;
		bool moneyQuotaCleared = SharedData.Instance.moneyQuota.Value <= SharedData.Instance.Money.Value; // 원하는 조건으로 수정

		if (missionQuotaCleared && moneyQuotaCleared)
		{
			SharedData.Instance.area.Value += 1; // 다음 라운드로 진행
		}
		else
		{
			// 클리어 실패
			Debug.Log("라운드 클리어 실패 - 조건 미달");
			GameOverAnimeClientRpc();
		}
	}

	[ServerRpc]
	public void GameClearAnimeServerRpc()
	{
		GameClearAnimeClientRpc(); // 클리어 연출 실행
	}

	[ClientRpc]
	private void GameOverAnimeClientRpc()
	{
		StartCoroutine(GameFailRoutine());
	}


	[ClientRpc]
	public void GameClearAnimeClientRpc()
	{
		StartCoroutine(GameOverRoutine());
	}

	private IEnumerator GameOverRoutine()
	{
		summaryCanvs.gameObject.SetActive(true);

		SetRoundClearSummaryTMP();
		SetRoundClearContentsTMP();
		SetRoundClearPSTMP();

		yield return null;
	}	
		
	
	private IEnumerator GameFailRoutine()
	{
		//yield return new WaitForSeconds();
		yield return null;
		UIAnimationManager.Instance.GameFailAnimation();
	}	
	
	private void SetRoundClearSummaryTMP()
	{
		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Result"; // 사용하고자 하는 키
		roundClearResultTMP.text = $"{localizedString.GetLocalizedString()}";

		StartCoroutine(ScrambleIn(roundClearResultTMP.text, roundClearResultTMP));
	}

	private void SetRoundClearContentsTMP()
	{
		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Completed Mission"; // 사용하고자 하는 키
		roundClearContentsTMP.text = $"{localizedString.GetLocalizedString()} : " + QuestManager.inst.nowClearedQuestTotal.Value + "\n";

		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Killed Enemy"; // 사용하고자 하는 키
		roundClearContentsTMP.text += $"{localizedString.GetLocalizedString()} : " + SharedData.Instance.area.Value + "\n";

		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "Escape"; // 사용하고자 하는 키
		roundClearContentsTMP.text += $"{localizedString.GetLocalizedString()} : " + PlayersManager.Instance.allPlayersDead.Value + "\n";

		StartCoroutine(ScrambleIn(roundClearContentsTMP.text, roundClearContentsTMP));
	}

	private void SetRoundClearPSTMP()
	{
		localizedString.TableReference = "UITable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = "PS"; // 사용하고자 하는 키
		roundClearPSTMP.text = $"{localizedString.GetLocalizedString()}";

		StartCoroutine(ScrambleIn(roundClearPSTMP.text, roundClearPSTMP));
	}

	#endregion

	[ServerRpc]
	public void GameClearServerRPC()
	{		
		SharedData.Instance.area.Value += 1;
		QuestManager.inst.QuestReset();
		//SharedData.Instance.questQuota.Value = 0;
		//SharedData.Instance.moneyQuota.Value = 0;
	}


	#region 텍스트 스크럼블

	private float scrambleSpeed = 0.05f;
	private float duration = 1f; // 전체 효과 지속 시간
	private string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789가나다라마바사아자차카타파하!@#$%^&*";

	public IEnumerator ScrambleIn(string finalText, TMP_Text textComponent)
	{
		int length = finalText.Length;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			float progress = elapsed / duration;
			int targetReveal = Mathf.FloorToInt(progress * length);

			string result = "";
			for (int i = 0; i < length; i++)
			{
				if (i < targetReveal)
				{
					result += characters[Random.Range(0, characters.Length)];
				}
				else
				{
					result += " ";
				}
			}

			textComponent.text = result;
			yield return new WaitForSeconds(scrambleSpeed);
			elapsed += scrambleSpeed;
		}

		// 최종 고정 텍스트 출력
		textComponent.text = finalText;


		yield return new WaitForSeconds(2f);

		StartCoroutine(ScrambleOut(finalText, textComponent));
	}


	IEnumerator ScrambleOut(string finalText, TMP_Text textComponent)
	{
		int length = finalText.Length;
		char[] currentChars = finalText.ToCharArray();
		float[] charTimers = new float[length]; // 각 글자마다 스크램블 시작 시점
		float startTime = Time.time;

		// 글자별 스크램블 시작 시간 세팅 (순차적으로 뒤에서 앞으로 시작)
		for (int i = 0; i < length; i++)
		{
			// 뒤에서부터 시작하기 때문에 i번째 글자는 duration 내에 마지막부터 사라지도록 오프셋 부여
			float ratio = (float)i / length;
			charTimers[i] = startTime + duration * ratio;
		}

		while (Time.time < startTime + duration)
		{
			for (int i = 0; i < length; i++)
			{
				if (Time.time >= charTimers[i])
				{
					// 남아있다면 스크램블 or 삭제
					if (currentChars[i] != ' ')
					{
						// 마지막 삭제 타이밍이 다가오면 삭제
						float deleteThreshold = charTimers[i] + (duration / length) * 0.8f;
						if (Time.time >= deleteThreshold)
						{
							currentChars[i] = ' ';
						}
						else
						{
							currentChars[i] = characters[Random.Range(0, characters.Length)];
						}
					}
				}
			}

			textComponent.text = new string(currentChars);
			yield return new WaitForSeconds(scrambleSpeed);
		}

		textComponent.text = "";
	}


	#endregion
}

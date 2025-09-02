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

	[Header("���")]
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
		// ĵ������ �̹��� �ʱ�ȭ
		summaryCanvs.gameObject.SetActive(false);

		// �ؽ�Ʈ �ʱ�ȭ
		/*		roundClearResultTMP.text = "";
				roundClearContentsTMP.text = "";
				roundClearPSTMP.text = "";*/
		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Area"; // ����ϰ��� �ϴ� Ű
		areaTMP.text = $"{localizedString.GetLocalizedString()} : " + SharedData.Instance.area.Value;

		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Mission Quota"; // ����ϰ��� �ϴ� Ű
		missionQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + QuestManager.inst.nowClearedQuestTotal.Value + " / " + SharedData.Instance.questQuota.Value;

		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Money Quota"; // ����ϰ��� �ϴ� Ű
		moneyQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + SharedData.Instance.Money.Value + " / " + SharedData.Instance.moneyQuota.Value;
	}

	private void SetLanguage(Locale newLocale)
	{
		Debug.Log($"�� {newLocale.LocaleName}�� ����Ǿ����ϴ�.");

		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Area"; // ����ϰ��� �ϴ� Ű
		areaTMP.text = $"{localizedString.GetLocalizedString()} : " + SharedData.Instance.area.Value;

		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Mission Quota"; // ����ϰ��� �ϴ� Ű
		missionQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + QuestManager.inst.nowClearedQuestTotal.Value + " / " + SharedData.Instance.questQuota.Value;

		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Money Quota"; // ����ϰ��� �ϴ� Ű
		moneyQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + SharedData.Instance.Money.Value + " / " + SharedData.Instance.moneyQuota.Value;
	}

	private void SetAreaTMP(int oldValue, int newValue)
	{
		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Area"; // ����ϰ��� �ϴ� Ű
		areaTMP.text = $"{localizedString.GetLocalizedString()} : " + newValue.ToString();
	}
	private void SetquestQuotaTMP(int oldValue, int newValue)
	{
		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Mission Quota"; // ����ϰ��� �ϴ� Ű
		missionQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + QuestManager.inst.nowClearedQuestTotal + " / " + SharedData.Instance.questQuota.Value;
	}
	private void SetmoneyQuotaTMP(int oldValue, int newValue)
	{
		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Money Quota"; // ����ϰ��� �ϴ� Ű
		moneyQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + SharedData.Instance.Money.Value + " / " + SharedData.Instance.moneyQuota.Value;
	}


	#region ���� Ŭ���� ����
	[ServerRpc]
	public void GameClearCheckServerRpc()
	{
		bool missionQuotaCleared = QuestManager.inst.nowClearedQuestTotal.Value >= SharedData.Instance.questQuota.Value;
		bool moneyQuotaCleared = SharedData.Instance.moneyQuota.Value <= SharedData.Instance.Money.Value; // ���ϴ� �������� ����

		if (missionQuotaCleared && moneyQuotaCleared)
		{
			SharedData.Instance.area.Value += 1; // ���� ����� ����
		}
		else
		{
			// Ŭ���� ����
			Debug.Log("���� Ŭ���� ���� - ���� �̴�");
			GameOverAnimeClientRpc();
		}
	}

	[ServerRpc]
	public void GameClearAnimeServerRpc()
	{
		GameClearAnimeClientRpc(); // Ŭ���� ���� ����
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
		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Result"; // ����ϰ��� �ϴ� Ű
		roundClearResultTMP.text = $"{localizedString.GetLocalizedString()}";

		StartCoroutine(ScrambleIn(roundClearResultTMP.text, roundClearResultTMP));
	}

	private void SetRoundClearContentsTMP()
	{
		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Completed Mission"; // ����ϰ��� �ϴ� Ű
		roundClearContentsTMP.text = $"{localizedString.GetLocalizedString()} : " + QuestManager.inst.nowClearedQuestTotal.Value + "\n";

		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Killed Enemy"; // ����ϰ��� �ϴ� Ű
		roundClearContentsTMP.text += $"{localizedString.GetLocalizedString()} : " + SharedData.Instance.area.Value + "\n";

		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Escape"; // ����ϰ��� �ϴ� Ű
		roundClearContentsTMP.text += $"{localizedString.GetLocalizedString()} : " + PlayersManager.Instance.allPlayersDead.Value + "\n";

		StartCoroutine(ScrambleIn(roundClearContentsTMP.text, roundClearContentsTMP));
	}

	private void SetRoundClearPSTMP()
	{
		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "PS"; // ����ϰ��� �ϴ� Ű
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


	#region �ؽ�Ʈ ��ũ����

	private float scrambleSpeed = 0.05f;
	private float duration = 1f; // ��ü ȿ�� ���� �ð�
	private string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789�����ٶ󸶹ٻ������īŸ����!@#$%^&*";

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

		// ���� ���� �ؽ�Ʈ ���
		textComponent.text = finalText;


		yield return new WaitForSeconds(2f);

		StartCoroutine(ScrambleOut(finalText, textComponent));
	}


	IEnumerator ScrambleOut(string finalText, TMP_Text textComponent)
	{
		int length = finalText.Length;
		char[] currentChars = finalText.ToCharArray();
		float[] charTimers = new float[length]; // �� ���ڸ��� ��ũ���� ���� ����
		float startTime = Time.time;

		// ���ں� ��ũ���� ���� �ð� ���� (���������� �ڿ��� ������ ����)
		for (int i = 0; i < length; i++)
		{
			// �ڿ������� �����ϱ� ������ i��° ���ڴ� duration ���� ���������� ��������� ������ �ο�
			float ratio = (float)i / length;
			charTimers[i] = startTime + duration * ratio;
		}

		while (Time.time < startTime + duration)
		{
			for (int i = 0; i < length; i++)
			{
				if (Time.time >= charTimers[i])
				{
					// �����ִٸ� ��ũ���� or ����
					if (currentChars[i] != ' ')
					{
						// ������ ���� Ÿ�̹��� �ٰ����� ����
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

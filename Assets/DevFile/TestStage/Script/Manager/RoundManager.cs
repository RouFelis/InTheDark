using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections;

public class RoundManager : MonoBehaviour
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


		InitResult();
	}

	private void InitResult()
	{
		// ĵ������ �̹��� �ʱ�ȭ
		summaryCanvs.gameObject.SetActive(false);

		// �ؽ�Ʈ �ʱ�ȭ
		roundClearResultTMP.text = "";
		roundClearContentsTMP.text = "";
		roundClearPSTMP.text = "";
	}


	public void GameClearAnime()
	{
		StartCoroutine(GameClearRoutine());
	}

	private IEnumerator GameClearRoutine()
	{
		summaryCanvs.gameObject.SetActive(true);

		SetRoundClearSummaryTMP();
		SetRoundClearContentsTMP();
		SetRoundClearPSTMP();

		yield return null;
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
		roundClearContentsTMP.text = $"{localizedString.GetLocalizedString()} : " + QuestManager.inst.nowClearedQuestTotal + "\n";

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
		missionQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + newValue.ToString();
	}
	private void SetmoneyQuotaTMP(int oldValue, int newValue)
	{
		localizedString.TableReference = "UITable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = "Money Quota"; // ����ϰ��� �ϴ� Ű
		moneyQuotaTMP.text = $"{localizedString.GetLocalizedString()} : " + newValue.ToString();
	}

	[ServerRpc]
	public void GameClearServerRPC()
	{		
		SharedData.Instance.area.Value += 1;
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

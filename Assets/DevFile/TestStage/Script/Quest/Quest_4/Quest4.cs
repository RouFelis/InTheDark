using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

public class Quest4 : QuestBase
{
    [Header("Prefabs")]
    public GameObject[] numberPrefabs; // 0~9 ������
    public Transform spawnParent;
    public Vector3 spawnStartPosition;
    public float spacing = 2f;


    [Header("UI")]
    [SerializeField] private float passwardWait = 1f;
    public TextMeshProUGUI passwordDisplay;

    private NetworkVariable<FixedString32Bytes> generatedPassword = new NetworkVariable<FixedString32Bytes>(readPerm:NetworkVariableReadPermission.Everyone);
    private string currentInput = "";


    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip correctSfx;
    public AudioClip wrongSfx;
    public AudioClip doorSfx;

    [Header("Prefab")]
    public GameObject[] questInteractableObjects;
    

    [Header("door")]
    [SerializeField] private Transform doorTransform;  // �� ������Ʈ
    [SerializeField] private float slideDistance = 2f; // ������ �󸶳� �̵�����
    [SerializeField] private float slideDuration = 2f; // �ִϸ��̼� �ð�

    public GameObject door;
    public Transform openTransform;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine currentCoroutine;
    private bool canInput = true;

    protected override void Start()
    {
        base.Start();
        // �ʱ� ��ġ ����
        closedPosition = doorTransform.localPosition;
        openPosition = closedPosition + Vector3.right * slideDistance; // ���������� �̵�

        if (IsServer)
        {
            GenerateAndSpawnPassword();
        }

        UpdatePasswordDisplay();
    }


    void GenerateAndSpawnPassword()
    {
        FixedString32Bytes password = new FixedString32Bytes();

        List<int> numbers = Enumerable.Range(0, 10).ToList(); // 0~9 ���� ����Ʈ

        for (int i = 0; i < 4; i++)
        {
            int index = Random.Range(0, numbers.Count); // �����ִ� ���� �� �ϳ� ����
            int digit = numbers[index];
            password.Append(digit.ToString());
            numbers.RemoveAt(index); // �ߺ� ������ ���� ���õ� ���� ����
        }

        generatedPassword.Value = password;
        Debug.Log($"[Server] Generated password: {generatedPassword.Value}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddDigitServerRpc(int digit)
	{
        AddDigitClientRpc(digit);

    }

    [ClientRpc]
    public void AddDigitClientRpc(int digit)
    {
        if (!canInput) return;

        if (currentInput.Length < 4)
        {
            currentInput += digit.ToString();
            UpdatePasswordDisplay();


            // �Է��� 4�ڸ� �̻��̸� Submit
            if (currentInput.Length >= 4)
            {
                SubmitInput();
            }
        }
    }

    // ��й�ȣ Ȯ���ϱ�
    public void SubmitInput()
    {
        if (!IsOwner || currentInput.Length < 4)
        {
            Debug.Log("�Է��� �����ϰų� ��ȿ���� ����");
            return;
        }

        CheckPasswordServerRpc(new FixedString32Bytes(currentInput));
    }

    // ��й�ȣ Ȯ��[����]
    [ServerRpc]
    void CheckPasswordServerRpc(FixedString32Bytes input)
    {
        bool isCorrect = input == generatedPassword.Value;
        if (isCorrect)
        {
            QuestSucceedServerRpc();
            CheckPasswordClientRpc(true, input, generatedPassword.Value);
            StartCoroutine(MoveDoor());
        }
        else
        {
            QuestFailedServerRpc();
            CheckPasswordClientRpc(false, input, generatedPassword.Value);
        }
    }

    // ������Ű.
    [ServerRpc(RequireOwnership = false)]
    public void CardPassServerRpc()
    {
        // ������ ���� ó��
        isCompleted.Value = true;

        // ���� Ŭ���̾�Ʈ�� ǥ��
        CheckPasswordClientRpc(true, generatedPassword.Value, generatedPassword.Value);

        // �������� �� ���� �ڷ�ƾ ����
        StartCoroutine(MoveDoor());
    }

    // Ŭ���̾�Ʈ �н����� ȿ��
    [ClientRpc]
    void CheckPasswordClientRpc(bool isCorrect, FixedString32Bytes input, FixedString32Bytes answer)
    {
        string inputStr = input.ToString();
        string answerStr = answer.ToString();

        int length = Mathf.Min(inputStr.Length, answerStr.Length);

        string coloredText = "";
        for (int i = 0; i < length; i++)
        {
            char c = inputStr[i];
            if (c == answerStr[i])
            {
                // �ʷϻ�
                coloredText += $"<color=#00FF00>{c}</color> ";
            }
            else if (answerStr.Contains(c.ToString()))
            {
                // �����
                coloredText += $"<color=#FFFF00>{c}</color> ";
            }
            else
            {
                // ������
                coloredText += $"<color=#FF0000>{c}</color> ";
            }
        }

        // ������ �ڸ����� �����(_)�� ä���
        for (int i = length; i < 4; i++)
        {
            coloredText += "_ ";
        }

        passwordDisplay.text = coloredText.TrimEnd();

        if (isCorrect)
        {
            StartCoroutine(HandleCorrectPassword());
        }
        else
        {
            StartCoroutine(HandleIncorrectPassword());
        }
    }

    // ��� ȿ��
    IEnumerator HandleCorrectPassword()
    {
        // �ʷϻ����� ���� + ���� ����
        passwordDisplay.color = Color.green;
        audioSource.PlayOneShot(correctSfx);
		foreach (var interactObject in questInteractableObjects)
		{
            interactObject.layer = 0;
		}

        yield return null;
    }

    // ��ȿ��
    private IEnumerator MoveDoor()
    {
        float elapsed = 0f;
        yield return new WaitForSeconds(0.5f);
        audioSource.PlayOneShot(doorSfx);
        while (elapsed < slideDuration)
        {
            float t = elapsed / slideDuration;
            door.transform.localPosition = Vector3.Lerp(door.transform.position, openTransform.position, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        door.transform.localPosition = openTransform.position;
    }

    IEnumerator HandleIncorrectPassword()
    {
        canInput = false; 
        audioSource.PlayOneShot(wrongSfx);

        for (int i = 0; i < 2; i++)
        {
            passwordDisplay.color = Color.clear;
            yield return new WaitForSeconds(0.15f);
            passwordDisplay.color = Color.white;
            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(passwardWait);

        ResetInput();
        canInput = true; 
    }

    void UpdatePasswordDisplay()
    {
        string display = "";

        for (int i = 0; i < 4; i++)
        {
            if (i < currentInput.Length)
                display += currentInput[i] + " ";
            else
                display += "_ ";
        }

        if (passwordDisplay != null)
            passwordDisplay.text = display.TrimEnd();

        // ���� �ʱ�ȭ
        passwordDisplay.color = Color.white;
    }

    public void ResetInput()
    {
        currentInput = "";
        UpdatePasswordDisplay();
    }
}

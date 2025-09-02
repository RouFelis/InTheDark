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
    public GameObject[] numberPrefabs; // 0~9 프리팹
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
    [SerializeField] private Transform doorTransform;  // 문 오브젝트
    [SerializeField] private float slideDistance = 2f; // 옆으로 얼마나 이동할지
    [SerializeField] private float slideDuration = 2f; // 애니메이션 시간

    public GameObject door;
    public Transform openTransform;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine currentCoroutine;
    private bool canInput = true;

    protected override void Start()
    {
        base.Start();
        // 초기 위치 저장
        closedPosition = doorTransform.localPosition;
        openPosition = closedPosition + Vector3.right * slideDistance; // 오른쪽으로 이동

        if (IsServer)
        {
            GenerateAndSpawnPassword();
        }

        UpdatePasswordDisplay();
    }


    void GenerateAndSpawnPassword()
    {
        FixedString32Bytes password = new FixedString32Bytes();

        List<int> numbers = Enumerable.Range(0, 10).ToList(); // 0~9 숫자 리스트

        for (int i = 0; i < 4; i++)
        {
            int index = Random.Range(0, numbers.Count); // 남아있는 숫자 중 하나 선택
            int digit = numbers[index];
            password.Append(digit.ToString());
            numbers.RemoveAt(index); // 중복 방지를 위해 선택된 숫자 제거
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


            // 입력이 4자리 이상이면 Submit
            if (currentInput.Length >= 4)
            {
                SubmitInput();
            }
        }
    }

    // 비밀번호 확인하기
    public void SubmitInput()
    {
        if (!IsOwner || currentInput.Length < 4)
        {
            Debug.Log("입력이 부족하거나 유효하지 않음");
            return;
        }

        CheckPasswordServerRpc(new FixedString32Bytes(currentInput));
    }

    // 비밀번호 확인[서버]
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

    // 마스터키.
    [ServerRpc(RequireOwnership = false)]
    public void CardPassServerRpc()
    {
        // 무조건 정답 처리
        isCompleted.Value = true;

        // 정답 클라이언트에 표시
        CheckPasswordClientRpc(true, generatedPassword.Value, generatedPassword.Value);

        // 서버에서 문 열기 코루틴 실행
        StartCoroutine(MoveDoor());
    }

    // 클라이언트 패스워드 효과
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
                // 초록색
                coloredText += $"<color=#00FF00>{c}</color> ";
            }
            else if (answerStr.Contains(c.ToString()))
            {
                // 노란색
                coloredText += $"<color=#FFFF00>{c}</color> ";
            }
            else
            {
                // 빨간색
                coloredText += $"<color=#FF0000>{c}</color> ";
            }
        }

        // 부족한 자리수는 언더바(_)로 채우기
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

    // 비번 효과
    IEnumerator HandleCorrectPassword()
    {
        // 초록색으로 변경 + 정답 사운드
        passwordDisplay.color = Color.green;
        audioSource.PlayOneShot(correctSfx);
		foreach (var interactObject in questInteractableObjects)
		{
            interactObject.layer = 0;
		}

        yield return null;
    }

    // 문효과
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

        // 색상 초기화
        passwordDisplay.color = Color.white;
    }

    public void ResetInput()
    {
        currentInput = "";
        UpdatePasswordDisplay();
    }
}

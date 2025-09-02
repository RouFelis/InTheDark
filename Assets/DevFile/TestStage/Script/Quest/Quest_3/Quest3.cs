using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;

public class Quest3 : QuestBase
{
    [Header("UI")]
    [SerializeField] private TMP_Text displayText;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip addSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failSound;

    [Header("Puzzle Settings")]
    [SerializeField] private int totalPuzzles = 3;
    private int currentPuzzleIndex = 0;

    private const int MaxDisplayLines = 12;

    private List<string> history = new List<string>(); // 서버만 보관
    private string cachedDisplay = "";

    private NetworkVariable<FixedString128Bytes> currentProblem = new NetworkVariable<FixedString128Bytes>(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<FixedString64Bytes> currentInput = new NetworkVariable<FixedString64Bytes>(writePerm: NetworkVariableWritePermission.Server);
	private NetworkVariable<int> currentAnswer = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

    private List<string> hackingEffectLines = new List<string>
{
    "> Verifying password...",
    "> Checking access permissions...",
    "> Verifying access level...",
    "> Performing system status check...",
    "> Confirming protocol execution stage...",
    "> Detecting abnormal external connections...",
    "> Connection verification denied...",
    "> Continuing execution..."
};

    [SerializeField] private float hackingEffectDelay = 0.1f;
    private Coroutine hackingEffectCoroutine;

    private void Awake()
    {
        AddInitialBootMessages();
    }

    public override void OnNetworkSpawn()
    {
        currentProblem.OnValueChanged += (_, _) => UpdateDisplay();
        currentInput.OnValueChanged += (_, _) => UpdateDisplay();

        if (IsServer)
            GenerateNextPuzzle();

        UpdateDisplay();
    }

    private void AddInitialBootMessages()
    {
        history.Add("> SEARCH ESCAPE PROTOCOL... ████████████ 100%");
        history.Add("> Launching tool: /System/Democracy/F1/Reset.exe");
    }


    // 숫자 추가 입력
    [ServerRpc(RequireOwnership = false)]
    public void AddDigitServerRpc(int digit)
    {
        if (digit < 0 || digit > 9) return;

        string current = currentInput.Value.ToString();
        if (current.Length < 3)
        {
            currentInput.Value = current + digit;
            PlayAddSoundClientRpc();
        }
    }

    // 제출 버튼 입력
    [ServerRpc(RequireOwnership = false)]
    public void SubmitInputServerRpc(ServerRpcParams rpcParams = default)
    {
        string input = currentInput.Value.ToString();
        history.Add($">> {input}");

        if (!int.TryParse(input, out int value))
        {
            history.Add("Invalid input.");
        }
        else if (value == currentAnswer.Value)
        {
            history.Add("Correct.");
            currentPuzzleIndex++;

            if (currentPuzzleIndex >= totalPuzzles)
            {
                history.Add("Complete.");
                isCompleted.Value = true;
                currentInput.Value = "";
                UpdateAllClientRpc(BuildDisplayText());
                return;
            }

            SoundPlaySuccessClientRpc();

            currentInput.Value = "";

            // 정답 시 해킹 효과 실행 (문제 갱신은 효과 종료 후 서버가 수행)
            PlayHackingEffectClientRpc();
            return;
        }
        else
        {
            QuestFailedServerRpc();
            SoundPlayFailureClientRpc();
            history.Add("Incorrect.");
        }

        currentInput.Value = "";
        UpdateDisplay();
    }

	private void GenerateNextPuzzle()
	{
        string equation = GenerateEquation(out int answer);
        currentProblem.Value = $"{equation} = ?";
        currentAnswer.Value = answer;

        history.Add("Input password");
        history.Add(currentProblem.Value.ToString());

        UpdateDisplay();
    }


    [ClientRpc]
    private void PlayHackingEffectClientRpc()
    {
        if (hackingEffectCoroutine != null)
            StopCoroutine(hackingEffectCoroutine);

        hackingEffectCoroutine = StartCoroutine(PlayHackingEffectAndRefresh());
    }

	private IEnumerator PlayHackingEffectAndRefresh()
	{
        foreach (string line in hackingEffectLines)
        {
            history.Add(line);
            displayText.text = BuildDisplayText();
            yield return new WaitForSeconds(hackingEffectDelay);
        }

        yield return new WaitForSeconds(0.5f);

        // 클라이언트 화면만 초기화
        history.Clear();
        cachedDisplay = "";
        displayText.text = "";

        // 서버에 다음 문제 요청
        if (IsServer)
        {
            GenerateNextPuzzle();
        }
    }

    private string GenerateEquation(out int answer)
    {
        int a = Random.Range(1, 99);
        int b = Random.Range(1, 99);
        string[] ops = { "+", "-", "x" };
        string op = ops[Random.Range(0, ops.Length)];

        answer = op switch
        {
            "+" => a + b,
            "-" => a - b,
            "x" => a * b,
            _ => 0
        };

        // 유효성 체크: 음수거나 너무 크면 다시 생성
        return (answer < 0 || answer > 999)
            ? GenerateEquation(out answer)
            : $"{a} {op} {b}";
    }

    [ClientRpc]
    private void UpdateAllClientRpc(string newDisplay)
    {
        cachedDisplay = newDisplay;
        displayText.text = cachedDisplay;
    }

    private void UpdateDisplay()
    {
        if (IsServer)
        {
            UpdateAllClientRpc(BuildDisplayText());
        }
        else
        {
            displayText.text = cachedDisplay;
        }
    }

    private string BuildDisplayText()
    {
        List<string> lines = new List<string>(history)
        {
            $">> {currentInput.Value}"
        };

        // Max line 수를 넘을 경우, 위에서부터 제거
        while (lines.Count > MaxDisplayLines)
            lines.RemoveAt(0);

        return string.Join("\n", lines);
    }

    [ClientRpc]
    private void PlayAddSoundClientRpc()
    {
        audioSource.clip = addSound;
        audioSource.Play();
    }

    [ClientRpc]
    private void SoundPlaySuccessClientRpc()
    {
        audioSource.clip = successSound;
        audioSource.Play();
    }

    [ClientRpc]
    private void SoundPlayFailureClientRpc()
    {
        audioSource.clip = failSound;
        audioSource.Play();
    }  
    



    [ServerRpc(RequireOwnership = false)]
    public override void QuestFailedServerRpc()
    {
        base.QuestFailedServerRpc();
        SoundPlayFailureClientRpc();
    }
}


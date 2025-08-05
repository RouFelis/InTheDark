using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
    public TextMeshProUGUI passwordDisplay;

    private NetworkVariable<FixedString32Bytes> generatedPassword = new NetworkVariable<FixedString32Bytes>(readPerm:NetworkVariableReadPermission.Everyone);
    private string currentInput = "";


    [Header("door")]
    [SerializeField] private Transform doorTransform;  // 문 오브젝트
    [SerializeField] private float slideDistance = 2f; // 옆으로 얼마나 이동할지
    [SerializeField] private float slideDuration = 1f; // 애니메이션 시간

    public GameObject door;
    public Transform openTransform;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private Coroutine currentCoroutine;

    protected override void Start()
    {
        base.Start();
        // 초기 위치 저장
        closedPosition = doorTransform.localPosition;
        openPosition = closedPosition + Vector3.right * slideDistance; // 오른쪽으로 이동
    }


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GenerateAndSpawnPassword();
        }

        UpdatePasswordDisplay(); // UI 초기화
    }

    void GenerateAndSpawnPassword()
    {
        string password = "";
        for (int i = 0; i < 4; i++)
        {
            int digit = Random.Range(0, 10);
            password += digit.ToString();

            Vector3 spawnPos = spawnStartPosition + Vector3.right * spacing * i;
            GameObject prefab = numberPrefabs[digit];

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity, spawnParent);
            obj.GetComponent<NetworkObject>().Spawn();
        }

        generatedPassword.Value = password;
        Debug.Log($"[Server] Generated password: {password}");
    }

    // 숫자 입력 받기
    public void AddDigit(int digit)
    {
        if (!IsOwner) return;

        if (currentInput.Length < 4)
        {
            currentInput += digit.ToString();
            UpdatePasswordDisplay();
        }
    }

    // 입력 제출
    public void SubmitInput()
    {
        if (!IsOwner || currentInput.Length < 4)
        {
            Debug.Log("입력이 부족하거나 유효하지 않음");
            return;
        }

        CheckPasswordServerRpc(currentInput);
    }

    [ServerRpc(RequireOwnership = false)]
    void CheckPasswordServerRpc(string input)
    {
        bool correct = input == generatedPassword.Value.ToString();
        if (correct)
            StartCoroutine(MoveDoor());
    }

    [ClientRpc]
    void CorrectPasswardClientRpc()
	{

	}

    private IEnumerator MoveDoor()
    {
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            float t = elapsed / slideDuration;
            door.transform.localPosition = Vector3.Lerp(door.transform.position, openTransform.position, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        door.transform.localPosition = openTransform.position;
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
    }
}

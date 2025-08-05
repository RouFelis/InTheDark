using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
    public TextMeshProUGUI passwordDisplay;

    private NetworkVariable<FixedString32Bytes> generatedPassword = new NetworkVariable<FixedString32Bytes>(readPerm:NetworkVariableReadPermission.Everyone);
    private string currentInput = "";


    [Header("door")]
    [SerializeField] private Transform doorTransform;  // �� ������Ʈ
    [SerializeField] private float slideDistance = 2f; // ������ �󸶳� �̵�����
    [SerializeField] private float slideDuration = 1f; // �ִϸ��̼� �ð�

    public GameObject door;
    public Transform openTransform;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private Coroutine currentCoroutine;

    protected override void Start()
    {
        base.Start();
        // �ʱ� ��ġ ����
        closedPosition = doorTransform.localPosition;
        openPosition = closedPosition + Vector3.right * slideDistance; // ���������� �̵�
    }


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GenerateAndSpawnPassword();
        }

        UpdatePasswordDisplay(); // UI �ʱ�ȭ
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

    // ���� �Է� �ޱ�
    public void AddDigit(int digit)
    {
        if (!IsOwner) return;

        if (currentInput.Length < 4)
        {
            currentInput += digit.ToString();
            UpdatePasswordDisplay();
        }
    }

    // �Է� ����
    public void SubmitInput()
    {
        if (!IsOwner || currentInput.Length < 4)
        {
            Debug.Log("�Է��� �����ϰų� ��ȿ���� ����");
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
